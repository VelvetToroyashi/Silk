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
		
		[HttpPost]
		public async Task<IActionResult> AddInfraction(AddInfraction.Request request)
		{
			var created = await _mediator.Send(request);
			
			return Created(nameof(GetInfraction), created);
		}
	}
}