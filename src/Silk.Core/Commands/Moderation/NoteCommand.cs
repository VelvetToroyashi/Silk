using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Silk.Core.Data.Models;
using Silk.Core.Services.Interfaces;
using Silk.Core.Types;
using Silk.Core.Utilities;
using Silk.Core.Utilities.HelpFormatter;

namespace Silk.Core.Commands.Moderation
{
	[Category(Categories.Mod)]
	public class NoteCommand : BaseCommandModule
	{
		private readonly IInfractionService _infractionHelper;
		public NoteCommand(IInfractionService infractionHelper) => _infractionHelper = infractionHelper;

		[Command]
		[RequireFlag(UserFlag.Staff)]
		[Description("Adds a moderation note to a user. Does not impact automatic infraction escalation.")]
		public async Task Note(CommandContext ctx, DiscordUser user, [RemainingText] string note)
		{
			var res = await _infractionHelper.AddNoteAsync(user.Id, ctx.Guild.Id, ctx.User.Id, note);
			string response = res switch
			{
				InfractionResult.SucceededDoesNotNotify => "Succesfully added note!",
				_ => $"An unexpected response was returned, but the note was added. This is probably safe to ignore! {res.Humanize(LetterCasing.Title)}"
			};

			await ctx.RespondAsync(response);
		}

	}
}