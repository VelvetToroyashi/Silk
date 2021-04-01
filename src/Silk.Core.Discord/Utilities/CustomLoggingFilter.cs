using System;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Silk.Core.Discord.Utilities
{
    /// <summary>
    /// Overriding Logging Filter for HttpClientFactory
    /// Source: https://www.stevejgordon.co.uk/httpclientfactory-asp-net-core-logging
    /// </summary>
    public class CustomLoggingFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly ILoggerFactory _loggerFactory;

        public CustomLoggingFilter(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            return builder =>
            {
                // Run other configuration first, we want to decorate.
                next(builder);

                var outerLogger =
                    _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler");

                builder.AdditionalHandlers.Insert(0, new CustomLoggingScopeHttpMessageHandler(outerLogger));
            };
        }
    }
}