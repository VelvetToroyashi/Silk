using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.ModelConfigurations.cs
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            //builder.Property(u => u.DatabaseId).ValueGeneratedOnAdd();
            builder.HasKey(u => u.DatabaseId);
            builder.HasMany(u => u.Infractions).WithOne(i => i.User);
        }
    }
}