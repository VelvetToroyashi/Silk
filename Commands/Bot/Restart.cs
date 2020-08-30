using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
            await ctx.Client.UpdateStatusAsync(userStatus: UserStatus.DoNotDisturb);
            await ctx.RespondAsync(embed:
                new DiscordEmbedBuilder()
                .WithTitle("Restart command recieved!")
                .WithDescription("Restarting... Commands will be processed when status is green.")
                .WithColor(new DiscordColor("#29ff29"))
                .WithFooter("Silk!", ctx.Client.CurrentUser.AvatarUrl)
                .WithTimestamp(DateTime.Now)
                );
            foreach (var config in ServerConfigurationManager.LocalConfiguration)
            {
                var appdataFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var localConfig = Path.Combine(appdataFilePath, "SilkBot", "ServerConfigs", $"{config.Key}.serverconfig");
                await File.WriteAllTextAsync(localConfig, JsonConvert.SerializeObject(config.Value, Formatting.Indented));
            }
            
            var globalConfig = JsonConvert.SerializeObject(SilkBot.Bot.GlobalConfig, Formatting.Indented);
            var globalFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ServerConfigs", "GlobalConfig.gconfig");
            await File.WriteAllTextAsync(globalFilePath, globalConfig);


            foreach (var guild in SilkBot.Bot.Instance.Client.Guilds.Keys)
            {
                if (!SilkBot.Bot.GuildPrefixes.ContainsKey(guild))
                    SilkBot.Bot.GuildPrefixes.Add(guild, "!");

            }

            var prefixConfig = JsonConvert.SerializeObject(SilkBot.Bot.GuildPrefixes, Formatting.Indented);
            var prefixPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SilkBot", "ServerConfigs", "prefixes.gconfig"); ;
            await File.WriteAllTextAsync(prefixPath, prefixConfig);

            Process.Start(@"C:\Users\Cinnamon\Desktop\Restart Bot.bat");
        }
        [RequireOwner]
        [Command("restart")]
        public async Task RestartBot(CommandContext ctx, bool readOnly)
        {
            if (!readOnly) return;
            await ctx.Client.UpdateStatusAsync(userStatus: UserStatus.DoNotDisturb);
            await ctx.RespondAsync(embed:
               new DiscordEmbedBuilder()
               .WithTitle("Restart command recieved!")
               .WithDescription("Restarting... Commands will be processed when status is green.")
               .WithColor(new DiscordColor("#29ff29"))
               .WithFooter("Silk!", ctx.Client.CurrentUser.AvatarUrl)
               .WithTimestamp(DateTime.Now)
               );
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
