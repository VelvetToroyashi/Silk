using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Remora.Commands.Conditions;
using Remora.Commands.Extensions;
using Remora.Commands.Results;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Results;
using Remora.Rest.Core;
using Remora.Results;
using Silk.Infrastructure;
using Silk.Services.Bot.Help;
using VTP.Remora.Commands.HelpSystem;

namespace Silk.Tests;

public class HelpServiceTests
{
    private readonly Snowflake _channelID = new(1);
    
    private CommandHelpService _help;
    private IServiceProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        _serviceProvider = new ServiceCollection()
            .AddCommands()
            .AddCommandTree()
            .WithCommandGroup<TestCommands>()
            .Finish()
            .AddSingleton(Mock.Of<IDiscordRestChannelAPI>())
            .AddScoped<TreeWalker>()
            .Configure<HelpSystemOptions>(h => h.AlwaysShowCommands = true)
            .AddScoped<CommandHelpService>()
            .BuildServiceProvider();
        
        _help = _serviceProvider.GetService<CommandHelpService>();
    }

    [Test]
    public void UnknownCommandReturnsError()
    {
        var result = _help.ShowHelpAsync(default, "unknown").Result; // Sync path, don't care
        
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("No command with the name \"unknown\" was found.", result.Error.Message);
    }

    [Test]
    public async Task EmptyInputInvokesTopLevelCommandHelp()
    {
        var formatterMock = new Mock<IHelpFormatter>();

        formatterMock
            .Setup(fm => fm.GetTopLevelHelpEmbeds(It.IsAny<IEnumerable<IGrouping<string, IChildNode>>>()))
            .Returns(new IEmbed[] { new Embed() { Title = "Top Level Commands"} });

        var services = new ServiceCollection()
            .AddSingleton(formatterMock.Object)
            .BuildServiceProvider();
        
        var help = new CommandHelpService
        (
            _serviceProvider.GetRequiredService<TreeWalker>(),
            services,
            _serviceProvider.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );
        
        var result = await help.ShowHelpAsync(_channelID, string.Empty);
        
        Assert.IsTrue(result.IsSuccess);
        
        formatterMock.Verify(fm => fm.GetTopLevelHelpEmbeds(It.IsAny<IEnumerable<IGrouping<string, IChildNode>>>()), Times.Once);
    }

    [Test]
    public async Task GroupReturnsSubcommands()
    {
        var formatterMock = new Mock<IHelpFormatter>();
        
        formatterMock
            .Setup(fm => fm.GetCommandHelp(It.IsAny<IEnumerable<IChildNode>>()))
            .Returns(new IEmbed[] { new Embed() { Title = "Showing subcommands for group" } });
        
        var services = new ServiceCollection()
            .AddSingleton(formatterMock.Object)
            .BuildServiceProvider();
        
        var help = new CommandHelpService
        (
            _serviceProvider.GetRequiredService<TreeWalker>(),
            services,
            _serviceProvider.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );
        
        var result = await help.ShowHelpAsync(_channelID, "group");
        
        Assert.IsTrue(result.IsSuccess);
        
        // This is technically wrong, see commit f1dec1fa
        // or the comment in this GetCommandHelp below
        formatterMock.Verify
        (
            fm => fm.GetCommandHelp(It.IsAny<IEnumerable<IChildNode>>()),
            Times.Once
        );
    }

    [Test]
    public async Task ExecutableGroupUsesSubCommands()
    {
        var formatterMock = new Mock<IHelpFormatter>();
        
        formatterMock
            .Setup(fm => fm.GetCommandHelp(It.IsAny<IEnumerable<IChildNode>>()))
            .Returns(new IEmbed[] { new Embed() { Title = "Showing subcommands for group" } });
        
        var services = new ServiceCollection()
            .AddSingleton(formatterMock.Object)
            .BuildServiceProvider();
        
        var help = new CommandHelpService
        (
            _serviceProvider.GetRequiredService<TreeWalker>(),
            services,
            _serviceProvider.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );
        
        var result = await help.ShowHelpAsync(_channelID, "executable-group");
        
        Assert.IsTrue(result.IsSuccess);
        
        formatterMock.Verify
        (
            fm => fm.GetCommandHelp
            (
                It.Is<IEnumerable<IChildNode>>
                (
                    s => s.Count() == 2 &&
                         s.First() is CommandNode && 
                         s.Last() is GroupNode
                )
            ),
            Times.Once
        );
    }
    
    [Test]
    public async Task SingleCommandReturnsSingleCommand()
    {
        var formatterMock = new Mock<IHelpFormatter>();
        
        formatterMock
            .Setup(fm => fm.GetCommandHelp(It.IsAny<IChildNode>()))
            .Returns( new Embed() { Title = "Showing single command" } );
        
        var services = new ServiceCollection()
            .AddSingleton(formatterMock.Object)
            .BuildServiceProvider();
        
        var help = new CommandHelpService
        (
            _serviceProvider.GetRequiredService<TreeWalker>(),
            services,
            _serviceProvider.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );
        
        var result = await help.ShowHelpAsync(_channelID, "command");
        
        Assert.IsTrue(result.IsSuccess);
        
        formatterMock.Verify
        (
            fm => fm.GetCommandHelp(It.Is<IChildNode>(c => c.Key == "command")),
            Times.Once
        );
    }

    [Test]
    public async Task CommandOverloadsHandledCorrectly()
    {
        var formatterMock = new Mock<IHelpFormatter>();
        
        formatterMock
            .Setup(fm => fm.GetCommandHelp((IEnumerable<IChildNode>)It.IsAny<IEnumerable<IGrouping<string,IChildNode>>>()))
            .Returns(new IEmbed[] { new Embed() { Title = "Showing subcommands for group" } });

        var services = new ServiceCollection()
            .AddSingleton(formatterMock.Object)
            .BuildServiceProvider();

        var help = new CommandHelpService
        (
            _serviceProvider.GetRequiredService<TreeWalker>(),
            services,
            _serviceProvider.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );
        
        var result = await help.ShowHelpAsync(_channelID, "overload");
        
        
        Assert.IsTrue(result.IsSuccess);
        
        formatterMock.Verify
        (
            fm => fm.GetCommandHelp
            (
                It.Is<IEnumerable<IChildNode>>
                (
                    s => s.Count() == 2 &&
                         s.All(sc => sc is CommandNode)
                )
            ),
            Times.Once
        );
    }

    [Test]
    public async Task RequiresRegisteredFormatter()
    {
        var help = new CommandHelpService
        (
            _serviceProvider.GetRequiredService<TreeWalker>(),
            new ServiceCollection().BuildServiceProvider(),
            _serviceProvider.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );
        
        var result = await help.ShowHelpAsync(_channelID, "command");
        
        Assert.IsFalse(result.IsSuccess);
        Assert.IsInstanceOf<InvalidOperationError>(result.Error);
    }

    [Test]
    public async Task UnregisteredConditionReturnsError()
    {
        var services = new ServiceCollection()
            .AddSingleton(Mock.Of<IHelpFormatter>())
            .Configure<HelpSystemOptions>(help => help.AlwaysShowCommands = false)
            .BuildServiceProvider();
        
        var help = new CommandHelpService
        (
            _serviceProvider.GetRequiredService<TreeWalker>(),
            services,
            services.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );

        Assert.ThrowsAsync<InvalidOperationException>(async () => await help.ShowHelpAsync(_channelID, "conditioned"));
    }
    
    [Test]
    public async Task UnregisteredGroupReturnsError()
    {
        var services = new ServiceCollection()
            .AddSingleton(Mock.Of<IHelpFormatter>())
            .Configure<HelpSystemOptions>(help => help.AlwaysShowCommands = false)
            .BuildServiceProvider();
        
        var help = new CommandHelpService
        (
            _serviceProvider.GetRequiredService<TreeWalker>(),
            services,
            services.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );
        
        Assert.ThrowsAsync<InvalidOperationException>(async () => await help.ShowHelpAsync(_channelID, "conditioned-group"));
    }

    [Test]
    public async Task CorrectlyChecksMultiTypeGroupConditions()
    {
        var conditionMock = new Mock<ICondition<RequireDiscordPermissionAttribute>>();
        
        conditionMock
            .Setup(c => c.CheckAsync(It.IsAny<RequireDiscordPermissionAttribute>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new PermissionDeniedError()));

        var services = new ServiceCollection()
            .AddCommands()
            .AddCommandTree()
            .WithCommandGroup<TestCommands2>()
            .WithCommandGroup<TestCommands3>()
            .Finish()
            .AddSingleton<TreeWalker>()
            .AddSingleton(conditionMock.Object)
            .AddSingleton(Mock.Of<IHelpFormatter>())
            .Configure<HelpSystemOptions>(help => help.AlwaysShowCommands = false)
            .BuildServiceProvider();

        var help = new CommandHelpService
        (
            services.GetRequiredService<TreeWalker>(),
            services,
            services.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );
        
        var result = await help.ShowHelpAsync(_channelID, "group2");
        
        Assert.IsFalse(result.IsSuccess);
        
        Assert.IsInstanceOf<ConditionNotSatisfiedError>(result.Error);
    }

    [Test]
    public async Task ConditionlessGroupIsAlwaysReturned()
    {
        var conditionMock = new Mock<ICondition<RequireDiscordPermissionAttribute>>();
        
        conditionMock
            .Setup(c => c.CheckAsync(It.IsAny<RequireDiscordPermissionAttribute>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new PermissionDeniedError()));
        
        var services = new ServiceCollection()
            .AddCommands()
            .AddCommandTree()
            .WithCommandGroup<TestCommands>()
            .Finish()
            .AddSingleton<TreeWalker>()
            .AddSingleton(conditionMock.Object)
            .AddSingleton(Mock.Of<IHelpFormatter>())
            .Configure<HelpSystemOptions>(help => help.AlwaysShowCommands = false)
            .BuildServiceProvider();
        
        var help = new CommandHelpService
        (
            services.GetRequiredService<TreeWalker>(),
            services,
            services.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );

        var result = await help.EvaluateNodeConditionsAsync(services.GetRequiredService<TreeWalker>().FindNodes("group"));
        
        Assert.AreEqual(1, result.Count());
    }

    [Test]
    public async Task EvaluatesCommandTypeConditionsCorrectly()
    {
        var conditionMock = new Mock<ICondition<RequireDiscordPermissionAttribute>>();
        
        conditionMock
            .SetupSequence(c => c.CheckAsync(It.IsAny<RequireDiscordPermissionAttribute>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new PermissionDeniedError()))
            .ReturnsAsync(Result.FromSuccess);
        
        var services = new ServiceCollection()
            .AddCommands()
            .AddCommandTree()
            .WithCommandGroup<TestCommands4>()
            .Finish()
            .AddSingleton<TreeWalker>()
            .AddSingleton(conditionMock.Object)
            .AddSingleton(Mock.Of<IHelpFormatter>())
            .Configure<HelpSystemOptions>(help => help.AlwaysShowCommands = false)
            .BuildServiceProvider();
        
        var help = new CommandHelpService
        (
            services.GetRequiredService<TreeWalker>(),
            services,
            services.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );

        var result = await help.EvaluateNodeConditionsAsync(services.GetRequiredService<TreeWalker>().FindNodes("conditioned-group-2 command"));
        
        Assert.IsEmpty(result);
        
        conditionMock.Verify
        (   
            c => c.CheckAsync(It.Is<RequireDiscordPermissionAttribute>(c => c.Permissions[0] == DiscordPermission.ManageChannels), It.IsAny<CancellationToken>()),
            Times.Once
        );
        
        conditionMock.Verify
        (   
            c => c.CheckAsync(It.Is<RequireDiscordPermissionAttribute>(c => c.Permissions[0] == DiscordPermission.ManageRoles), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    public async Task EvaluatesCommandMethodConditionsCorrectly()
    {
        var conditionMock = new Mock<ICondition<RequireDiscordPermissionAttribute>>();
        
        conditionMock
            .SetupSequence(c => c.CheckAsync(It.IsAny<RequireDiscordPermissionAttribute>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromSuccess)
            .ReturnsAsync(Result.FromError(new PermissionDeniedError()));
        
        var services = new ServiceCollection()
            .AddCommands()
            .AddCommandTree()
            .WithCommandGroup<TestCommands4>()
            .Finish()
            .AddSingleton<TreeWalker>()
            .AddSingleton(conditionMock.Object)
            .AddSingleton(Mock.Of<IHelpFormatter>())
            .Configure<HelpSystemOptions>(help => help.AlwaysShowCommands = false)
            .BuildServiceProvider();
        
        var help = new CommandHelpService
        (
            services.GetRequiredService<TreeWalker>(),
            services,
            services.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );

        var result = await help.EvaluateNodeConditionsAsync(services.GetRequiredService<TreeWalker>().FindNodes("conditioned-group-2 command"));
        
        Assert.IsEmpty(result);
        
        conditionMock.Verify
        (   
            c => c.CheckAsync(It.Is<RequireDiscordPermissionAttribute>(a => a.Permissions[0] == DiscordPermission.ManageChannels), It.IsAny<CancellationToken>()),
            Times.Once
        );
        
        conditionMock.Verify
        (   
            c => c.CheckAsync(It.Is<RequireDiscordPermissionAttribute>(a => a.Permissions[0] == DiscordPermission.ManageRoles), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
    
    [Test]
    public async Task ShowsAllTopLevelHelpWhenUsingShowAllCommands()
    {
        var conditionMock = new Mock<ICondition<RequireDiscordPermissionAttribute>>();
        
        conditionMock
            .Setup(c => c.CheckAsync(It.IsAny<RequireDiscordPermissionAttribute>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new PermissionDeniedError()));
        
        var formatterMock = new Mock<IHelpFormatter>();
            
        var services = new ServiceCollection()
            .AddCommands()
            .AddCommandTree()
            .WithCommandGroup<TopLevelHelp>()
            .Finish()
            .AddSingleton<TreeWalker>()
            .AddSingleton(conditionMock.Object)
            .AddSingleton(formatterMock.Object)
            .Configure<HelpSystemOptions>(help => help.AlwaysShowCommands = true)
            .BuildServiceProvider();
        
        var help = new CommandHelpService
        (
            services.GetRequiredService<TreeWalker>(),
            services,
            services.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );

        var result = await help.ShowHelpAsync(_channelID);
            
        Assert.IsTrue(result.IsSuccess);
            
        formatterMock.Verify
        (
            f => f.GetTopLevelHelpEmbeds(It.Is<IEnumerable<IGrouping<string,IChildNode>>>(g => g.Count() == 5)),
            Times.Once
        );
    }
    
    [Test]
    public async Task CorrectlyHidesConditionedCommandsForTopLevelHelp()
    {
        var conditionMock = new Mock<ICondition<RequireDiscordPermissionAttribute>>();
        
        conditionMock
            .Setup(c => c.CheckAsync(It.IsAny<RequireDiscordPermissionAttribute>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.FromError(new PermissionDeniedError()));
        
        var formatterMock = new Mock<IHelpFormatter>();
            
        var services = new ServiceCollection()
            .AddCommands()
            .AddCommandTree()
            .WithCommandGroup<TopLevelHelp>()
            .Finish()
            .AddSingleton<TreeWalker>()
            .AddSingleton(conditionMock.Object)
            .AddSingleton(formatterMock.Object)
            .Configure<HelpSystemOptions>(help => help.AlwaysShowCommands = false)
            .BuildServiceProvider();
        
        var help = new CommandHelpService
        (
            services.GetRequiredService<TreeWalker>(),
            services,
            services.GetRequiredService<IOptions<HelpSystemOptions>>(),
            Mock.Of<IDiscordRestChannelAPI>()
        );

        var result = await help.ShowHelpAsync(_channelID);
            
        Assert.IsTrue(result.IsSuccess);
            
        formatterMock.Verify
        (
            f => f.GetTopLevelHelpEmbeds(It.Is<IEnumerable<IGrouping<string,IChildNode>>>(g => g.Count() == 4)),
            Times.Once
        );
    }
}