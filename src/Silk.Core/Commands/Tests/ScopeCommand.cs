using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Silk.Extensions;

namespace Silk.Core.Commands.Tests
{
    [ModuleLifespan(ModuleLifespan.Singleton)]
    public class ScopeCommand : BaseCommandModule
    {
        [Command]
        public async Task Scope(CommandContext ctx)
        {
            var s = ctx.Services.Get<Startup.Scoped>();
            var t = ctx.Services.Get<Startup.Transient>();

            await ctx.RespondAsync("Transient!\n" +
                                   $"*Scoped* Id: {s.Id}\n" +
                                   $"*Transient* Id: {t.Id}");
        }
    }
}