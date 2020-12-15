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
using SilkBot.Database.Models;
using SilkBot.Extensions;
using SilkBot.Models;
using SilkBot.Services;

namespace SilkBot.Tools.EventHelpers
{
    public class GuildHelper
    {
        private readonly ILogger<GuildHelper> _logger;
        private readonly PrefixCacheService _prefixService;
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        private bool startupCacheCompleted;
        private volatile int currentGuild;
        
        public GuildHelper(ILogger<GuildHelper> logger, PrefixCacheService prefixService, IDbContextFactory<SilkDbContext> dbFactory)
        {
            _logger = logger;
            _prefixService = prefixService;
            _dbFactory = dbFactory;
        }

        // Run on startup to cache all members //
        public async Task OnGuildAvailable(DiscordClient c, GuildCreateEventArgs e)
        {
            if (startupCacheCompleted) return;
            _ = Task.Run(async () =>
            {
                _logger.LogDebug($"Beginning Cache Shard [{c.ShardId}/{c.ShardCount}] | Guild {++currentGuild}/");
                await using SilkDbContext db = _dbFactory.CreateDbContext();
                GuildModel guild = await GetOrCreateGuildAsync(db, e.Guild.Id);
                CacheStaffMembers(guild, e.Guild.Members.Values); 
                await db.SaveChangesAsync();
            });
        }
        
        // Run when Silk! joins a new guild // 
        public async Task OnGuildJoin(DiscordClient c, GuildCreateEventArgs e)
        {
            _ = Task.Run(async () =>
            {
                
                await using SilkDbContext db = _dbFactory.CreateDbContext();
                await SendWelcomeMessage(c, e);
                GuildModel guild = await GetOrCreateGuildAsync(db, e.Guild.Id);
                CacheStaffMembers(guild, e.Guild.Members.Values);
                await db.SaveChangesAsync();
                //_prefixService.UpdatePrefix(e.Guild.Id, Bot.DefaultCommandPrefix);
                // I don't think this should be needed, assuming I'm not totally inept. ~Velvet. //
            });
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
                                        .WithFooter("Did I break? DM me ticket create [message] and I'll forward it to the owners <3")
                                        .WithTimestamp(DateTime.Now);

            var sb = new StringBuilder();
            sb.Append("Thank you for choosing Silk! to join your server <3")
              .AppendLine("I am a relatively lightweight bot with many functions - partially in moderation, ")
              .AppendLine("partially in games, with many more features to come!")
              .Append("If there's an issue, feel free to [Open an issue on GitHub](https://github.com/VelvetThePanda/Silkbot/issues), ")
              .AppendLine("or if you're not familiar with GitHub, feel free")
              .AppendLine($"to message the developers directly via {Bot.DefaultCommandPrefix}`ticket create <your message>`.")
              .Append($"By default, the prefix is `{Bot.DefaultCommandPrefix}`, or <@{c.CurrentUser.Id}>, but this can be changed by !setprefix <your prefix here>.");

            embed.WithDescription(sb.ToString());

            await firstChannel.SendMessageAsync(embed: embed);
        }

        private async Task<GuildModel> GetOrCreateGuildAsync(SilkDbContext db, ulong Id)
        {
            GuildModel? guild = db.Guilds.FirstOrDefault(g => g.Id == Id);
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
            _logger.LogDebug($"Captured {staff.Count()} members marked with staff flag.");
            
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