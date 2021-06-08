using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace Silk.Core.SlashCommands
{
    [SlashCommandGroup("test", "Testing!")]
    public class Test : SlashCommandModule
    {
        [SlashCommandGroup("testing", "aaaaa")]
        public class Test2 : SlashCommandModule
        {
            [SlashCommand("aaaa", "aaaa, but 1")]
            public Task A(InteractionContext ctx) => ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() {Content = "A", IsEphemeral = true});

            [SlashCommand("bbbb", "aaaa, but 1")]
            public Task B(InteractionContext ctx) => ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() {Content = "B", IsEphemeral = true});

            [SlashCommand("cccc", "aaaa, but 1")]
            public Task C(InteractionContext ctx) => ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() {Content = "C", IsEphemeral = true});

        }
    }
}