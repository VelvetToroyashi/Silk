using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Menus;
using DSharpPlus.Menus.Attributes;
using DSharpPlus.Menus.Entities;

namespace Silk.Core.Types
{
	/// <summary>
	/// A wrapper for <see cref="Menu"/> that allows backtracking.
	/// </summary>
	public abstract class StagedMenu : Menu
	{
		protected readonly Menu _previous;
		public abstract DiscordUser User { get; init; }
		protected StagedMenu(DiscordClient client, Menu previous, TimeSpan? timeout = null)
			: base(client, timeout) => _previous = previous;

		[SecondaryButton("Back!", Row = ButtonPosition.Fifth, Location = ButtonPosition.Fifth)]
		public Task BackAsync(ButtonContext ctx)
			=> ctx.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent(ctx.Message.Content).AddMenu(_previous));

		public override Task<bool> CanBeExecuted(ButtonContext args)
			=> Task.FromResult(args.User == User);
	}
}