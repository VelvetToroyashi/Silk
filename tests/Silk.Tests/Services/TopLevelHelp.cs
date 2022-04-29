using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Remora.Commands.Conditions;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Results;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Services.Bot.Help;

namespace Silk.Tests;

public partial class HelpFormatterTests
{
    public class TopLevelHelp
    {
        [Test]
        public void TopLevelHelpShowsAllCommandsCorrectly()
        {
            var services = new ServiceCollection()
                .AddCommands()
                .AddCommandTree()
                .WithCommandGroup<Tests.TopLevelHelp>()
                .Finish()
                .AddSingleton<TreeWalker>()
                .BuildServiceProvider();
            
            var walker = services.GetRequiredService<TreeWalker>();
            
            var formatter = new DefaultHelpFormatter();

            var result = formatter.GetTopLevelHelpEmbeds(walker.FindNodes(null).GroupBy(x => x.Key));
            
            Assert.AreEqual(1, result.Count());
            
            var embed = result.First();
            
            Assert.AreEqual("`command-1` `command-2` `command-3` `group-1` `group-2*` ", embed.Description.Value);
        }
    }
}