using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Remora.Commands.Extensions;
using Remora.Commands.Trees.Nodes;
using Silk.Services.Bot.Help;
using Silk.Tests.Data;

namespace Silk.Tests;

public class TreeWalkerTests
{
    private const string TreeName = "TestTree";
    
    private TreeWalker _walker;
    private IServiceProvider _services;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();

        services
            .AddCommands()
            .AddCommandTree()
            .WithCommandGroup<TestCommands>()
            .Finish()
            .AddCommandTree(TreeName)
            .WithCommandGroup<TestCommands>()
            .Finish()
            .AddScoped<TreeWalker>();
        
        _services = services.BuildServiceProvider();
        _walker = _services.GetRequiredService<TreeWalker>();
    }

    [Test]
    public void EmptyInputReturnsAllCommands()
    {
        var command = _walker.FindNodes(null);
        
        Assert.AreEqual(10, command.Count);
    }
    
    [Test]
    public void ReturnsCorrectTopLevelCommand()
    {
        var command = _walker.FindNodes("command");
        
        Assert.AreEqual(1, command.Count);
        Assert.AreEqual("command", command[0].Key);
        Assert.AreEqual(command[0].GetType(), typeof(CommandNode));
    }

    [Test]
    public void ReturnsOverloadsCorrectly()
    {
        var command = _walker.FindNodes("overload");
        
        Assert.AreEqual(2, command.Count);
        
        Assert.AreEqual("overload", command[0].Key);
        Assert.AreEqual("overload", command[1].Key);
        
        Assert.AreEqual(command[0].GetType(), typeof(CommandNode));
        Assert.AreEqual(command[1].GetType(), typeof(CommandNode));
    }
    
    [Test]
    public void ReturnsCorrectSubCommand()
    {
        var command = _walker.FindNodes("group command");
        
        Assert.AreEqual(1, command.Count);
        Assert.AreEqual("command", command[0].Key);
        Assert.AreEqual(command[0].GetType(), typeof(CommandNode));
    }

    [Test]
    public void ReturnsCommandAndGroupCorrectly()
    {
        var command = _walker.FindNodes("executable-group");
        
        Assert.AreEqual(2, command.Count);
        Assert.AreEqual("executable-group", command[0].Key);
        Assert.AreEqual("executable-group", command[1].Key);
        
        Assert.AreEqual(command[0].GetType(), typeof(CommandNode));
        Assert.AreEqual(command[1].GetType(), typeof(GroupNode));
    }

    [Test]
    public void ReturnsTopLevelGroupCorrectly()
    {
        var command = _walker.FindNodes("group");
        
        Assert.AreEqual(1, command.Count);
        Assert.AreEqual("group", command[0].Key);
        Assert.AreEqual(command[0].GetType(), typeof(GroupNode));
    }

    [Test]
    public void ReturnsNestedGroupCorrectly()
    {
        var command = _walker.FindNodes("nested-group-1 nested-group-2");
        
        Assert.AreEqual(1, command.Count);
        Assert.AreEqual("nested-group-2", command[0].Key);
        Assert.AreEqual(command[0].GetType(), typeof(GroupNode));
    }

    [Test]
    public void AccessesNamedTreeCorrectly()
    {
        var command = _walker.FindNodes("command", TreeName);
        
        Assert.AreEqual(1, command.Count);
    }
    
    [Test]
    public void ReturnsEmptyResultWhenNoCommandFound()
    {
        var command = _walker.FindNodes("unknown");
        
        Assert.AreEqual(0, command.Count);
    }
    
    [Test]
    public void ReturnsEmptyResultWhenNoTreeFound()
    {
        var command = _walker.FindNodes("command", "unknown");
        
        Assert.AreEqual(0, command.Count);
    }

    [Test]
    public void ReturnsEmptyResultWhenGroupNotFound()
    {
        var command = _walker.FindNodes("group command unknown");
        
        Assert.AreEqual(0, command.Count);
    }

    [Test]
    public void CorrectlySearchesAliases()
    {
        var command = _walker.FindNodes("aliased");
        
        Assert.AreEqual(1, command.Count);
        Assert.AreEqual("aliased-command", command[0].Key);
    }
    
}