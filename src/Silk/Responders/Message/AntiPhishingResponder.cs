using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Silk.Services.Guild;

namespace Silk.Responders.Message;

public class AntiPhishingResponder : IResponder<IMessageCreate>
{
    private readonly PhishingDetectionService _phishing;

    public AntiPhishingResponder(PhishingDetectionService phishing) => _phishing = phishing;

    public Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
        => _phishing.DetectPhishingAsync(gatewayEvent);
}