namespace Silk.Shared.Constants;

public static class StringConstants
{
    /// <summary>
    /// An invite to the Silk! server, that links directly to the support channel.
    /// </summary>
    public const string SupportInvite = "https://discord.gg/XsHcuvUWda";
    
    /// <summary>
    /// The default command prefix used by Silk!.
    /// </summary>
    public const string DefaultCommandPrefix = "s!";
    
    /// <summary>
    /// The current version of Silk!.
    /// </summary>
    public const string Version = "3.7.4"; 

    /// <summary>
    /// A special expression based log template that allows for conditional event ID insertion.
    /// </summary>
    public const string LogFormat            = "[{@t:h:mm:ss ff tt}] [{@l:u3}] [{Shard}/{Shards}] {#if EventId is not null}[{EventId.Name}] {#end}[{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}] {@m}\n{@x}";
    
    /// <summary>
    /// The log format for files. e.g. [12:24:15 75 AM] [INF] [Program] This is a log message.
    /// </summary>
    public const string FileLogFormat        = "[{Timestamp:h:mm:ss ff tt}] [{Level:u3}] [{Shard}/{Shards}] [{SourceContext}] {Message:lj} {Exception:j}{NewLine}";
    
    public const string AutoModMessagePrefix = "[AUTOMOD]";

    /// <summary>
    /// An identifier to be used as a user agent for HTTP requests. Some APIs do not allow access without this.
    /// </summary>
    public const string ProjectIdentifier = "Silk! v" + Version + " By VelvetThePanda & Contributors";
}