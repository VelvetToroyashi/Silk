using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Constants;
using Silk.Core.Services.Interfaces;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Server.Configuration
{
    public partial class BaseConfigCommand
    {
        public partial class BaseEditConfigCommand
        {
            [Group("moderation")]
            public class EditModerationConfigCommand : BaseCommandModule
            {
                //Does nested class ctor injection work???? //
                private readonly IDatabaseService? _dbService;
                private readonly IServiceCacheUpdaterService _cacheUpdaterService;
                
                [GroupCommand]
                public async Task EditConfig(CommandContext ctx) =>
                    await new DiscordMessageBuilder()
                        .WithReply(ctx.Message.Id, true)
                        .WithContent($"See `{ctx.Prefix}help config edit moderation`.")
                        .SendAsync(ctx.Channel);

                [Command]
                public async Task Mute(CommandContext ctx, DiscordRole role)
                {
                    var builder = new DiscordMessageBuilder();
                    builder.WithoutMentions();
                    builder.WithReply(ctx.Message.Id);
                    
                    if (role.IsManaged)
                    {
                        builder.WithContent("This is a bot role!");
                        await ctx.RespondAsync(builder);
                        return;
                    }
                    if (role.Permissions.HasFlag(Permissions.SendMessages))
                    {
                        builder.WithContent("This doesn't restrict members!");
                        await ctx.RespondAsync(builder);
                        return; 
                    }
                    builder.WithContent($"Alrighty, muted role is now {role.Mention}!");
                    await ctx.Message.CreateReactionAsync(Emojis.EConfirm);
                    await ctx.RespondAsync(builder);
                    _cacheUpdaterService.UpdateGuild(ctx.Guild.Id);
                }

                public EditModerationConfigCommand(IDatabaseService dbService, IServiceCacheUpdaterService cacheUpdaterService)
                {
                    _dbService = dbService;
                    _cacheUpdaterService = cacheUpdaterService;
                }
            }
        } 
    }
}