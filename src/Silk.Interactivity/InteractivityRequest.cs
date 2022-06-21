using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Results;

namespace Silk.Interactivity;

public record InteractivityRequest<TEvent>(TaskCompletionSource<Result<TEvent?>> Wait, Func<TEvent, bool> Predicate) : InteractivityRequest where TEvent : IGatewayEvent;

public record InteractivityRequest;