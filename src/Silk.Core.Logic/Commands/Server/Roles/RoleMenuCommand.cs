using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Silk.Extensions.DSharpPlus;
using Silk.Shared.Constants;

namespace Silk.Core.Logic.Commands.Server.Roles
{
    [Hidden]
    [RequireGuild]
    [Aliases("rm")]
    [Group("rolemenu")]
    [ModuleLifespan(ModuleLifespan.Transient)] // We're gonna hold some states. //
    public class RoleMenuCommand : BaseCommandModule
    {
        private record Result<T>(bool Succeeded, T? Value = default)
        {
            public static implicit operator T?(Result<T> result) => result.Value;
            public static implicit operator bool(Result<T> result) => result.Succeeded;
        }


        private readonly List<ulong> _roles = new();
        private readonly List<DiscordEmoji> _reactions = new();

        private const int TimeoutDelay = 3000;

        private const string InitialRoleInputMessage = "Please provide the Id of a role you'd like to add. Type `done` to finish setup!\n\n" +
                                                       "(Tip: Right-click or tap a user with the role you want to copy the Id of. Alternatively, you can find the Id in the server settings!\n" +
                                                       "Putting a backslash (\\\\) in front of the ping will give show the Id, but mention the role! Use with caution if you're running this in a public channel.)";

        private const string WaitingForTitleMessage = "**Role Menu setup in progress. Waiting for title.**";

        private const string InitialGetTitleMessage = "Welcome! What would you like to name this role menu? (Menu will be prefixed with **`RoleMenu: `** | Input limited to 50 characters";
        private const string GetTitleMessageAfterLoop = "What would you like to name this role menu? (Menu will be prefixed with **`RoleMenu: `** | Input limited to 50 characters";

        private const string DuplicatedRoleId = "Sorry! But you've already assigned that role. Pick a different one and try again.";
        private const string AlreadyReactedErrorMessage = "Sorry, but you've already used that emoji! Please pick a different one and try again.";
        private const string NonSharedEmojiErrorMessage = "Hey! I can't use that emoji, as I'm not in the server it came from. Pick a different emoji and try again!";

        private const string TitleInputLengthExceedsLimit = "Sorry, but the title can only be 50 letters long!";
        private const string GiveIdMessage = "Please provide the Id of a role you'd like to add. Type `done` to finish setup!";

        private const string NonRoleIdErrorMessage = "Hmm.. That doesn't appear to be a role on this server! Copy a different Id and try again.";

        private const string HierarchyDisalowsAssigningRoleErrorMessage = "I can't give that role out to people! I can only give out roles below my own. Try a different one and try again.";


        [Command]
        [RequireBotPermissions(Permissions.ManageRoles)]
        public async Task Create(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            DiscordMessage roleMenu = await ctx.RespondAsync(WaitingForTitleMessage);

            InteractivityExtension input = ctx.Client.GetInteractivity();
            Result<string> title = await GetTitleAsync(ctx, input);

            if (!title)
            {
                await ctx.RespondAsync("Sorry, but you took too long to respond!");
                return;
            }

        }

        private async Task<Result<string>> GetTitleAsync(CommandContext ctx, InteractivityExtension input)
        {
            var initMessage = await ctx.RespondAsync(InitialGetTitleMessage);
            string? inputMessage = null;

            while (true)
            {
                var result = await input.WaitForMessageAsync(m => m.Channel == ctx.Channel && m.Author == ctx.Message.Author && !string.IsNullOrWhiteSpace(m.Content));
                if (result.TimedOut) return new(false);
                else
                {

                    if (result.Result.Content.Length > 50)
                    {
                        await result.Result.DeleteAsync();
                        await initMessage.ModifyAsync(TitleInputLengthExceedsLimit);
                        await Task.Delay(TimeoutDelay);
                        await initMessage.ModifyAsync(GetTitleMessageAfterLoop);
                    }
                    else
                    {
                        Result<string>? title = await ValidateMessageAsync(input, initMessage, result.Result);

                        if (title is not null)
                            return title;
                    }
                }
            }
        }

        private async Task<Result<string>?> ValidateMessageAsync(InteractivityExtension input, DiscordMessage initMessage, DiscordMessage result)
        {
            await initMessage.ModifyAsync("Are you sure?");
            DiscordEmoji[] yesno = await initMessage.CreateReactionsAsync(Emojis.ConfirmId, Emojis.DeclineId);

            var yesnoResult = await input.WaitForReactionAsync(r => r.Emoji == yesno.First() || r.Emoji == yesno.Last() && r.User == result.Author);

            if (yesnoResult.TimedOut)
            {
                return new(false);
            }
            else if (yesnoResult.Result.Emoji == yesno[1]) // yesno[1] == decline //
            {
                await initMessage.ModifyAsync(GetTitleMessageAfterLoop);
                await initMessage.DeleteAllReactionsAsync();
                return null;
            }
            else
            {
                await result.DeleteAsync();
                return new(true, result.Content);
            }
        }
    }
}