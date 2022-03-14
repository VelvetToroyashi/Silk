using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Services.Guild;

namespace Silk.Responders.Message;

[ResponderGroup(ResponderGroup.Early)]
public class MessageLoggerResponder : IResponder<IMessageUpdate>, IResponder<IMessageDelete>
{
    private readonly MessageLoggerService _messageLogger;
    public MessageLoggerResponder(MessageLoggerService messageLogger) => _messageLogger = messageLogger;

    public Task<Result> RespondAsync(IMessageUpdate gatewayEvent, CancellationToken ct = default) 
        => _messageLogger.LogEditAsync(gatewayEvent);
    
    public Task<Result> RespondAsync(IMessageDelete gatewayEvent, CancellationToken ct = default) => default;
}