using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Core.Services;

namespace Silk.Core.Commands.Tests
{
    public class Time : BaseCommandModule
    {
        public ConfigService Service { private get; set; }

        [Command]
        public async Task GetTimeAsync(CommandContext ctx, TimeSpan t)
        {
            await ctx.RespondAsync(t.ToString());
        }

        [Command]
        public async Task GetConfig(CommandContext ctx)
        {
            await ctx.RespondAsync((await Service.GetConfigFromDatabaseAsync(ctx.Guild.Id)).IsPremium.ToString());
        }
    }
}