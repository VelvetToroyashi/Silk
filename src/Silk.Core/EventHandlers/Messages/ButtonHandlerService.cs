using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Silk.Core.EventHandlers.Messages
{
    public class ButtonHandlerService
    {
        private readonly ILogger<ButtonHandlerService> _logger;
        public ButtonHandlerService(ILogger<ButtonHandlerService> logger) => _logger = logger;

        public async Task OnButtonPress(DiscordClient client, ComponentInteractionEventArgs args)
        {
            _logger.LogInformation("Button pressed!");
            await args.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);
            await args.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("Success!").AsEphemeral(true));
        }
    }
}