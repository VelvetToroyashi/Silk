using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Discord.Services.Interfaces;

namespace Silk.Core.Logic.Commands.Server.Roles
{
    [Hidden]
    [RequireGuild]
    [Aliases("rm")]
    [Group("rolemenu")]
    [ModuleLifespan(ModuleLifespan.Transient)] // We're gonna hold some states. //
    public class RoleMenuCommand : BaseCommandModule
    {
        private record RoleMenuOption(string Name, ulong EmojiId, ulong RoleId);

        private readonly IInputService _input;
        private readonly List<DiscordEmoji> _reactions = new();
        private readonly List<ulong> _roles = new();


        private const string InitialRoleInputMessage = "Please provide the Id of a role you'd like to add. Type `done` to finish setup!\n\n" +
                                                       "(Tip: Right-click or tap a user with the role you want to copy the Id of. Alternatively, you can find the Id in the server settings!\n" +
                                                       "Putting a backslash (\\\\) in front of the ping will give show the Id, but mention the role! Use with caution if you're running this in a public channel.)";

        private const string DuplicatedRoleId = "Sorry! But you've already assigned that role. Pick a different one and try again.";
        private const string AlreadyReactedErrorMessage = "Sorry, but you've already used that emoji! Please pick a different one and try again.";
        private const string NonSharedEmojiErrorMessage = "Hey! I can't use that emoji, as I'm not in the server it came from. Pick a different emoji and try again!";

        private const string TitleInputLengthExceedsLimit = "Sorry, but the title can only be 50 letters long!";
        private const string GiveIdMessage = "Please provide the Id of a role you'd like to add. Type `done` to finish setup!";

        private const string NonRoleIdErrorMessage = "Hmm.. That doesn't appear to be a role on this server! Copy a different Id and try again.";

        private const string HierarchyDisalowsAssigningRoleErrorMessage = "I can't give that role out to people! I can only give out roles below my own. Try a different one and try again.";


        [Command]
        [RequireBotPermissions(Permissions.ManageRoles)]
        public async Task Create(CommandContext ctx) { }

    }
}