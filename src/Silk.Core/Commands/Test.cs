using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Silk.Core.Commands
{
	public class Test : BaseCommandModule
	{
		[Command("mention"), Description("Attempts to mention a user")]
        public async Task MentionablesAsync(CommandContext ctx, DiscordUser user)
        {
            var content = $"Hey, {user.Mention}! Listen!";
            await ctx.Channel.SendMessageAsync("✔ should ping, ❌ should not ping.").ConfigureAwait(false);

            await ctx.RespondAsync("✔ Default Behaviour: " + content).ConfigureAwait(false);                                                                                            //Should ping User

            await new DiscordMessageBuilder()
                .WithContent("✔ UserMention(user): " + content)
                .WithAllowedMentions(new IMention[] { new UserMention(user) })
                .WithReply(ctx.Message.Id)
                .SendAsync(ctx.Channel)
                .ConfigureAwait(false);                                                                                                                      //Should ping user

            await new DiscordMessageBuilder()
                .WithContent("✔ UserMention(): " + content)
                .WithAllowedMentions(new IMention[] { new UserMention() })
                .WithReply(ctx.Message.Id)
                .SendAsync(ctx.Channel)
                .ConfigureAwait(false);                                                                                                                      //Should ping user

            await new DiscordMessageBuilder()
                .WithContent("✔ User Mention Everyone & Self: " + content)
                .WithAllowedMentions(new IMention[] { new UserMention(), new UserMention(user) })
                .WithReply(ctx.Message.Id)
                .SendAsync(ctx.Channel)
                .ConfigureAwait(false);                                                                                                                      //Should ping user


            await new DiscordMessageBuilder()
               .WithContent("✔ UserMention.All: " + content)
               .WithAllowedMentions(new IMention[] { UserMention.All })
               .WithReply(ctx.Message.Id)
               .SendAsync(ctx.Channel)
               .ConfigureAwait(false);                                                                                                                       //Should ping user

            await new DiscordMessageBuilder()
               .WithContent("❌ Empty Mention Array: " + content)
               .WithAllowedMentions(new IMention[0])
               .WithReply(ctx.Message.Id)
               .SendAsync(ctx.Channel)
               .ConfigureAwait(false);                                                                                                                       //Should ping no one

            await new DiscordMessageBuilder()
               .WithContent("❌ UserMention(SomeoneElse): " + content)
               .WithAllowedMentions(new IMention[] { new UserMention(545836271960850454L) })
               .WithReply(ctx.Message.Id)
               .SendAsync(ctx.Channel)
               .ConfigureAwait(false);                                                                                                                       //Should ping no one (@user was not pinged)

            await new DiscordMessageBuilder()
               .WithContent("❌ Everyone():" + content)
               .WithAllowedMentions(new IMention[] { new EveryoneMention() })
               .WithReply(ctx.Message.Id)
               .SendAsync(ctx.Channel)
               .ConfigureAwait(false);                                                                                                                          //Should ping no one (@everyone was not pinged)
        }
	}
}