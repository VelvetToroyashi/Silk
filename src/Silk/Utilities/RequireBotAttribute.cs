using System.Threading.Tasks;
using DSharpPlus.SlashCommands;

namespace Silk.Utilities;

public sealed class RequireBotAttribute : SlashCheckBaseAttribute
{
    public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx) => ctx.Interaction.GuildId is not null ^ ctx.Member is null;
}