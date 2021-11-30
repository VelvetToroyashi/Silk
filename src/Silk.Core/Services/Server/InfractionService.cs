using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Silk.Core.Data.Entities;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Infractions;
using Silk.Core.Services.Data;
using Silk.Core.Services.Interfaces;
using Silk.Core.Types;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Constants;

namespace Silk.Core.Services.Server
{
    public sealed class InfractionService : IHostedService, IInfractionService
    {
        private readonly DiscordClient _client;

        private readonly ConfigService _config;

        private readonly InfractionStepEntity _ignoreStep = new() { Type = InfractionType.Ignore };

        // Holds all temporary infractions. This could be a separate hashset like the mutes, but I digress ~Velvet //
        private readonly List<InfractionEntity>      _infractions = new();
        private readonly ILogger<IInfractionService> _logger;
        private readonly IMediator                   _mediator;

        // Fast lookup for mutes. Populated on startup. //
        private readonly HashSet<(ulong user, ulong guild)> _mutes = new();

        private readonly ConcurrentDictionary<ulong, SemaphoreSlim> _semaphoreDict = new();
        private readonly AsyncTimer                                 _timer;
        private readonly ICacheUpdaterService                       _updater;

        private readonly DiscordWebhookClient _webhookClient = new();

        public InfractionService(IMediator mediator, DiscordClient client, ConfigService config, ICacheUpdaterService updater, ILogger<IInfractionService> logger)
        {
            _mediator = mediator;
            _client = client;
            _config = config;
            _updater = updater;
            _logger = logger;
            _timer = new(DequeueInfractionsAsync, TimeSpan.FromSeconds(1), true);
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(EventIds.Service, "Started!");
            DateTime now = DateTime.UtcNow;
            await Task.Yield();

            _logger.LogInformation(EventIds.Service, "Loading infractions");
            Task<IEnumerable<InfractionEntity>>? infractionsTask = _mediator.Send(new GetCurrentInfractionsRequest(), cancellationToken);
            Task? delayTask = Task.Delay(200, cancellationToken);

            if (await Task.WhenAny(infractionsTask, delayTask) != delayTask)
            {
                await LoadAndCacheInfractionsAsync();
            }
            else
            {
                _logger.LogWarning(EventIds.Service, "Slow load for infractions. Offloading to ThreadPool.");
                _ = Task.Run(async () => await LoadAndCacheInfractionsAsync(), cancellationToken);
            }

            async Task LoadAndCacheInfractionsAsync()
            {
                IEnumerable<InfractionEntity>? allInfractions = await infractionsTask;
                foreach (var infraction in allInfractions)
                {
                    if (infraction.InfractionType is InfractionType.AutoModMute or InfractionType.Mute)
                        _mutes.Add((infraction.UserId, infraction.GuildId));
                    _infractions.Add(infraction);
                }
                TimeSpan tsNow = DateTime.UtcNow - now;
                _logger.LogInformation(EventIds.Service, "Loaded {Infractions} infractions in {Time} ms", allInfractions.Count(), tsNow.TotalMilliseconds.ToString("N0"));
                _timer.Start();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Dispose(); // Stops the timer. It's fine. //
        }

        /* TODO: Make these methods return Task<InfractionResult>
         Also did I mention how much I *love* Multi-line to-do statements */
        public async Task<InfractionResult> KickAsync(ulong userId, ulong guildId, ulong enforcerId, string reason)
        {
            DiscordGuild? guild = _client.Guilds[guildId];
            DiscordMember? member = guild.Members[userId];
            DiscordMember? enforcer = guild.Members[enforcerId];
            DiscordEmbedBuilder? embed = CreateUserInfractionEmbed(enforcer, guild.Name, InfractionType.Kick, reason);

            if (member.IsAbove(enforcer))
                return InfractionResult.FailedGuildHeirarchy;

            if (!guild.CurrentMember.HasPermission(Permissions.KickMembers))
                return InfractionResult.FailedSelfPermissions;

            var notified = true;

            try { await member.SendMessageAsync(embed); }
            catch (UnauthorizedException) { notified = false; }

            try { await member.RemoveAsync(reason); }
            catch (NotFoundException) { return InfractionResult.FailedGuildMemberCache; }
            catch (UnauthorizedException) { return InfractionResult.FailedSelfPermissions; } /* This shouldn't apply, but. */

            InfractionEntity? inf = await GenerateInfractionAsync(userId, guildId, enforcerId, InfractionType.Kick, reason, null);

            await LogInfractionAsync(inf);
            return notified ? InfractionResult.SucceededWithNotification : InfractionResult.SucceededWithoutNotification;
        }

        public async Task<InfractionResult> BanAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration = null)
        {
            DiscordGuild? guild = _client.Guilds[guildId];
            DiscordMember? enforcer = guild.Members[enforcerId];
            bool userExists = guild.Members.TryGetValue(userId, out var member);
            DiscordEmbedBuilder? embed = CreateUserInfractionEmbed(enforcer, guild.Name, expiration is null ? InfractionType.Ban : InfractionType.SoftBan, reason, expiration);

            if ((member?.Hierarchy ?? -1) > enforcer.Hierarchy)
                return InfractionResult.FailedGuildHeirarchy;

            if ((member?.Hierarchy ?? -1) > guild.CurrentMember.Hierarchy)
                return InfractionResult.FailedGuildHeirarchy;

            var notified = false;

            try
            {
                if (userExists)
                {
                    await member?.SendMessageAsync(embed)!;
                    notified = true;
                }
            }
            catch (UnauthorizedException) { }

            try
            {
                await guild.BanMemberAsync(userId, 0, reason);

                InfractionEntity? inf = await GenerateInfractionAsync(userId, guildId, enforcerId, expiration is null ? InfractionType.Ban : InfractionType.SoftBan, reason, expiration);

                if (inf.Duration is not null)
                    _infractions.Add(inf);

                await LogInfractionAsync(inf);
                return notified ?
                    InfractionResult.SucceededWithNotification :
                    InfractionResult.SucceededWithoutNotification;
            }
            catch (UnauthorizedException) /*Shouldn't happen, but you know.*/
            {
                return InfractionResult.FailedSelfPermissions;
            }
        }

