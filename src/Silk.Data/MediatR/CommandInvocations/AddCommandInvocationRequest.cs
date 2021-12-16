using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Silk.Data.Entities;

namespace Silk.Data.MediatR.CommandInvocations;

/// <summary>
///     Request for adding a <see cref="CommandInvocationEntity" />.
/// </summary>
public record AddCommandInvocationRequest(string CommandName) : IRequest;

/// <summary>
///     The default handler for <see cref="AddCommandInvocationRequest" />.
/// </summary>
public class AddCommandInvocationHandler : IRequestHandler<AddCommandInvocationRequest>
{
    private readonly GuildContext _db;

    public AddCommandInvocationHandler(GuildContext db) => _db = db;

    public async Task<Unit> Handle(AddCommandInvocationRequest request, CancellationToken cancellationToken)
    {
        CommandInvocationEntity command = new()
        {
            CommandName    = request.CommandName,
            InvocationTime = DateTime.UtcNow
        };

        _db.CommandInvocations.Add(command);

        await _db.SaveChangesAsync(cancellationToken);
        return new();
    }
}