using System.ComponentModel;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Core.Errors;
using Silk.Core.Services.Interfaces;

namespace Silk.Core.Commands;

public class TestCommand : CommandGroup
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly ICommandContext        _context;
    private readonly IInfractionService     _infractions;
    public TestCommand(IDiscordRestChannelAPI channelApi, IInfractionService infractions, ICommandContext context)
    {
        _channelApi  = channelApi;
        _infractions = infractions;
        _context     = context;
    }

    [Command("test")]
    [Description("Test command")]
    public async Task<Result> A(IUser user)
    {
        Result res = await _infractions.BanAsync(_context.GuildID.Value, user.ID, _context.User.ID, 0, "Testing!..");

        if (!res.IsSuccess)
        {
            await _channelApi.CreateMessageAsync(_context.ChannelID, $"{res.Error}\n{(res.Error is InfractionError ie ? ie.Message + ie.Inner.Error : res.Inner?.Error)}");
            return res;
        }

        await _channelApi.CreateMessageAsync(_context.ChannelID, "Banned user. Unbanning...");

        res = await _infractions.UnBanAsync(_context.GuildID.Value, user.ID, _context.User.ID);

        if (!res.IsSuccess)
            await _channelApi.CreateMessageAsync(_context.ChannelID, $"{res.Error}\n{(res.Error is InfractionError ie ? ie.Inner.Error : res.Inner?.Error)}");

        else await _channelApi.CreateMessageAsync(_context.ChannelID, "Unbanned user.");

        return Result.FromSuccess();
    }

}