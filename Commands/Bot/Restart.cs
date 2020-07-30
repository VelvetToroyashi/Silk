using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Newtonsoft.Json;
using SilkBot.ServerConfigurations;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SilkBot.Commands.Bot
{

    public class Restart : BaseCommandModule
    {
        [RequireOwner]
        [Command("restart")]
        public async Task RestartBot(CommandContext ctx)
        {
            foreach(var config in ServerConfigurationManager.Configs)
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ServerConfigs", $"{config.Key}.serverconfig"), JsonConvert.SerializeObject(config.Value, Formatting.Indented));
            await Task.CompletedTask;
            Process.Start(@"C:\Users\Cinnamon\Desktop\Restart Bot.bat");
        }
    }
}
