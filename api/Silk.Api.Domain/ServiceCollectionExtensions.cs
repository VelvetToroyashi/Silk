using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Silk.Api.Domain.Behaviors;
using Silk.Api.Domain.Feature.Infractions;

namespace Silk.Api.Domain
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddValidators(this IServiceCollection services)
		{
			services.AddTransient<IValidator<AddInfraction.Request>, AddInfraction.Validator>();
			services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
			
			return services;
		}
	}
}