using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.Commands.Server.Config
{
    public partial class ConfigCommand
    {
        public sealed class SetLogChannelConfigCommand : BaseCommandModule
        {
            private readonly IInputService _input;
            private readonly IMediator _mediator;
            private readonly ICacheUpdaterService _updater;
            public SetLogChannelConfigCommand(IInputService input, ICacheUpdaterService updater, IMediator mediator)
            {
                _input = input;
                _updater = updater;
                _mediator = mediator;
            }


            [Command("Log")]
            public async Task SetLoggingChannel(CommandContext ctx, DiscordChannel channel)
            {
                DiscordMessage? msg = await ctx.RespondAsync($"Alright, so you would like me to log to {channel.Mention}?");
                bool? result = await _input.GetConfirmationAsync(msg, ctx.Message.Author.Id);

                if (result is null)
                {
                    await ctx.RespondAsync("Alrighty. You're free to try again later!");
                }
                else if (result.Value)
                {
                    await SetLogChannelAsync(ctx.Guild!.Id, channel.Id);
                    await ctx.RespondAsync($"Alright! I'll log all moderation messages to {channel.Mention}!");
                }
                else
                {
                    await ctx.RespondAsync("Alright! I'll be here if you change your mind :)");
                }
            }

            private async Task SetLogChannelAsync(ulong guildId, ulong loggingChannel)
            {
                await _mediator.Send(new UpdateGuildConfigRequest(guildId) {LoggingChannel = loggingChannel});
                _updater.UpdateGuild(guildId);
            }
        }
    }
}