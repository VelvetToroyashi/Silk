using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Remora.Commands.Extensions;
using Silk.Services.Bot.Help;

namespace Silk.Tests;

public partial class HelpFormatterTests
{
    public class CommandHelp
    {
        private TreeWalker _treeWalker;
        private DefaultHelpFormatter _formatter;
        private IServiceProvider _services;
    
        [SetUp]
        public void Setup()
        {
            _services = new ServiceCollection()
                .AddCommands()
                .AddCommandTree()
                .WithCommandGroup<FormatterTestCommands>()
                .Finish()
                .AddSingleton<DefaultHelpFormatter>()
                .AddSingleton<TreeWalker>()
                .BuildServiceProvider();
        
            _formatter = _services.GetRequiredService<DefaultHelpFormatter>();
            _treeWalker = _services.GetRequiredService<TreeWalker>();
        }
        
        [Test]
        public void ShowsCorrectCommandName()
        {
            var command = _treeWalker.FindNodes("parameterless")[0];

            var embed = _formatter.GetCommandHelp(command);
            
            Assert.AreEqual("Help for parameterless", embed.Title.Value);
        }
        
        [Test]
        public void WorksForParmeterlessCommand()
        {
            var command = _treeWalker.FindNodes("parameterless")[0];
            
            var embed = _formatter.GetCommandHelp(command);

            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("This command can be used without any parameters.\r", description[10]);
        }
        
        [Test]
        public void WorksWithParameterizedCommand()
        {
            var command = _treeWalker.FindNodes("parameterized")[0];
            
            var embed = _formatter.GetCommandHelp(command);

            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`<parameter>` No description set.\r", description[10]);
        }

        [Test]
        public void WorksWithOptionalParameter()
        {
            var command = _treeWalker.FindNodes("optional-parameterized")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`[parameter]` No description set.\r", description[10]);
        }

        [Test]
        public void WorksWithParameterWithDescription()
        {
            var command = _treeWalker.FindNodes("described-parameter")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`<parameter>` description\r", description[10]);
        }

        [Test]
        public void WorksWithMultiParmeterCommand()
        {
            var command = _treeWalker.FindNodes("multi-parameterized")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`<parameter1>` No description set.\r", description[10]);
            Assert.AreEqual("`<parameter2>` No description set.\r", description[12]);
        }
        
        [Test]
        public void WorksWithMultiParameterWithOptional()
        {
            var command = _treeWalker.FindNodes("multi-parameterized-optional")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`<parameter1>` No description set.\r", description[10]);
            Assert.AreEqual("`[parameter2]` No description set.\r", description[12]);
        }

        [Test]
        public void WorksWithShortNamedOption()
        {
            var command = _treeWalker.FindNodes("short-named-option")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`<-o parameter>` No description set.\r", description[10]);
        }
        
        [Test]
        public void WorksWithLongNamedOption()
        {
            var command = _treeWalker.FindNodes("long-named-option")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`<--option parameter>` No description set.\r", description[10]);
        }
        
        [Test]
        public void WorksWithOptionWithShortAndLongName()
        {
            var command = _treeWalker.FindNodes("short-long-named-option")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`<-o/--option parameter>` No description set.\r", description[10]);
        }
        
        [Test]
        public void WorksWithOptionalShortNamedOption()
        {
            var command = _treeWalker.FindNodes("optional-short-named-option")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`[-o parameter]` No description set.\r", description[10]);
        }
        
        [Test]
        public void WorksWithOptionalLongNamedOption()
        {
            var command = _treeWalker.FindNodes("optional-long-named-option")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`[--option parameter]` No description set.\r", description[10]);
        }
        
        [Test]
        public void WorksWithOptionalShortAndLongNamedOption()
        {
            var command = _treeWalker.FindNodes("optional-short-long-named-option")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`[-o/--option parameter]` No description set.\r", description[10]);
        }
        
        [Test]
        public void WorksWithShortNamedSwitch()
        {
            var command = _treeWalker.FindNodes("short-named-switch")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`[-s]` No description set.\r", description[10]);
        }
        
        [Test]
        public void WorksWithLongNamedSwitch()
        {
            var command = _treeWalker.FindNodes("long-named-switch")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`[--switch]` No description set.\r", description[10]);
        }
        
        [Test]
        public void WorksWithShortAndLongNamedSwitch()
        {
            var command = _treeWalker.FindNodes("short-long-named-switch")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`[-s/--switch]` No description set.\r", description[10]);
        }
        
        [Test]
        public void WorksWithDescriptionlessCommand()
        {
            var command = _treeWalker.FindNodes("descriptionless")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("No description set.\r", description[7]);
        }

        [Test]
        public void WorksWithDescriptedCommand()
        {
            var command = _treeWalker.FindNodes("descriptioned")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("Descriptioned command\r", description[7]);
        }
        
        [Test]
        public void WorksWithPermissionGatedCommand()
        {
            var command = _treeWalker.FindNodes("permissioned")[0];
            
            var embed = _formatter.GetCommandHelp(command);
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("This command requires the following permissions: SendMessages\r", description[^3]);
        }

        [Test]
        public void DoesntWorkWithGroup()
        {
            var group = _treeWalker.FindNodes("standalone-group")[0];

            Assert.Throws<InvalidCastException>(() => _formatter.GetCommandHelp(group));
        }
            
    }
}