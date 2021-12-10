using System;
using System.ComponentModel;
using System.Diagnostics;
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
        var    sw  = Stopwatch.StartNew();
        var    now = DateTimeOffset.UtcNow;
        Result res = await _infractions.BanAsync(_context.GuildID.Value, user.ID, _context.User.ID, 0, "Testing!..");
        var    ban = sw.ElapsedMilliseconds;
        
        if (!res.IsSuccess)
        {
            await _channelApi.CreateMessageAsync(_context.ChannelID, $"{res.Error}\n{(res.Error is InfractionError ie ? ie.Message + ie.Inner.Error : res.Inner?.Error)}");
            return res;
        }
        
        sw.Stop();
        
        await _channelApi.CreateMessageAsync(_context.ChannelID, "Banned user. Unbanning...");
        
        sw.Start();

        var unbanStart = sw.ElapsedMilliseconds;
        res = await _infractions.UnBanAsync(_context.GuildID.Value, user.ID, _context.User.ID);
        var unbanEnd = sw.ElapsedMilliseconds;
        
        sw.Stop();
        if (!res.IsSuccess)
        {
            await _channelApi.CreateMessageAsync(_context.ChannelID, $"{res.Error}\n{(res.Error is InfractionError ie ? ie.Inner.Error : res.Inner?.Error)}");
            return res;
        }
        await _channelApi.CreateMessageAsync(_context.ChannelID, "Unbanned user.");

        await _channelApi.CreateMessageAsync(_context.ChannelID,
                                             ""                                                         +
                                             "Debug information:\n"                                     +
                                             $"Ban time: {ban} ms\n"                                    +
                                             $"Unban time: {unbanEnd - unbanStart} ms\n"                +
                                             $"Total time (infractions): {sw.ElapsedMilliseconds} ms\n" +
                                             $"Total time (all): {DateTimeOffset.UtcNow.Subtract(now).TotalMilliseconds:N0} ms");
        

        return Result.FromSuccess();
    }

}