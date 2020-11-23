using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using SilkBot.Commands.General;
using SilkBot.Utilities;
using static SilkBot.Bot;

namespace SilkBot.Commands.Bot
{
    public sealed class MessageCreationHandler
    {
        private readonly TicketService _ticketService;
        private readonly GuildConfigCacheService _guildCache;
        private readonly ILogger<MessageCreationHandler> _logger;

        public MessageCreationHandler(DiscordShardedClient sc, TicketService ts, GuildConfigCacheService guildCache,
            ILogger<MessageCreationHandler> logger)
        {
            _ticketService = ts;
            _guildCache = guildCache;
            _logger = logger;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task OnMessageCreate(DiscordClient c, MessageCreateEventArgs e)
#pragma warning restore CS1998
        {
            //Bots shouldn't be running commands.    
            if (e.Author.IsBot)
            {
                CommandTimer.Stop();
                return;
            }

            _ = Task.Run(async () =>
            {
                //Silk specific, but feel free to use the same code, modified to fit your DB or other prefix-storing method.
                if (e.Guild is not null)
                {
                    //Feel free to enable this for your own bot; Icba to do ratelimiting so people don't spam :^)
                    if (e.Message.Content.Trim().ToLower().StartsWith("@someone"))
                    {
                        //try
                        //{
                        //    string msgContent = e.Message.Content;
                        //    await e.Message.DeleteAsync();
                        //    var rand = new Random();
                        //    var users = e.Channel.Users.Where(u => !u.IsBot).ToList();
                        //    var selecteduser = users[rand.Next(users.Count())];
                        //    msgContent = msgContent.Replace("@someone", selecteduser.Mention);
                        //    await e.Channel.SendMessageAsync(msgContent + $"    ||This message was sent by {e.Author.Username} ({e.Author.Id})||");
                        //}
                        //catch { }
                    }

                    GuildConfiguration config = await _guildCache.GetConfigAsync(e.Guild.Id);
                    CommandTimer.Restart();
                    var sw = Stopwatch.StartNew();
                    CheckForInvite(e, config);
                    sw.Stop();
                    _logger.LogInformation($"Checked for information in {sw.ElapsedMilliseconds} ms.");
                }

                if (_ticketService.CheckForTicket(e.Message.Channel, e.Message.Author.Id))
                    await _ticketService.RespondToBlindTicketAsync(c, e.Message.Author.Id, e.Message.Content);
                CommandTimer.Stop();
            });
        }

        private void CheckForInvite(MessageCreateEventArgs e, GuildConfiguration config)
        {
            if (config.WhitelistsInvites)
            {
                string messageContent = e.Message.Content;
                if (messageContent.Contains("discord.gg/") || messageContent.Contains("discord.com/invite/"))
                {
                    Match inviteLinkMatched =
                        Regex.Match(messageContent.ToLower(), @"(discord\.gg\/.+)", RegexOptions.IgnoreCase)
                        ?? Regex.Match(messageContent.ToLower(), @"(discord\.com\/invite\/.+)",
                            RegexOptions.IgnoreCase);

                    if (!inviteLinkMatched.Success) return;

                    string inviteLink = string.Join("", messageContent
                                                        .Skip(inviteLinkMatched.Index)
                                                        .TakeWhile(c => c != ' '))
                                              .Replace("discord.com/invite", "discord.gg");
                    if (!config.WhiteListedLinks.Any(link => link.Link == inviteLink))
                        e.Message.DeleteAsync().GetAwaiter();
                }
            }
        }
    }
}