using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions;

namespace Silk.Core.Commands.Bot
{
    [Category(Categories.Bot)]
    public class ShardsCommand : BaseCommandModule
    {
        private const string EmbedTitle = "`|\t[Shard]\t|\t[Ping]\t|\t[Guilds]\t|\t[Members]\t|`";

        private readonly Main _main;
        public ShardsCommand(Main main)
        {
            _main = main;
        }

        [Command]
        public async Task Shards(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder();
            builder.WithReply(ctx.Message.Id);

            InteractivityExtension? interactivity = ctx.Client.GetInteractivity();

            DiscordEmbedBuilder? embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Gold)
                .WithTitle("Shard info:");
            var sb = new StringBuilder();

            IReadOnlyDictionary<int, DiscordClient>? shards = _main.ShardClient.ShardClients;
            int averagePing = shards.Sum(c => c.Value.Ping) / shards.Count;

            embed.AddField("​", $"**{EmbedTitle}**");
            embed.WithFooter($"You are shard {ctx.Client.ShardId + 1}!");

            sb.Append("`|");
            sb.Append($" {shards.Count}".Center("\t[Shard]\t"));
            sb.Append('|');
            sb.Append($"{averagePing / shards.Count}".Center("\t[Ping]\t"));
            sb.Append('|');
            sb.Append($"{shards.Sum(c => c.Value.Guilds.Count)}".Center("\t[Guilds]\t"));
            sb.Append('|');
            sb.Append($" {shards.Values.SelectMany(g => g.Guilds.Values).Sum(g => g.Members.Count)}".Center("\t[Members]\t"));
            sb.Append("|`");

            embed.AddField($"{Formatter.Bold("T o t a l:")}", sb.ToString());

            sb.Clear();
            foreach ((int id, var shard) in shards)
            {
                int members = shard.Guilds.SelectMany(g => g.Value.Members).Count();
                sb.Append("`|");
                sb.Append($" {id + 1}".Center("\t[Shard]\t"));
                sb.Append('|');
                sb.Append($"{shard.Ping}ms".Center("\t[Ping]\t"));
                sb.Append('|');
                sb.Append($"{shard.Guilds.Count}".Center("\t[Guilds]\t"));
                sb.Append('|');
                sb.Append($" {members}".Center("\t[Members]\t"));
                sb.Append("|`");
                sb.AppendLine();
            }

            IEnumerable<Page>? paginated = interactivity.GeneratePagesInEmbed(sb.ToString(), SplitType.Line, embed);

            await ctx.Channel.SendPaginatedMessageAsync(ctx.User, paginated);
        }
    }
}