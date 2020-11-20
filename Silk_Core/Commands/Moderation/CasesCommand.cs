using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using SilkBot.Commands.Moderation.Utilities;
using SilkBot.Extensions;
using SilkBot.Models;
using SilkBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using DSharpPlus.CommandsNext.Attributes;

namespace SilkBot.Commands.Moderation
{
    [UsedImplicitly]
    public class CasesCommand : BaseCommandModule
    {

        private readonly InfractionService _infractionService;

        public CasesCommand(InfractionService infractionService)
        {
            _infractionService = infractionService;
        }

        [Command]
        public async Task Cases(CommandContext ctx, DiscordUser user)
        {
            IEnumerable<UserInfractionModel> infractions = _infractionService.GetInfractions(user.Id);

            if (infractions?.Count() is 0 or null)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Cases for {user.Username} ({user.Id})")
                    .WithColor(DiscordColor.PhthaloGreen)
                    .WithDescription("User has no infractions!")
                    .WithThumbnail(user.AvatarUrl);
                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                IEnumerable<IGrouping<InfractionType, InfractionType>> groupedInfractions = infractions.GroupBy(i => i.InfractionType, i => i.InfractionType);
                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Cases for {user.Username} ({user.Id})")
                    .WithColor(DiscordColor.Gold)
                    .WithThumbnail(user.AvatarUrl)
                    .WithDescription(infractions
                            .Take(3)
                            .Select(i => 
                            $"Infraction Occured: {i.InfractionTime:dd/mm/yyyy h:mm} {DateTime.Now.Subtract(i.InfractionTime).Humanize(3, minUnit: TimeUnit.Minute)} ago\n" +
                            $"Enforcer: <@{i.Enforcer}> ({i.Enforcer})\n" +
                            $"Type: {i.InfractionType.Humanize()}\n" +
                            $"Reason: {i.Reason}\n")
                            .JoinString('\n'));
                foreach(var inf in groupedInfractions)
                {
                    switch (inf.Key)
                    {
                        case InfractionType.Kick:
                            embed.AddField("Kicks:", inf.Count().ToString());
                            break;
                        case InfractionType.Mute:
                            embed.AddField("Mutes", inf.Count().ToString());
                            break;
                    }
                }
                await ctx.RespondAsync(embed: embed);
            }
        }
    }
}
