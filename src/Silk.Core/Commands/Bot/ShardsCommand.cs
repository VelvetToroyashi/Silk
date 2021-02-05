using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Extensions;

namespace Silk.Core.Commands.Bot
{
    public class ShardsCommand : BaseCommandModule
    {
        private const string embedTitle = "`|\t[Shard]\t|\t[Ping]\t|\t[Guilds]\t|\t[Members]\t|`";
        
        [Command]
        public async Task Shards(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder();
            builder.WithReply(ctx.Message.Id);

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Gold)
                .WithTitle("Shard info:");
            var sb = new StringBuilder();

            var shards = Core.Bot.Instance!.Client.ShardClients;
            var totalPing = 0;
            foreach ((int id, var shard) in shards)
            {
                int members = shard.Guilds.SelectMany(g => g.Value.Members).Count();
                sb.Append("`|");
                sb.Append($" {id + 1}"            .Center("\t[Shard]\t"));
                sb.Append('|');
                sb.Append($"{shard.Ping}ms"       .Center("\t[Ping]\t"));
                sb.Append('|');
                sb.Append($"{shard.Guilds.Count}" .Center("\t[Guilds]\t"));
                sb.Append('|');
                sb.Append($" {members}"           .Center("\t[Members]\t"));
                sb.Append("|`");
                sb.AppendLine();
                
                totalPing += shard.Ping;
            }
            embed.WithDescription($"**{embedTitle}** \n{sb}");
            //embed.AddField()
            sb.Clear();
            
            sb.Append("`|");
            sb.Append($" {ctx.Client.ShardId + 1}"  .Center("\t[Shard]\t"));
            sb.Append('|');
            sb.Append($"{totalPing / shards.Count}"       .Center("\t[Ping]\t"));
            sb.Append('|');
            sb.Append($"{Core.Bot.Instance!.Client.ShardClients.SelectMany(c => c.Value.Guilds.Keys).Count()}".Center("\t[Guilds]\t"));
            sb.Append('|');
            sb.Append($" {Core.Bot.Instance!.Client.ShardClients.Values.SelectMany(g => g.Guilds.Values).SelectMany(g => g.Members.Keys).Count()}".Center("\t[Members]\t"));
            sb.Append("|`");
            embed.AddField($"{Formatter.Bold("T o t a l:")}", sb.ToString());

            embed.WithFooter($"You are shard {ctx.Client.ShardId + 1}!");
            builder.WithEmbed(embed);
            await ctx.RespondAsync(builder);
        }
        
    }
}