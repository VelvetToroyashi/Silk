using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core.Commands.General
{
    [Category(Categories.General)]
    public class PickPeopleCommand : BaseCommandModule
    {
        [Command]
        [Description("Pick random members on your server! Useful for...something.")]
        public async Task PickRandom(CommandContext ctx, uint sampleSize)
        {
            if (sampleSize is 0)
                throw new ArgumentOutOfRangeException(nameof(sampleSize), "Sample size must be > 1");

            List<DiscordMember> users = ctx.Guild.Members.Values.Where(u => !u.IsBot).ToList();
            sampleSize = (uint) Math.Min(sampleSize, users.Count);

            int seed = DateTime.UtcNow.Millisecond + DateTime.UtcNow.Second;
            var random = new Random(seed);
            var selectedUsers = new List<string>();


            for (var i = 0; i < sampleSize; i++)
            {
                DiscordMember selectedUser = users[random.Next(users.Count)];
                selectedUsers.Add(selectedUser.Mention);
                users.Remove(selectedUser);
            }

            string result = string.Join('\n', selectedUsers);

            var embed = new DiscordEmbedBuilder()
                .WithTitle($"Selected {sampleSize} random users!")
                .WithDescription($"Result:\n{result}")
                .WithColor(DiscordColor.SapGreen)
                .WithFooter($"RNG Seed: {seed}");
            await ctx.RespondAsync(embed);
        }
    }
}