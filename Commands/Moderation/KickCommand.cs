using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.ServerConfigurations;
using SilkBot.Utilities;
using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace SilkBot.Commands.Moderation
{
    public class KickCommand : BaseCommandModule
    {
        [Command("Kick")]
        [HelpDescription("Kick a user! *Note, caller requires moderator permission.*")]
        public async Task Kick(CommandContext ctx, DiscordMember user, [RemainingText] string reason = "Not Given.")
        {
            await ctx.Message.DeleteAsync();
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (!ctx.Member.HasPermission(Permissions.KickMembers) && !ctx.Member.IsOwner)
            {
                if (!ctx.Member.HasPermission(Permissions.Administrator)) 
                    await ctx.RespondAsync("Sorry, only moderators and administrators are allowed to kick people.");
                        return;
            }





            //var userRole = user.Roles.Last();

            if (user.IsAbove(bot))
            {
                var errorReason = "";
                if (user == bot)
                    errorReason = "I wish I could kick myself, but I sadly cannot.";
                else if (user == ctx.Guild.Owner)
                    errorReason = $"I can't kick the owner ({user.Mention}) out of their own server!";
                else if (user.HasPermission(Permissions.KickMembers))
                    errorReason = $"I can't kick {user.Mention}! They're a moderator! ({user.Roles.Last().Mention})";
                else if (user.HasPermission(Permissions.Administrator))
                    errorReason = $"I can't kick {user.Mention}! They're an admin! ({user.Roles.Last().Mention})";





                await ctx.Client.SendMessageAsync(ctx.Channel,
                                            embed: new DiscordEmbedBuilder()
                                                .WithAuthor(ctx.Member.DisplayName, "", ctx.Member.AvatarUrl)
                                                .WithColor(DiscordColor.Red)
                                                .WithDescription(errorReason == "" ? "Something went horribly wrong, and it's assuming you're below the user you're trying to kick." : errorReason)
                                                .WithFooter("Silk!", bot.AvatarUrl)
                                                .WithTimestamp(DateTime.UtcNow));



            }
            else
            {


                var embed = new DiscordEmbedBuilder(EmbedGenerator.CreateEmbed(ctx, $"You've been kicked from {ctx.Guild.Name}!", "")).AddField("Reason:", reason);


                try 
                {
                    await DMCommand.DM(ctx, user, embed);
                }
                catch(InvalidOperationException invalidop)
                {
                    ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "Silk!", invalidop.Message, DateTime.Now, invalidop);
                }
                

                await ctx.Guild.BanMemberAsync(user, 0, reason);
                await ctx.Guild.UnbanMemberAsync(user);



                ServerConfigurationManager.LocalConfiguration.TryGetValue(ctx.Guild.Id, out var guildConfig);
                var logChannelID = guildConfig?.LoggingChannel;
                var logChannelValue = logChannelID ?? ctx.Channel.Id;
                await ctx.Client.SendMessageAsync(await ServerInfo.Instance.ReturnChannelFromID(ctx, logChannelValue),
                    embed: new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Member.DisplayName, "", ctx.Member.AvatarUrl)
                    .WithColor(DiscordColor.SpringGreen)
                    .WithDescription($":boot: Kicked {user.Mention}! (User notified with direct message)")
                    .WithFooter("Silk")
                    .WithTimestamp(DateTime.Now));
            }
        }
    }
}
