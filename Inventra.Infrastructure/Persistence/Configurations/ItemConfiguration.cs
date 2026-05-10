using Inventra.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventra.Infrastructure.Persistence.Configurations
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.CustomId)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.Version)
                .IsRowVersion();

            builder.Property(i => i.CustomString1Value).HasMaxLength(500);
            builder.Property(i => i.CustomString2Value).HasMaxLength(500);
            builder.Property(i => i.CustomString3Value).HasMaxLength(500);

            builder.Property(i => i.CustomLink1Value).HasMaxLength(2000);
            builder.Property(i => i.CustomLink2Value).HasMaxLength(2000);
            builder.Property(i => i.CustomLink3Value).HasMaxLength(2000);

            builder.Property(i => i.CustomInt1Value).HasPrecision(18, 4);
            builder.Property(i => i.CustomInt2Value).HasPrecision(18, 4);
            builder.Property(i => i.CustomInt3Value).HasPrecision(18, 4);

            builder.HasIndex(i => new { i.InventoryId, i.CustomId })
                .IsUnique();

            builder.HasOne(i => i.Inventory)
                .WithMany(inv => inv.Items)
                .HasForeignKey(i => i.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(i => i.CreatedBy)
                .WithMany()
                .HasForeignKey(i => i.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Navigation(i => i.Likes)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
