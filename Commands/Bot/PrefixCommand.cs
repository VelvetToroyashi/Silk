using DSharpPlus.CommandsNext;
using System.Threading.Tasks;
using SilkBot;
using Newtonsoft.Json;
using System.IO;
using System;
using DSharpPlus.CommandsNext.Attributes;
using SilkBot.ServerConfigurations;
using SilkBot.Utilities;

namespace SilkBot.Commands.Bot
{
    public class PrefixCommand : BaseCommandModule
    {
        [Command("Prefix")]
        [Aliases("SetPrefix")]
        public async Task SetPrefix(CommandContext ctx, string prefix)
        {
            if (!ServerConfigurationManager.LocalConfiguration.ContainsKey(ctx.Guild.Id))
                await ServerConfigurationManager.Instance.GenerateConfigurationFromIdAsync(ctx.Guild.Id);
            if (!ctx.Member.IsAdministrator())
            {
                await ctx.RespondAsync("Sorry, but you're not allowed to change the prefix!");
                return;
            }

            SilkBot.Bot.GuildPrefixes[ctx.Guild.Id] = prefix;
            var prefixConfig = JsonConvert.SerializeObject(SilkBot.Bot.GuildPrefixes, Formatting.Indented);
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configLocation = Path.Combine(appdata, "SilkBot", "ServerConfigs");
            File.WriteAllText(Path.Combine(configLocation, "prefixes.gconfig"), prefixConfig);
            await ctx.RespondAsync($"Done! I'll respond to `{prefix}` from now on.");
        }

        [Command("Prefix")]
        public async Task SetPrefix(CommandContext ctx)
        {
            await ctx.RespondAsync($"My prefix is `{SilkBot.Bot.GuildPrefixes[ctx.Guild.Id]}`, but you can always use commands by mentioning me! ({ctx.Client.CurrentUser.Mention})");
        }


    }
}
