using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Core.Services.Interfaces;
using Silk.Core.Types;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Moderation
{
	[Category(Categories.Mod)]
	public class BanCommand : BaseCommandModule
	{
		private readonly IInfractionService _infractions;
		public BanCommand(IInfractionService infractions) => _infractions = infractions;

		[Command]
		[RequireGuild]
		[RequireBotPermissions(Permissions.BanMembers)]
		[RequireUserPermissions(Permissions.BanMembers)]
		[Description("Permanently ban someone from the server!")]
		public async Task BanAsync(CommandContext ctx, DiscordUser user, [RemainingText] string reason = "Not Given.")
		{
			if (user == ctx.Guild.CurrentMember)
			{
				await ctx.RespondAsync("Surely you didn't wanna get rid of me...Right?..Right?");
				return;
			}

			if (user == ctx.Member)
			{
				await ctx.RespondAsync("Are you *really* deserving of a ban, though?");
				return;
			}

			var result = await _infractions.BanAsync(user.Id, ctx.Guild.Id, ctx.User.Id, reason);

			var message = result switch
			{
				InfractionResult.SucceededWithNotification => $"Banned **{user.ToDiscordName()}** (User notified with Direct Message)",
				InfractionResult.SucceededWithoutNotification => $"Banned **{user.ToDiscordName()}**! (Failed to DM.)",
				InfractionResult.FailedGuildHeirarchy => "I can't ban that person due to heirarchy.",
				InfractionResult.FailedSelfPermissions => "I don't know how you managed to do this, but I don't have permission to ban that person!"
			};

			await ctx.RespondAsync(message);
		}
		
		
		[Command]
		[RequireGuild]
		[RequireBotPermissions(Permissions.BanMembers)]
		[RequireUserPermissions(Permissions.BanMembers)]
		[Description("Permanently ban someone from the server!")]
		public async Task TempBanAsync(CommandContext ctx, DiscordUser user, TimeSpan duration, [RemainingText] string reason = "Not Given.")
		{
			if (user == ctx.Guild.CurrentMember)
			{
				await ctx.RespondAsync("Surely you didn't wanna get rid of me...Right?..Right?");
				return;
			}

			if (user == ctx.Member)
			{
				await ctx.RespondAsync("Are you *really* deserving of a ban, though?");
				return;
			}

			var result = await _infractions.BanAsync(user.Id, ctx.Guild.Id, ctx.User.Id, reason, DateTime.UtcNow + duration);

			var message = result switch
			{
				InfractionResult.SucceededWithNotification => $"Banned **{user.ToDiscordName()}** (User notified with Direct Message)",
				InfractionResult.SucceededWithoutNotification => $"Banned **{user.ToDiscordName()}**! (Failed to DM.)",
				InfractionResult.FailedGuildHeirarchy => "I can't ban that person due to heirarchy.",
				InfractionResult.FailedSelfPermissions => "I don't know how you managed to do this, but I don't have permission to ban that person!"
			};

			await ctx.RespondAsync(message);
		}
	}
}