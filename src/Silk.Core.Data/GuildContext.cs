using Microsoft.EntityFrameworkCore;
using Silk.Core.Data.Entities;

namespace Silk.Core.Data
{
    public class GuildContext : DbContext
    {
        public GuildContext(DbContextOptions<GuildContext> options) : base(options) { }
        
        public DbSet<TagEntity>               Tags               { get; set; }
        public DbSet<UserEntity>              Users              { get; set; }
        public DbSet<GuildEntity>             Guilds             { get; set; }
        public DbSet<ReminderEntity>          Reminders          { get; set; }
        public DbSet<InfractionEntity>        Infractions        { get; set; }
        public DbSet<GuildConfigEntity>       GuildConfigs       { get; set; }
        public DbSet<GuildModConfigEntity>    GuildModConfigs    { get; set; }
        public DbSet<CommandInvocationEntity> CommandInvocations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(GuildContext).Assembly);
            base.OnModelCreating(builder);
        }
    }
}