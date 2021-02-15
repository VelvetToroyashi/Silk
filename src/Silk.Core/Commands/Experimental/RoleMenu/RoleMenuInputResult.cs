using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Silk.Core.Constants;
using Silk.Extensions;

namespace Silk.Core.Commands.Experimental.RoleMenu
{
    /// <summary>
    /// Helper object to abstract interactivity and provide convenience methods for gathering user input when making a role-menu.
    /// </summary>
    public sealed record RoleMenuInputResult
    {
        /// <summary>
        /// The result of a GetInput method, which remains null if the operation did not succeed.
        /// </summary>
        public object? Result { get; private set; }

        /// <summary>
        /// Whether or not the operation succeeded. Timing out will cause it to fail.
        /// </summary>
        public bool Succeeded { get; private set; }

        public static List<DiscordMessage> Messages { get; } = new();
        private const string Canceled = "Canceled (User Requested).";
        private const string TimedOut = "Canceled (Timed out).";

        private RoleMenuInputResult() { } // prevent instantiation //

        public static async Task<RoleMenuInputResult> GetReactionAsync(InteractivityExtension interactivity, CommandContext ctx, DiscordMessage message)
        {
            var result = new RoleMenuInputResult();
            var builder = new DiscordMessageBuilder();

            var inputResult = await interactivity.WaitForReactionAsync(message, ctx.User);
            if (inputResult.TimedOut)
            {
                builder.WithContent(TimedOut);
                var msg = await ctx.RespondAsync(builder);
                Messages.Add(msg);
            }
            else
            {
                result.Result = inputResult.Result.Emoji;
                result.Succeeded = true;
            }

            return result;
        }

        /// <summary>
        /// Get a confirmation from user input with a check and cross reaction.
        /// </summary>
        /// <returns>A <see cref="RoleMenuInputResult"/> with the user input, or false, if it timed out.</returns>
        public static async Task<RoleMenuInputResult> GetConfirmationAsync(InteractivityExtension interactivity, CommandContext ctx, DiscordMessage message)
        {
            DiscordEmoji confirm = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.Confirm.ToEmojiId());
            DiscordEmoji decline = DiscordEmoji.FromGuildEmote(ctx.Client, Emojis.Decline.ToEmojiId());

            await message.CreateReactionAsync(Emojis.EConfirm);
            await message.CreateReactionAsync(Emojis.EDecline);

            var result = new RoleMenuInputResult();
            var builder = new DiscordMessageBuilder().WithReply(ctx.Message.Id);
            var inputResult = await interactivity.WaitForReactionAsync(m => m.Emoji == confirm || m.Emoji == decline, message, ctx.User);

            if (inputResult.TimedOut)
            {
                builder.WithContent(TimedOut);
                var msg = await ctx.RespondAsync(builder);
                Messages.Add(msg);
            }
            else if (inputResult.Result.Emoji == decline)
            {
                builder.WithContent(Canceled);
                var msg = await ctx.RespondAsync(builder);
                Messages.Add(msg);
            }
            else
            {
                result.Succeeded = true;
            }

            return result;
        }

        /// <summary>
        /// Wait for user input for 30 seconds, else return a failed <see cref="RoleMenuInputResult"/>.
        /// </summary>
        /// <returns>A <see cref="RoleMenuInputResult"/> with the user input, or false, if it timed out.</returns>
        public static async Task<RoleMenuInputResult> GetInputAsync(InteractivityExtension interactivity, CommandContext ctx, DiscordMessage message)
        {
            var result = new RoleMenuInputResult();
            var inputResult = await interactivity.WaitForMessageAsync(m => m.Author == ctx.User);

            if (inputResult.TimedOut)
            {
                var builder = new DiscordMessageBuilder()
                    .WithReply(message.Id)
                    .WithContent(TimedOut);
                var msg = await ctx.RespondAsync(builder);
                Messages.Add(msg);
            }
            else
            {
                result.Result = inputResult.Result;
                result.Succeeded = true;
            }
            return result;

        }
    }
}