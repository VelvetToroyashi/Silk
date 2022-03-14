using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Remora.Commands.Attributes;
using Remora.Discord.API.Objects;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Guilds;
using Silk.Extensions;
using Silk.Shared.Constants;

namespace Silk.Commands.Server;

public partial class ConfigCommands
{
    public partial class ViewConfigCommands
    {
        [Command("greetings", "welcome")]
        [Description("View greeting settings for your server.")]
        public async Task<IResult> ViewGreetingsAsync
        (
            [Description("The ID of the specific greeting to view. Leave blank to view all greetings.")] int? id = null
        )
        {
            string GetGreetingText(GuildGreetingEntity g)
            {
                var enabled  = g.Option is GreetingOption.DoNotGreet ? Emojis.DisabledEmoji : Emojis.EnabledEmoji;
                var role     = g.Option is GreetingOption.GreetOnRole ? $"(<@&{g.MetadataID}>) " : null;
                var option   = g.Option.Humanize(LetterCasing.Title);
                var greeting = g.Message.Truncate(50, " [...]");
                var channel  = $"<#{g.ChannelID}>";

                return $"{enabled} **`{g.Id}`** ➜ {option} {role}in {channel} \n> {greeting}\n";
            }

            var config = await _mediator.Send(new GetGuildConfig.Request(_context.GuildID.Value));

            Embed embed;

            if (id is null)
            {
                var greetings = config.Greetings.Select(GetGreetingText);

                embed = new()
                {
                    Colour      = Color.Goldenrod,
                    Title       = $"All greetings ({greetings.Count()})",
                    Description = greetings.Join("\n")
                };
            }
            else
            {
                if (config.Greetings.FirstOrDefault(g => g.Id == id) is not { } greeting)
                    return await _channels.CreateMessageAsync(_context.ChannelID, "I don't see a greeting with that ID!");

                embed = new()
                {
                    Colour      = Color.Goldenrod,
                    Title       = $"Greeting {id}",
                    Description = greeting.Message
                };
            }

            return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
        }
    }
}