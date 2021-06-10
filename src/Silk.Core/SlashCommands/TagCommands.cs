using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using MediatR;
using Silk.Core.Data.MediatR.Tags;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Data.Models;
using Silk.Core.Services;
using Silk.Extensions.DSharpPlus;

namespace Silk.Core.SlashCommands
{
    public class TagCommands : SlashCommandModule
    {
        [SlashCommandGroup("tag", "Tag related commands!")]
        public class TagCommandGroup : SlashCommandModule
        {
            private readonly IMediator _mediator;
            private readonly TagService _tags;
            public TagCommandGroup(TagService tags, IMediator mediator)
            {
                _tags = tags;
                _mediator = mediator;
            }

            [SlashCommand("raw", "View the raw content of a tag!")]
            public async Task Raw(InteractionContext ctx, [Option("tag", "The tag to view")] string tag)
            {
                await ctx.CreateThinkingResponseAsync();
            }
            
            [SlashCommand("list", "List server tags!")]
            public async Task List(InteractionContext ctx)
            {
                await ctx.CreateThinkingResponseAsync();

                IEnumerable<Tag> tags = await _tags.GetGuildTagsAsync(ctx.Interaction.GuildId.Value);
                if (!tags.Any())
                {
                    await ctx.EditResponseAsync(new() {Content = "This server doesn't have any tags!"});
                    return;
                }

                string allTags = string.Join('\n', tags
                    .Select(t =>
                    {
                        var s = $"`{t.Name}`";
                        if (t.OriginalTagId is not null) s += $" → `{t.OriginalTag!.Name}`";
                        return s;
                    }));
                DiscordEmbedBuilder? builder = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Blurple)
                    .WithTitle($"Tags in {ctx.Guild.Name}:")
                    .WithFooter($"Silk! | Requested by {ctx.User.Id}");

                if (tags.Count() < 10)
                {
                    builder.WithDescription(allTags);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                }
                else
                {
                    InteractivityExtension? interactivity = ctx.Client.GetInteractivity();

                    IEnumerable<Page>? pages = interactivity.GeneratePagesInEmbed(allTags, SplitType.Line, builder);
                    await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);
                }
            }

