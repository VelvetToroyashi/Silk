using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Commands.Groups;
using Remora.Discord.Gateway.Extensions;

namespace Silk.Extensions.Remora
{
	public static class RemoraIServiceCollectionExtensions
	{
		public static IServiceCollection AddResponders(this IServiceCollection collection, Assembly assembly)
		{
			var types = assembly
				.GetTypes()
				.Where(t => t.IsClass && !t.IsAbstract && t.IsResponder());

			foreach (var type in types)
				collection.AddResponder(type);

			return collection;
		}

		public static IServiceCollection AddCommands(this IServiceCollection collection, Assembly assembly)
		{
			var types = assembly
				.GetExportedTypes()
				.Where(t => t.IsClass && !t.IsNested && !t.IsAbstract && t.IsAssignableTo(typeof(CommandGroup)));

			foreach (var type in types)
				collection.AddCommandGroup(type);

			return collection;
		}
	}
}