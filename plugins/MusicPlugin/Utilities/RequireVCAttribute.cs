using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using YumeChan.PluginBase.Infrastructure;

namespace MusicPlugin.Utilities
{
	public sealed class RequireVCAttribute : PluginCheckBaseAttribute
	{
		public override string ErrorMessage { get; protected set; }
		
		public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
		{
			if (help) return help;
			
			if (ctx.Member.VoiceState?.Channel is null)
			{
				ErrorMessage = "You need to be in a voice channel to use this!";
				return false;
			}
			
			return true;
		}
	}

	public sealed class RequireSameVCAttribute : PluginCheckBaseAttribute
	{
		public override string ErrorMessage { get; protected set; }
		private readonly bool _requireBot;

		public RequireSameVCAttribute(bool requireBot = false) => _requireBot = requireBot;
	
		public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
		{
			if (help) return help;

			if (_requireBot && ctx.Guild.CurrentMember.VoiceState?.Channel is null)
			{
				ErrorMessage = "I need to be in VC for this!";
				return false;
			}
			
			if (ctx.Member.VoiceState?.Channel != ctx.Guild.CurrentMember.VoiceState.Channel)
			{
				ErrorMessage = "We need to be in the same VC!";
				return false;
			}
			
			return true;
		}
	}
}