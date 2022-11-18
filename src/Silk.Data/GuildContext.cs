using System.Runtime.CompilerServices;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Remora.Rest.Core;
using Silk.Data.Entities;

[assembly: InternalsVisibleTo("Silk")]
//[assembly: MediatorOptions(ServiceLifetime = ServiceLifetime.Transient)]
namespace Silk.Data;

public class GuildContext : DbContext
{
    public GuildContext(DbContextOptions<GuildContext> options) : base(options) { }
    
    public DbSet<UserEntity>        Users     { get; set; }
    public DbSet<GuildEntity>       Guilds    { get; set; }
    public DbSet<ReminderEntity>    Reminders { get; set; }
    public DbSet<UserHistoryEntity> Histories { get; set; }
    
    public DbSet<GuildUserEntity>         GuildUsers         { get; set; }
    public DbSet<InfractionEntity>        Infractions        { get; set; }
    public DbSet<GuildConfigEntity>       GuildConfigs       { get; set; }
    public DbSet<GuildGreetingEntity>     GuildGreetings     { get; set; }
    public DbSet<PendingGreetingEntity>   PendingGreetings   { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(GuildContext).Assembly);
        
        base.OnModelCreating(builder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        base.ConfigureConventions(builder);

        builder.Properties<Snowflake>().HaveConversion(typeof(SnowflakeConverter));
        builder.Properties<Snowflake?>().HaveConversion(typeof(NullableSnowflakeConverter));
    }
}