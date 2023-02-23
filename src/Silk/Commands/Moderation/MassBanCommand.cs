using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Services.Interfaces;
using Silk.Utilities;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.Moderation;

[Category(Categories.Mod)]
public class @MassBanCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IInfractionService     _infractions;
    private readonly IDiscordRestChannelAPI _channels;
    
    public MassBanCommand(ICommandContext context, IInfractionService infractions, IDiscordRestChannelAPI channels)
    {
        _context     = context;
        _infractions = infractions;
        _channels    = channels;
    }

    [Command("massban", "mass-ban", "m-ban", "503")]
    [Description("Swing the ban hammer on multiple users!")]
    [RequireDiscordPermission(DiscordPermission.BanMembers)]
    public async Task<IResult> MassBanAsync
    (
        [Greedy]
        [Range(Min = 1)]
        [Description("The users to ban")]
        IUser[] users,
    
        [Option('d', "days")]
        [Description("How many days to clear of the user's message history.")]
        byte days = 0,
        
        [Greedy]
        [Option('r', "reason")]
        [Description("Why the users are being banned.")]
        string reason = "Not given.",
    
        [Switch('s', "silent")]
        [Description("Whether to send a message to the user about the ban.")]
        bool silent = false
    )
    {
        users = users.DistinctBy(u => u.ID).ToArray();

        days = Math.Min((byte)7, days);
        
        var result = await Task.WhenAll(users.Select((u, i) => _infractions.BanAsync(_context.GetGuildID(), u.ID, _context.GetUserID(), days, reason, notify: !silent && i < 15)));

        var failed = result.Count(r => !r.IsSuccess);

        await _channels.CreateMessageAsync(_context.GetChannelID(), $"Banned {users.Length - failed} user(s) {(failed > 0 ? $"(Failed to ban {failed} users)" : null)}");
        
        return Result.FromSuccess();
    }
}