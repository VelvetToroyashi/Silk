/*using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Silk.Data.Entities;
using Silk.Services.Interfaces;
using Silk.Types;
using Silk.Utilities;
using Silk.Utilities.HelpFormatter;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Commands.Moderation
{
    [ExcludeFromSlashCommands]
    [HelpCategory(Categories.Mod)]
    public class KickCommand : BaseCommandModule
    {
        private readonly IInfractionService _infractionService;
        public KickCommand(IInfractionService infractionService) => _infractionService = infractionService;


        [Command]
        
        [RequirePermissions(Permissions.KickMembers)]
        [Description("Boot someone from the guild!")]
        public async Task Kick(CommandContext ctx, DiscordMember user, [RemainingText] string reason = "Not given.")
        {
            DiscordMember bot = ctx.Guild.CurrentMember;

            if (!user.IsAbove(bot) && !user.IsAbove(ctx.Member) && ctx.User != user)
            {
                InfractionResult response = await _infractionService.KickAsync(user.Id, ctx.Guild.Id, ctx.User.Id, reason);
                string? message = response switch
                {
                    InfractionResult.FailedGuildHeirarchy         => "I can't kick that person due to role hierarchy!",
                    InfractionResult.FailedSelfPermissions        => "I don't have permission to kick members!", /* In retrospect, these should never happen, but. #1#
                    InfractionResult.SucceededWithNotification    => $"Kicked {Formatter.Bold($"{user.Username}#{user.Discriminator}")}  (Notified with direct message).",
                    InfractionResult.SucceededWithoutNotification => $"Kicked {Formatter.Bold($"{user.Username}#{user.Discriminator}")} (Unable to notify with Direct Message).",
                    _                                             => $"Unexpected response: {response}"
                };

                await ctx.RespondAsync(message);
            }
            else
            {
                DiscordEmbed embed = await CreateHierarchyEmbedAsync(ctx, bot, user);
                await ctx.RespondAsync(embed);
            }
        }

        private static async Task<DiscordEmbedBuilder> CreateHierarchyEmbedAsync(CommandContext ctx, DiscordMember bot, DiscordMember user)
        {
            bool isBot = user   == bot;
            bool isOwner = user == ctx.Guild.Owner;
            bool isMod = user.HasPermission(Permissions.KickMembers);
            bool isAdmin = user.HasPermission(Permissions.Administrator);
            bool isCurrent = ctx.User == user;
            string errorReason = user.IsAbove(bot) switch
            {
                _ when isBot     => "I wish I could kick myself, but I sadly cannot.",
                _ when isOwner   => $"I can't kick the owner ({user.Mention}) out of their own server!",
                _ when isMod     => $"I can't kick {user.Mention}! They're a staff! ({user.Roles.Last().Mention})",
                _ when isAdmin   => $"I can't kick {user.Mention}! They're staff! ({user.Roles.Last().Mention})",
                _ when isCurrent => "Very funny, I like you, but no, you can't kick yourself.",
                _                => "Something has gone really wrong, and I don't know what *:(*"
            };

            return new DiscordEmbedBuilder()
                  .WithAuthor(ctx.Member.Username, ctx.Member.GetUrl(), ctx.Member.AvatarUrl)
                  .WithDescription(errorReason)
                  .WithColor(DiscordColor.Red);
        }
    }
}*/