        public async Task<InfractionResult> UnBanAsync(ulong userId, ulong guildId, ulong enforcerId, string reason = "Not Given.")
        {
            DiscordGuild? guild = _client.Guilds[guildId];

            if (!guild.CurrentMember.HasPermission(Permissions.BanMembers))
                return InfractionResult.FailedSelfPermissions;
            try
            {
                await guild.UnbanMemberAsync(userId, reason);
            }
            catch (NotFoundException) { }

            _infractions.RemoveAll(inf => inf.InfractionType is InfractionType.Ban or InfractionType.SoftBan && inf.UserId == userId);

            IEnumerable<InfractionEntity>? userInfractions = await _mediator.Send(new GetUserInfractionsRequest(guildId, userId));
            InfractionEntity? banInfraction = userInfractions.OrderByDescending(inf => inf.InfractionTime).FirstOrDefault(inf => inf.InfractionType is InfractionType.Ban or InfractionType.SoftBan);

            if (!userInfractions.Any() || banInfraction is null)
                return InfractionResult.FailedResourceDeleted;

            await _mediator.Send(new UpdateInfractionRequest(banInfraction.Id, banInfraction.Expiration, banInfraction.Reason, true));

            //TODO: Make expiration parameter optional because it bugs me ~Velvet
            InfractionEntity? infraction = await GenerateInfractionAsync(userId, guildId, enforcerId, InfractionType.Unban, reason, null);
            await LogInfractionAsync(infraction);

            return InfractionResult.SucceededDoesNotNotify;
        }

        public async Task<InfractionResult> StrikeAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, bool autoEscalate = false)
        {
            DiscordGuild? guild = _client.Guilds[guildId];
            DiscordMember? enforcer = guild.Members[enforcerId];
            bool exists = guild.Members.TryGetValue(userId, out var victim);


            if (!autoEscalate)
            {
                InfractionEntity? infraction = await GenerateInfractionAsync(userId, guildId, enforcerId, InfractionType.Strike, reason, null);
                DiscordEmbedBuilder? embed = CreateUserInfractionEmbed(enforcer, guild.Name, InfractionType.Strike, reason);

                InfractionResult logResult = await LogInfractionAsync(infraction);

                if (logResult is InfractionResult.FailedLogPermissions)
                    return logResult;

                if (!exists)
                    return InfractionResult.FailedGuildMemberCache;

                try
                {
                    await victim!.SendMessageAsync(embed);
                    return InfractionResult.SucceededWithNotification;
                }
                catch (UnauthorizedException)
                {
                    return InfractionResult.SucceededWithoutNotification;
                }
            }

            IEnumerable<InfractionEntity>? infractions = await _mediator.Send(new GetUserInfractionsRequest(guildId, userId));
            InfractionStepEntity? step = await GetCurrentInfractionStepAsync(guildId, infractions);

            Task<InfractionResult>? task = step.Type switch
            {
                InfractionType.Ignore when enforcer == _client.CurrentUser => AddNoteAsync(userId, guildId, enforcerId, reason),
                InfractionType.Ignore when enforcer != _client.CurrentUser => StrikeAsync(userId, guildId, enforcerId, reason),
                InfractionType.Kick                                        => KickAsync(userId, guildId, enforcerId, reason),
                InfractionType.Ban                                         => BanAsync(userId, guildId, enforcerId, reason),
                InfractionType.SoftBan                                     => BanAsync(userId, guildId, enforcerId, reason, DateTime.UtcNow + step.Duration.Time),
                InfractionType.Mute                                        => MuteAsync(userId, guildId, enforcerId, reason, DateTime.Now   + step.Duration.Time),
                _                                                          => Task.FromResult(InfractionResult.SucceededDoesNotNotify)
            };

            InfractionResult res = await task;
            InfractionEntity? lastInfraction = _infractions.Last(inf => inf.UserId == userId && inf.Reason == reason);
            await _mediator.Send(new UpdateInfractionRequest(lastInfraction.Id, lastInfraction.Expiration, reason, false, true));

            return res;
        }

