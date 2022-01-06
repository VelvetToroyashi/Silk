using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remora.Commands.Extensions;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;
using Remora.Results;

[assembly: RemoraPlugin(typeof(TestPlugin.TestPlugin))]

namespace TestPlugin;

public class TestPlugin : PluginDescriptor
{
    public override string Name        { get; } = "Test Plugin";
    public override string Description { get; } = "This is a test plugin";

    public override Result ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddCommandGroup<TestCommand>();
        
        return Result.FromSuccess();
    }

    public override ValueTask<Result> InitializeAsync(IServiceProvider serviceProvider, CancellationToken ct = default)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<TestPlugin>>();
        logger.LogTrace("Test plugin loaded!");
        return ValueTask.FromResult(Result.FromSuccess());
    }
}