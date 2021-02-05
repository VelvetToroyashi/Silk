using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Silk.Core.Constants;
using Silk.Core.Database.Models;
using Silk.Core.Services.Interfaces;
using Silk.Extensions;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.Commands.Server.Configuration
{
    public partial class BaseConfigCommand
    {
        public partial class BaseEditConfigCommand
        {
            [Group("moderation")]
            public class EditModerationConfigCommand : BaseCommandModule
            {
                //Does nested class ctor injection work???? //
                // Yes! //
                private readonly IDatabaseService? _dbService;
                private readonly IServiceCacheUpdaterService _cacheUpdaterService;

                [GroupCommand]
                public async Task EditConfig(CommandContext ctx)
                {
                    Command? cmd = ctx.CommandsNext.RegisteredCommands["help"];
                    CommandContext? context = ctx.CommandsNext.CreateContext(ctx.Message, null, cmd, "config edit moderation");
                    _ = ctx.CommandsNext.ExecuteCommandAsync(context);
                }

                [Command]
                public async Task Mute(CommandContext ctx, DiscordRole role)
                {
                    var builder = new DiscordMessageBuilder();
                    builder.WithoutMentions();
                    builder.WithReply(ctx.Message.Id);
                    
                    if (role.IsManaged)
                    {
                        builder.WithContent("This is a bot role!");
                        await ctx.RespondAsync(builder);
                        return;
                    }
                    if (role.Permissions.HasFlag(Permissions.SendMessages))
                    {
                        await ConfigureRoleAsync(ctx, builder, role);
                        return; 
                    }
                    builder.WithContent($"Alrighty, muted role is now {role.Mention}!");
                    await ctx.Message.CreateReactionAsync(Emojis.EConfirm);
                    await ctx.RespondAsync(builder);
                    
                    GuildConfig config = await _dbService!.GetConfigAsync(ctx.Guild.Id);
                    config.MuteRoleId = role.Id;
                    await _dbService.UpdateConfigAsync(config);
                    
                    _cacheUpdaterService.UpdateGuild(ctx.Guild.Id);
                }

                private async Task ConfigureRoleAsync(CommandContext ctx, DiscordMessageBuilder builder, DiscordRole role)
                {
                    builder.WithContent("This role doesn't restrict members! Would you like me to configure it for you?");
                    InteractivityExtension? interactivity = ctx.Client.GetInteractivity();
                    DiscordEmoji? no = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.Decline.ToEmojiId());
                    DiscordEmoji? yes = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.Confirm.ToEmojiId());
                    DiscordMessage? msg = await ctx.RespondAsync(builder);

                    await msg.CreateReactionAsync(yes);
                    await msg.CreateReactionAsync(no);
                    
                    var result = await interactivity.WaitForReactionAsync(x =>
                    {
                        bool isCorrectEmoji = x.Emoji == yes || x.Emoji == no;
                        bool isCorrectMessage = x.Message == msg;
                        return isCorrectEmoji && isCorrectMessage;
                    });

                    if (result.TimedOut)
                    {
                        builder.WithContent("I'll take your silence as a no :o");
                        await ctx.RespondAsync(builder);
                        return;
                    }
                    else if (result.Result.Emoji == no)
                    {
                        builder.WithContent("Alrighty, canceled.");
                        await ctx.RespondAsync(builder);
                        return;
                    }
                    else
                    {
                        builder.WithContent("Alright, give me a second to configure this role!");
                        DiscordMessage waitMessage = await ctx.RespondAsync(builder);
                        role.Permissions.Revoke(Permissions.SendMessages);
                        await ctx.Guild.GetRole(role.Id).ModifyAsync(x => x.Permissions = role.Permissions & ~Permissions.SendMessages);
                        builder.WithContent("Done!");
                        await waitMessage.ModifyAsync(builder);
                    }
                }

                public EditModerationConfigCommand(IDatabaseService dbService, IServiceCacheUpdaterService cacheUpdaterService)
                {
                    _dbService = dbService;
                    _cacheUpdaterService = cacheUpdaterService;
                }
            }
        }
    }
}