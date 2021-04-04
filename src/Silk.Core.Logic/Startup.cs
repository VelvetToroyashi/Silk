using System.Threading.Tasks;
using Silk.Core.Discord;

namespace Silk.Core.Logic
{
    public class Startup
    {
        public static async Task Main()
        {
            _ = Task.Run(async () => await Program.Start());

            await Task.Delay(6000); // It should be started by now, I think. //
            var services = Program.Services;
        }
    }
}