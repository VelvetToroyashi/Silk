using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using YumeChan.PluginBase.Infrastructure;

namespace MusicPlugin
{
	public class VCCheckAttribute : PluginCheckBaseAttribute
	{
		public override string ErrorMessage { get; protected set; }
		private readonly bool _requireBot;

		public VCCheckAttribute(bool RequiresBotInVC = false) => _requireBot = RequiresBotInVC;
		
		public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
		{
			if (ctx.Member.VoiceState is null)
				ErrorMessage = "You must be in a VC to use this!";

			if (_requireBot)
				if (ctx.Guild.CurrentMember.VoiceState is null)
					ErrorMessage = "I need to be in a VC first!";

			if (_requireBot && ctx.Member.VoiceState?.Channel != ctx.Guild.CurrentMember.VoiceState?.Channel)
				ErrorMessage = "We need to both be in the same channel for this!";

			return ErrorMessage is null;
		}
		
	}
}