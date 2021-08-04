using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Menus;
using DSharpPlus.Menus.Attributes;
using DSharpPlus.Menus.Entities;
using Silk.Core.Data.Models;
using Silk.Core.Types;
using Silk.Core.Utilities;

namespace Silk.Core.Commands
{
	[RequireFlag(UserFlag.Staff)]
	public class Test : BaseCommandModule
	{
		[Command]
		public async Task Config(CommandContext ctx)
		{
			var menu = new BaseConfigMenu(ctx.Client) {Owner = ctx.User};
			await menu.StartAsync();
			await ctx.RespondAsync(m => m.WithContent("** **").AddMenu(menu));
		}
	}
	
	public sealed class BaseConfigMenu : Menu
	{
		public DiscordUser Owner { get; init; }

		public override async Task<bool> CanBeExecuted(ButtonContext args) => args.Interaction.User == Owner;

		public BaseConfigMenu(DiscordClient client, TimeSpan? timeout = null) : base(client, timeout) { }
		
		private async Task Interact(ButtonContext ctx)
		{
			await ctx.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddMenu(this).WithContent("** **"));
			await Task.Delay(2000);
		}


		private async Task ShowNewMenuAsync(ButtonContext ctx)
		{
			var menu = new ModConfigMenu(Client, this);
			await menu.StartAsync();
			await ctx.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddMenu(menu).WithContent("** **"));
		}

		// For this button to be registered it must have one of the button attributes,
		// have `ComponentInteractionCreateEventArgs` as first and only parameter and return `Task`
		[PrimaryButton("Moderation settings", Row = ButtonPosition.First)]
		public Task DangerAsync(ButtonContext ctx) => Interact(ctx);
		
	}

	public sealed class ModConfigMenu : StagedMenu
	{
		public override DiscordUser User { get; init; }
		public ModConfigMenu(DiscordClient client, Menu previous , TimeSpan? timeout = null) : base(client, previous, timeout) { }
	}
}