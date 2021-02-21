using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Silk.Core.Commands.Moderation.SClean
{
    public class SCleanCommand : BaseCommandModule
    {
        private readonly RootCommand _command;

        public SCleanCommand()
        {
            _command = new();
            _command.AddOption(new("-i") { IsRequired = false });             // Images (png, jpg, and jpeg)
            _command.AddOption(new("-f") { IsRequired = false });             // Anything with a file
            _command.AddOption(new("-b") { IsRequired = false });             // Bots
            _command.AddOption(new("-n") { IsRequired = false });             // Invites
            _command.AddOption(new Option<ulong>("-u")  { IsRequired = false });    // User
            _command.AddOption(new Option<ulong>("-c")  { IsRequired = false });    // Channel
            _command.AddOption(new Option<string>("-r") { IsRequired = false });    // Regex that mf
            
        }
        
        [Command]
        public async Task SClean(CommandContext ctx, int messages, [RemainingText] string options)
        {
            _command.Handler = CommandHandler
                .Create
                <bool, bool, bool, 
                bool, ulong, ulong, string>(async 
                    (i, f, b, n, u, c, r) => 
                    await GetResult(ctx, messages, i, f, b, n, u, c, r));

            await _command.InvokeAsync(options.Split(' '));
        }

        private async Task GetResult(CommandContext ctx,int messages, bool images, bool files, bool bots, bool invites, ulong user, ulong channel, string regex)
        {
            if (user is not 0)
            {
                var chnId = channel is 0 ? ctx.Channel.Id : channel;
                try
                {
                    var chn = ctx.Guild.Channels[chnId];
                    
                }
                catch (KeyNotFoundException)
                {
                    await ctx.RespondAsync("**`-c: Not a valid channel!`**");
                }
            }
        }
    }
}