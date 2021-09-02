using System.Net;

namespace Silk.Api.ApiResponses.Spotify
{
    public class ApiResponse<TResponse>
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Raw { get; set; }
        public TResponse Response { get; set; }
    }
}