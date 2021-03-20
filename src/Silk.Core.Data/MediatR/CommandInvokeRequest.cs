using System.Collections.Generic;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR
{
    public class CommandInvokeRequest
    {
        public record Add(ulong UserId, ulong? GuildId, string CommandName) : IRequest;

        public record GetByUserId(ulong UserId) : IRequest<IEnumerable<CommandInvocation>>;

        public record GetByGuildId(ulong GuildId) : IRequest<IEnumerable<CommandInvocation>>;

        public record GetMostUsed : IRequest<IEnumerable<CommandInvocation>>;
    }
    
}