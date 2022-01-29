using System.Threading.Tasks;

namespace AnnoucementPlugin.Services
{
	/// <summary>
	///     A service for grabbing user confirmation via buttons.
	/// </summary>
	public interface IConfirmationService
    {
	    /// <summary>
	    ///     Prompts a user for confirmation.
	    /// </summary>
	    /// <param name="userId">The id of the user to prompt.</param>
	    /// <param name="guildId">The id of the guild to prompt in.</param>
	    /// <param name="channelId">The id of the channel to prompt in.</param>
	    /// <param name="prompt">What to prompt the user with.</param>
	    /// <returns>A bool representing if the user agreed.</returns>
	    public Task<bool> GetConfirmationAsync(ulong userId, ulong guildId, ulong channelId, string prompt);
    }
}