        public async ValueTask<bool> IsMutedAsync(ulong userId, ulong guildId)
        {
            EnsureSemaphoreExists(guildId);
            await _semaphoreDict[guildId].WaitAsync();

            try
            {
                bool isInMemory = _mutes.Contains((userId, guildId));

                if (isInMemory)
                    return true;

                IEnumerable<InfractionEntity>? dbInf = await _mediator.Send(new GetUserInfractionsRequest(guildId, userId));
                InfractionEntity? inf = dbInf.SingleOrDefault(inf => inf.HeldAgainstUser && inf.InfractionType is InfractionType.Mute or InfractionType.AutoModMute);

                // ReSharper disable once InvertIf
                if (inf is not null)
                {
                    _infractions.Add(inf);
                    _mutes.Add((userId, guildId));
                }

                return inf is not null;
            }
            finally
            {
                _semaphoreDict[guildId].Release();
            }
        }

        public async Task<InfractionResult> MuteAsync(ulong userId, ulong guildId, ulong enforcerId, string reason, DateTime? expiration, bool updateExpiration = true)
        {
            DiscordGuild guild = _client.Guilds[guildId];
            GuildModConfigEntity conf = await _config.GetModConfigAsync(guildId);
            DiscordRole? muteRole = guild.GetRole(conf!.MuteRoleId);

            if (await IsMutedAsync(userId, guildId))
            {
                InfractionEntity? memInfraction = _infractions.Find(inf => inf.UserId == userId && inf.InfractionType is InfractionType.Mute or InfractionType.AutoModMute);
                /* Replying mute */

                if (updateExpiration && expiration != memInfraction!.Expiration)
                {
                    InfractionEntity? newInf = await _mediator.Send(new UpdateInfractionRequest(memInfraction!.Id, expiration, reason));
                    await LogUpdatedInfractionAsync(memInfraction, newInf);

                    _ = memInfraction = newInf; // Update the in memory infraction //
                }

                muteRole ??= await GenerateMuteRoleAsync(guild, guild.Members[userId]);
                /* It *should* be almost impossible for someone to leave a server this fast without self-botting */
                try
                {
                    DiscordMember? mem = guild.Members[userId];

                    if (!mem.Roles.Contains(muteRole))
                        await mem.GrantRoleAsync(muteRole, "Reapplying active mute.");
                }
                catch (NotFoundException) { }
                catch (ServerErrorException) { }
                catch (UnauthorizedException) { }


                return InfractionResult.SucceededDoesNotNotify;
            }


            bool exists = guild.Members.TryGetValue(userId, out DiscordMember? member);

            if (conf.MuteRoleId is 0 || muteRole is null)
                muteRole = await GenerateMuteRoleAsync(guild, member!);

            InfractionType infractionType = enforcerId == _client.CurrentUser.Id ? InfractionType.AutoModMute : InfractionType.Mute;
            InfractionEntity infraction = await GenerateInfractionAsync(userId, guildId, enforcerId, infractionType, reason, expiration);

            await LogInfractionAsync(infraction);
            _mutes.Add((userId, guildId));
            _infractions.Add(infraction);

            try
            {
                await member!.GrantRoleAsync(muteRole, reason);
            }
            catch (NotFoundException)
            {
                return InfractionResult.FailedGuildMemberCache;
            }

            var notified = false;

            // ReSharper disable once InvertIf
            if (exists)
            {
                try
                {
                    DiscordEmbed muteEmbed = CreateUserInfractionEmbed(guild.Members[enforcerId], guild.Name, infractionType, reason, expiration);
                    await member.SendMessageAsync(muteEmbed);
                    notified = true;
                }
                catch
                {
                    /* This could only be un-authed exception. */
                }
            }

            return notified ?
                InfractionResult.SucceededWithNotification :
                InfractionResult.SucceededWithoutNotification;
        }
        public async Task<InfractionResult> UnMuteAsync(ulong userId, ulong guildId, ulong enforcerId, string reason = "Not Given.")
        {
            if (!await IsMutedAsync(userId, guildId))
                return InfractionResult.FailedGenericRequirementsNotFulfilled;

            await _semaphoreDict[guildId].WaitAsync();

            int index = _infractions.FindIndex(inf => inf.UserId == userId && inf.GuildId == guildId && inf.InfractionType is InfractionType.Mute or InfractionType.AutoModMute);
            InfractionEntity? infraction = _infractions[index];

            _infractions.RemoveAt(index);
            _mutes.Remove((userId, guildId));

            await _mediator.Send(new UpdateInfractionRequest(infraction.Id, infraction.Expiration, infraction.Reason, true)); // Only set it to say it's handled. //
            InfractionEntity? unmute = await GenerateInfractionAsync(userId, guildId, enforcerId, InfractionType.Unmute, reason, null);

            DiscordGuild? guild = _client.Guilds[guildId];
            if (guild.Members.TryGetValue(userId, out var member))
            {
                GuildModConfigEntity? conf = await _config.GetModConfigAsync(guildId);
                DiscordRole? role = guild.Roles[conf.MuteRoleId];
                await member.RevokeRoleAsync(role);

                DiscordEmbedBuilder? embed = CreateUserInfractionEmbed(guild.Members[enforcerId], guild.Name, InfractionType.Unmute, reason);

                try
                {
                    await member.SendMessageAsync(embed);
                    return InfractionResult.SucceededWithNotification;
                }
                catch (UnauthorizedException) { return InfractionResult.SucceededWithoutNotification; }
                finally
                {
                    await _mediator.Send(new UpdateInfractionRequest(infraction.Id, DateTime.UtcNow, infraction.Reason, true));
                    await LogInfractionAsync(unmute);
                    _semaphoreDict[guildId].Release();
                }
            }

            await _mediator.Send(new UpdateInfractionRequest(infraction.Id, DateTime.UtcNow, infraction.Reason, true));
            await LogInfractionAsync(unmute);

            _semaphoreDict[guildId].Release();
            return InfractionResult.SucceededDoesNotNotify;
        }

