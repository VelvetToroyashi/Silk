using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data
{
    public class GuildContext : DbContext
    {
        public GuildContext(DbContextOptions<GuildContext> options) : base(options) { }

        public DbSet<GuildEntity> Guilds { get; set; } = null!;

        /// <summary>
        ///     Users on a guild level; holds information and states that reflect such.
        /// </summary>
        public DbSet<UserEntity> Users { get; set; } = null!;

        public DbSet<InfractionEntity> Infractions { get; set; } = null!;
        
        public DbSet<CommandInvocationEntity> CommandInvocations { get; set; } = null!;

        public DbSet<GuildConfigEntity> GuildConfigs { get; set; } = null!;

        public DbSet<GuildModConfigEntity> GuildModConfigs { get; set; } = null!;

        public DbSet<TagEntity> Tags { get; set; } = null!;

        public DbSet<ReminderEntity> Reminders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(GuildContext).Assembly);
            base.OnModelCreating(builder);
        }
    }
}