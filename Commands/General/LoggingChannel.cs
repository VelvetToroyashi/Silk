using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.ServerConfigurations;
using System.Linq;
using System.Threading.Tasks;

namespace SilkBot
{
    public class LoggingChannel : BaseCommandModule
    {
        [Command("SetLoggingChannel")]
        [Aliases("Logging", "LogChannel", "lc", "log")]
        [RequireBotPermissions(Permissions.ManageChannels)]
        [HelpDescription("Sets the channel Silk will log to. *[In implementation, settings will not be saved.]*")]
        public async Task SetLoggingChannel(CommandContext ctx, DiscordChannel logChannel)
        {
            await ctx.RespondAsync(embed: EmbedHelper.CreateEmbed(ctx, "Log channel set!", $"I'll log actions to {logChannel.Mention}!", DiscordColor.SapGreen));
            var serverConfigExists = !(ServerConfigurationManager.LocalConfiguration.FirstOrDefault(config => config.Key == ctx.Guild.Id).Value is null);
            if (!serverConfigExists) 
            {
                var config = await ServerConfigurationManager.Instance.GenerateConfigurationFromIdAsync(ctx.Guild.Id);
                config.LoggingChannel = logChannel.Id;
            }
            else 
            {
                ServerConfigurationManager.LocalConfiguration.First(config => config.Value.Guild == ctx.Guild.Id).Value.LoggingChannel = logChannel.Id;
            }
                
        }
    }
}
