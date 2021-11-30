using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PluginMigrationHelper
{
	public class Program
	{
		static void Main(string[] args)
		{
			string ver = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
			Console.WriteLine($"PluginMigrationHelper v{ver} \n\n" +
			                  "USAGE: \n" +
			                  "dotnet ef migrations add <migration> [-c <context>] -s PluginMigrationHelper -p <plugin> -- <postgres | sqlite> -- <connection_string> \n\n" +
			                  "PLUGIN MUST BE REFERENCED BY THIS PROJECT!");
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder()
				.ConfigureServices(services =>
				{
					if (args[0] == "postgres")
						services.AddSingleton(new DbContextOptionsBuilder().UseNpgsql(args[1]).Options);
					else if (args[0] == "sqlite")
						services.AddSingleton(new DbContextOptionsBuilder().UseSqlite(args[1]).Options);
					else throw new NotSupportedException("Unknown database provider");
				});
		}
	}
}