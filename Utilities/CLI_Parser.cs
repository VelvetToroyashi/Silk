using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace SilkBot.Utilities
{
    public static class CLI_Parser
    {

        public static async void TakeCLICommand()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    var command = Console.ReadLine();
                    switch (command.Split(' ').First().ToLower())
                    {
                        case "status":
                            var client = Bot.Instance.Client;
                            try
                            {
                                client.UpdateStatusAsync(client.CurrentUser.Presence.Activity, (UserStatus)Enum.Parse(typeof(UserStatus), command.Split(' ').Last()));
                            }
                            catch (ArgumentException e)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.BackgroundColor = ConsoleColor.Black;
                                Console.WriteLine("Invalid state");
                                Console.ResetColor();
                                Console.WriteLine(e.Message);
                            }
                            break;

                        default:
                            Colorful.Console.WriteLine($"You borked somewhere. \"{command.Split(' ').First().ToLower()}\"", Color.IndianRed);
                            break;

                    }
                }


            });
        }




    }
}
