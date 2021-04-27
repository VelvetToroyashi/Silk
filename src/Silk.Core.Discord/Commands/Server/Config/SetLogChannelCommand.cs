using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Discord.Services.Interfaces;
using Silk.Shared.Abstractions.DSharpPlus.Concrete;
using Silk.Shared.Abstractions.DSharpPlus.Interfaces;

namespace Silk.Core.Discord.Commands.Server.Config
{
    public partial class ConfigCommand
    {
        public sealed class SetLogChannelConfigCommand : BaseCommandModule
        {
            private readonly IInputService _input;
            private readonly IMediator _mediator;
            private readonly IMessageSender _sender;
            private readonly IServiceCacheUpdaterService _updater;
            public SetLogChannelConfigCommand(IInputService input, IServiceCacheUpdaterService updater, IMediator mediator, IMessageSender sender)
            {
                _input = input;
                _updater = updater;
                _mediator = mediator;
                _sender = sender;
            }


            [Command("Log")]
            public async Task LogWrapper(CommandContext ctx, DiscordChannel channel) =>
                await SetLoggingChannel(new CommandExecutionContext(ctx.Message, ctx.Channel, ctx.Guild, ctx.Prefix, _sender), (Channel) channel);

            public async Task SetLoggingChannel(ICommandExecutionContext ctx, IChannel channel)
            {
                var msg = await ctx.RespondAsync($"Alright, so you would like me to log to {channel.Mention}?");
                var result = await _input.GetConfirmationAsync(msg, ctx.Message.Author.Id);

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