        public async Task<InfractionStepEntity> GetCurrentInfractionStepAsync(ulong guildId, IEnumerable<InfractionEntity> infractions)
        {
            GuildModConfigEntity conf = await _config.GetModConfigAsync(guildId);
            if (!conf.InfractionSteps.Any())
                return _ignoreStep;

            int infractionCount = GetElegibleInfractions(infractions);
            List<InfractionStepEntity>? infLevels = conf.InfractionSteps;

            int index = Math.Max(infLevels.Count - 1, infractionCount - 1);

            return infLevels[index];

            int GetElegibleInfractions(IEnumerable<InfractionEntity> inf)
            {
                return inf.Count(i => !i.HeldAgainstUser && i.Enforcer == _client.CurrentUser.Id && i.InfractionType is InfractionType.Strike);
            }
        }

        public Task<InfractionEntity> GenerateInfractionAsync(ulong userId, ulong guildId, ulong enforcerId, InfractionType type, string reason, DateTime? expiration)
        {
            return _mediator.Send(new CreateInfractionRequest(userId, enforcerId, guildId, reason, type, expiration));
        }

        public async Task<InfractionResult> AddNoteAsync(ulong userId, ulong guildId, ulong noterId, string note)
        {
            InfractionEntity? infractionNote = await GenerateInfractionAsync(userId, guildId, noterId, InfractionType.Note, note, null);

            var mainNoteEmbed = new DiscordEmbedBuilder();
            DiscordGuild? guild = _client.Guilds[guildId];

            DiscordUser? user;
            _ = guild.Members.TryGetValue(noterId, out var enforcer);
            _ = guild.Members.TryGetValue(userId, out var tmem);
            user = tmem;
            user ??= await _client.GetUserAsync(userId);

            mainNoteEmbed
               .WithAuthor($"{user.Username}#{user.Discriminator}", user.GetUrl(), user.AvatarUrl)
               .WithThumbnail(enforcer!.AvatarUrl, 4096, 4096)
               .WithDescription("A new case has been added to this guild's list of infractions.")
               .WithColor(DiscordColor.Gold)
               .AddField("Type:", infractionNote.InfractionType.Humanize(LetterCasing.Title), true)
               .AddField("Created:", Formatter.Timestamp(infractionNote.InfractionTime, TimestampFormat.LongDateTime), true)
               .AddField("Case Id:", $"#{infractionNote.CaseNumber}", true)
               .AddField("User:", $"**{user.ToDiscordName()}**\n(`{user.Id}`)", true)
               .AddField("Noted by:", user == _client.CurrentUser ? "[AUTO-MOD]" : $"**{enforcer.ToDiscordName()}**\n(`{enforcer.Id}`)", true);

            DiscordEmbedBuilder? noteReasonEmbed = new DiscordEmbedBuilder()
                                                  .WithDescription(note)
                                                  .WithColor(DiscordColor.Gold)
                                                  .WithTitle($"Note (case {infractionNote.CaseNumber})");

            await EnsureModLogChannelExistsAsync(guildId);
            GuildModConfigEntity? conf = await _config.GetModConfigAsync(guildId);
            if (conf.LoggingChannel is 0)
                return InfractionResult.FailedNotConfigured;
            DiscordChannel? channel = guild.Channels[conf.LoggingChannel];

            if (!channel.PermissionsFor(guild.CurrentMember).HasPermission(Permissions.SendMessages | Permissions.EmbedLinks))
                return InfractionResult.FailedLogPermissions;

            try
            {
                if (conf.UseWebhookLogging && _webhookClient.GetRegisteredWebhook(conf.WebhookLoggingId) is DiscordWebhook wh)
                    await wh.ExecuteAsync(new DiscordWebhookBuilder().WithAvatarUrl(guild.CurrentMember.AvatarUrl).AddEmbeds(new DiscordEmbed[] { mainNoteEmbed, noteReasonEmbed }));
                else
                    await channel.SendMessageAsync(buil => buil.AddEmbeds(new DiscordEmbed[] { mainNoteEmbed, noteReasonEmbed }));
            }
            catch
            {

                /* ??? */
            }

            return InfractionResult.SucceededDoesNotNotify;
        }

