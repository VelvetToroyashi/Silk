using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Silk_Items.Entities;
using Silk_Items.Tools;
using SilkBot.Database.Models;
using SilkBot.Extensions;

namespace SilkBot.Commands.Tests
{

    public abstract class Base : BaseCommandModule { }

    [Group]
    public class T : Base
    {
        [GroupCommand]
        public async Task A(CommandContext ctx) => ctx.RespondAsync("A Works from class T");
        [Command]
        public async Task B(CommandContext ctx) => ctx.RespondAsync("B Works from class T");
    }

    public class Tc : T
    {
        [Command]
        public async Task C(CommandContext ctx) => ctx.RespondAsync("C works from Tc");
    }
    
    
    
    
    [Group]
    public class Test : BaseCommandModule
    {
        private readonly Sword _sword = new();

        private readonly IDbContextFactory<SilkDbContext> _dbFactory;

        public Test(IDbContextFactory<SilkDbContext> dbFactory) => _dbFactory = dbFactory;
        
        [Command]
        public async Task GetSword(CommandContext ctx) => 
            await ctx.RespondAsync($"Sword has: {_sword.Count()} components; " +
                                   $"`{_sword.Select(c => c.GetType().Name).JoinString(", ")}`");

        [Command]
        public async Task SaveSword(CommandContext ctx)
        {
            using var db = _dbFactory.CreateDbContext();

            string swordJson = ItemDatabaseHelper.Serialize(_sword);
            GlobalUserModel? user = await db.GlobalUsers.FirstOrDefaultAsync(u => u.Id == ctx.User.Id);
            if (user is null) user = new() {Id = ctx.User.Id};
            user.Items.Add(new(){Owner = user, State = swordJson});
            bool success = (await db.SaveChangesAsync()) is 0;

            Task<DiscordMessage> m = success
                ? ctx.RespondAsync("Oh no! Something went wrong while trying to save that item. :(")
                : ctx.RespondAsync("Save successful.");
            await m;
        }
        
        [Command]
        public async Task AddComponent(CommandContext ctx, string component = "")
        {
            
        }
        
    }
}