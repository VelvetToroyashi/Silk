using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using SilkBot.Database.Models;
using SilkBot.Economy.Shop.Items;

namespace SilkBot.Commands.Tests
{
    public class AddCommand : BaseCommandModule
    {
        private readonly IDbContextFactory<SilkDbContext> _dbContext;
        public AddCommand(IDbContextFactory<SilkDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        [Command]
        public async Task Add(CommandContext ctx, string item)
        {
            using SilkDbContext db = _dbContext.CreateDbContext();

            var sItem = JsonConvert.SerializeObject(new Potion {Name = "Potion", Description = "Literally a potion"});
            
            GlobalUserModel user = db.GlobalUsers.FirstOrDefault(u => u.Id == ctx.User.Id);
            if (user is null) 
            {
                await ctx.RespondAsync("Who are you :kekw:").ConfigureAwait(false);
                return;
            } ;

            
            if (Enum.GetNames<Items>().Contains(item))
                user.Items.Add(new ItemModel { Id = 1, InstanceState = sItem, Owner = user });
            int savedItems = await db.SaveChangesAsync().ConfigureAwait(false);
            if (savedItems is < 1) Log.Logger.Warning("Attempted to save 1 item, saved 0");
        }
        
    }
}