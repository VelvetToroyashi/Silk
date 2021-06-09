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
using Silk.Core.Data.Models;
using Silk.Core.Services;

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


            [SlashCommand("list", "List server tags!")]
            public async Task Tag(InteractionContext ctx)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                IEnumerable<Tag> tags = await _tags.GetGuildTagsAsync(ctx.Guild.Id);
                if (!tags.Any())
                {
                    await ctx.EditResponseAsync(new() {Content = "This server doesn't have any tags!"});
                    return;
                }

                string allTags = string.Join('\n', tags
                    .Select(t =>
                    {
                        var s = $"`{t.Name}`";
                        if (t.OriginalTagId is not null)
                        {
                            s += $" → `{t.OriginalTag!.Name}`";
                        }
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
            public async Task Tag(
                InteractionContext ctx, [Option("tag-name", "whats the name of the tag you want to use?")]
                string tag)
            {
                if (ctx.Guild is null)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() {Content = "Sorry, but you need to be in a guild to use tags!", IsEphemeral = true});
                    return;
                }

                Tag? dbtag = await _tags.GetTagAsync(tag, ctx.Guild.Id);
                if (dbtag is null)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() {Content = "Sorry, but I don't see a tag by that name!", IsEphemeral = true});
                    return;
                }

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() {Content = dbtag.Content});
                await _mediator.Send(new UpdateTagRequest(dbtag.Name, ctx.Guild.Id) {Uses = dbtag.Uses + 1});
            }

            [SlashCommand("by", "See all a given user's tags!")]
            public async Task UserTags(
                InteractionContext ctx, [Option("user", "Who's tags do you want to see?")]
                DiscordUser user)
            {
                if (ctx.Guild is null)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new() {Content = "Sorry, but you need to be on a server to use this!"});
                    return;
                }
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new() {IsEphemeral = true});

                var tags = await _tags.GetUserTagsAsync(user.Id, ctx.Guild.Id);

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


        }
    }
}