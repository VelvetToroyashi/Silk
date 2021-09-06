namespace MusicPlugin.Models
{
	public enum ChannelJoinResult
	{
		AlreadyInChannel,
		CannotJoinChannel,
		ConnectedToChannel,
		CannotUnsupressInStage,
		DisconnectedFromCurrentVC,
		DisconnectedFromVCAlready,
	}
}