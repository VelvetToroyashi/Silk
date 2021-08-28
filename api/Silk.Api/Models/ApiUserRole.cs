using Microsoft.AspNetCore.Identity;

namespace Silk.Api.Models
{
	public sealed class ApiUserRole : IdentityRole
	{
		public ApiUserRole() : base() { }
		public ApiUserRole(string name) : base(name) { }
	}
}