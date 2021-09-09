using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Silk.Api.Domain.Behaviors;

namespace Silk.Api.Domain
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddValidators(this IServiceCollection services)
		{
			services.AddValidatorsFromAssemblyContaining(typeof(ServiceCollectionExtensions));
			services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
			
			return services;
		}
	}
}