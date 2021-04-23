using System.Collections.Generic;
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
            var roleMenuMessage = await _sender.SendAsync(ctx.Channel.Id, "**Awaiting setup.**");
            var setupInitializerMessage = await _sender.SendAsync(ctx.Channel.Id, "Hello! What would you like to name this role menu? " +
                                                                                  "\n(Menu will be prefixed with **Role Menu:**, title limited to 50 characters)");
            var titleMessage = string.Empty;
            while (string.IsNullOrEmpty(titleMessage))
            {
                titleMessage = await GetTitleAsync(new CommandExecutionContext(ctx, _sender));
            }

            await roleMenuMessage.EditAsync($"**RoleMenu: {titleMessage}**");

            var roleIdInputResult = string.Empty;

            while (roleIdInputResult.ToLower() is not "cancel" or null)
            {
                break;
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

                    if (!confirmationResult ?? true) return null;

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