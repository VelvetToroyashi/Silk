using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Commands.Conditions;
using Silk.Extensions.Remora;
using Silk.Services.Interfaces;
using Silk.Shared.Constants;
using Silk.Utilities;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.Moderation
{
    
    [Category(Categories.Mod)]
    public class BanCommand : CommandGroup
    {
        private readonly ICommandContext        _context;
        private readonly IDiscordRestChannelAPI _channels;
        private readonly IInfractionService     _infractions;
        
        public BanCommand
        (
            ICommandContext        context,
            IDiscordRestChannelAPI channels,
            IInfractionService     infractions
        )
        {
            _context     = context;
            _channels    = channels;
            _infractions = infractions;
        }

        [Command("ban", "403", "301")]
        [RequireContext(ChannelContext.Guild)]
        [RequireDiscordPermission(DiscordPermission.BanMembers)]
        [RequireBotDiscordPermissions(DiscordPermission.BanMembers)]
        [Description("Permanently or temporarily ban someone from the server!")]
        [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
        public async Task<Result<IMessage>> BanAsync
        (
            [NonSelfActionable]
            [Description("The user to ban.")]
            IUser user, 
            
            [Option('f', "for")]
            [Description("How long to ban the user for.")]
            TimeSpan? banDuration = null,
            
            [Option('d', "days")]
            [Description("The number days to clear messages from. Range: 0-7")]
            int days = 0, 
            
            [Greedy]
            [Description("The reason for the ban.")]
            string reason = "Not Given.",
        
            [Switch('s', "silent")]
            [Description("Whether to send a message to the user.")]
            bool silent = false
        )
        {
            var infractionResult = await _infractions.BanAsync(_context.GetGuildID(), user.ID, _context.GetUserID(), days, reason, banDuration, notify: !silent);
            var notified         = silent ? "(Skipped notification)" : infractionResult.Entity.Notified ? "(User notified with DM)" : "(Failed to DM)";
            
            return await _channels.CreateMessageAsync
                (
                 _context.GetChannelID(),
                 !infractionResult.IsSuccess
                     ? infractionResult.Error.Message
                     : $"{Emojis.BanEmoji} Banned **{user.ToDiscordTag()}**! {notified}"
                );
        }
    }
}
