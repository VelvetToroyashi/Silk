namespace SilkBot
{
    using System.Threading.Tasks;
    public class Program
    {
        public static async Task Main()
            => await Bot.Instance.RunBotAsync();
    } 
}