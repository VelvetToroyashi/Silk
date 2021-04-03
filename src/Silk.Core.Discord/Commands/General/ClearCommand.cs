using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using MediatR;
using Silk.Core.Data.MediatR.Unified.Guilds;
using Silk.Core.Data.Models;
using Silk.Core.Discord.Utilities.HelpFormatter;

namespace Silk.Core.Discord.Commands.General
{
    [Category(Categories.Mod)]
    public class ClearCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;

        public ClearCommand(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Command]
        [Description("Cleans all messages from all users.")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task Clear(CommandContext ctx, int messages = 5)
        {
            GuildConfig guildConfig = await GetOrCreateGuildConfig(ctx);
            
            ulong loggingChannelId = guildConfig.LoggingChannel;
            DiscordChannel? loggingChannel = ctx.Guild.GetChannel(loggingChannelId) ?? ctx.Channel;

            IReadOnlyList<DiscordMessage> queriedMessages = await ctx.Channel.GetMessagesAsync(messages + 1);

            var commandIssuingUser = $"{ctx.User.Username}{ctx.User.Discriminator}";

            var clearedMessagesEmbed = new DiscordEmbedBuilder()
                .WithTitle("Cleared Messages:")
                .WithDescription(
                    $"User: {ctx.User.Mention}\n" +
                    $"Channel: {ctx.Channel.Mention}\n" +
                    $"Amount: **{messages}**")
                .AddField("User ID:", ctx.User.Id.ToString(), true)
                .WithThumbnail(ctx.Message.Author.AvatarUrl)
                .WithFooter("Cleared Messages at (UTC)")
                .WithTimestamp(DateTime.Now.ToUniversalTime())
                .WithColor(DiscordColor.Red);

            await ctx.Channel.DeleteMessagesAsync(queriedMessages, $"{commandIssuingUser} called clear command.");
            await loggingChannel.SendMessageAsync(clearedMessagesEmbed);
            
            DiscordMessage responseMsg = await ctx.RespondAsync(new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.DisplayName, null, ctx.Member.AvatarUrl)
                .WithColor(DiscordColor.SpringGreen)
                .WithDescription($"Cleared {messages} messages!"));
            
            await Task.Delay(5000);
            try { await ctx.Channel.DeleteMessageAsync(responseMsg); }
            catch (NotFoundException) {}
        }

        private async Task<GuildConfig> GetOrCreateGuildConfig(CommandContext ctx)
        {
            GuildConfig? guildConfig = await _mediator.Send(new GetGuildConfigRequest(ctx.Guild.Id));
            if ((GuildConfig?) guildConfig is not null) return guildConfig;
            
            var guild = await _mediator.Send(new GetOrCreateGuildRequest(ctx.Guild.Id, Discord.Bot.DefaultCommandPrefix));
            guildConfig = guild.Configuration;

            return guildConfig;
        }
    }
}