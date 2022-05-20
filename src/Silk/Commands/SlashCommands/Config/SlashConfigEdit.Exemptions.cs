using System;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Attributes;
using Remora.Rest.Core;
using Remora.Results;

namespace Silk.Commands.SlashCommands.Config;

public partial class SlashConfig
{
    public partial class Edit
    {
        [Command("exemptions")]
        public async Task<IResult> EditExemptionsAsync([DiscordTypeHint(TypeHint.String)] string targets)
        {
            throw new NotImplementedException();
        }
    }
}
