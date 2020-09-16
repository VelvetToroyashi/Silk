using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SilkBot.Commands.Tests
{
    public class SourceCommand : BaseCommandModule
    {
        private const string baseURL = "https://github.com/VelvetThePanda/SilkBot/blob/development";
        [Command("source")]
        [Description("Get the source code of ")]
        public async Task Source(CommandContext ctx, [Description("The file you want to see the source of. \n(Does not ensure the file exists on the repo)")] string file)
        {
            var asm = Assembly.GetEntryAssembly();
            var asmClass = asm.GetTypes().AsEnumerable().FirstOrDefault(a => a.Name.ToLower().Contains(file.ToLower()));
            if (asmClass is null)
            {
                await ctx.RespondAsync("Sorry, but I can't find that file.");
            }
            else
            {
                if(asmClass.Namespace.Contains("SilkBot."))
                {
                    var namespaces = asmClass.Namespace[7..^0].Replace('.', '/');
                    await ctx.RespondAsync($"{baseURL}{namespaces}/{asmClass.Name}.cs");
                }
                else
                {
                    await ctx.RespondAsync($"{baseURL}/{asmClass}.cs");
                }
                
            }
        }

    }
}
