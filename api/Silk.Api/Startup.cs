using System;
using System.IO;
using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Silk.Api.Data;
using Silk.Api.Domain;
using Silk.Api.Domain.Feature.Infractions;

namespace Silk.Api
{
	public class Startup
	{
		public IConfiguration Configuration { get; }
		
		public Startup(IConfiguration configuration) => Configuration = configuration;
		
		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddDbContext<ApiContext>((a, d) =>
			{
				d.UseNpgsql("Server=localhost; Username=silk; Password=silk; Database=api");
				//a.GetService<ApiContext>()!.Database.Migrate();
			});

			services.AddValidators();
			services.AddMediatR(typeof(AddInfraction));

			services.AddRouting(r => r.LowercaseUrls = true);


			services.AddControllers();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Silk.Api", Version = "v1" });
				c.CustomSchemaIds(t => t.ToString());
				
				// Set the comments path for the Swagger JSON and UI.
				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				c.IncludeXmlComments(xmlPath);
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApiContext ctx)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Silk.Api v1"));
			}
			ctx.Database.Migrate();
			
			app.UseMiddleware<InternalServerErrorWrapper>();
			
			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}