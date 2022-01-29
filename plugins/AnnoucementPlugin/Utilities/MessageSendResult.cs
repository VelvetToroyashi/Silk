namespace AnnoucementPlugin.Utilities
{
	/// <summary>
	///     A result of attempting to send a message.
	/// </summary>
	/// <param name="Succeeded">Whether or not sending the message succeeded</param>
	/// <param name="ErrorType">The reason the message errored.</param>
	public sealed record MessageSendResult(bool Succeeded, MessageSendErrorType ErrorType = MessageSendErrorType.None);
}