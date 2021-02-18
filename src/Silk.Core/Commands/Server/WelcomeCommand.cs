using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using MediatR;
using Silk.Core.Services.Interfaces;
using Silk.Core.Utilities;
using Silk.Data.MediatR;
using Silk.Data.Models;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Server
{
    [RequireFlag(UserFlag.Staff)]
    [Description("Welcome message settings! Currently supported substitutions:\n`{u}` -> Username, `{@u}` -> Mention, `{s}` -> Server Name")]
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
        [Description("Set the welcome message for the server!")]
        public async Task SetWelcome(CommandContext ctx, [RemainingText] string message)
        {
            var builder = new DiscordMessageBuilder().WithoutMentions().WithReply(ctx.Message.Id);
            var one = DiscordEmoji.FromUnicode(":one:");
            var two = DiscordEmoji.FromUnicode(":two:");
            var three = DiscordEmoji.FromUnicode(":three:");

            var interactivity = ctx.Client.GetInteractivity();

            if (ctx.Guild.Features.Contains("MEMBER_VERIFICATION_GATE_ENABLED"))
            {
                builder.WithContent("Great! Would you like me to greet people as soon as they join (1), when they pass membership screening (2), or when you give them a role? (3)");
                var msg = await ctx.RespondAsync(builder);
                await msg.CreateReactionAsync(one);
                await msg.CreateReactionAsync(two);
                await msg.CreateReactionAsync(three);

                var result = await interactivity.WaitForReactionAsync(m => m.Emoji == one || m.Emoji == two || m.Emoji == three, msg, ctx.Member);

                if (result.TimedOut)
                {
                    builder.WithContent("Timed out!");
                    await ctx.RespondAsync(builder);
                }
                else
                {
                    if (result.Result.Emoji == one)
                    {
                        builder.WithContent("Great! I'll greet people as they join :)");
                        await _mediator.Send(new GuildConfigRequest.UpdateGuildConfigRequest {GuildId = ctx.Guild.Id, GreetMembers = true});
                        _updater.UpdateGuild(ctx.Guild.Id);
                    }
                    else if (result.Result.Emoji == two)
                    {
                        builder.WithContent("Great! I'll greet people as soon as they agree to the rules!");
                        await _mediator.Send(new GuildConfigRequest.UpdateGuildConfigRequest {GuildId = ctx.Guild.Id, GreetMembers = true, GreetOnScreeningComplete = true});
                        _updater.UpdateGuild(ctx.Guild.Id);
                    }
                    else
                    {
                        DiscordRole role = null!;
                        var gotValidRole = false;
                        builder.WithContent("Alrighty, what role do you want me to check for? (type `cancel` to cancel)");
                        while (!gotValidRole)
                        {
                            msg = (await interactivity.WaitForMessageAsync(m => 
                                string.Equals(m.Content, "cancel", StringComparison.OrdinalIgnoreCase) || 
                                Regex.IsMatch(m.Content, @"^<?@?&?[0-9]{10,}>?$"))).Result;
                            
                            var roleId = ulong.Parse(msg.Content.Replace("<@&", null).Replace(">", null));

                            gotValidRole = ctx.Guild.Roles.ContainsKey(roleId);
                            if (gotValidRole) role = ctx.Guild.Roles[roleId];
                        }
                        
                        
                        builder.WithContent($"Great! I'll greet people as they get {role.Mention}!");
                        await _mediator.Send(new GuildConfigRequest.UpdateGuildConfigRequest {GuildId = ctx.Guild.Id, GreetMembers = true});
                        _updater.UpdateGuild(ctx.Guild.Id);
                    }
                }
                
            }
            else
            {
                builder.WithContent("Great! Would you like me to greet people as soon as they join (1), or when you give them a role? (2)");
                var msg = await ctx.RespondAsync(builder);
                await msg.CreateReactionAsync(one);
                await msg.CreateReactionAsync(two);
                var result = await interactivity.WaitForReactionAsync(m => m.Emoji == one || m.Emoji == two, msg, ctx.Member);
            }
            
            
            

        }
        
    }
}