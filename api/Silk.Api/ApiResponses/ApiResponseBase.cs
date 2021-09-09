using Newtonsoft.Json;

namespace Silk.Api.ApiResponses
{
	public class ApiResponseBase
	{
		[JsonIgnore]
		public int StatusCode { get; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Message { get; }

		public ApiResponseBase(int statusCode, string message = null)
		{
			StatusCode = statusCode;
			Message = message ?? GetDefaultMessageForStatusCode(statusCode);
		}

		private static string GetDefaultMessageForStatusCode(int statusCode)
		{
			return statusCode switch
			{
				404 => "Resource not found",
				500 => "An unhandled error occurred",
				_ => null
			};
		}
	}
}