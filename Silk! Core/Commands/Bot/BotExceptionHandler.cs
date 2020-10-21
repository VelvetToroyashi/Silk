using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;

namespace SilkBot.Commands.Bot
{
    public class BotExceptionHandler
    {
        public BotExceptionHandler(DiscordClient client) 
        {
            client.ClientErrored += OnClientErrored;
            client.GetCommandsNext().CommandErrored += OnCommandErrored;
        }

        private Task OnCommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private Task OnClientErrored(DiscordClient sender, ClientErrorEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
