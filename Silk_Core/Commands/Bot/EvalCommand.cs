using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using SilkBot.Utilities;


namespace SilkBot.Commands.Bot
{
    [Category(Categories.Bot)]
    public class EvalCommand : BaseCommandModule
    {
        [Command("Eval"), RequireOwner()]
        public async Task Eval(CommandContext ctx, [RemainingText] string code) 
        {
            
            await ctx.Message.DeleteAsync();
            try
            {
                object o = CSharpScript.EvaluateAsync(code, globals: new Globals() with { ctx = ctx }).ConfigureAwait(false);
                {
                    GC.SuppressFinalize(o);
                }
            }
            catch (Exception e)
            {
                await ctx.RespondAsync($"Eval threw the following: `{e.Message}`");
            }
            
            
        }
    }

    public record Globals
    {
        public CommandContext ctx { get; init; }
    }
    public record CodeBlock
    {
        public string Content { get; private init; }
    }
}
