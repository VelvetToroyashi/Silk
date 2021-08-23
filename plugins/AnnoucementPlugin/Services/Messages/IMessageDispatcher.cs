using System.Threading.Tasks;
using AnnoucementPlugin.Utilities;
using DSharpPlus;

namespace AnnoucementPlugin.Services
{
	/// <summary>
	/// An abstraction for a <see cref="DiscordClient"/> or <see cref="DiscordShardedClient"/> for outbound messages.
	/// </summary>
	public interface IMessageDispatcher
	{
		/// <summary>
		/// Dispatches a message to a channel based on the given parameters.
		/// </summary>
		/// <param name="guild">The id of the guild to send a message to.</param>
		/// <param name="channel">The id of the channel to send a message to.</param>
		/// <param name="message">The message to send.</param>
		public Task<MessageSendResult> DispatchMessage(ulong guild, ulong channel, string message);
	}
}