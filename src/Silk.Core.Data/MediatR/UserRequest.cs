using System.Collections;
using System.Collections.Generic;
using MediatR;
using Silk.Core.Data.Models;

namespace Silk.Core.Data.MediatR
{
    public class UserRequest
    {
        /// <summary>
        /// Gets a user from the database, or null, if it does not exist.
        /// </summary>
        public record Get(ulong GuildId, ulong UserId) : IRequest<User?>;

        /// <summary>
        /// Updates a user in the database.
        /// </summary>
        public record Update(ulong GuildId, ulong UserId, UserFlag? Flags = null) : IRequest<User>;
        
        /// <summary>
        /// Adds a user to the database.
        /// </summary>
        public record Add(ulong GuildId, ulong UserId, UserFlag? Flags = null) : IRequest<User>;

        /// <summary>
        /// IMPLEMENTATION COMING SOON™.
        /// </summary>
        public record AddOrUpdate(ulong GuildId, ulong UserId, UserFlag? Flags = null) : IRequest<User>;

        /// <summary>
        /// Updates users in the database en masse.
        /// </summary>
        public record BulkUpdate(ulong GuildId, IEnumerable<User> Users) : IRequest<IEnumerable<User>>;
        
        /// <summary>
        /// Gets a user from the database, and creates one if it does not exist.
        /// </summary>
        public record GetOrCreate(ulong GuildId, ulong UserId) : IRequest<User>
        {
            public UserFlag? Flags { get; init; }
        }
        
    }
}