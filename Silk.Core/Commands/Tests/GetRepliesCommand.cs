using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Silk.Core.Commands.Tests
{
    public class GetRepliesCommand : BaseCommandModule
    {
        [Command]
        public async Task GetRepliesAsync(CommandContext ctx, DiscordMessage message)
        {
            DiscordMessageReference reference;
            DiscordMessage msg;
            if (message.ReferencedMessage is null) 
                new DiscordMessageBuilder()
                .WithReply(ctx.Message.Id, true)
                .WithContent("That message doensn't have a reply!")
                .SendAsync(ctx.Channel).GetAwaiter().GetResult();
            
            msg = message.ReferencedMessage!;
            reference = message.Reference!;
            
            do
            {
                msg = await ctx.Channel.GetMessageAsync(msg.Id);
                if (msg.ReferencedMessage is not null)
                {
                    msg = msg.ReferencedMessage;
                    reference = msg.Reference;
                }

                if (msg.ReferencedMessage is null)
                {
                    msg = await ctx.Channel.GetMessageAsync(msg.Id);
                    if (msg.ReferencedMessage is not null)
                    {
                        msg = msg.ReferencedMessage;
                        reference = msg.Reference; 
                    }
                    
                    else break;
                }

                else break;

            } while (true);




            await new DiscordMessageBuilder()
                .WithReply(msg.Id)
                .WithContent("This is the first message reply from referenced message!")
                .SendAsync(ctx.Channel);
            await new DiscordMessageBuilder()
                .WithReply(reference.Message.Id)
                .WithContent("This is the first message reply message reference!")
                .SendAsync(ctx.Channel);

        }
    }
}