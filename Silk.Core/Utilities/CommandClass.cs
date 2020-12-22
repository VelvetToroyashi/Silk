#region

using DSharpPlus.CommandsNext;
using Microsoft.EntityFrameworkCore;
using Silk.Core.Database;

#endregion

namespace Silk.Core.Utilities
{
    public abstract class CommandClass : BaseCommandModule
    {
        private readonly IDbContextFactory<SilkDbContext> _db;

        public CommandClass(IDbContextFactory<SilkDbContext> _db)
        {
            this._db = _db;
        }

        public SilkDbContext GetDbContext()
        {
            return _db.CreateDbContext();
        }
    }
}