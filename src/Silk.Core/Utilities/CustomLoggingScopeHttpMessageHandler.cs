using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Silk.Core.Utilities
{
    /// <summary>
    /// Overriding Logging Scope Handler for HttpClientFactory
    /// Source: https://www.stevejgordon.co.uk/httpclientfactory-asp-net-core-logging
    /// </summary>
    public class CustomLoggingScopeHttpMessageHandler : DelegatingHandler
    {
        private readonly ILogger _logger;

        public CustomLoggingScopeHttpMessageHandler(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (Log.BeginRequestPipelineScope(_logger, request))
            {
                Log.RequestPipelineStart(_logger, request);
                var response = await base.SendAsync(request, cancellationToken);
                Log.RequestPipelineEnd(_logger, response);

                return response;
            }
        }

        private static class Log
        {
            private static class EventIds
            {
                public static readonly EventId PipelineStart = new(100, "RequestPipelineStart");
                public static readonly EventId PipelineEnd = new(101, "RequestPipelineEnd");
            }

            private static readonly Func<ILogger, HttpMethod, Uri, IDisposable> _beginRequestPipelineScope =
                LoggerMessage.DefineScope<HttpMethod, Uri>(
                    "HTTP {HttpMethod} {Uri}");

            private static readonly Action<ILogger, HttpMethod, Uri, Exception> _requestPipelineStart =
                LoggerMessage.Define<HttpMethod, Uri>(
                    LogLevel.Trace,
                    EventIds.PipelineStart,
                    "Start processing HTTP request {HttpMethod} {Uri}");

            private static readonly Action<ILogger, HttpStatusCode, Exception> _requestPipelineEnd =
                LoggerMessage.Define<HttpStatusCode>(
                    LogLevel.Trace,
                    EventIds.PipelineEnd,
                    "End processing HTTP request - {StatusCode}");

            public static IDisposable BeginRequestPipelineScope(ILogger logger, HttpRequestMessage request)
            {
                return _beginRequestPipelineScope(logger, request.Method, request.RequestUri!);
            }

            public static void RequestPipelineStart(ILogger logger, HttpRequestMessage request)
            {
                _requestPipelineStart(logger, request.Method, request.RequestUri!, null!);
            }

            public static void RequestPipelineEnd(ILogger logger, HttpResponseMessage response)
            {
                _requestPipelineEnd(logger, response.StatusCode, null!);
            }
        }
    }
}