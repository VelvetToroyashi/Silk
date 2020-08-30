using System.Threading.Tasks;

namespace SilkBot
{
    public class Program
    {
        public static async Task Main() => await Bot.Instance.RunBotAsync();
    }
}