        public async Task<InfractionResult> PardonAsync(ulong userId, ulong guildId, ulong enforcerId, string reason = "Not Given.")
        {
            IEnumerable<InfractionEntity>? infractions = await _mediator.Send(new GetUserInfractionsRequest(guildId, userId));
            InfractionEntity? rescindedInfraction = infractions.OrderBy(inf => inf.InfractionTime).LastOrDefault(inf => inf.HeldAgainstUser && inf.InfractionType is InfractionType.Strike || inf.EscalatedFromStrike);

            if (rescindedInfraction is null)
                return InfractionResult.FailedGenericRequirementsNotFulfilled;

            await _mediator.Send(new UpdateInfractionRequest(rescindedInfraction.Id, rescindedInfraction.Expiration, rescindedInfraction.Reason, true));

            InfractionEntity? infraction = await GenerateInfractionAsync(userId, guildId, enforcerId, InfractionType.Pardon, reason, null);

            await EnsureModLogChannelExistsAsync(guildId);
            await LogInfractionAsync(infraction);

            DiscordGuild? guild = _client.Guilds[guildId];

            if (!guild.Members.TryGetValue(userId, out var member))
                return InfractionResult.SucceededDoesNotNotify;

            DiscordUser? user = await _client.GetUserAsync(enforcerId);
            try
            {
                await member.SendMessageAsync(CreateUserInfractionEmbed(user, guild.Name, InfractionType.Pardon, reason));
                return InfractionResult.SucceededWithNotification;
            }
            catch
            {
                return InfractionResult.SucceededWithoutNotification;
            }
        }


