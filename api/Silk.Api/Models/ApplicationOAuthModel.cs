using System.Diagnostics.CodeAnalysis;

namespace Silk.Api.Models
{
	public sealed record ApplicationOAuthModel(ulong Id, [property: NotNull] string Secret);
}