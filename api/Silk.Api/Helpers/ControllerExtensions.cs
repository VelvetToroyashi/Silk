using Microsoft.AspNetCore.Mvc;

namespace Silk.Api.Helpers
{
	public static class ControllerExtensions
	{
		public static IActionResult NotImplemented(this Controller _) => new StatusCodeResult(501);
	}
}