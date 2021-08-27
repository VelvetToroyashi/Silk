using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Silk.Api.Domain.Feature.Infractions;

namespace Silk.Api.Controllers
{
	[ApiController]
	[Route("api/v1/[controller]")]
	public sealed class InfractionsController : ControllerBase
	{
		private readonly IMediator _mediator;
		public InfractionsController(IMediator mediator) => _mediator = mediator;

		[HttpGet(Name = "GetInfraction")]
		public async Task<IActionResult> GetInfraction(GetInfraction.Request request)
		{
			var infraction = await _mediator.Send(request);

			if (infraction is null)
				return NotFound();

			else return Ok(infraction);
		}

		/// <response code="200">A guilds's infractions were successfully queried</response>
		/// <response code="404">The guild was not registered with the API.</response>
		[HttpGet("guild/{guild}")]
		public async Task<IActionResult> GetGuildInfractions(ulong guild)
		{
			return new StatusCodeResult(501);
		}
		
		/// <summary>
		/// Test
		/// </summary>
		/// <response code="200">A user's infractions were successfully queried</response>
		[HttpGet("user/{user}")]
		public async Task<IActionResult> GetUserInfractions(ulong user)
		{
			return new StatusCodeResult(501);
		}
		
		[HttpPost]
		public async Task<IActionResult> AddInfraction(AddInfraction.Request request)
		{
			var created = await _mediator.Send(request);
			
			return Created(nameof(GetInfraction), created);
		}

		[HttpPatch]
		public async Task<IActionResult> PatchInfraction(UpdateInfraction.Request request)
		{
			var res = await _mediator.Send(request);

			if (res is null)
				return NotFound();
			
			if (res.Changed)
				return Ok();
			
			return NoContent();
		}
	}
}