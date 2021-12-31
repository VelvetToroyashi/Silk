using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using MediatR;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Data.Entities;
using Silk.Data.MediatR.Infractions;
using Silk.Extensions;
using Silk.Interactivity;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.Moderation;


[HelpCategory(Categories.Mod)]
public class CasesCommand : CommandGroup
{
    private readonly IMediator              _mediator;
    private readonly ICommandContext        _context;
    private readonly IDiscordRestChannelAPI _channels;
    
    private readonly InteractivityExtension _interactivity;
    
    public CasesCommand(IMediator mediator, ICommandContext context, IDiscordRestChannelAPI channels, InteractivityExtension interactivity)
    {
        _mediator      = mediator;
        _context       = context;
        _channels      = channels;
        _interactivity = interactivity;
    }

    [Command("case")]
    [RequireContext(ChannelContext.Guild)]
    [Description("View information about a specific case.")]
    [RequireDiscordPermission(DiscordPermission.ManageMessages)]
    public async Task<Result<IMessage>> ViewCaseAsync(int caseID)
    {
        var infCase = await _mediator.Send(new GetUserInfractionRequest(default, _context.GuildID.Value, default, caseID));
        
        if (infCase is null)
            return await _channels.CreateMessageAsync(_context.ChannelID, "Case not found.");

        var embed = new Embed
        {
            Title       = $"Case { caseID } - {infCase.Type.Humanize(LetterCasing.Title)}",
            Colour      = Color.Goldenrod,
            Description = $"**Reason:** {infCase.Reason}",
            Fields = new IEmbedField[]
            {
                new EmbedField("Target:", $"{infCase.TargetID}\n<@{infCase.TargetID}>", true),
                new EmbedField("Enforcer:", $"{infCase.EnforcerID}\n<@{infCase.EnforcerID}>", true),
                new EmbedField("Duration:", infCase.Duration?.Humanize() ?? "Permanent", true),
                new EmbedField("Created:", infCase.CreatedAt.ToTimestamp(),true),
                new EmbedField("Expires:", infCase.ExpiresAt?.Humanize() ?? "Never", true),
                new EmbedField("Pardoned:", (!infCase.AppliesToTarget).ToString(), true)
            }
        };
        
        //TODO: Pagination, waiting on Remora to support it
        
        return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] { embed });
    }

    

    [Command("cases")]
    [RequireContext(ChannelContext.Guild)]
    [RequireDiscordPermission(DiscordPermission.ManageMessages)]
    [Description("Fetch all infractions for a user including kicks, mutes, and more.")]
    public async Task<Result<IMessage>> Cases(IUser user)
    {
        var cases = await _mediator.Send(new GetUserInfractionsRequest(_context.GuildID.Value, user.ID));
        
        if (!cases.Any())
            return await _channels.CreateMessageAsync(_context.ChannelID, "It appears this user is clean. They should keep it up!");

        var embed = new Embed()
        {
            Title       = $"Infractions for {user.Username}#{user.Discriminator:0000}",
            Colour      = Color.Goldenrod,
            Description = cases.Select(GetCaseDescription).Join("\n")
        };
        
        return await _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] {embed});
    }

    private string GetCaseDescription(InfractionEntity infraction) =>
        $"| {infraction.CaseNumber} "                            +
        $"| **{infraction.Type.Humanize(LetterCasing.Title)}** " +
        $"| {infraction.CreatedAt.ToTimestamp()} "               +
        $"| {infraction.EnforcerID} "                            +
        $"|\n Reason: {infraction.Reason.Truncate(150, " [...]")}";
}