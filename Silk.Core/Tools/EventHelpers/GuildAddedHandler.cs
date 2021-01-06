using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Silk.Core.Database;
using Silk.Core.Database.Models;
using Silk.Extensions;

namespace Silk.Core.Tools.EventHelpers
{
    public class GuildAddedHandler
    {
        private int currentGuild;
        private bool startupCacheCompleted;
        
        
        private readonly ILogger<GuildAddedHandler> _logger;
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        
        private readonly List<DiscordGuild> cacheQueue = new();

        public GuildAddedHandler(ILogger<GuildAddedHandler> logger, IDbContextFactory<SilkDbContext> dbFactory) => (_logger, _dbFactory) = (logger, dbFactory);

        // Run on startup to cache all members //
        public Task OnGuildAvailable(DiscordClient c, GuildCreateEventArgs e)
        {
            if (startupCacheCompleted) return Task.CompletedTask; // Prevent double logging when joining a new guild //
            _logger.LogDebug($"Beginning Cache. Shard [{c.ShardId + 1}/{c.ShardCount}] | Guild [{++currentGuild}/{c.Guilds.Count}]");
            cacheQueue.Add(e.Guild);
            startupCacheCompleted = currentGuild == c.Guilds.Count;
            return Task.CompletedTask;
        }

        public Task OnGuildDownloadComplete(DiscordClient c, GuildDownloadCompletedEventArgs e)
        {
            Task.Run(async () =>
            {
                for (int i = 0; i < cacheQueue.Count; i++)
                {
                    await DoCacheAsync(cacheQueue[i]);
                }
                    
            });
            return Task.CompletedTask;
        }
        
        // Run when Silk! joins a new guild // 
        public  Task OnGuildJoin(DiscordClient c, GuildCreateEventArgs e) =>
            Task.Run(async () =>
            {
                await DoCacheAsync(e.Guild);
                await SendWelcomeMessage(c, e);
            });

        private async Task DoCacheAsync(DiscordGuild e)
        {
            SilkDbContext db = _dbFactory.CreateDbContext();
            GuildModel guild = await GetOrCreateGuildAsync(db, e.Id);
            CacheStaffMembers(guild, e.Members.Values); 
            await db.SaveChangesAsync();
        }

        // Used in conjunction with OnGuildJoin() //
        private async Task SendWelcomeMessage(DiscordClient c, GuildCreateEventArgs e)
        {
            IOrderedEnumerable<DiscordChannel> allChannels =
                (await e.Guild.GetChannelsAsync()).OrderBy(channel => channel.Position);
            DiscordMember botAsMember = await e.Guild.GetMemberAsync(c.CurrentUser.Id);

            DiscordChannel firstChannel = allChannels.First(channel =>
                channel.PermissionsFor(botAsMember).HasPermission(Permissions.SendMessages) &&
                channel.Type == ChannelType.Text);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                        .WithTitle("Thank you for adding me!")
                                        .WithColor(new DiscordColor("94f8ff"))
                                        .WithThumbnail(c.CurrentUser.AvatarUrl)
                                        .WithFooter("Did I break? DM me ticket create [message] and I'll forward it to the owners <3");

            var sb = new StringBuilder();
            sb.Append("Thank you for choosing Silk! to join your server <3")
              .AppendLine("I am a relatively lightweight bot with many functions - partially in moderation, ")
              .AppendLine("partially in games, with many more features to come!")
              .Append("If there's an issue, feel free to [Open an issue on GitHub](https://github.com/VelvetThePanda/Silk/issues), ")
              .AppendLine("or if you're not familiar with GitHub, feel free")
              .AppendLine($"to message the developers directly via {Bot.DefaultCommandPrefix}`ticket create <your message>`.")
              .Append($"By default, the prefix is `{Bot.DefaultCommandPrefix}`, or <@{c.CurrentUser.Id}>, but this can be changed by !setprefix <your prefix here>.");

            embed.WithDescription(sb.ToString());

            await firstChannel.SendMessageAsync(embed: embed);
        }

        private async Task<GuildModel> GetOrCreateGuildAsync(SilkDbContext db, ulong Id)
        {
            GuildModel? guild = await db.Guilds.Include(g => g.Users).FirstOrDefaultAsync(g => g.Id == Id);
            if (guild is null)
            {
                guild = new();
                guild.Id = Id;
                guild.Prefix = Bot.DefaultCommandPrefix;
                db.Guilds.Add(guild);
            }
            return guild;
        }
        
        
        private void CacheStaffMembers(GuildModel guild, IEnumerable<DiscordMember> members)
        {
            
            IEnumerable<DiscordMember> staff = members.Where(m => 
                   ((m.HasPermission(Permissions.KickMembers | Permissions.ManageMessages) 
                || m.HasPermission(Permissions.Administrator) 
                || m.IsOwner) && !m.IsBot));
            _logger.LogInformation($"{staff.Count()}/{members.Count()} members marked as staff.");
            
            foreach (DiscordMember member in staff)
            {
                var flags = UserFlag.Staff;
                if (member.HasPermission(Permissions.Administrator) || member.IsOwner) flags.Add(UserFlag.EscalatedStaff);

                UserModel? user = guild.Users.FirstOrDefault(u => u.Id == member.Id);
                if (user is not null) //If user exists
                {
                    if (!user.Flags.Has(UserFlag.Staff)) // Has flag
                        user.Flags.Add(UserFlag.Staff); // Add flag
                    if (member.HasPermission(Permissions.Administrator) || member.IsOwner)
                        user.Flags.Add(UserFlag.EscalatedStaff);
                }
                else guild.Users.Add(new UserModel {Id = member.Id, Flags = flags});
            }
        }
    }
}