            [SlashCommand("use", "Display a tag!")]
            public async Task Use(InteractionContext ctx, [Option("tag-name", "whats the name of the tag you want to use?")] string tag)
            {

                if (ctx.Interaction.GuildId is null)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() {Content = "Sorry, but you need to be in a guild to use tags!", IsEphemeral = true});
                    return;
                }

                Tag? dbtag = await _tags.GetTagAsync(tag, ctx.Interaction.GuildId.Value);
                if (dbtag is null)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() {Content = "Sorry, but I don't see a tag by that name!", IsEphemeral = true});
                    return;
                }

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() {Content = dbtag.Content});
                await _mediator.Send(new UpdateTagRequest(dbtag.Name, ctx.Interaction.GuildId.Value) {Uses = dbtag.Uses + 1});
            }

            [SlashCommand("by", "See all a given user's tags!")]
            public async Task ListByUser(InteractionContext ctx, [Option("user", "Who's tags do you want to see?")] DiscordUser user)
            {
                await ctx.CreateThinkingResponseAsync();
                
                if (ctx.Interaction.GuildId is null)
                {
                    await ctx.EditResponseAsync(new() {Content = "Sorry, but you need to be on a server to use this!"});
                    return;
                }

                IEnumerable<Tag> tags = await _tags.GetUserTagsAsync(user.Id, ctx.Interaction.GuildId.Value);

                if (!tags.Any())
                {
                    await ctx.EditResponseAsync(new() {Content = "Looks like that user doesn't actually have any tags!"});
                    return;
                }

                string allTags = string.Join('\n', tags
                    .Select(t =>
                    {
                        var s = $"`{t.Name}`";

                        if (t.OriginalTagId is not null)
                            s += $" → `{t.OriginalTag!.Name}`";

                        return s;
                    }));

                DiscordEmbedBuilder? builder = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Blurple)
                    .WithTitle($"Tags for {user.Username}:")
                    .WithFooter($"Silk! | Requested by {ctx.User.Id}");

                if (tags.Count() < 10)
                {
                    builder.WithDescription(allTags);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
                }
                else
                {
                    InteractivityExtension? interactivity = ctx.Client.GetInteractivity();

                    IEnumerable<Page>? pages = interactivity.GeneratePagesInEmbed(allTags, SplitType.Line, builder);
                    await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);
                }

            }

            [SlashCommand("server", "Show the tags on this server!")]
            public async Task Server(InteractionContext ctx)
            {
                await ctx.CreateThinkingResponseAsync();
                
                if (ctx.Interaction.GuildId is null)
                {
                    await ctx.EditResponseAsync(new() {Content = "Sorry, but you need to be on a server to use this!"});
                    return;
                }

                IEnumerable<Tag> tags = await _tags.GetGuildTagsAsync(ctx.Interaction.GuildId.Value);
                if (!tags.Any())
                {
                    await ctx.EditResponseAsync(new() {Content = "This server doesn't have any tags! You could be the first."});
                    return;
                }
                
                string allTags = string.Join('\n', tags.Take(30)
                    .Select(t =>
                    {
                        var s = $"`{t.Name}`";
                        if (t.OriginalTagId is not null) s += $" → `{t.OriginalTag!.Name}`";
                        return s;
                    }));
                
                DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Blurple)
                    .WithTitle($"Tags in {ctx.Guild.Name}:")
                    .WithDescription(allTags + (tags.Count() > 30 ? $"\n+ {tags.Count() - 30} more..." : ""))
                    .WithFooter($"Silk! | Requested by {ctx.User.Id}");

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(builder));
            }

            [SlashCommand("claim", "Claim a tag. Owner must not be in server.")]
            public async Task Claim(InteractionContext ctx, [Option("tag", "What tag do you want to claim? **Requires staff**")] string tag)
            {
                await ctx.CreateThinkingResponseAsync();
                
                if (ctx.Interaction.GuildId is null)
                {
                    await ctx.EditResponseAsync(new() {Content = "Sorry, but you have to be on a guild to use this!"});
                    return;
                }
                if (ctx.Guild is null)
                {
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder()
                            .WithContent("Sorry, but to claim tags, I need to see if the member exists on the server." +
                                         "\nThis isn't possible as I wasn't invited with the bot scope, and thus can't access the members.")
                            .AddComponents(new DiscordLinkButtonComponent($"https://discord.com/oauth2/authorize?client_id={ctx.Client.CurrentApplication.Id}&permissions=502656214&scope=bot%20applications.commands", "Invite with bot scope")));
                    return;
                }

                Tag? dbTag = await _tags.GetTagAsync(tag, ctx.Interaction.GuildId.Value);
                
                if (dbTag is null)
                {
                    await ctx.EditResponseAsync(new() {Content = "Sorry, but that tag doesn't exist!"});
                    return;
                }
                
                var exists = ctx.Guild.Members.TryGetValue(dbTag.OwnerId, out _);
                
                User? user = await _mediator.Send(new GetUserRequest(ctx.Interaction.GuildId.Value, ctx.User.Id));
                var staff = user?.Flags.HasFlag(UserFlag.Staff) ?? false;

                if (!staff)
                {
                    await ctx.EditResponseAsync(new() {Content = "Sorry, but you're not allowed to claim tags!"});
                    return;
                }
                
                if (exists)
                {
                    await ctx.EditResponseAsync(new() {Content = "The tag owner is still on the server."});
                    return;
                }

                await _tags.ClaimTagAsync(tag, ctx.Interaction.GuildId.Value, ctx.User.Id);
                await ctx.EditResponseAsync(new() {Content = "Successfully claimed tag!"});

            }
        }
    }
}