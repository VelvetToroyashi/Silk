using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using SilkBot.Database;
using SilkBot.Database.Models;
using SilkBot.Economy.Shop.Items;

namespace SilkBot.Commands.Tests
{
    public class AddCommand : BaseCommandModule
    {
        private readonly IDbContextFactory<SilkDbContext> _dbFactory;
        public AddCommand(IDbContextFactory<SilkDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        [Command]
        public async Task Add(CommandContext ctx, string item)
        {
            using SilkDbContext db = _dbFactory.CreateDbContext();

            var sItem = JsonConvert.SerializeObject(new Potion {Name = "Potion", Description = "Literally a potion"});
            
            GlobalUserModel? user = db.GlobalUsers.FirstOrDefault(u => u.Id == ctx.User.Id);
            if (user is null) 
            {
                await ctx.RespondAsync("Who are you :kekw:").ConfigureAwait(false);
                return;
            };

            if (Enum.GetNames<Items>().Contains(item))
                user.Items.Add(new ItemModel {InstanceState = sItem});
            else return;
            int savedItems = await db.SaveChangesAsync().ConfigureAwait(false);
            if (savedItems is 0) Log.Logger.Warning("Attempted to save 1 item, saved 0");
        }

        [Command]
        public async Task Inventory(CommandContext ctx)
        {
            using SilkDbContext db = _dbFactory.CreateDbContext();
            var item = JsonConvert.DeserializeObject<Potion>(db.GlobalUsers.Include(u => u.Items).Single(u => u.Id == ctx.User.Id).Items.FirstOrDefault()?.InstanceState);
            await ctx.RespondAsync($"You have a {item.Name}!").ConfigureAwait(false);
        }
        
    }
}