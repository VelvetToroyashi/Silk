using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DSharpPlus.Entities;

namespace Silk.Extensions.DSharpPlus
{
    public static class MessageBuilderExtensions
    {
        /// <summary>
        ///     Remove all mentions from a message. They will still be rendered by Discord but affected mentions will not ping anyone.
        /// </summary>
        /// <param name="builder">The builder to remove mentions from.</param>
        /// <returns>The builder to allow more calls to be chained.</returns>
        public static DiscordMessageBuilder WithoutMentions(this DiscordMessageBuilder builder)
        {
            builder.WithAllowedMentions(Mentions.None);
            return builder;
        }


        /// <summary>
        ///     Clears the components of a <see cref="DiscordMessageBuilder" />.
        /// </summary>
        /// <param name="builder">The builder to clear.</param>
        /// <returns>The updated builder to chain calls with.</returns>
        public static DiscordMessageBuilder ClearComponents(this DiscordMessageBuilder builder)
        {
            object prop = typeof(DiscordMessageBuilder).GetField("_components", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(builder)!;
            (prop as List<DiscordActionRowComponent>)!.Clear();
            return builder;
        }

        /// <summary>
        ///     Remove a single user mention from a message without having to form <see cref="IMention" /> for every mention and remove it manually.
        /// </summary>
        /// <param name="builder">The builder to remove a mention from.</param>
        /// <param name="mention">The Id of the user to remove from the mentions.</param>
        /// <returns>The builder to allow more calls to be chained.</returns>
        /// <exception cref="ArgumentException">The message does not mention any users.</exception>
        public static DiscordMessageBuilder WithoutUserMention(this DiscordMessageBuilder builder, ulong mention)
        {
            IEnumerable<IMention> userMentions = builder.Mentions.Where(m => m is UserMention);
            if (userMentions.Count() is 0) throw new ArgumentException("Message does not contain any user mentions!");
            IEnumerable<IMention> userMention = userMentions.Where(m => ((UserMention) m).Id == mention);
            if (userMention.Count() is 0) return builder; // User wasn't in the mentions to begin with //
            builder.WithAllowedMentions(builder.Mentions.Except(userMention));
            return builder;
        }
        /// <summary>
        ///     Add role mentions to a message.
        /// </summary>
        /// <param name="builder">The builder to add role mentions to.</param>
        /// <param name="mentions">The mentions to add to the message.</param>
        /// <returns>The builder to allow more calls to be chained.</returns>
        public static DiscordMessageBuilder WithRoleMentions(this DiscordMessageBuilder builder, IEnumerable<ulong> mentions)
        {
            IEnumerable<IMention> mentionCollection = mentions.Select(r => (IMention) new RoleMention(r));
            IEnumerable<IMention> allMentions = builder.Mentions.Union(mentionCollection);
            builder.WithAllowedMentions(allMentions);
            return builder;
        }
        /// <summary>
        ///     Add user mentions to a message.
        /// </summary>
        /// <param name="builder">The builder to add user mentions to.</param>
        /// <param name="mentions">The mentions to add to the message.</param>
        /// <returns>The builder to allow more calls to be chained.</returns>
        public static DiscordMessageBuilder WithUserMentions(this DiscordMessageBuilder builder, IEnumerable<ulong> mentions)
        {
            IEnumerable<IMention> mentionCollection = mentions.Select(m => (IMention) new UserMention(m));
            if (builder.Mentions is null)
            {
                builder.WithAllowedMentions(mentions.Select(u => (IMention) new UserMention(u)));
            }
            else
            {
                IEnumerable<IMention> allMentions = builder.Mentions.Union(mentionCollection);
                builder.WithAllowedMentions(allMentions);
            }

            return builder;
        }
    }
}