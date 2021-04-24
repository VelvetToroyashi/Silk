using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IInputService _input;
        private readonly IMessageSender _sender;

        private readonly List<IMessage> _residualMessages = new();
        public RoleMenuCommand(IInputService input, IMessageSender sender)
        {
            _input = input;
            _sender = sender;
        }

        [Command]
        public async Task Create(CommandContext ctx)
        {
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
            var inputResult = string.Empty;
            IMessage roleInputMessage = default!;
            while (inputResult is not null or "done")
            {
                roleInputMessage = await context.RespondAsync("Please provide the Id of a role you'd like to add.\n\n" +
                                                              "(Tip: Right-click or tap a user with the role you want to copy the Id of. Alternatively, you can find the Id in the server settings!\n" +
                                                              "Putting a backslash (\\\\) in front of the ping will give show the Id, but mention the role! Use with caution if you're running this in a public channel.)");
                var result = await _input.GetInputAsync(context.User.Id, context.Channel.Id, context.Guild!.Id);

                if (result is null)
                {
                    await roleMenuMessage.DeleteAsync();
                    await roleInputMessage.DeleteAsync();
                    var msg = await context.RespondAsync("Timed out!");
                    await Task.Delay(3000);
                    await msg.DeleteAsync();
                    return;
                }


                if (!ulong.TryParse(result.Content, out var id)) continue;

                if (context.Guild.Roles.Contains(id))
                {
                    inputResult = result.Content;
                }
                else
                {
                    var notFoundMessage = await context.RespondAsync("That's not a role!");
                    await result.DeleteAsync();
                    await Task.Delay(3000);
                    await notFoundMessage.DeleteAsync();
                }

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
                    var lengthExceededMessage = await ctx.RespondAsync("Sorry! But the title must not exceed 50 characters!");
                    await Task.Delay(4000);
                    await result.DeleteAsync();
                    await lengthExceededMessage.DeleteAsync();
                    return null;
                }
            }
        }
    }
}