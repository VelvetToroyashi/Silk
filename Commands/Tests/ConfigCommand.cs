using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using static SilkBot.Bot;

namespace SilkBot.Commands.Tests
{
    public class ConfigCommand : BaseCommandModule
    {
        private readonly List<ulong> emojis = new List<ulong>()  {
                                                            blobcat0, blobcat1,
                                                            blobcat2, blobcat3,
                                                            blobcat4, blobcat5,
                                                            blobcat6, blobcat7,
                                                            blobcat8, blobcat9,
                                                        };
        private const ulong blobcat1 = 751968316158902303, blobcat2 = 751968132469489685,
                            blobcat3 = 751968150660055150, blobcat4 = 751968169710715030,
                            blobcat5 = 751968193555333251, blobcat6 = 751968212530364456,
                            blobcat7 = 751968266670440468, blobcat8 = 751968266670440468,
                            blobcat9 = 751968277634220132, blobcat0 = 751968296580153415;



        [Command("configure")]
        public async Task GuildConfigurationCommand(CommandContext ctx)
        {
            var configurationMessage = await ctx.RespondAsync("1: Toggle whitelisting invites, 2: Log message edits and deletions, 3: Log member joins and leaves, 4: Log role changes, 5: Set mute role, 6: Set log channel");
            AddNumberedReaction(configurationMessage, 6);

            var configMessageRaction = await configurationMessage.WaitForReactionAsync(ctx.User, TimeSpan.FromSeconds(120));
            if (configMessageRaction.TimedOut)
            {
                await configurationMessage.DeleteAsync("Timed out.");
                return;
            }
            switch (configMessageRaction.Result.Emoji.Id) 
            {
                case 751968316158902303:
                    WhiteListInvites(configurationMessage);
                    break;
                case 751968132469489685:
                    ToggleLogMessageChanges(configurationMessage);
                    break;
                case 751968150660055150:
                    LogMemberJoinLeave(configurationMessage);
                    break;
                case 751968169710715030:
                    LogRoleChanges(configurationMessage);
                    break;
                case 751968193555333251:
                    SetMute(configurationMessage);
                    break;
                case 751968212530364456:
                    SetGeneralLogChannel(configurationMessage);
                    break;
                default: break;
            }
        }

        private async Task WhiteListInvites(DiscordMessage configurationMessage)
        {
            await configurationMessage.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(Instance.Client, 751968316158902303), configurationMessage.Author);
        }

        private async Task ToggleLogMessageChanges(DiscordMessage configurationMessage)
        {
            await configurationMessage.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(Instance.Client, 751968132469489685), configurationMessage.Author);
        }

        private async Task LogMemberJoinLeave(DiscordMessage configurationMessage)
        {
            await configurationMessage.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(Instance.Client, 751968150660055150), configurationMessage.Author);
        }

        private async Task LogRoleChanges(DiscordMessage configurationMessage)
        {
            await configurationMessage.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(Instance.Client, 751968169710715030), configurationMessage.Author);
        }

        private async Task SetMute(DiscordMessage configurationMessage)
        {
            await configurationMessage.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(Instance.Client, 751968193555333251), configurationMessage.Author);
        }

        private async Task SetGeneralLogChannel(DiscordMessage configurationMessage)
        {
            await configurationMessage.DeleteReactionAsync(DiscordEmoji.FromGuildEmote(Instance.Client, 751968212530364456), configurationMessage.Author);
        }

        private async void AddNumberedReaction(DiscordMessage msg, int numbers)
        {
            for (int i = 0; i < numbers; i++)
                await msg.CreateReactionAsync(DiscordEmoji.FromGuildEmote(Instance.Client, emojis[i]));
        }
    }
}
