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
            foreach (var config in ServerConfigurationManager.LocalConfiguration)
            {
                var appdataFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var localConfig = Path.Combine(appdataFilePath, "SilkBot", "ServerConfigs", $"{config.Key}.serverconfig");
                await File.WriteAllTextAsync(localConfig, JsonConvert.SerializeObject(config.Value, Formatting.Indented));
            }
            Process.Start(@"C:\Users\Cinnamon\Desktop\Restart Bot.bat");
        }
    }
}
