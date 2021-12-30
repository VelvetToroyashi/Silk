using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Commands.Conditions;
using Silk.Services.Interfaces;
using Silk.Utilities.HelpFormatter;

namespace Silk.Commands.Moderation
{
    
    [HelpCategory(Categories.Mod)]
    public class BanCommand : CommandGroup
    {
        private readonly ICommandContext        _context;
        private readonly IDiscordRestUserAPI    _users;
        private readonly IDiscordRestChannelAPI _channels;
        private readonly IInfractionService     _infractions;
        
        public BanCommand
        (
            ICommandContext context,
            IDiscordRestUserAPI users,
            IDiscordRestChannelAPI channels,
            IInfractionService infractions
        )
        {
            _context     = context;
            _users       = users;
            _channels    = channels;
            _infractions = infractions;
        }

        [Command("ban")]
        [RequireContext(ChannelContext.Guild)]
        [RequireDiscordPermission(DiscordPermission.BanMembers)]
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
            string reason = "Not Given."
        )
        {
            var infractionResult = await _infractions.BanAsync(_context.GuildID.Value, user.ID, _context.User.ID, days, reason, banDuration);

            return infractionResult.IsSuccess
                ? await _channels.CreateMessageAsync(_context.ChannelID, $"Successfully banned <@{user.ID}>! " + (infractionResult.Entity.UserNotified ? "(User notified with DM)" : "(Failed to DM user)"))
                : await _channels.CreateMessageAsync(_context.ChannelID, infractionResult.Error.Message);
        }
    }
}
