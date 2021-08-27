using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Silk.Api.Controllers
{
	[ApiController]
	[Helpers.Authorize]
	[Route("api/[controller]")]
	public class AuthenticationController : ControllerBase
	{
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Authenticate()
		{


			return Ok();
		}

		[HttpDelete]
		public async Task<IActionResult> RevokeAccount()
		{


			return NoContent();
		}
	}
}