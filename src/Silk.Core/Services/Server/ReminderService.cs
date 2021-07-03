using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Net;
using FluentAssertions.Common;
using Humanizer;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.MediatR.Reminders;
using Silk.Core.Data.Models;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Services.Server
{
    public sealed class ReminderService : BackgroundService
    {
        private const string MissingChannel = "Hey!, you wanted me to remind you of something, but the channel was deleted, or is otherwise inaccessible to me now.\n";
        private readonly DiscordShardedClient _client;
        private readonly ILogger<ReminderService> _logger;
        private readonly IMediator _mediator;
        private readonly IServiceProvider _services;

        private List<Reminder> _reminders; // We're gonna slurp all reminders into memory. Yolo, I guess.

        public ReminderService(ILogger<ReminderService> logger, IServiceProvider services, DiscordShardedClient client, IMediator mediator)
        {
            _logger = logger;
            _services = services;
            _client = client;
            _mediator = mediator;

        }

        public async Task CreateReminder
        (
            DateTime expiration, ulong ownerId,
            ulong channelId, ulong messageId, ulong? guildId,
            string? messageContent, bool wasReply = false,
            ReminderType type = ReminderType.Once, ulong? replyId = null,
            ulong? replyAuthorId = null, string? replyMessageContent = null)
        {
            Reminder reminder = await _mediator.Send(new CreateReminderRequest(expiration, ownerId, channelId, messageId, guildId, messageContent, wasReply, type, replyId, replyAuthorId, replyMessageContent));
            _reminders.Add(reminder);
        }

        public async Task<IEnumerable<Reminder>> GetRemindersAsync(ulong userId)
        {
            IEnumerable<Reminder> reminders = _reminders.Where(r => r.OwnerId == userId);
            Reminder[] reminderArr = reminders as Reminder[] ?? reminders.ToArray();
            return reminderArr.Length is 0 ? Array.Empty<Reminder>() : reminderArr;
        }

        public async Task RemoveReminderAsync(int id)
        {
            Reminder? reminder = _reminders.SingleOrDefault(r => r.Id == id);
            if (reminder is null)
            {
                _logger.LogWarning("Reminder was not present in memory. Was it dispatched already?");
            }
            else
            {
                _reminders.Remove(reminder);
                await _mediator.Send(new RemoveReminderRequest(id));
            }
        }

        private async Task Tick()
        {
            // ReSharper disable once ForCanBeConvertedToForeach //
            //             Collection gets modified.             //
            for (var i = 0; i < _reminders.Count; i++)
            {
                Reminder r = _reminders[i];
                if (r.Expiration > DateTime.UtcNow) continue;

                Task t = DispatchReminderAsync(r);
                Task timeout = Task.Delay(900);
                Task tr = await Task.WhenAny(t, timeout);

                if (tr == timeout)
                    _logger.LogWarning("Slow dispatch on reminder! Expect failed dispatch log.");
            }
        }

        /// <summary>
        /// The main Dispatch method. Determines the dispatch route to take.
        /// </summary>
        /// <param name="reminder"></param>
        private async Task DispatchReminderAsync(Reminder reminder)
        {
            if (reminder.MessageId is 0)
            {
                await DispatchSlashCommandReminderAsync(reminder); // Was executed with a slash command. Don't send the reminder in the server. //
                return;
            }
            IEnumerable<KeyValuePair<ulong, DiscordGuild>> guilds = _client.ShardClients.SelectMany(s => s.Value.Guilds);
            if (guilds.FirstOrDefault(g => g.Key == reminder.GuildId).Value is not { } guild)
            {
                _logger.LogWarning("{GuildId} is not present on the client. Was the guild removed?", reminder.GuildId);
                _logger.LogTrace("Removing all reminders from memory pointing to {GuildId}", reminder.GuildId);
                _reminders.RemoveAll(r => r.GuildId == reminder.GuildId);
            }
            else
            {
                if (reminder.Type is ReminderType.Once)
                {
                    await SendGuildReminderAsync(reminder, guild);
                    await RemoveReminderAsync(reminder.Id);
                }
                else
                {
                    guild.Channels.TryGetValue(reminder.ChannelId, out var channel);
                    channel ??= await _client.GetMember(m => m.Id == reminder.OwnerId)?.CreateDmChannelAsync()!;

                    if (channel is null)
                    {
                        _logger.LogWarning("Member has a recurring reminder but is not present on the guild. Were they kicked?");
                        _logger.LogTrace("Removing all {UserId}'s reminders...", reminder.OwnerId);

                        foreach (var remind in _reminders.Where(r => r.OwnerId == reminder.OwnerId))
                            await RemoveReminderAsync(remind.Id);

                        return; // Member doesn't exist //
                    }

                    await DispatchRecurringReminderAsync(reminder, channel);
                }
            }
        }

        /// <summary>
        /// Dispatches a reminder created via slash command to the user's DMs.
        /// If the user's DMs are closed, it will attempt to send to the guild it was created on.
        /// </summary>
        /// <param name="reminder"></param>
        private async Task DispatchSlashCommandReminderAsync(Reminder reminder)
        {
            var apiClient = (DiscordApiClient) typeof(DiscordClient).GetProperty("ApiClient", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(_client.ShardClients[0])!;
            var channel = await (Task<DiscordDmChannel>) typeof(DiscordApiClient).GetMethod("CreateDmAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(apiClient, new object[] {reminder.OwnerId})!;

            await RemoveReminderAsync(reminder.Id);
            _logger.LogTrace("Removed reminder from queue");

            _logger.LogTrace("Preparring to send reminder");
            try
            {
                await channel.SendMessageAsync($"Hey! You wanted me to remind you of something <t:{reminder.CreationTime.ToDateTimeOffset().ToUnixTimeSeconds()}:R>! \nReminder: {reminder.MessageContent}");
                _logger.LogTrace("Successfully dispatched reminder.");
            }
            catch (UnauthorizedException)
            {
                _logger.LogWarning("Failed to dispatch reminder invoked by slash-command. Did they leave they close their DMs?");

                if (reminder.GuildId is 0 or null)
                {
                    _logger.LogWarning("Reminder does not have associated guild. Skipping.");
                    return;
                }

                var shard = _client.GetShard(reminder.GuildId.Value);
                var foundGuild = shard.Guilds.TryGetValue(reminder.GuildId.Value, out var guild);

                if (!foundGuild)
                {
                    _logger.LogWarning("GuildId was present on reminder but not on the client. Skipping.");
                    return;
                }

                var gotChannel = guild!.Channels.TryGetValue(reminder.ChannelId, out var guildChannel);

                if (!gotChannel)
                {
                    _logger.LogWarning("Reminder pointed to guild channel, but is not present on guild. Skipping.");
                    return;
                }

                await guildChannel!.SendMessageAsync($"Hey! You wanted me to remind you of something <t:{reminder.CreationTime.ToDateTimeOffset().ToUnixTimeSeconds()}:R>! \nReminder: {reminder.MessageContent}");
                _logger.LogTrace("Successfully dispatched reminder.");
            }
        }

        /// <summary>
        /// Updates the expiration of a recurring reminder.
        /// </summary>
        /// <param name="reminder">The reminder to update.</param>
        /// <exception cref="ArgumentException">The repetition period is unsupported.</exception>
        private async Task UpdateRecurringReminderAsync(Reminder reminder)
        {
            DateTime time = reminder.Type switch
            {
                ReminderType.Hourly => DateTime.UtcNow + TimeSpan.FromHours(1),
                ReminderType.Daily => DateTime.UtcNow + TimeSpan.FromDays(1),
                ReminderType.Weekly => DateTime.UtcNow + TimeSpan.FromDays(7),
                ReminderType.Monthly => DateTime.UtcNow + TimeSpan.FromDays(30),
                ReminderType.Once => throw new ArgumentException("Non-recurring reminders are not supported."),
                _ => throw new ArgumentException()
            };
            int index = _reminders.IndexOf(reminder);
            _reminders[index] = await _mediator.Send(new UpdateReminderRequest(reminder, time));
        }

        /// <summary>
        /// Sends a regular, non-recurring reminder to the server.
        /// </summary>
        /// <param name="reminder">The reminder to dispatch.</param>
        /// <param name="guild">The guild the reminder belongs to.</param>
        private async Task SendGuildReminderAsync(Reminder reminder, DiscordGuild guild)
        {
            if (reminder.Type is ReminderType.Once)
                _logger.LogTrace("Dequeing reminder");

            if (!guild.Channels.TryGetValue(reminder.ChannelId, out var channel)) { await DispatchDmReminderMessageAsync(reminder, guild); }
            else { await DispatchReplyReminderAsync(reminder, channel); }
        }
    
        /// <summary>
        /// Dispatches a reminder that is set to be recurring, updating it's expiration as necessary.
        /// </summary>
        /// <param name="reminder">The reminder to update.</param>
        /// <param name="channel">The channel the reminder should be sent to.</param>
        private async Task DispatchRecurringReminderAsync(Reminder reminder, DiscordChannel channel)
        {
            DiscordMessageBuilder? builder = new DiscordMessageBuilder().WithAllowedMention(new UserMention(reminder.OwnerId));
            var message = $"Hey, <@{reminder.OwnerId}>! You wanted to reminded {reminder.Type.Humanize(LetterCasing.LowerCase)}: \n{reminder.MessageContent}";
            builder.WithContent(message);

            await channel.SendMessageAsync(builder);
            await UpdateRecurringReminderAsync(reminder);
        }

        /// <summary>
        /// Dispatches a reminder that was created with a reply to another message.
        /// </summary>
        /// <param name="reminder">The reminder to dispatch.</param>
        /// <param name="channel">The channel the reminder was created in.</param>
        private async Task DispatchReplyReminderAsync(Reminder reminder, DiscordChannel channel)
        {
            _logger.LogTrace("Preparing to send reminder");
            DiscordMessageBuilder? builder = new DiscordMessageBuilder().WithAllowedMention(new UserMention(reminder.OwnerId));
            string? mention = reminder.WasReply ? $" <@{reminder.OwnerId}>," : null;
            var message = $"Hey, {mention}! {Formatter.Timestamp(DateTime.UtcNow - reminder.CreationTime)}:\n{reminder.MessageContent}";

            // These are misleading names (They don't actually dispatch a message) I know but w/e. //
            if (reminder.WasReply) { await SendReplyReminderAsync(reminder, channel, builder, message); }
            else { await SendReminderAsync(reminder, channel, builder, message); }

            await builder.SendAsync(channel);
            _logger.LogTrace("Sent reminder succesfully");
        }
        
        /// <summary>
        /// Sends a reminder replying to the referenced message in the reply.
        /// </summary>
        /// <param name="reminder">The reminder to send.</param>
        /// <param name="channel">The channel the reminder was created in.</param>
        /// <param name="builder">The builder to tack a reply to.</param>
        /// <param name="message"></param>
        private static async Task SendReminderAsync(Reminder reminder, DiscordChannel channel, DiscordMessageBuilder builder, string message)
        {
            bool validMessage;

            try { validMessage = await channel.GetMessageAsync(reminder.MessageId) is not null; }
            catch (NotFoundException) { validMessage = false; }
            if (validMessage)
            {
                builder.WithReply(reminder.MessageId, true);
                builder.WithContent($"You wanted me to remind you of this {Formatter.Timestamp(DateTime.UtcNow - reminder.CreationTime)}!");
            }
            else
            {
                message += "\n(Your message was deleted, hence the lack of a reply!)";
                builder.WithContent(message);
            }
        }
        
        /// <summary>
        /// Sends a reminder that intiially replied to another message.
        /// </summary>
        /// <param name="reminder"></param>
        /// <param name="channel"></param>
        /// <param name="builder"></param>
        /// <param name="message"></param>
        private static async Task SendReplyReminderAsync(Reminder reminder, DiscordChannel channel, DiscordMessageBuilder builder, string message)
        {
            bool validReply;

            try { validReply = await channel.GetMessageAsync(reminder.ReplyId.Value) is not null; }
            catch (NotFoundException) { validReply = false; }

            if (validReply)
            {
                builder.WithReply(reminder.ReplyId.Value);
                builder.WithContent(message);
            }
            else
            {
                message += "\n(You replied to someone, but that message was deleted!)\n";
                message += $"Replying to:\n> {reminder.ReplyMessageContent!.Pull(..250)}\n" +
                           $"From: <@{reminder.ReplyAuthorId}>";
                builder.WithContent(message);
            }
        }
        
        
        /// <summary>
        /// Sends a reminder to the creator of said reminder as a fallback in case the channel the reminder was created in is
        /// deleted or otherwise no longer accessible.
        /// </summary>
        /// <param name="reminder">The reminder to send.</param>
        /// <param name="guild">The guild the reminder was initialy sent on.</param>
        private async Task DispatchDmReminderMessageAsync(Reminder reminder, DiscordGuild guild)
        {
            _logger.LogWarning("Channel doesn't exist on guild! Attempting to DM user");

            try
            {
                DiscordMember member = await guild.GetMemberAsync(reminder.OwnerId);
                await member.SendMessageAsync(MissingChannel + $"<t:{reminder.CreationTime.ToDateTimeOffset().ToUnixTimeSeconds()}:R>: \n{reminder.MessageContent}");
            }
            catch (UnauthorizedException) { _logger.LogWarning("Failed to message user, skipping "); }
            catch (NotFoundException) { _logger.LogWarning("Member left guild, skipping"); }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Started!");

            using IServiceScope scope = _services.CreateScope();
            var mediator = scope.ServiceProvider.Get<IMediator>();

            _reminders = (await mediator!.Send(new GetAllRemindersRequest(), stoppingToken)).ToList();
            _logger.LogTrace("Loaded {ReminderCount} reminders", _reminders.Count);
            _logger.LogDebug("Starting reminder callback timer");

            var timer = new Timer(__ => _ = Tick(), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            try { await Task.Delay(-1, stoppingToken); }
            catch (TaskCanceledException) { }
            finally
            {
                _logger.LogDebug("Cancelation requested. Stopping service. ");
                await timer.DisposeAsync();
                // It's safe to clear the list as it's all saved to the database prior when they're added. //
                _reminders.Clear();
            }
        }
    }
}