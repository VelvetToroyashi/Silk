using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Silk.Core.Database.Models;

namespace Silk.Core.Database.ModelConfigurations.cs
{
    public class UserModelConfiguration : IEntityTypeConfiguration<UserModel>
    {

        public void Configure(EntityTypeBuilder<UserModel> builder)
        {
            builder.Property(u => u.DatabaseId).ValueGeneratedOnAdd();
            builder.HasKey(u => u.DatabaseId);
            builder.HasMany(u => u.Infractions).WithOne(i => i.User);
        }
    }
}