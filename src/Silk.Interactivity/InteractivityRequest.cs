using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Results;

namespace Silk.Interactivity;

public record InteractivityRequest<TEvent>(TaskCompletionSource<Result<TEvent?>> Wait, Func<TEvent, bool> Predicate) where TEvent : IGatewayEvent;