using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Silk.Core.Data.MediatR.Users;
using Silk.Core.Data.Models;
using Silk.Extensions;

namespace Silk.Core.Discord.Utilities
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class RequireFlagAttribute : CheckBaseAttribute
    {
        private static readonly HashSet<ulong> _cachedMembers = new();

        /// <summary>
        ///     Check for a requisite flag from the database, and execute if check passes.
        /// </summary>
        /// <param name="requisiteUserFlag">The required flag for the command to run; this flag is ignored when run in a help context</param>
        /// <param name="requireGuild">Restrict command usage to guild as well as requisite flag. Defaults to false.</param>
        public RequireFlagAttribute(UserFlag requisiteUserFlag, bool requireGuild = false)
        {
            RequisiteUserFlag = requisiteUserFlag;
            RequireGuild = requireGuild;
        }
        public bool RequireGuild { get; }
        public UserFlag RequisiteUserFlag { get; }

        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (help) return help;
            if (ctx.Guild is null && RequireGuild) return false; //Is a private channel and requires a Guild//
            if (_cachedMembers.Contains(ctx.User.Id) && RequireGuild) return true;

            IMediator mediator = ctx.Services.CreateScope().ServiceProvider.Get<IMediator>();
            User? member = await mediator.Send(new GetUserRequest(ctx.Guild!.Id, ctx.User.Id));

            if (member is null) return false;
            if (member.Flags.HasFlag(UserFlag.Staff)) _cachedMembers.Add(member.Id);
            return member.Flags.HasFlag(UserFlag.Staff);
        }
    }
}