using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace SilkBot
{
    public class SpamCommand : BaseCommandModule
    {
        [Command("whywouldyoueverevenguessthiscommandlmfao")]
        public async Task Spam(CommandContext ctx)
        {
            if (ctx.Member.Id != 209279906280898562)
            {
                await ctx.RespondAsync("Sorry, but you are not allowed to use this command!");
                return;
            }

            var rand = new Random();
            for (int i = 0; i < 10; i++)
            {
                await ctx.Channel.SendMessageAsync(RandomString(rand.Next(10, 200), false));
                Thread.Sleep(500);
            }
        }

        [Command("yes")]
        public async Task Spam(CommandContext ctx, int number)
        {
            if (ctx.Member.Id != 209279906280898562)
            {
                await ctx.RespondAsync("Sorry, but you are not allowed to use this command!");
                return;
            }

            var rand = new Random();
            for (int i = 0; i < number; i++)
            {
                await ctx.Channel.SendMessageAsync(RandomString(rand.Next(10, 200), false));
                Thread.Sleep(100);
            }
        }

        [Command("yes")]
        [RequireOwner]
        public async Task Spam(CommandContext ctx, int number, int min, int max, bool lowercase)
        {
            if (ctx.Member.Id != 209279906280898562)
            {
                await ctx.RespondAsync("Sorry, but you are not allowed to use this command!");
                return;
            }

            var rand = new Random();
            for (int i = 0; i < number; i++)
            {
                await ctx.Channel.SendMessageAsync(RandomString(rand.Next(min, max), lowercase));
                Thread.Sleep(500);
            }
        }


        public string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

    }
}
