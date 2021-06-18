using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Data;
using Silk.Core.Data.Models;
using Silk.Core.Utilities;
using Silk.Extensions;

namespace Silk.Core.Commands
{
	public class Test : BaseCommandModule
	{
		private readonly GuildContext _context;
		public Test(GuildContext context) => _context = context;

		[Command("infract")]
		public async Task Infract(CommandContext ctx)
		{
			var inf = new Infraction()
			{
				Enforcer = ctx.Guild.CurrentMember.Id,
				GuildId = ctx.Guild.Id,
				UserId = ctx.User.Id
			};

			_context.Infractions.Add(inf);
			try
			{
				await _context.SaveChangesAsync();
			}
			catch (Exception e)
			{
				await ctx.RespondAsync($"Sorry, but something went wrong with saving the infraction. {e.Message}");
				return;
			}

			var g = await _context
				.Guilds
				.AsQueryable()
				.Include(g => g.Infractions)
				.FirstAsync(g => g.Id == ctx.Guild.Id);
			inf.Guild.Configuration = null!;
			await ctx.RespondAsync(m => m.WithContent("Successfully saved infraction.").WithFile("result.json", ObjectDumper.DumpAsJson(g.Infractions).AsStream()));

		}
	}
}