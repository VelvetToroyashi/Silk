using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Silk.Core.Logic.Commands.Server.Roles
{
    [Hidden]
    [RequireGuild]
    [Aliases("rm")]
    [Group("rolemenu")]
    [ModuleLifespan(ModuleLifespan.Transient)] // We're gonna hold some states. //
    public class RoleMenuCommand : BaseCommandModule
    {
        private readonly Regex EmojiRegex = new(@"<a?:(.*):(\d+)>");

        [Command]
        [Description("Automagically configure a role menu based on a message! Must provide message link!")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        public async Task Create(CommandContext ctx, DiscordMessage messageLink) { }


    }
}