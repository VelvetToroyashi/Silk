using System;
using MediatR;
using Silk.Data.Models;

namespace Silk.Data.MediatR
{
    public class GlobalUserRequest
    {
        public record Get(ulong UserId) : IRequest<GlobalUser?>;

        public record Add(ulong UserId) : IRequest<GlobalUser>
        {
            public int? Cash { get; init; }
        }

        public record Update(ulong UserId) : IRequest<GlobalUser>
        {
           public int Cash { get; init; }
           public DateTime LastCashOut { get; init; }
        }

        public record GetOrCreate(ulong UserId) : IRequest<GlobalUser>;
    }
}