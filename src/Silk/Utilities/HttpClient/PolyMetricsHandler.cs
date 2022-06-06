using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.NoOp;

namespace Silk.Utilities.HttpClient;

public class PolyMetricsHandler : AsyncPolicy<HttpResponseMessage>
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
            var sanitizedEndpoint = Regex.Replace(Regex.Replace(endpoint, @"(webhooks/\d+/((?!/)\S)+)", "webhooks/:webhook_id/:webhook_token"), @"([a-z]-)*([a-z]+\B)(s)?/(\d+|\S+(/@me))", "$1$2$3/:$2_id$5");
        
            SilkMetric.HttpRequests.WithLabels(request.Method.Method, ((int)res.StatusCode).ToString(), sanitizedEndpoint).Inc();
        }

        return res;
    }

    public static PolyMetricsHandler Create() => new();
}