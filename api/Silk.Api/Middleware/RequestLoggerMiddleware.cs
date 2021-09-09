using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Silk.Api
{
	public class RequestLoggerMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<RequestLoggerMiddleware> _logger;

		public RequestLoggerMiddleware(RequestDelegate next, ILogger<RequestLoggerMiddleware> logger)
		{
			_next = next;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Invoke(HttpContext context)
		{
			_logger.LogInformation("Request to {Method} {Url}{Query}",context.Request.Method, context.Request.Path, context.Request.QueryString);
			await _next.Invoke(context);
			
		}
	}
}