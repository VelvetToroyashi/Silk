using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Tests
{
    public class TodoCommand : BaseCommandModule
    {
        [Command]
        [RequireOwner]
        public async Task Todo(CommandContext ctx, [RemainingText] string message)
        {
            DiscordChannel? channel = ctx.Guild.Channels.Values.SingleOrDefault(c => c.Name is "todo-channel");
            if (channel is not null)
            {
                DiscordEmbed embed = new DiscordEmbedBuilder()
                    .WithTitle("New To-Do Item Added!")
                    .WithAuthor(ctx.User.Username, ctx.User.GetUrl(), ctx.User.AvatarUrl)
                    .WithColor(DiscordColor.SpringGreen)
                    .WithDescription(message)
                    .WithTimestamp(DateTime.Now.ToUniversalTime())
                    .Build();
                await new DiscordMessageBuilder().WithReply(ctx.Message.Id).WithEmbed(embed).SendAsync(channel);
                
                
                
                //await channel.SendMessageAsync(embed);
            }
        }
    }
}