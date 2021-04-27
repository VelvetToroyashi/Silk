using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Discord.Services.Interfaces;
using Silk.Shared.Abstractions.DSharpPlus.Concrete;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Core.Discord.Commands.Server.Roles
{
    [RequireGuild]
    [Aliases("rm")]
    [Group("rolemenu")]
    [ModuleLifespan(ModuleLifespan.Transient)] // We're gonna hold some states. //
    public class RoleMenuCommand : BaseCommandModule
    {


        private record RoleMenuOption(string Name, ulong EmojiId, ulong RoleId);

        private readonly IInputService _input;
        private readonly IMessageSender _sender;
        private readonly List<IEmoji> _reactions = new();
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

        public RoleMenuCommand(IInputService input, IMessageSender sender)
        {
            _input = input;
            _sender = sender;
        }

        [Command]
        [RequireBotPermissions(Permissions.ManageRoles)]
        public async Task Create(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            var context = new CommandExecutionContext(ctx, _sender);
            var roleMenuMessage = await _sender.SendAsync(ctx.Channel.Id, "**Awaiting setup.**");
            var setupInitializerMessage = await _sender.SendAsync(ctx.Channel.Id, "Hello! What would you like to name this role menu? " +
                                                                                  "\n(Menu will be prefixed with **Role Menu:**, title limited to 50 characters)");
            var titleMessage = string.Empty;

            while (string.IsNullOrEmpty(titleMessage))
            {
                titleMessage = await GetTitleAsync(context);
            }

            await roleMenuMessage.EditAsync($"**RoleMenu: {titleMessage}**");
            await setupInitializerMessage.DeleteAsync();

            await ConfigureRoleEmojiDictionaryAsync(roleMenuMessage, context);

        }

        private async Task ConfigureRoleEmojiDictionaryAsync(IMessage roleMenuMessage, ICommandExecutionContext context)
        {
            IMessage roleInputMessage = await context.RespondAsync(InitialRoleInputMessage);

            var optionsEnumerable = CreateOptionsListAsync(roleInputMessage, roleMenuMessage, context);
            if (optionsEnumerable is null)
            {
                await SendTimedOutMessageAsync(roleInputMessage, context, roleInputMessage);
                return;
            }

            var options = new List<RoleMenuOption>();

            await foreach (var rm in optionsEnumerable)
                options.Add(rm);

            await Task.Delay(0);
            // Start doing db stuff here //
        }

        private async IAsyncEnumerable<RoleMenuOption>? CreateOptionsListAsync(IMessage roleInputMessage, IMessage roleMenuMessage, ICommandExecutionContext context)
        {
            while (true)
            {
                var roleIdInputMessage = await _input.GetInputAsync(context.User.Id, context.Channel.Id, context.Guild!.Id);

                if (roleIdInputMessage is null)
                {
                    await SendTimedOutMessageAsync(roleMenuMessage, context, roleInputMessage);
                    break;
                }

                if (string.Equals(roleIdInputMessage.Content, "done", StringComparison.OrdinalIgnoreCase)) break;
                if (!ulong.TryParse(roleIdInputMessage.Content, out var id)) continue;

                if (!_roles.Contains(id))
                {
                    _roles.Add(id);
                }
                else
                {
                    await roleIdInputMessage.DeleteAsync();
                    await SendDuplicatedRoleMessageAsync(context);
                    continue;
                }

                if (context.Guild.Roles.Contains(id))
                {
                    await roleIdInputMessage.DeleteAsync();

                    (IEmoji? emoji, bool timedOut) emojiResult = default;

                    while (emojiResult.emoji is null)
                        emojiResult = await GetReactionAsync(context, roleInputMessage, id);

                    if (emojiResult.timedOut)
                    {
                        await SendTimedOutMessageAsync(roleMenuMessage, context, roleInputMessage);
                        break;
                    }
                    else
                    {
                        await PromptForNextIdAsync(roleInputMessage, roleMenuMessage, emojiResult, id);
                        _reactions.Add(emojiResult.emoji);

                        yield return new(emojiResult.emoji!.Name, emojiResult.emoji!.Id, id);
                    }
                }
                else
                {
                    await roleIdInputMessage.DeleteAsync();
                    await SendErrorAsync(context, NonRoleIdErrorMessage);
                }
            }
        }
        private static async Task PromptForNextIdAsync(IMessage roleInputMessage, IMessage roleMenuMessage, (IEmoji? emoji, bool timedOut) emojiResult, ulong id)
        {
            await roleMenuMessage.CreateReactionAsync(emojiResult.emoji!);
            await roleMenuMessage.EditAsync(roleMenuMessage.Content + $"\n{emojiResult.emoji}: <@&{id}>");
            await Task.Delay(250);
            await roleInputMessage.RemoveReactionsAsync();
            await Task.Delay(250);
            await roleInputMessage.EditAsync(GiveIdMessage);
        }

        private static async Task SendDuplicatedRoleMessageAsync(ICommandExecutionContext context)
        {
            var dupeRoleMessage = await context.RespondAsync(DuplicatedRoleId);
            await Task.Delay(3000);
            await dupeRoleMessage.DeleteAsync();
        }

        private async Task<(IEmoji? emoji, bool timedOut)> GetReactionAsync(ICommandExecutionContext context, IMessage roleInputMessage, ulong inputResult)
        {
            await roleInputMessage.EditAsync($"Alright! React with what emoji you want to use for people to get <@&{inputResult}>?");

            var reaction = await _input.GetReactionInputAsync(context.User.Id, roleInputMessage.Id, context.Guild!.Id, TimeSpan.FromMinutes(2));

            if (reaction is null)
            {
                return (null, true);
            }
            else
            {
                if (!reaction.Emoji.IsSharedEmoji())
                {
                    await reaction.DeleteAsync();
                    await SendErrorAsync(context, NonSharedEmojiErrorMessage);

                    return (null, false);
                }
                if (_reactions.Contains(reaction.Emoji))
                {
                    await SendErrorAsync(context, AlreadyReactedErrorMessage);
                }
                _reactions.Add(reaction.Emoji);
                return (reaction.Emoji, false);
            }
        }

        private async Task<string?> GetTitleAsync(ICommandExecutionContext ctx)
        {
            var result = await _input.GetInputAsync(ctx.User.Id, ctx.Channel.Id, ctx.Guild!.Id);
            if (string.IsNullOrEmpty(result?.Content))
            {
                return null;
            }
            else
            {
                if (result.Content.Length < 50)
                {
                    var confirmationMessage = await ctx.RespondAsync("Are you sure?");
                    var confirmationResult = await _input.GetConfirmationAsync(confirmationMessage, ctx.User.Id);

                    if (!confirmationResult ?? true)
                    {
                        await result.DeleteAsync();
                        await confirmationMessage.DeleteAsync();
                        return null;
                    }

                    await confirmationMessage.DeleteAsync();
                    await result.DeleteAsync();
                    return result.Content;
                }
                else
                {
                    await result.DeleteAsync();
                    await SendErrorAsync(ctx, TitleInputLengthExceedsLimit);
                    return null;
                }
            }
        }

        /// <summary>
        /// Sends an error to the user.
        /// </summary>
        /// <param name="context">The context the error happened in.</param>
        /// <param name="errorMessage">The message to prompt the user with.</param>
        private static async Task SendErrorAsync(ICommandExecutionContext context, string errorMessage)
        {
            var msg = await context.RespondAsync(errorMessage);
            await Task.Delay(6000);
            await msg.DeleteAsync();
        }

        private static async Task SendTimedOutMessageAsync(IMessage roleMenuMessage, ICommandExecutionContext context, IMessage roleInputMessage)
        {
            await roleMenuMessage.DeleteAsync();
            await roleInputMessage.DeleteAsync();
            var msg = await context.RespondAsync("Timed out!");
            await Task.Delay(3000);
            await msg.DeleteAsync();
        }


    }
}