        /// <summary>
        ///     Creates a formatted embed to be sent to a user.
        /// </summary>
        /// <param name="enforcer">The user that created this infraction.</param>
        /// <param name="guildName">The name of the guild the infraction occured on.</param>
        /// <param name="type">The type of infraction.</param>
        /// <param name="reason">Why the infraction was created.</param>
        /// <param name="expiration">When the infraction expires.</param>
        /// <returns>A <see cref="DiscordEmbed" /> populated with relevant information.</returns>
        /// <exception cref="ArgumentException">An unknown infraction type was passed.</exception>
        private static DiscordEmbedBuilder CreateUserInfractionEmbed(DiscordUser enforcer, string guildName, InfractionType type, string reason, DateTime? expiration = default)
        {
            string? action = type switch
            {
                InfractionType.Kick                          => $"You've been kicked from {guildName}!",
                InfractionType.Ban                           => $"You've been permanently banned from {guildName}!",
                InfractionType.SoftBan                       => $"You've been temporarily banned from {guildName}!",
                InfractionType.Mute                          => $"You've been muted on {guildName}!",
                InfractionType.AutoModMute                   => $"You've been automatically muted on {guildName}!",
                InfractionType.Strike                        => $"You've been warned on {guildName}!",
                InfractionType.Unmute                        => $"You've been un-muted on {guildName}!",
                InfractionType.Pardon                        => $"You've been pardoned from one (1) infraction on {guildName}!",
                InfractionType.Ignore or InfractionType.Note => null,
                _                                            => throw new ArgumentException($"Unexpected enum value: {type}")
            };

            DateTimeOffset now = DateTimeOffset.UtcNow;
            DiscordEmbedBuilder? embed = new DiscordEmbedBuilder()
                                        .WithTitle(action)
                                        .WithAuthor($"{enforcer.Username}#{enforcer.Discriminator}", enforcer.GetUrl(), enforcer.AvatarUrl)
                                        .AddField("Reason:", reason)
                                        .AddField("Infraction occured:",
                                                  $"{Formatter.Timestamp(now, TimestampFormat.LongDateTime)}\n\n({Formatter.Timestamp(now)})")
                                        .AddField("Enforcer:", enforcer.Id.ToString());

            if (expiration.HasValue)
                embed.AddField("Expires:", Formatter.Timestamp(expiration.Value));

            return embed;
        }


        /// <summary>
        ///     Sends a message to the appropriate log channel that an infraction (note, reason, or duration) was updated.
        /// </summary>
        private async Task<InfractionResult> LogUpdatedInfractionAsync(InfractionEntity infOld, InfractionEntity infNew)
        {
            await EnsureModLogChannelExistsAsync(infNew.GuildId);
            GuildModConfigEntity? conf = await _config.GetModConfigAsync(infNew.GuildId);

            ulong modLog = conf.LoggingChannel;
            DiscordGuild? guild = _client.Guilds[infNew.GuildId];

            if (modLog is 0)
                return InfractionResult.FailedNotConfigured;

            if (!guild.Channels.TryGetValue(modLog, out var chn))
                return InfractionResult.FailedResourceDeleted;

            DiscordUser? user = await _client.GetUserAsync(infOld.UserId); /* User may not exist on the server anymore. */
            DiscordMember? enforcer = _client.Guilds[infNew.GuildId].Members[infNew.Enforcer];

            IEnumerable<DiscordEmbed>? infractionEmbeds = GenerateUpdateEmbed(user, enforcer, infOld, infNew);

            try
            {
                var builder = new DiscordMessageBuilder();

                builder.AddEmbeds(infractionEmbeds);

                await chn.SendMessageAsync(builder);
            }
            catch (UnauthorizedException)
            {
                return InfractionResult.FailedLogPermissions;
            }
            return InfractionResult.SucceededDoesNotNotify;


            IEnumerable<DiscordEmbed> GenerateUpdateEmbed(DiscordUser victim, DiscordUser enforcer, InfractionEntity infractionOLD, InfractionEntity infractionNEW)
            {
                var builder = new DiscordEmbedBuilder();
                builder
                   .WithColor(DiscordColor.Gold)
                   .WithThumbnail(enforcer.AvatarUrl, 4096, 4096)
                   .WithTitle("An infraction in this guild has been updated.")
                   .WithAuthor($"{victim.Username}#{victim.Discriminator}", victim.GetUrl(), victim.AvatarUrl)
                   .AddField("Type:", infractionOLD.InfractionType.Humanize(LetterCasing.Title), true)
                   .AddField("Created:", Formatter.Timestamp(infractionOLD.InfractionTime, TimestampFormat.LongDateTime), true)
                   .AddField("Case Id:", $"#{infractionOLD.CaseNumber}", true)
                   .AddField("Offender:", $"**{victim.ToDiscordName()}**\n(`{victim.Id}`)", true)
                   .AddField("Enforcer:", enforcer == _client.CurrentUser ? "[AUTO-MOD]" : $"**{enforcer.ToDiscordName()}**\n(`{enforcer.Id}`)", true);


                if (infractionOLD.Duration.HasValue || infractionNEW.Duration.HasValue)
                {
                    string expiry = (infractionOLD.Duration, infractionNEW.Duration) switch
                    {
                        (TimeSpan t1, TimeSpan t2) => $"{Formatter.Timestamp(t1, TimestampFormat.LongDateTime)} → {Formatter.Timestamp(t2, TimestampFormat.LongDateTime)}",
                        (TimeSpan t, null)         => $"{Formatter.Timestamp(t, TimestampFormat.LongDateTime)} → Never",
                        (null, TimeSpan t)         => $"Never → {Formatter.Timestamp(t, TimestampFormat.LongDateTime)}",
                        (null, null)               => "Never"
                    };
                    builder.AddField("Expiration", expiry);
                }

                if (infractionOLD.Reason.Length < 2000 && infractionNEW.Reason.Length < 2000)
                {
                    if (!string.Equals(infractionOLD.Reason, infractionNEW.Reason))
                        builder.WithDescription($"Old reason: ```\n\n{infractionOLD.Reason}``` \n\nNew reason: ```\n{infractionNEW.Reason}```");
                    return new DiscordEmbed[] { builder };
                }

                builder.WithFooter("The reason was over 2000 characters, and have been added to a second embed!");
                DiscordEmbedBuilder? b2 = new DiscordEmbedBuilder().WithColor(DiscordColor.Gold).WithDescription(infractionNEW.Reason);
                return new DiscordEmbed[] { builder, b2 };
            }
        }

