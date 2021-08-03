using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Menus.Entities;

namespace Silk.Core.Types
{
	/// <summary>
	/// A wrapper for <see cref="Menu"/> that allows backtracking.
	/// </summary>
	public abstract class StagedMenu : Menu
	{
		protected readonly Menu _previous;
		protected StagedMenu(DiscordClient client, Menu previous, TimeSpan? timeout = null)
			: base(client, timeout) => _previous = previous;

		public abstract Task BackAsync(ButtonContext ctx);
	}
}