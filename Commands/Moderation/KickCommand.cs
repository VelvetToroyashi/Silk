using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.ServerConfigurations;
using SilkBot.Utilities;
using System;
using System.Linq;
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
                var isBot = user == bot;
                var isOwner = user == ctx.Guild.Owner;
                var isMod   = user.HasPermission(Permissions.KickMembers);
                var isAdmin = user.HasPermission(Permissions.Administrator);
                string errorReason;
                _ = user.IsAbove(bot) switch
                {
                    true when isBot => errorReason = "I wish I could kick myself, but I sadly cannot.",
                    true when isOwner => errorReason = $"I can't kick the owner ({user.Mention}) out of their own server!",
                    true when isMod => errorReason = $"I can't kick {user.Mention}! They're a moderator! ({user.Roles.Last().Mention})",
                    true when isAdmin => errorReason = $"I can't kick {user.Mention}! They're an admin! ({user.Roles.Last().Mention})",

                    _ => errorReason = "`ROLE_CHECK_NULL_REASON.` That's all I know."
                };

                await ctx.RespondAsync(embed: EmbedHelper.CreateEmbed(ctx, errorReason, DiscordColor.Red));
            }
            else
            {


                var embed = new DiscordEmbedBuilder(EmbedHelper.CreateEmbed(ctx, $"You've been kicked from {ctx.Guild.Name}!", "")).AddField("Reason:", reason);


                try 
                {
                    await DMCommand.DM(ctx, user, embed);
                }
                catch(InvalidOperationException invalidop)
                {
                    ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "Silk!", invalidop.Message, DateTime.Now, invalidop);
                }
                
                await ctx.Member.RemoveAsync(reason);

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
