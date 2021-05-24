using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

namespace Silk.Core.EventHandlers.Messages
{
    public class ButtonHandlerService
    {
        private readonly ILogger<ButtonHandlerService> _logger;
        public ButtonHandlerService(ILogger<ButtonHandlerService> logger) => _logger = logger;

        public async Task OnButtonPress(DiscordClient client, ComponentInteractionEventArgs args)
        {
            _logger.LogInformation("{User} pushed {Button}", args.User.Username, args.Id);
            //await args.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate);
            //await args.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().WithContent("Success!").AsEphemeral(true));
            await Task.Delay(2000);
            try { await args.Interaction.CreateResponseAsync(InteractionResponseType.DefferedMessageUpdate); }
            catch (NotFoundException)
            {
                /* button was ACK'd already. */
            }
        }
        public async Task OnInteraction(DiscordClient client, InteractionCreateEventArgs args)
        {
            var p = new DiscordButtonComponent(ButtonStyle.Primary, "P_", "Blurple", emoji: new(833475075474063421));
            var c = new DiscordButtonComponent(ButtonStyle.Secondary, "C_", "Grey", emoji: new(833475015114358854));
            var b = new DiscordButtonComponent(ButtonStyle.Success, "B_", "Green", emoji: new(831306677449785394));
            var y = new DiscordButtonComponent(ButtonStyle.Danger, "Y_", "Red", emoji: new(833886629792972860));
            var z = new DiscordLinkButtonComponent("https://velvetthepanda.dev", "Link", new(826108356656758794));

            var d1 = new DiscordButtonComponent(ButtonStyle.Primary, "disabled", "and", true);
            var d2 = new DiscordButtonComponent(ButtonStyle.Secondary, "disabled2", "these", true);
            var d3 = new DiscordButtonComponent(ButtonStyle.Success, "disabled3", "are", true);
            var d4 = new DiscordButtonComponent(ButtonStyle.Danger, "disabled4", "disabled~!", true);


            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent("Poggers")
                    .AsEphemeral(true)
                    .WithComponents(new[] {p})
                    .WithComponents(new[] {c, b})
                    .WithComponents(new DiscordComponent[] {y, z})
                    .WithComponents(new[] {d1, d2, d3, d4}));
        }
    }
}