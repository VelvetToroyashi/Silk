using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Services.Interfaces;

namespace Silk.Commands;

public class MassBonkCommand : CommandGroup
{
    private readonly ICommandContext        _context;
    private readonly IInfractionService     _infractions;
    private readonly IDiscordRestChannelAPI _channels;
    
    public MassBonkCommand(ICommandContext context, IInfractionService infractions, IDiscordRestChannelAPI channels)
    {
        _context     = context;
        _infractions = infractions;
        _channels    = channels;
    }

    [Command("massbonk")]
    [Description("Bonks everyone in the server.")]
    public async Task<IResult> MassBonk
    (
        [Greedy]
        IUser[] users
    )
    {
        await Task.WhenAll(users.Select(user => _infractions.AddNoteAsync(_context.GuildID.Value, user.ID, _context.User.ID, "Bonketh.")));
        
        return await _channels.CreateMessageAsync(_context.ChannelID, "Bonketh.");
    }
}