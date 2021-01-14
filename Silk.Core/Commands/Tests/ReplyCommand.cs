using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Tests
{
    public class ReplyCommand : BaseCommandModule
    {
        [Command]
        public async Task Reply(CommandContext ctx, DiscordMember user)
        {
            var builder = new DiscordMessageBuilder();
            builder.WithContent($"This should mention {ctx.Member.Username} and  {user.Mention}");
            builder.WithReply(ctx.Message.Id, true);
            builder.WithUserMentions(new[] {user.Id});
            await ctx.RespondAsync(builder);
        }
    }
}