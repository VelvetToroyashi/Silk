using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Remora.Commands.Extensions;
using Silk.Infrastructure;
using Silk.Services.Bot.Help;

namespace Silk.Tests.Services;

public partial class HelpFormatterTests
{
    public class TopLevelHelp
    {
        [Test]
        public void ShowsUngroupedTopLevelHelpCorrectly()
        {
            var services = new ServiceCollection()
                .AddCommands()
                .AddCommandTree()
                .WithCommandGroup<Data.TopLevelHelp.Uncategorized>()
                .Finish()
                .Configure<HelpSystemOptions>(_ => { })
                .AddSingleton<TreeWalker>()
                .AddSingleton<DefaultHelpFormatter>()
                .BuildServiceProvider();
            
            var walker = services.GetRequiredService<TreeWalker>();
            
            var formatter = services.GetRequiredService<DefaultHelpFormatter>();
            
            var result = formatter.GetTopLevelHelpEmbeds(walker.FindNodes(null).GroupBy(x => x.Key));
            
            Assert.AreEqual(1, result.Count());
            
            var embed = result.First();
            
            Assert.AreEqual("`command-1` `command-2` `command-3` `group-1` `group-2*` ", embed.Description.Value);
        }
        
        [Test]
        public void ShowsGroupedTopLevelHelpCorrectly()
        {
            var services = new ServiceCollection()
                .AddCommands()
                .AddCommandTree()
                .WithCommandGroup<Data.TopLevelHelp.Categorized>()
                .Finish()
                .Configure<HelpSystemOptions>(h => h.CommandCategories.Add("category 1"))
                .AddSingleton<TreeWalker>()
                .AddSingleton<DefaultHelpFormatter>()
                .BuildServiceProvider();
            
            var walker = services.GetRequiredService<TreeWalker>();
            
            var formatter = services.GetRequiredService<DefaultHelpFormatter>();
            
            var result = formatter.GetTopLevelHelpEmbeds(walker.FindNodes(null).GroupBy(x => x.Key));
            
            Assert.AreEqual(1, result.Count());
            
            var embed = result.First();
            
            Assert.AreEqual("**`category 1`**\r", embed.Description.Value.Split('\n')[0]);
            Assert.AreEqual("`categorized-command` \r", embed.Description.Value.Split('\n')[1]);
        }

        [Test]
        public void ShowsCategorizedGroupCorrectly()
        {
            var services = new ServiceCollection()
                .AddCommands()
                .AddCommandTree()
                .WithCommandGroup<Data.TopLevelHelp.Categorized>()
                .Finish()
                .Configure<HelpSystemOptions>(h => h.CommandCategories.Add("category 2"))
                .AddSingleton<TreeWalker>()
                .AddSingleton<DefaultHelpFormatter>()
                .BuildServiceProvider();
            
            var walker = services.GetRequiredService<TreeWalker>();
            
            var formatter = services.GetRequiredService<DefaultHelpFormatter>();
            
            var result = formatter.GetTopLevelHelpEmbeds(walker.FindNodes(null).GroupBy(x => x.Key));
            
            Assert.AreEqual(1, result.Count());
            
            var embed = result.First();
            
            Assert.AreEqual("**`category 2`**\r", embed.Description.Value.Split('\n')[0]);
        }

        [Test]
        public void IgnoresUnspecifiedCategoryCorrectly()
        {
            var services = new ServiceCollection()
                .AddCommands()
                .AddCommandTree()
                .WithCommandGroup<Data.TopLevelHelp.Categorized>()
                .Finish()
                .Configure<HelpSystemOptions>(h => h.CommandCategories.Add("category 3"))
                .AddSingleton<TreeWalker>()
                .AddSingleton<DefaultHelpFormatter>()
                .BuildServiceProvider();
            
            
            var walker = services.GetRequiredService<TreeWalker>();
            
            var formatter = services.GetRequiredService<DefaultHelpFormatter>();
            
            var result = formatter.GetTopLevelHelpEmbeds(walker.FindNodes(null).GroupBy(x => x.Key));
            
            Assert.AreEqual(1, result.Count());
            
            var embed = result.First();
            
            Assert.AreEqual("**`Uncategorized`**\r", embed.Description.Value.Split('\n')[0]);
        }
    }
}