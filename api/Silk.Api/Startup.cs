using System;
using System.IO;
using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Silk.Api.Data;
using Silk.Api.Domain;
using Silk.Api.Domain.Services;
using Silk.Api.Helpers;
using AuthenticationService = Silk.Api.Services.AuthenticationService;
using ServiceCollectionExtensions = Silk.Api.Domain.ServiceCollectionExtensions;

namespace Silk.Api
{
	public class Startup
	{
		public IConfiguration Configuration { get; }
		
		public Startup(IConfiguration configuration) => Configuration = configuration;
		
		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{

			services.Configure<ApiSettings>(Configuration.GetSection("Api"));
			
			services.AddMediatR(typeof(ServiceCollectionExtensions));
			services.AddValidators();
			services.AddDbContext<ApiContext>(d => d
				.UseNpgsql("Server=localhost; Username=silk; Password=silk; Database=api"), ServiceLifetime.Transient, ServiceLifetime.Transient);

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => Configuration.Bind("Jwt", options));

			services.AddRouting(r => r.LowercaseUrls = true);

			services.AddTransient<CryptoHelper>();

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
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApiContext ctx, IServiceProvider services)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Silk.Api v1"));
			}
			ctx.Database.Migrate();
			services.GetService<AuthenticationService>();
			Console.WriteLine("Pulled service???");
			
			app.UseMiddleware<InternalServerErrorWrapper>();
			app.UseMiddleware<AuthenticationMiddleware>();
			
			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}