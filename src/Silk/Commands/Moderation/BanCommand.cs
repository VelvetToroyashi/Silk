using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;
using Silk.Services.Interfaces;
using Silk.Utilities.HelpFormatter;
using Silk.Extensions.DSharpPlus;

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
        public async Task<Result<IMessage>> BanAsync(IUser user, [Greedy] string reason = "Not Given.")
        {
            var self = await _users.GetCurrentUserAsync();

            if (!self.IsSuccess)
                return Result<IMessage>.FromError(self);
            
            if (user.ID == self.Entity.ID)
                return await _channels.CreateMessageAsync(_context.ChannelID, "Sadly the ban hammer must come down upon someone else!");

            if (user.ID == _context.User.ID)
                return await _channels.CreateMessageAsync(_context.ChannelID,"The ban hammer must swing upon someone else!");

            var infractionResult = await _infractions.BanAsync(_context.GuildID.Value, user.ID, _context.User.ID, 0, reason);

            return infractionResult.IsSuccess
                ? await _channels.CreateMessageAsync(_context.ChannelID, $"Successfully banned <@{user.ID}>{(infractionResult.Entity.UserNotified ? "(User notified with DM)" : null)}!")
                : await _channels.CreateMessageAsync(_context.ChannelID, infractionResult.Error.Message);
        }


        //[Command("ban")]
        [RequireContext(ChannelContext.Guild)]
        [RequireDiscordPermission(DiscordPermission.BanMembers)]
        [Description("Temporarily ban someone from the server!")]
        public async Task<Result<IMessage>> TempBanAsync(CommandContext ctx, IUser user, TimeSpan duration, [Greedy] string reason = "Not Given.")
        {
            /*if (user == ctx.Guild.CurrentMember)
            {
                await ctx.RespondAsync("Surely you didn't wanna get rid of me...Right?..Right?");
                return;
            }

            if (user == ctx.Member)
            {
                await ctx.RespondAsync("Are you *really* deserving of a ban, though?");
                return;
            }

            InfractionResult result = await _infractions.BanAsync(user.Id, ctx.Guild.Id, ctx.User.Id, reason, DateTime.UtcNow + duration);

            string? message = result switch
            {
                InfractionResult.SucceededWithNotification    => $"Banned **{user.ToDiscordName()}** (User notified with Direct Message)",
                InfractionResult.SucceededWithoutNotification => $"Banned **{user.ToDiscordName()}**! (Failed to DM.)",
                InfractionResult.FailedGuildHeirarchy         => "I can't ban that person due to hierarchy.",
                InfractionResult.FailedSelfPermissions        => "I don't know how you managed to do this, but I don't have permission to ban that person!"
            };

            await ctx.RespondAsync(message);*/
            return default;
        }
    }
}
