using System;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;

namespace Silk.Utilities;

public static class IOperationContextExtensions
{
    public static IUser GetUser(this IOperationContext context)
    {
        return context switch
        {
            IMessageContext messageContext         => messageContext.Message.Author.Value,
            IInteractionContext interactionContext => interactionContext.Interaction.User.Value,
            _                                      => throw new InvalidOperationException("Unknown context type")
        };
    } 
    
    public static Snowflake GetChannelID(this IOperationContext context)
    {
        return context switch
        {
            IMessageContext messageContext         => messageContext.Message.ChannelID.Value,
            IInteractionContext interactionContext => interactionContext.Interaction.ChannelID.Value,
            _                                      => throw new InvalidOperationException("Unknown context type")
        };
    }
    
    public static Snowflake GetGuildID(this IOperationContext context)
    {
        return context switch
        {
            IMessageContext messageContext         => messageContext.GuildID.Value,
            IInteractionContext interactionContext => interactionContext.Interaction.GuildID.Value,
            _                                      => throw new InvalidOperationException("Unknown context type")
        };
    }
    
    public static Snowflake GetUserID(this IOperationContext context)
    {
        return context switch
        {
            IMessageContext messageContext         => messageContext.Message.Author.Value.ID,
            IInteractionContext interactionContext => interactionContext.Interaction.User.Value.ID,
            _                                      => throw new InvalidOperationException("Unknown context type")
        };
    }
    
    public static Snowflake GetMessageID(this IOperationContext context)
    {
        return context switch
        {
            IMessageContext messageContext         => messageContext.Message.ID.Value,
            IInteractionContext interactionContext => interactionContext.Interaction.Message.Value.ID,
            _                                      => throw new InvalidOperationException("Unknown context type")
        };
    }
}
