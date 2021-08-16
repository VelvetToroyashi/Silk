using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutoFake;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MediatR;
using Moq;
using NUnit.Framework;
using Silk.Core.Commands;
using Silk.Core.Data.MediatR.Guilds;
using Silk.Core.Data.MediatR.Guilds.Config;
using Silk.Core.Data.Models;

namespace Silk.Core.Tests.Commands
{
	public class Test
	{
		[Test]
		public async Task ConfigModule_View()
		{
			// Arrange
			var mediatr = new Mock<IMediator>();

			mediatr.Setup(m => m.Send(It.IsAny<GetGuildConfigRequest>(), CancellationToken.None)).ReturnsAsync(new GuildConfig());
			mediatr.Setup(m => m.Send(It.IsAny<GetGuildModConfigRequest>(), CancellationToken.None)).ReturnsAsync(new GuildModConfig());

			var module = new Fake<ConfigModule.ViewConfigModule>(mediatr.Object);
			var guild = typeof(DiscordGuild).GetTypeInfo().DeclaredConstructors.First().Invoke(null) as DiscordGuild;
			var ctx = typeof(CommandContext).GetTypeInfo().DeclaredConstructors.First().Invoke(null) as CommandContext;
			
			var sut = module.Rewrite(m => m.View(ctx));

			sut.Replace((CommandContext ctx) => ctx.RespondAsync(Arg.IsAny<DiscordEmbed>()))
				.Return(Task.FromResult<DiscordMessage>(default));
			
			sut.Replace((CommandContext ctx) => ctx.Prefix).Return("s!");
			sut.Replace((CommandContext ctx) => ctx.Guild).Return(guild);

			//Act
			await sut.Execute();
			
			//Assert
			mediatr.Verify(m => m.Send(It.IsAny<GetGuildConfigRequest>(), CancellationToken.None), Times.Once);
			mediatr.Verify(m => m.Send(It.IsAny<GetGuildModConfigRequest>(), CancellationToken.None), Times.Once);
		}
	}
}