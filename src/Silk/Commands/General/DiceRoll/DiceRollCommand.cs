using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.General.DiceRoll;


[Category(Categories.General)]
public class DiceRollCommand : CommandGroup
{
    private readonly Random                 _random;
    private readonly ICommandContext        _context;
    private readonly IDiscordRestChannelAPI _channels;
    
    public DiceRollCommand(Random random, ICommandContext context, IDiscordRestChannelAPI channels)
    {
        _random   = random;
        _context  = context;
        _channels = channels;
    }

    [Command("random")]
    [Description("Generate a random number in a given range; defaults to 100. (Hard limit of ~2.1 billion)")]
    public Task<Result<IMessage>> Random(Int64 max = 100)
        => _channels.CreateMessageAsync(_context.ChannelID, $"{_random.NextInt64(max)} is your number!");

    [Command("roll")]
    [Description("Roll die like it's DnD! Example: 2d4 + 10 + d7")]
    public Task<Result<IMessage>> Roll([Greedy] string roll)
    {
        if (string.IsNullOrEmpty(roll))
            return _channels.CreateMessageAsync(_context.ChannelID, "You need to specify a roll!");
        
        var         parser = new DiceParser(roll);
        IList<Step> steps  = parser.Run();

        
        var modifiers   = new List<int>();
        var rolls       = new List<int>();
        var embedFields = new List<IEmbedField>();
        for (var i = 0; i < steps.Count; i++)
        {
            if (steps[i].Type is StepType.Addition)
            {
                modifiers.Add(steps[i].TotalNumber);
            }
            else
            {
                var localRolls = new List<int>();
                for (var j = 0; j < steps[i].TotalNumber; j++)
                {
                    int result = _random.Next(1, steps[i].DiceNoSides + 1);
                    localRolls.Add(result);
                }
                int sum = localRolls.Sum();
                rolls.Add(sum);
                embedFields.Add(new EmbedField($"🎲{steps[i].TotalNumber}d{steps[i].DiceNoSides}", $"\t{string.Join(", ", localRolls)}   =   {sum}"));
            }
        }
        
        embedFields.Add(new EmbedField("Total", $"{rolls.Sum() + modifiers.Sum()}"));

        var embed = new Embed()
        {
            Title  = "Roll Result",
            Colour = Color.DarkBlue,
            Fields = embedFields
        };
        
        return _channels.CreateMessageAsync(_context.ChannelID, embeds: new[] {embed});
    }
    
}