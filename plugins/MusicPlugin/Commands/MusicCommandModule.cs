using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MusicPlugin.Services;

namespace MusicPlugin.Commands
{
	[RequireGuild]
	public sealed class MusicCommandModule : BaseCommandModule
	{
		private readonly MusicVoiceService _voice;
		public MusicCommandModule(MusicVoiceService voice)
		{
			_voice = voice;
		}
		
		[Command]
		public async Task Play(CommandContext ctx, string url)
		{
			if (ctx.Member.VoiceState is null)
			{
				await ctx.RespondAsync("You need to be in a voice channel!");
				return;
			}

			await _voice.JoinAsync(ctx.Member.VoiceState.Channel, ctx.Channel);

			await ctx.RespondAsync($"Connected to {ctx.Member.VoiceState.Channel.Mention} and bound to {ctx.Channel.Mention}!");
		}

		[Command]
		public async Task Leave(CommandContext ctx)
		{
			if (ctx.Guild.CurrentMember.VoiceState is null)
			{
				await ctx.RespondAsync("I'm not even in a channel!");
				return;
			}

			if (!await _voice.LeaveAsync(ctx.Guild.CurrentMember.VoiceState.Channel))
				await ctx.RespondAsync("Something went wrong while leaving the channel.");
		}


		private async Task<bool> ValidateLeaveRequirementsAsync(CommandContext ctx)
		{
			if (ctx.Member.VoiceState is null)
			{
				await ctx.RespondAsync("You're not even in a channel, silly!");
				return false;
			}


			return true;
		}
	}
}