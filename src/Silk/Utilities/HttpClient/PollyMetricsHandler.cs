using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace Silk.Utilities.HttpClient;

public class PollyMetricsHandler : AsyncPolicy<HttpResponseMessage>
{
    protected override async Task<HttpResponseMessage> ImplementationAsync
    (
        Func<Context, CancellationToken, Task<HttpResponseMessage>> next,
        Context                                                     context,
        CancellationToken                                           cancellationToken,
        bool                                                        continueOnCapturedContext
    )
    {
        var res = await next(context, cancellationToken);
        
        if (!context.TryGetValue("endpoint", out var rawEndpoint) || rawEndpoint is not string endpoint)
        {
            throw new InvalidOperationException("No endpoint was specified in the context.");
        }

        // Pre-emptive ratelimiting doesn't return the request
        // HttpClient methods do, so we know if we actually made a call
        // If we haven't, don't log metrics; we're retrying the request via Polly
        if (res.RequestMessage is { } request)
        {
            try
            {
                var sanitizedEndpoint = SanitizeEndpoint(endpoint);

                SilkMetric.HttpRequests.WithLabels(request.Method.Method, ((int)res.StatusCode).ToString(), sanitizedEndpoint).Inc();
            }
            catch { /* */}

        }

        return res;
    }

    public static PollyMetricsHandler Create() => new();

    private static string SanitizeEndpoint(string endpoint)
    {
        var split = endpoint.Split('/');

        for (int i = 0; i < split.Length; i++)
        {
            if (i > 0 && split[i - 1] is "webhooks" or "interactions")
            {
                // Edge case: GET /webhooks/:webhook_id will throw IOOBE
                split[i + 1] = ':' + split[i - 1][..^1] + "_id";
                split[i + 2] = ':' + split[i - 1][..^1] + "_token";
            }
            
            // Edge case: GET /channels/:channel_id/messages/:message_id/reactions will throw IOOBE
            if (split[i] is "reactions")
                split[i + 1] = ":emoji";
            
            if (ulong.TryParse(split[i], out _))
                split[i] = ':' + split[i - 1].Split('-')[^1][..^1] + "_id";
        }
      
        return string.Join('/', split);
    }
}