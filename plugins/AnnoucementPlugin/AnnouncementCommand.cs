using System;
using System.Threading.Tasks;
using AnnoucementPlugin.Services;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace AnnoucementPlugin
{
	public sealed class AnnouncementCommand : BaseCommandModule
	{
		private readonly AnnouncementService _announcement;
		public AnnouncementCommand(AnnouncementService announcement) => _announcement = announcement;
		
		[Command]
		[RequireGuild]
		public async Task Announce(CommandContext ctx, DiscordChannel announcementChannel, [RemainingText] string announcement)
		{
			await _announcement.ScheduleAnnouncementAsync(announcement, ctx.Guild.Id, ctx.Channel.Id, TimeSpan.Zero);
		} 
	}
}