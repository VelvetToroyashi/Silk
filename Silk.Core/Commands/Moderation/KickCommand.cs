#region

using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Silk.Core.Commands.Moderation.Utilities;
using Silk.Core.Database.Models;
using Silk.Core.Services;
using Silk.Core.Utilities;
using SilkBot.Extensions;
using SilkBot.Extensions.DSharpPlus;

#endregion

namespace Silk.Core.Commands.Moderation
{
    [Category(Categories.Mod)]
    public class KickCommand : BaseCommandModule
    {
        private readonly ILogger<KickCommand> _logger;
        private readonly DatabaseService _dbService;


        public KickCommand(ILogger<KickCommand> logger, DatabaseService dbService) => (_logger, _dbService) = (logger, dbService);

        [Command]
        [RequireFlag(UserFlag.Staff)]
        [RequireBotPermissions(Permissions.KickMembers)]
        [Description("Boot someone from the guild! Requires kick members permission.")]
        public async Task Kick(CommandContext ctx, [Description("The person to kick.")] DiscordMember user, [RemainingText] string? reason = null)
        {
            DiscordMember bot = ctx.Guild.CurrentMember;
            

            if (!ctx.Guild.CurrentMember.HasPermission(Permissions.KickMembers))
            {
                await ctx.RespondAsync(embed: EmbedHelper.CreateEmbed(ctx, "I don't have permission to kick members!",
                    DiscordColor.Red)).ConfigureAwait(false);
                return;
            }
            
            if (user.IsAbove(bot) || ctx.User == user)
            {
                bool isBot = user == bot;
                bool isOwner = user == ctx.Guild.Owner;
                bool isMod = user.HasPermission(Permissions.KickMembers);
                bool isAdmin = user.HasPermission(Permissions.Administrator);
                bool isCurrent = ctx.User == user;
                string errorReason = user.IsAbove(bot) switch
                {
                    _ when isBot     => "I wish I could kick myself, but I sadly cannot.",
                    _ when isOwner   => $"I can't kick the owner ({user.Mention}) out of their own server!",
                    _ when isMod     => $"I can't kick {user.Mention}! They're a moderator! ({user.Roles.Last().Mention})",
                    _ when isAdmin   => $"I can't kick {user.Mention}! They're an admin! ({user.Roles.Last().Mention})",
                    _ when isCurrent => "Very funny, I like you, but no, you can't kick yourself.",
                    _                => "Something has gone really wrong, and I don't know what *:(*"
                };

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                            .WithAuthor(ctx.Member.Username, ctx.Member.GetUrl(), ctx.Member.AvatarUrl)
                                            .WithDescription(errorReason)
                                            .WithColor(DiscordColor.Red);

                await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
            }
            else
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                                            .WithAuthor(ctx.Member.Username, ctx.Member.GetUrl(), ctx.Member.AvatarUrl)
                                            .WithColor(DiscordColor.Blurple)
                                            .WithThumbnail(ctx.Guild.IconUrl)
                                            .WithDescription($"You've been kicked from `{ctx.Guild.Name}`!")
                                            .AddField("Reason:", reason ?? "No reason has been attached to this infraction.");


                UserModel mUser = await _dbService.GetOrAddUserAsync(ctx.Guild.Id, user.Id);
                await _dbService.UpdateGuildUserAsync(mUser, u => u.Infractions.Add(new()
                {
                    Enforcer = ctx.User.Id, Reason = reason!, InfractionType = InfractionType.Kick,
                    InfractionTime = DateTime.Now, GuildId = ctx.Guild.Id
                }));
                
                try
                {
                    await user.SendMessageAsync(embed: embed).ConfigureAwait(false);
                }
                catch (InvalidOperationException)
                {
                    _logger.LogWarning("Couldn't DM member when notifying kick.");
                }

                await user.RemoveAsync(reason);

                GuildModel guildConfig = await _dbService.GetGuildAsync(ctx.Guild.Id);
                ulong logChannelID = guildConfig.Configuration.GeneralLoggingChannel;
                ulong logChannelValue = logChannelID == default ? ctx.Channel.Id : logChannelID;
                await ctx.Client.SendMessageAsync(await ctx.Client.GetChannelAsync(logChannelValue),
                    embed: new DiscordEmbedBuilder()
                           .WithAuthor(ctx.Member.DisplayName, "", ctx.Member.AvatarUrl)
                           .WithColor(DiscordColor.SpringGreen)
                           .WithDescription($":boot: Kicked {user.Mention}! (User notified with direct message)")).ConfigureAwait(false);
            }
        }
    }
}