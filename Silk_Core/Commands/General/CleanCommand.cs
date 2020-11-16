using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SilkBot.Utilities;
using System.Linq;
using System.Threading.Tasks;
using static SilkBot.Extensions.EmbedHelper;



namespace SilkBot.Commands.General
{


    [Category(Categories.General)]
    public class CleanCommand : BaseCommandModule
    {

        [HelpDescription("Cleans all bot commands", "**!clean <number of messages> [true/false]**")]
        [Command("Clean")]
        [RequirePermissions(Permissions.ManageMessages)]

        public async Task Clean(CommandContext ctx, [HelpDescription("The number of messages to clean. *Defaults to 10*.")] string NumberOfMessages = "10", [HelpDescription("Should I clear your initial messages as well? <true/false>")] bool deleteInitialMessage = false)
        {
            if (!int.TryParse(NumberOfMessages.Split(',').First(), out var numberOfMessages))
            {
                await ctx.RespondAsync(embed: CreateEmbed(ctx, "Invalid argument!", $"Sorry, but `{NumberOfMessages.Split(',').First()}` is not a valid number!", DiscordColor.Red));
            }

            var unfilteredMessages = ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, numberOfMessages).Result.ToList();
            await ctx.TriggerTypingAsync();
            if (deleteInitialMessage)
            {
                await Task.Run(async () =>
                {
                    for (int i = 0; i < unfilteredMessages.Count(); i++)
                    {
                        if (unfilteredMessages[i].Author.IsBot || unfilteredMessages[i].Content.StartsWith(ctx.Prefix))
                        {
                            await ctx.Channel.DeleteMessageAsync(unfilteredMessages[i]);
                        }
                    }
                });
                await ctx.Message.DeleteAsync();
                await ctx.RespondAsync(embed: CreateEmbed(ctx, null, $"Cleared {unfilteredMessages.Count} messages!"));
            }
            else
            {
                await Task.Run(async () =>
                {
                    foreach (var message in unfilteredMessages)
                    {
                        if (message.Author.IsBot)
                        {
                            await ctx.Channel.DeleteMessageAsync(message);
                        }
                    }
                });
                await ctx.Message.DeleteAsync();
                await ctx.RespondAsync(embed: CreateEmbed(ctx, null, $"Cleared {unfilteredMessages.Count} messages!"));
            }
        }




        [HelpDescription("Cleans all bot commands", "!clean 15")]
        [Command("Clean")]

        [RequirePermissions(Permissions.ManageMessages)]
        public async Task Clean(CommandContext ctx, [HelpDescription("The number of messages to clean, defaults to 10")] int numberOfMessages = 10)
        {
            var unfilteredMessages = ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, numberOfMessages).Result.ToList();
            await ctx.TriggerTypingAsync();
            await Task.Run(async () =>
            {
                foreach (var message in unfilteredMessages)
                {
                    if (message.Author.IsBot)
                    {
                        await ctx.Channel.DeleteMessageAsync(message);
                    }
                }
            });
            if (!ctx.Channel.IsPrivate)
            {
                await ctx.RespondAsync(embed: CreateEmbed(ctx, null, $"Cleared {unfilteredMessages.Count} messages!"));
            }
        }

        //TODO: Rewrite this attrocity.//
        [Command("Clean")]
        [RequirePermissions(Permissions.ManageMessages)]
        [HelpDescription("Cleans all bot commands", "!clean 10", "Cleans all bot commands", "!clean 10")]
        public async Task Clean(CommandContext ctx, [Description("The number of messages to clean. *Defaults to 10*.")] int numberOfMessages = 10, bool deleteInitialMessage = false)
        {
            var unfilteredMessages = ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, numberOfMessages).Result.ToList();
            await ctx.TriggerTypingAsync();
            if (deleteInitialMessage)
            {
                await Task.Run(async () =>
                {
                    for (int i = 0; i < unfilteredMessages.Count(); i++)
                    {
                        if (unfilteredMessages[i].Author.IsBot || unfilteredMessages[i].Content.StartsWith(ctx.Prefix))
                        {
                            await ctx.Channel.DeleteMessageAsync(unfilteredMessages[i]);
                        }
                    }
                });
                await ctx.RespondAsync(embed: CreateEmbed(ctx, null, $"Cleared {unfilteredMessages.Count} messages!"));
            }
            else
            {
                await Task.Run(async () =>
                {
                    foreach (var message in unfilteredMessages)
                    {
                        if (message.Author.IsBot)
                        {
                            await ctx.Channel.DeleteMessageAsync(message);
                        }
                    }
                });
                await ctx.RespondAsync(embed: CreateEmbed(ctx, null, $"Cleared {unfilteredMessages.Count} messages!"));
            }
        }

        [HelpDescription("Cleans all bot commands", "!clean 10")]
        [Command("Clean")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task Clean(CommandContext ctx, [Description("The number of messages to clean, defaults to 10")] string numOfMessagesString = "10")
        {
            if (!int.TryParse(numOfMessagesString.Split(',').First(), out var numberOfMessages))
            {
                await ctx.RespondAsync(embed: CreateEmbed(ctx, "Invalid argument!", $"Sorry, but `{numOfMessagesString.Split(',').First()}` is not a valid number!", DiscordColor.Red));
            }

            var unfilteredMessages = ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, numberOfMessages).Result.ToList();
            await ctx.TriggerTypingAsync();
            await Task.Run(async () =>
            {
                foreach (var message in unfilteredMessages)
                {
                    if (message.Author.IsBot)
                    {
                        await ctx.Channel.DeleteMessageAsync(message);
                    }
                }
            });
            await ctx.RespondAsync(embed: CreateEmbed(ctx, null, $"Cleared {unfilteredMessages.Count} messages!"));


        }




    }
}
