using Microsoft.Extensions.Hosting;

namespace Silk.Core.Logic
{
    public static class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) => Startup.ConfigureServices(Startup.CreateHostBuilder());
    }
}