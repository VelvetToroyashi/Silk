using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using MediatR;
using Silk.Core.Constants;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Data.MediatR;
using Silk.Data.Models;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Server
{
    
    public class WelcomeCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;
        private readonly IServiceCacheUpdaterService _updater;

        public WelcomeCommand(IMediator mediator, IServiceCacheUpdaterService updater)
        {
            _mediator = mediator;
            _updater = updater;
        }
        
        [Command]
        [RequireFlag(UserFlag.Staff)]
        [Description
            ("Welcome message settings! Currently supported substitutions:" +
             "\n`{u}` -> Username, `{@u}` -> Mention, `{s}` -> Server Name")]
        public async Task SetWelcome(CommandContext ctx, [RemainingText] string message)
        {
            DiscordEmoji confirm = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.Confirm.ToEmojiId());
            DiscordEmoji deny = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.Decline.ToEmojiId());
            DiscordMessage msg = null!;
            
            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithoutMentions().WithReply(ctx.Message.Id);
            GuildConfig config = await _mediator.Send(new GuildConfigRequest.Get {GuildId = ctx.Guild.Id});
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            
            if (config.GreetingChannel is 0) 
                await SetupGreetingChannelAsync(ctx, interactivity, builder, config);

            if (ctx.Guild.Features.Contains("MEMBER_VERIFICATION_GATE_ENABLED"))
                await PromptScreeningConfirmationAsync(ctx, builder, interactivity, msg, message, confirm, deny);
            else
                await PromptForRoleVerification(ctx, builder, interactivity, msg, message, confirm, deny);
            
        }

        private async Task PromptScreeningConfirmationAsync(
            CommandContext ctx,
            DiscordMessageBuilder builder,
            InteractivityExtension interactivity,
            DiscordMessage msg,
            string message,
            DiscordEmoji confirm,
            DiscordEmoji deny)
        {
            builder.WithContent("It seems you have membership gating enabled on this server!\n" +
                                "Would you like me to greet people after they complete screening?");
                
            msg = await ctx.RespondAsync(builder);
                
            await msg.CreateReactionAsync(confirm);
            await msg.CreateReactionAsync(deny);
                
            var result = await interactivity.WaitForReactionAsync(m =>
                m.Message == msg &&
                m.Emoji == confirm || m.Emoji == deny, msg, ctx.User);

            if (result.TimedOut)
            {
                builder.WithContent("Timed out!");
                await ctx.RespondAsync(builder);
                return;
            }

            if (result.Result.Emoji == deny)
            {
                await PromptForRoleVerification(ctx, builder, interactivity, msg, message, confirm, deny);
            }
            else
            {
                                        
                builder.WithReply(msg.Id)
                    .WithContent("Great, I'll greet people when they complete membership screening!");
                var request = new GuildConfigRequest.Update
                {
                    GuildId = ctx.Guild.Id,
                    GreetMembers = true,
                    GreetingText = message,
                    GreetOnScreeningComplete = true
                };
                    
                await _mediator.Send(request);
            }
        }
        
        private async Task PromptForRoleVerification(
            CommandContext ctx,
            DiscordMessageBuilder builder,
            InteractivityExtension interactivity,
            DiscordMessage msg,
            string message,
            DiscordEmoji confirm,
            DiscordEmoji deny)
        {
            InteractivityResult<MessageReactionAddEventArgs> result = new();
            builder.WithReply(msg.Id)
                .WithContent("Alrighty, would you like me to greet people when you give them a role? Declining will greet on join!");
            msg = await ctx.RespondAsync(builder);
                    
            await msg.CreateReactionAsync(confirm);
            await msg.CreateReactionAsync(deny);
            result = await interactivity.WaitForReactionAsync(m => m.Emoji == confirm || m.Emoji == deny, msg, ctx.User);

            if (result.TimedOut)
            {
                builder.WithContent("Timed out!");
                await ctx.RespondAsync(builder);
                return;
            }
            if (result.Result.Emoji == deny)
            {
                builder.WithReply(msg.Id).WithContent("Alright, I'll greet people as they join!");
                await ctx.RespondAsync(builder);
                var request = new GuildConfigRequest.Update
                {
                    GuildId = ctx.Guild.Id,
                    GreetMembers = true,
                    GreetOnScreeningComplete = false,
                    GreetOnVerificationRole = false,
                    GreetingText = message,
                };
                await _mediator.Send(request);
            }
            else
            {
                await OnRoleGivenAsync(ctx, interactivity, message);
            }
        }
        
        private async Task OnRoleGivenAsync(CommandContext ctx, InteractivityExtension interactivity, string message)
        {
            var builder = new DiscordMessageBuilder().WithContent("Alright, you'll need to specify a role. Simply Mention (@) the role, and I'll record your response!");
            await ctx.RespondAsync(builder);
                        
            var res = await interactivity.WaitForMessageAsync(m => m.Content is "cancel" || m.MentionedRoles.Count is 1);
                        
            if (res.TimedOut)
            {
                builder.WithContent("Timed out!");
                await ctx.RespondAsync(builder);
                return;
            }

            ulong role = res.Result.MentionedRoles[0].Id;
            var request = new GuildConfigRequest.Update
            {
                GuildId = ctx.Guild.Id,
                GreetingText = message,
                GreetOnScreeningComplete = false,
                GreetOnVerificationRole = true,
                VerificationRoleId = role
            };
            await _mediator.Send(request);
            builder.WithContent("And you're set! Current welcome message:\n> " +
                                message
                                    .Replace("{u}", ctx.Member.Username)
                                    .Replace("{s}", ctx.Member.Guild.Name)
                                    .Replace("{@u}", ctx.Member.Mention));
            await ctx.RespondAsync(builder);
        }
        
        private async Task SetupGreetingChannelAsync(CommandContext ctx, InteractivityExtension interactivity, DiscordMessageBuilder builder, GuildConfig config)
        {
            builder.WithContent("You need to set up a greeting channel! Simply mention the channel you want to set as the greeting channel, and I'll handle the rest! :)");
            _ = await ctx.RespondAsync(builder);
            var result = await interactivity.WaitForMessageAsync(m => m.MentionedChannels.Count is 1);

            if (result.TimedOut)
            {
                throw new TimeoutException();
            }
            else
            {
                config.GreetingChannel = result.Result.MentionedChannels[0].Id;
                builder.WithContent($"Alright, {result.Result.MentionedChannels[0].Mention} it is :)").WithReply(result.Result.Id);
                await ctx.RespondAsync(builder);
                await _mediator.Send(new GuildConfigRequest.Update {GuildId = ctx.Guild.Id, GreetingChannelId = result.Result.MentionedChannels[0].Id});
            }
        }
    }
}