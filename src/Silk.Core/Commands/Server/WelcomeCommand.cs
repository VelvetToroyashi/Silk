using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
            DiscordEmoji confirm = DiscordEmoji.FromName(ctx.Client, Emojis.Confirm.ToEmojiString());
            DiscordEmoji deny = DiscordEmoji.FromName(ctx.Client, Emojis.Decline.ToEmojiString());
            DiscordMessage msg;
            
            DiscordMessageBuilder builder = new DiscordMessageBuilder().WithoutMentions().WithReply(ctx.Message.Id);
            GuildConfig config = await _mediator.Send(new GuildConfigRequest.GetGuildConfigRequest {GuildId = ctx.Guild.Id});
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            
            
            if (config.GreetingChannel is 0) 
                await SetupGreetingChannelAsync(ctx, interactivity, builder, config);
            
            if (ctx.Guild.Features.Contains("MEMBER_VERIFICATION_GATE_ENABLED"))
            {
                builder.WithContent("It seems you have membership gating enabled on this server!\n" +
                                    "Would you like me to greet people after they complete screening?");
                
                msg = await ctx.RespondAsync(builder);
                
                await msg.CreateReactionAsync(confirm);
                await msg.CreateReactionAsync(deny);
                
            }
            
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
            }
        }
    }
}