        /// <summary>
        ///     Logs to the designated mod-log channel, if any.
        /// </summary>
        /// <param name="inf">The infraction to log.</param>
        private async Task<InfractionResult> LogInfractionAsync(InfractionEntity inf)
        {
            await EnsureModLogChannelExistsAsync(inf.GuildId);

            GuildModConfigEntity? config = await _config.GetModConfigAsync(inf.GuildId);
            DiscordGuild? guild = _client.Guilds[inf.GuildId];

            if (config.LoggingChannel is 0)
                return InfractionResult.FailedNotConfigured; /* It couldn't create a mute channel :(*/

            DiscordUser? user = await _client.GetUserAsync(inf.UserId); /* User may not exist on the server anymore. */
            DiscordMember? enforcer = guild.Members[inf.Enforcer];

            var builder = new DiscordEmbedBuilder();

            builder
               .WithAuthor(user.ToDiscordName(), user.GetUrl(), user.AvatarUrl)
               .WithThumbnail(enforcer.AvatarUrl, 4096, 4096)
               .WithDescription("A new case has been added to this guild's list of infractions.")
               .WithColor(DiscordColor.Gold)
               .AddField("Type:", inf.InfractionType.Humanize(LetterCasing.Title), true)
               .AddField("Created:", Formatter.Timestamp(inf.InfractionTime, TimestampFormat.LongDateTime), true)
               .AddField("Case Id:", $"#{inf.CaseNumber}", true)
               .AddField("Offender:", $"**{user.ToDiscordName()}**\n(`{user.Id}`)", true)
               .AddField("Enforcer:", user == _client.CurrentUser ? "[AUTO-MOD]" : $"**{enforcer.ToDiscordName()}**\n(`{enforcer.Id}`)", true)
               .AddField("Reason:", inf.Reason);

            if (inf.Duration is TimeSpan ts)
                builder.AddField("Expires:", Formatter.Timestamp(ts));

            try
            {
                if (config.UseWebhookLogging && config.LoggingWebhookUrl is not null)
                {
                    if (_webhookClient.GetRegisteredWebhook(config.WebhookLoggingId) is DiscordWebhook wh)
                    {
                        await wh.ExecuteAsync(new DiscordWebhookBuilder().WithAvatarUrl(guild.CurrentMember.AvatarUrl).AddEmbed(builder));
                    }
                    // TODO: Log error via webhook & DB
                }
                else
                {
                    await guild.Channels[config.LoggingChannel].SendMessageAsync(builder);
                }
            }
            catch (UnauthorizedException)
            {
                return InfractionResult.FailedLogPermissions;
            }
            return InfractionResult.SucceededDoesNotNotify;
        }

        private async Task<DiscordRole> GenerateMuteRoleAsync(DiscordGuild guild, DiscordMember member)
        {
            if (!guild.CurrentMember.Permissions.HasPermission(Permissions.ManageRoles))
                throw new InvalidOperationException("Current member does not have permission to create roles.");

            if (!guild.CurrentMember.Permissions.HasPermission(Permissions.ManageChannels))
                throw new InvalidOperationException("Current member does not have permission to manage channels.");


            DiscordChannel[]? channels = guild.Channels.Values
                                              .OfType(ChannelType.Text)
                                              .Where(c => guild.CurrentMember.PermissionsIn(c).HasPermission(Permissions.ManageChannels | Permissions.AccessChannels | Permissions.SendMessages /*| Permissions.ManageRoles */))
                                              .ToArray();

            foreach (var role in guild.Roles.Values)
            {
                if (role.Position <= member.Hierarchy)
                    continue;

                if (member.Roles.Contains(role))
                    continue;

                if (!channels.All(r => r.PermissionOverwrites.Any(p => p.Id == role.Id)))
                    continue;

                await _mediator.Send(new UpdateGuildModConfigRequest(guild.Id) { MuteRoleId = role.Id });
                return role;
            }

            DiscordRole mute = await guild.CreateRoleAsync("Muted", null, new("#363636"), false, false, "Mute role was not present on guild");
            await mute.ModifyPositionAsync(guild.CurrentMember.Hierarchy - 1);

            foreach (var c in channels)
            {
                if (!c.PermissionsFor(member).HasPermission(Permissions.SendMessages | Permissions.AccessChannels))
                    continue;
                await c.AddOverwriteAsync(mute, Permissions.None, Permissions.SendMessages);
            }

            await _mediator.Send(new UpdateGuildModConfigRequest(guild.Id) { MuteRoleId = mute.Id });
            _updater.UpdateGuild(guild.Id);
            return mute;
        }

