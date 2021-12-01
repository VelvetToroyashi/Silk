using System.ComponentModel;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Results;

namespace Silk.Core.Commands
{
    [Group("config")]
    [Description("Configure various settings for your guild!")]
    //[RequireDiscordPermission(DiscordPermission.ManageGuild)]
    public class ConfigTestCommand : CommandGroup
    {
        [Group("edit")]
        [Description("Edit various settings for your guild!")]
        public class EditConfig : CommandGroup
        {
            [Command("aaa")]
            [Description("AAAAAAAAAAAAAAAAAAAAAAAA")]
            public async Task<Result> A() => Result.FromSuccess();
        }
    }
}