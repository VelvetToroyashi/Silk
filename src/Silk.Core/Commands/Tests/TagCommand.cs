using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Silk.Core.Utilities.HelpFormatter;
using Silk.Data.MediatR;
using Silk.Data.Models;

namespace Silk.Core.Commands.Tests
{
    [Group("tag")]
    [Category(Categories.Server)]
    public class TagCommand : BaseCommandModule
    {
        private readonly IMediator _mediator;
        public TagCommand(IMediator mediator)
        {
            _mediator = mediator;
        }

        [GroupCommand]
        //TODO: Increment uses when using a tag; this should be done in the service layer
        public async Task Tag(CommandContext ctx, string tag)
        {
            Tag? dbTag = await _mediator.Send(new TagRequest.Get(tag.ToLower(), ctx.Guild.Id));
            if (dbTag is null)
            {
                await ctx.RespondAsync("Tag not found! :(");
            }
            else
            {
                await ctx.RespondAsync(dbTag.Content);
            }
        }

        [Command]
        public async Task Info(CommandContext ctx, string tag)
        {
            //This will be object? eventually, when I feel like making that tag service
            Tag? dbTag = await _mediator.Send(new TagRequest.Get(tag.ToLower(), ctx.Guild.Id));
            if (dbTag is null)
            {
                await ctx.RespondAsync("Tag not found! :(");
            }
            else
            {
                DiscordUser tagOwner = await ctx.Client.GetUserAsync(dbTag.OwnerId);
                var builder = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Blurple)
                    .WithAuthor(tagOwner.Username, iconUrl: tagOwner.AvatarUrl);
                //TODO: if (dbTag is TagAlias) { ... } 
                builder.WithTitle(dbTag.Name);
                builder.AddField("Created At:", dbTag.CreatedAt.ToUniversalTime().ToString("MM/dd/yyyy - h:mm UTC"));

                await ctx.RespondAsync(builder);

            }
        }
    }
}
