using System.Collections.Generic;
using MediatR;
using Silk.Data.Models;

namespace Silk.Data.MediatR
{
    public class CommandInvokeRequest
    {
        public record Add(ulong UserId, ulong? GuildId, string CommandName) : IRequest;

        public record GetByUserId(ulong UserId) : IRequest<IEnumerable<CommandInvocation>>;

        public record GetByGuildId(ulong GuildId) : IRequest<IEnumerable<CommandInvocation>>;

        public record GetMostUsed : IRequest<IEnumerable<CommandInvocation>>;
    }
    
}