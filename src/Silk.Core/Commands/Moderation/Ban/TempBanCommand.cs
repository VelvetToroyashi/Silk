using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Commands.Moderation.Utilities;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Data;
using Silk.Data.Models;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;
using InfractionType = Silk.Data.Models.InfractionType;

namespace Silk.Core.Commands.Moderation.Ban
{
    [Category(Categories.Mod)]
    public class TempBanCommand : BaseCommandModule
    {
        private const string defaultFormat = "$mention was $action from the guild for {d} for {reason}";

        [Hidden]
        [RequireGuild]
        [Command("tempban")]
        [Description("Temporarily ban a member from the Guild")]
        public async Task TempBan(CommandContext ctx, DiscordMember user, TimeSpan duration, [RemainingText] string reason = "Not provided.")
        {
           
        }
    }
}