using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using SilkBot.Commands.Economy;
using System;

namespace SilkBot.Economy
{
    [Serializable]
    public sealed class DiscordEconomicUser
    {
        [JsonProperty]
        public ulong UserId { get; set; }
        [JsonProperty]
        public uint Cash { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public bool IsUserNameOverrided { get; set; }

        [JsonProperty]
        public DateTime LastCashInTime { get => lastDailyCashIn; set => lastDailyCashIn = value; }

        [JsonProperty]
        public DateTime NameChangeTimestamp { get => nameChangedTimestamp; set => nameChangedTimestamp = value; }

        private DateTime lastDailyCashIn;
        
        private DateTime nameChangedTimestamp;
        
        private readonly TimeSpan dailyCooldown = TimeSpan.FromDays(1);
        
        

        public DiscordEconomicUser(ulong UserId, string Name) 
        {
            this.UserId = UserId;
            this.Name = Name;
        }

        public void Widthdraw(uint amount)
        {
            if (amount > Cash)
                throw new InsufficientFundsException("You do not have enough cash to widthdraw.");
            else Cash -= amount;
        }




        public DiscordEmbed DoDaily(CommandContext ctx)
        {
            if(DateTime.Now - LastCashInTime > dailyCooldown)
            {
                lastDailyCashIn = DateTime.Now;
                var returnEmbed = EmbedHelper.CreateEmbed(ctx, "Daily reward:", "You've claimed your 200 dollars, come back tomorrow for more!", DiscordColor.SpringGreen);
                Cash += 200;
                return new DiscordEmbedBuilder(returnEmbed).WithAuthor(ctx.User.Username, iconUrl: ctx.Member.AvatarUrl);
            }
            else
            {
                var lastCash = lastDailyCashIn + dailyCooldown;
                var timeToReturn = DateTime.Now - lastCash;



                var returnEmbed = EmbedHelper.CreateEmbed(ctx, "Daily reward:", $"come back in {-(int)timeToReturn.TotalHours} hours!", DiscordColor.IndianRed);
                return new DiscordEmbedBuilder(returnEmbed).WithAuthor(ctx.User.Username, iconUrl: ctx.Member.AvatarUrl);
            }

        }

    }
}
