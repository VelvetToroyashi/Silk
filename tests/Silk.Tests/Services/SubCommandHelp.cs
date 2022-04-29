using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Remora.Commands.Extensions;
using Remora.Commands.Trees.Nodes;
using Silk.Services.Bot.Help;

namespace Silk.Tests;

public partial class HelpFormatterTests
{
    public class SubCommandHelp
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
        public void WorksWithOverloads()
        {
            var command = _treeWalker.FindNodes("overload");

            var embeds = _formatter.GetCommandHelp(command);
            
            Assert.AreEqual(2, embeds.Count());
            
            Assert.AreEqual("Help for overload (overload 1 of 2)", embeds.First().Title.Value);
            Assert.AreEqual("Help for overload (overload 2 of 2)", embeds.Last().Title.Value);
            
            // TODO: Check parameters? Description?
        }
        
        [Test]
        public void DisplaysCorrectGroupInformationInSingleEmbed()
        {
            var command = _treeWalker.FindNodes("standalone-group");

            var embeds = _formatter.GetCommandHelp(command);
            
            Assert.AreEqual(1, embeds.Count());
            Assert.AreEqual("Showing sub-command help for standalone-group", embeds.First().Title.Value);
        }
        
        [Test]
        public void WorksWithSingleChildGroup()
        {
            var command = _treeWalker.FindNodes("standalone-group");

            var embed = _formatter.GetCommandHelp(command).Single();
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("`command`\r", description[10]);
        }

        [Test]
        public void WorksWithMultiChildGroup()
        {
            var command = _treeWalker.FindNodes("multi-child-group");
            
            var embed = _formatter.GetCommandHelp(command).Single();
            
            var description = embed.Description.Value.Split('\n');

            Assert.AreEqual("`command-1`\r", description[10]);
            Assert.AreEqual("`command-2`\r", description[11]);
        }

        [Test]
        public void WorksWithParameterlessExecutableGroup()
        {
            var command = _treeWalker.FindNodes("parameterless-executable-group");
            
            var embed = _formatter.GetCommandHelp(command).Single();

            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("This group can be executed like a command without parameters.\r", description[10]);
        }
        
        [Test]
        public void WorksWithParameterizedExecutableGroup()
        {
            var command = _treeWalker.FindNodes("parameterized-executable-group");
            
            var embed = _formatter.GetCommandHelp(command).Single();

            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("This group can be executed like a command.\r", description[10]);
            
            Assert.AreEqual("`parameterized-executable-group <parameter>`\r", description[12]);
        }
        
        [Test]
        public void WorksWithOverloadedParameterizedExecutableGroup()
        {
            var command = _treeWalker.FindNodes("overloaded-parameterized-executable-group");
            
            var embed = _formatter.GetCommandHelp(command).Single();
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("This group can be executed like a command without parameters.\r", description[10]);
            
            Assert.AreEqual("`overloaded-parameterized-executable-group <parameter>`\r", description[12]);
        }

        [Test]
        public void WorksWithParameterizedExecutableGroupWithOption()
        {
            var command = _treeWalker.FindNodes("parameterized-executable-group-with-option");
            
            var embed = _formatter.GetCommandHelp(command).Single();
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("This group can be executed like a command.\r", description[10]);
            
            Assert.AreEqual("`parameterized-executable-group-with-option <-o parameter>`\r", description[12]);
        }
        
        [Test]
        public void WorksWithParameterizedExecutableGroupWithSwitch()
        {
            var command = _treeWalker.FindNodes("parameterized-executable-group-with-switch");
            
            var embed = _formatter.GetCommandHelp(command).Single();
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("This group can be executed like a command.\r", description[10]);
            
            Assert.AreEqual("`parameterized-executable-group-with-switch [-s]`\r", description[12]);
        }

        [Test]
        public void DescribedExecutableGroupUsesGroupDescription()
        {
            var command = _treeWalker.FindNodes("described-executable-group");
            
            var embed = _formatter.GetCommandHelp(command).Single();
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("Group description\r", description[7]);
        }

        [Test]
        public void DescribedExecutableGroupUsesCommandDescriptionCorrectly()
        {
            var command = _treeWalker.FindNodes("described-executable-group-2");
            
            var embed = _formatter.GetCommandHelp(command).Single();
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("Command description\r", description[7]);
        }
        
        [Test]
        public void FallsBackToCommandDescription()
        {
            var command = _treeWalker.FindNodes("parameterized-executable-group");
            
            var embed = _formatter.GetCommandHelp(command).Single();
            
            var description = embed.Description.Value.Split('\n');
            
            Assert.AreEqual("No description set.\r", description[7]);
        }

        [Test]
        public void HandlesComplexGroupsCorrectly()
        {
            var command = _treeWalker.FindNodes("complex-group");
            
            var embeds = _formatter.GetCommandHelp(command).Single();
            
            var description = embeds.Description.Value.Split('\n');
            
            Assert.AreEqual("`command`\r", description[10]);
            Assert.AreEqual("`overload`\r", description[11]);
            Assert.AreEqual("`nested-executable-group*`\r", description[12]);
        }
        
        [Test]
        public void HandlesNestedGroupsCorrectly()
        {
            var command = _treeWalker.FindNodes("executable-group-with-executable-children");
            
            var embeds = _formatter.GetCommandHelp(command).Single();
            
            var description = embeds.Description.Value.Split('\n');
            
            Assert.AreEqual("`command`\r", description[13]);
            Assert.AreEqual("`executable-child-group*`\r", description[14]);
        }

        [Test]
        public void ReturnsCommandHelpForSingleCommand()
        {
            var command = _treeWalker.FindNodes("parameterless");
            var embed = _formatter.GetCommandHelp(command).Single();
            
            Assert.AreEqual("Help for parameterless", embed.Title.Value);
        }

        [Test]
        public void ShowsSubcommandHelpWhenGivenGroupChildren()
        {
            var command = (_treeWalker.FindNodes("multi-child-group").First() as IParentNode).Children;
            
            var embed = _formatter.GetCommandHelp(command).Single();
            Assert.AreEqual("Showing sub-command help for multi-child-group", embed.Title.Value);

        }
    }
}