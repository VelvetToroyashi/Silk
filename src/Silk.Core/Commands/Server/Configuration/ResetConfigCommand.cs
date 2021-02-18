using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Silk.Core.Services.Interfaces;
using Silk.Data.Models;

namespace Silk.Core.Commands.Server.Configuration
{
    public class ResetConfigCommand : BaseCommandModule
    {
        private readonly IDatabaseService _dbService;
        public ResetConfigCommand(IDatabaseService dbService) => _dbService = dbService;

        [Command]
        [Description("Reset your configuration!")]
        public async Task Reset(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder();
            var confirmationCode = new Random((int) ctx.Message.Id).Next(1000, 10000).ToString();
            builder.WithReply(ctx.Message.Id, true);
            builder.WithContent($"**All settings will be reset** (This does not include server prefix) | Are you sure? Type `{confirmationCode}` to confirm. Type cancel to cancel");
            await ctx.RespondAsync(builder);

            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            InteractivityResult<DiscordMessage> result = await interactivity.WaitForMessageAsync(m => m.Content.Equals("cancel", StringComparison.CurrentCultureIgnoreCase) || m.Content.Equals(confirmationCode));

            if (result.Result?.Content == confirmationCode)
            {
                GuildConfig config = (await _dbService.GetConfigAsync(ctx.Guild.Id))!;
                GuildConfig temp = config;
                config = new() {Id = temp.Id, GuildId = temp.GuildId};

                await _dbService.UpdateConfigAsync(config);

                builder.WithContent("Guild settings have been reset!");
                await ctx.RespondAsync(builder);
            }
            else
            {
                builder.WithContent("Canceled.");
                await ctx.RespondAsync(builder);
            }
        }
    }
}