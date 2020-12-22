#region

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Silk.Core.Utilities;

#endregion

namespace Silk.Core.Commands.Bot
{
    [Category(Categories.Bot)]
    public class EvalCommand : BaseCommandModule
    {
        [Command("eval")]
        [Aliases("evalcs", "cseval", "roslyn")]
        [Description("Evaluates C# code.")]
        [Hidden]
        [RequireOwner]
        public async Task EvalCS(CommandContext ctx, [RemainingText] string code)
        {
            DiscordMessage msg = ctx.Message;

            int cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            int cs2 = code.LastIndexOf("```");

            if (cs1 is -1 || cs2 is -1)
                throw new ArgumentException("You need to wrap the code into a code block.");

            string cs = code.Substring(cs1, cs2 - cs1);

            msg = await ctx.RespondAsync("", embed: new DiscordEmbedBuilder()
                                                    .WithColor(new DiscordColor("#FF007F"))
                                                    .WithDescription("Evaluating...")
                                                    .Build()).ConfigureAwait(false);
                
            try
            {
                var globals = new TestVariables(ctx.Message, ctx.Client, ctx);

                var sopts = ScriptOptions.Default;
                sopts = sopts.WithImports("System", "System.Collections.Generic", "System.Linq", "System.Text",
                    "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.CommandsNext", "DSharpPlus.Interactivity",
                    "Microsoft.Extensions.Logging");
                sopts = sopts.WithReferences(AppDomain.CurrentDomain.GetAssemblies()
                                                      .Where(xa =>
                                                          !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

                Script<object> script = CSharpScript.Create(cs, sopts, typeof(TestVariables));
                script.Compile();
                ScriptState<object> result = await script.RunAsync(globals).ConfigureAwait(false);

                if (result?.ReturnValue != null && !string.IsNullOrWhiteSpace(result.ReturnValue.ToString()))
                    await msg.ModifyAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Evaluation Result", Description = result.ReturnValue.ToString(),
                        Color = new DiscordColor("#007FFF")
                    }.Build()).ConfigureAwait(false);
                else
                    await msg.ModifyAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Evaluation Successful", Description = "No result was returned.",
                        Color = new DiscordColor("#007FFF")
                    }.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await msg.ModifyAsync(embed: new DiscordEmbedBuilder
                {
                    
                    Title = "Evaluation Failure",
                    Description = string.Concat("**", ex.GetType().ToString(), "**: ", ex.Message),
                    Color = new DiscordColor("#FF0000")
                }.Build()).ConfigureAwait(false);
            }
        }
    }

    public class TestVariables
    {
        public DiscordMessage Message { get; set; }
        public DiscordChannel Channel { get; set; }
        public DiscordGuild Guild { get; set; }
        public DiscordUser User { get; set; }
        public DiscordMember Member { get; set; }
        public CommandContext Context { get; set; }

        public TestVariables(DiscordMessage msg, DiscordClient client, CommandContext ctx)
        {
            Client = client;

            Message = msg;
            Channel = msg.Channel;
            Guild = Channel.Guild;
            User = Message.Author;
            if (Guild != null)
                Member = Guild.GetMemberAsync(User.Id).ConfigureAwait(false).GetAwaiter().GetResult();
            Context = ctx;
        }

        public DiscordClient Client;
    }
}