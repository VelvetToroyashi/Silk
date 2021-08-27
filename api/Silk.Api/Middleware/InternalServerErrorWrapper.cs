using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Silk.Api.ApiResponses;

namespace Silk.Api
{
	public class InternalServerErrorWrapper 
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<InternalServerErrorWrapper> _logger;

		public InternalServerErrorWrapper(RequestDelegate next, ILogger<InternalServerErrorWrapper> logger)
		{
			_next = next;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task Invoke(HttpContext context)
		{
			try
			{
				await _next.Invoke(context);
			}
			catch (Exception ex)
			{
				await HandleException(context, ex);
			}

			if (!context.Response.HasStarted)
			{
				context.Response.ContentType = "application/json";

				var response = new ApiResponseBase(context.Response.StatusCode);
				var json = JsonConvert.SerializeObject(response);

				await context.Response.WriteAsync(json);
			}
		}

		private async Task HandleException(HttpContext context, Exception ex)
		{
			if (ex is ValidationException ve)
			{
				context.Response.ContentType = "application/json";
				context.Response.StatusCode = 400;

				var errors = ve.Errors.Select(x => x.ErrorMessage);

				var response = new ApiBadRequestResponse(errors);
				var json = JsonConvert.SerializeObject(response);

				await context.Response.WriteAsync(json);
				
			}
			else
			{
				_logger.LogError(500, ex, ex.Message);

				context.Response.ContentType = "application/json";
				var response = new ApiResponseBase(context.Response.StatusCode);
				var json = JsonConvert.SerializeObject(response);
				await context.Response.WriteAsync(json);
			}
		}
		
	}
}