        /// <summary>
        ///     Ensures a moderation channel exists. If it doesn't one will be created, and hidden.
        /// </summary>
        private async Task EnsureModLogChannelExistsAsync(ulong guildId)
        {
            GuildModConfigEntity config = await _config.GetModConfigAsync(guildId);
            DiscordGuild guild = _client.Guilds[guildId];

            if (config.LoggingChannel is not 0)
            {
                if (_webhookClient.GetRegisteredWebhook(config.WebhookLoggingId) is null)
                {
                    try
                    {
                        DiscordWebhook? wh = await guild.Channels[config.LoggingChannel].CreateWebhookAsync("Silk! Logging");

                        _webhookClient.AddWebhook(wh);

                        await _mediator.Send(new UpdateGuildModConfigRequest(guildId)
                        {
                            UseWebhookLogging = true,
                            WebhookLoggingId = wh.Id,
                            WebhookLoggingUrl = $"https://discord.com/api/v9/webhooks/{wh.Id}/{wh.Token}"
                        });

                        _updater.UpdateGuild(guildId);
                    }
                    catch
                    {
                        //TODO: Log
                    }
                }

                return;
            }

            if (!guild.CurrentMember.HasPermission(Permissions.ManageChannels))
                return; /* We can't create channels. Sad. */

            try
            {
                var overwrites = new DiscordOverwriteBuilder[]
                {
                    new(guild.EveryoneRole) { Denied = Permissions.AccessChannels },
                    new(guild.CurrentMember)
                    {
                        Allowed =
                            Permissions.AccessChannels |
                            Permissions.SendMessages   |
                            Permissions.EmbedLinks     |
                            (guild.CurrentMember.HasPermission(Permissions.ManageWebhooks) ? Permissions.ManageWebhooks : Permissions.None)
                    }
                };

                DiscordChannel? chn = await guild.CreateChannelAsync("mod-log", ChannelType.Text, guild.Channels.Values.OfType(ChannelType.Category).Last(), overwrites: overwrites);
                await chn.SendMessageAsync("A logging channel was not available when this infraction was created, so one has been generated.");

                DiscordWebhook? wh = null;

                if (guild.CurrentMember.Permissions.HasPermission(Permissions.ManageWebhooks))
                {
                    wh = await chn.CreateWebhookAsync("Silk! Logging");
                    _webhookClient.AddWebhook(wh);
                }

                await _mediator.Send(new UpdateGuildModConfigRequest(guildId)
                {
                    LoggingChannel = chn.Id,
                    UseWebhookLogging = wh is not null,
                    WebhookLoggingId = wh?.Id,
                    WebhookLoggingUrl = wh is null ? null : $"https://discord.com/api/v9/webhooks/{wh.Id}/{wh.Token}"
                });

                _updater.UpdateGuild(guildId);
            }
            catch
            {
                /* Ignore. We can't do anything about it :( */
            }
        }

        private void EnsureSemaphoreExists(ulong guildId)
        {
            if (!_semaphoreDict.TryGetValue(guildId, out _))
                _semaphoreDict[guildId] = new(1);
        }

        private async Task DequeueInfractionsAsync()
        {
            foreach (var infraction in _infractions)
                if (infraction.Expiration < DateTime.UtcNow)
                    await DequeAsync(this, infraction);

            static async Task DequeAsync(InfractionService service, InfractionEntity infraction)
            {
                ulong id = service._client.CurrentUser.Id;
                ulong guildId = infraction.GuildId;
                Task? task = infraction.InfractionType switch
                {
                    InfractionType.Mute or InfractionType.AutoModMute => service.UnMuteAsync(infraction.UserId, guildId, id, "Automatic unmute | This infraction has expired."),
                    InfractionType.SoftBan                            => service.UnBanAsync(infraction.UserId, guildId, id, "Automatic unban | This infraction has expired."),
                    _                                                 => Task.CompletedTask
                };
                await task;
            }
        }
    }
}