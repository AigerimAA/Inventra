using Inventra.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventra.Infrastructure.Persistence.Configurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.ImageUrl)
                .HasMaxLength(2000);

            builder.Property(i => i.Version)
                .IsRowVersion();

            foreach (var prop in new[]
            {
                nameof(Inventory.CustomString1Name), nameof(Inventory.CustomString2Name),
                nameof(Inventory.CustomString3Name), nameof(Inventory.CustomInt1Name),
                nameof(Inventory.CustomInt2Name), nameof(Inventory.CustomInt3Name),
                nameof(Inventory.CustomText1Name), nameof(Inventory.CustomText2Name),
                nameof(Inventory.CustomText3Name), nameof(Inventory.CustomBool1Name),
                nameof(Inventory.CustomBool2Name), nameof(Inventory.CustomBool3Name),
                nameof(Inventory.CustomLink1Name), nameof(Inventory.CustomLink2Name),
                nameof(Inventory.CustomLink3Name)
            })
            {
                builder.Property(prop).HasMaxLength(200);
            }

            builder.HasOne(i => i.Owner)
                .WithMany(u => u.OwnedInventories)
                .HasForeignKey(i => i.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(i => i.Category)
                .WithMany(c => c.Inventories)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(i => i.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(i => i.InventoryTags)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(i => i.AccessList)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Navigation(i => i.Comments)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
