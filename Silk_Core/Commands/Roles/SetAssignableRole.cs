using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using SilkBot.Exceptions;
using SilkBot.Extensions;
using SilkBot.Models;
using SilkBot.Utilities;

namespace SilkBot.Commands.Roles
{
    [Category(Categories.Roles)]
    public class SetAssignableRole : BaseCommandModule
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public SetAssignableRole(IDbContextFactory<SilkDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        [Command("Assign")]
        [Aliases("sar", "selfassignablerole", "selfrole")]
        [HelpDescription(
            "Allows you to set self assignable roles. Role menu coming soon:tm:. All Self-Assignable Roles are opt-*in*.")]
        public async Task SetSelfAssignableRole(CommandContext ctx, params DiscordRole[] roles)
        {
            using SilkDbContext db = _dbFactory.CreateDbContext();

            GuildModel guild = await db.Guilds.Include(g => g.SelfAssignableRoles).FirstAsync(g => g.Id == ctx.Guild.Id);

            IEnumerable<ulong> currentlyAssignableRoles = guild.SelfAssignableRoles.Select(r => r.RoleId);
            
            List<string> added = new();
            List<string> removed = new();

            foreach (var role in roles)
            {
                if (currentlyAssignableRoles.Any(r => r == role.Id))
                {
                    removed.Add(role.Mention);
                    var r = guild.SelfAssignableRoles.Single(ro => ro.RoleId == role.Id);
                    guild.SelfAssignableRoles.Remove(r);
                    //await db.SaveChangesAsync();
                }
                else
                {
                    added.Add(role.Mention);
                    var r = new SelfAssignableRole {RoleId = role.Id};
                    guild.SelfAssignableRoles.Add(r);
                    //await db.SaveChangesAsync();
                }
            }
            await db.SaveChangesAsync();
   
            string addedString = string.IsNullOrWhiteSpace(added.Join('\n')) ? "none" :  added.Join('\n');
            string removedString = string.IsNullOrWhiteSpace(removed.Join('\n')) ? "none" : removed.Join('\n');

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                        .WithColor(GetEmbedColor(roles))
                                        .WithTitle("Self-Assignable roles")
                                        .AddField("Added:", addedString, true)
                                        .AddField("Removed:", removedString, true);
        }

        private static DiscordColor GetEmbedColor(DiscordRole[] roles) =>
            roles.Length is 1 ? roles[0].Color : DiscordColor.Gold;
    }
}