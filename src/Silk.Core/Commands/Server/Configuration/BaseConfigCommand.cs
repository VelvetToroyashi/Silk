using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Utilities;

namespace Silk.Core.Commands.Server.Configuration
{
    
    [RequireGuild]
    [Group("config")] 
    [Category(Categories.Server)]
    [Aliases("configuration")] 
    [RequireUserPermissions(Permissions.ManageGuild)]
    [Description("Edit configurations the caveman way!\nOr perhaps we just haven't launched the dashboard yet..")]
    public partial class BaseConfigCommand : BaseCommandModule
    {
        [GroupCommand]
        public async Task Config(CommandContext ctx) =>
            await new DiscordMessageBuilder()
                .WithReply(ctx.Message.Id, true)
                .WithContent($"See `{ctx.Prefix}help config` instead.")
                .SendAsync(ctx.Channel);
        // This just serves to tell the user to see the help instead. 
    }
}