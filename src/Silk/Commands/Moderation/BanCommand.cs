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
        [Description("Permanently ban someone from the server!")]
        [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
        public async Task<Result<IMessage>> BanAsync
        (
            [Description("The user to ban.")]
            IUser user, 
            
            [Option('d', "days")]
            [MinValue(0d), MaxValue(7d)]
            [Description("The number days to clear messages from. Range: 0-7")]
            int days = 0, 
            
            [Greedy]
            [Description("The reason for the ban.")]
            string reason = "Not Given."
        )
        {
            var self = await _users.GetCurrentUserAsync();

            if (!self.IsSuccess)
                return Result<IMessage>.FromError(self);
            
            if (user.ID == self.Entity.ID)
                return await _channels.CreateMessageAsync(_context.ChannelID, "As much as I'm happy to serve, I can't ban myself!");

            if (user.ID == _context.User.ID)
                return await _channels.CreateMessageAsync(_context.ChannelID,"The ban hammer must swing upon someone else!");

            var infractionResult = await _infractions.BanAsync(_context.GuildID.Value, user.ID, _context.User.ID, days, reason);

            return infractionResult.IsSuccess
                ? await _channels.CreateMessageAsync(_context.ChannelID, $"Successfully banned <@{user.ID}>! " + (infractionResult.Entity.UserNotified ? "(User notified with DM)" : "(Failed to DM user)"))
                : await _channels.CreateMessageAsync(_context.ChannelID, infractionResult.Error.Message);
        }


        //[Command("ban")]
        [RequireContext(ChannelContext.Guild)]
        [RequireDiscordPermission(DiscordPermission.BanMembers)]
        [Description("Temporarily ban someone from the server!")]
        [SuppressMessage("ReSharper", "RedundantBlankLines", Justification = "Readability")]
        public async Task<Result<IMessage>> TempBanAsync(
            [Description("The user to ban.")]
            IUser user,
            
            [Description("The duration of the ban.")]
            TimeSpan duration,
            
            [Option('d', "days")]
            [Description("The number days to clear messages from. Range: 0-7")]
            int deleteDays = 0,
            
            [Greedy] 
            [Description("The reason for the ban.")]
            string reason = "Not Given.")
        {
            var self = await _users.GetCurrentUserAsync();

            if (!self.IsSuccess)
                return Result<IMessage>.FromError(self);
            
            if (user.ID == self.Entity.ID)
                return await _channels.CreateMessageAsync(_context.ChannelID, "As much as I'm happy to serve, I can't ban myself!");

            if (user.ID == _context.User.ID)
                return await _channels.CreateMessageAsync(_context.ChannelID,"The ban hammer must swing upon someone else!");

            var infractionResult = await _infractions.BanAsync(_context.GuildID.Value, user.ID, _context.User.ID, deleteDays, reason, duration);

            return infractionResult.IsSuccess
                ? await _channels.CreateMessageAsync(_context.ChannelID, $"Successfully banned <@{user.ID}>! " + (infractionResult.Entity.UserNotified ? "(User notified with DM)" : "(Failed to DM user)"))
                : await _channels.CreateMessageAsync(_context.ChannelID, infractionResult.Error.Message);
        }
    }
}
