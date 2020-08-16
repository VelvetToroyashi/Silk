using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.ServerConfigurations;
using System.Threading.Tasks;

namespace SilkBot.Commands.TestCommands
{
    public class SendMessageToLogChannel : BaseCommandModule
    {
        [Command("Config")]
        [HelpDescription("Something wrong with your settings? Run this command to verify your configuration is set properly!")]
        public async Task TestLogChannelWorks(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await Task.Delay(1500);
            await ctx.RespondAsync("Verifying config is set...");
            var embed = EmbedHelper.CreateEmbed(ctx, "", "");

            await Task.Delay(3000);
            ServerConfigurationManager.LocalConfiguration.TryGetValue(ctx.Guild.Id, out var serverConfigurationObject);
            if(serverConfigurationObject is null)
            {
                var newEmbed = new DiscordEmbedBuilder(embed)
                    .WithAuthor(ctx.Member.Nickname ?? ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                    .WithDescription("Server configuration integrety check failed!")
                    .WithColor(DiscordColor.IndianRed);
                await ctx.RespondAsync(embed: newEmbed);
            }
            else
            {
                //var testMessage = await ctx.Client.SendMessageAsync(
                //    await ServerConfig.ReturnChannelFromID(ctx, serverConfigurationObject.LoggingChannel), 
                //    "This message is to ensure the integrity of the logging channel set via the `SetLoggingChannelCommand`. `!help SetLoggingChannel` for more information.");
                var newEmbed = new DiscordEmbedBuilder(embed)
                    .WithAuthor(ctx.Member.Nickname ?? ctx.Member.DisplayName, iconUrl: ctx.Member.AvatarUrl)
                    .WithDescription("Server configuration integrety check passed!")
                    .WithColor(DiscordColor.PhthaloGreen);
                await ctx.RespondAsync(embed: newEmbed);
            }
        }


    }
}
