using Inventra.Domain.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventra.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
    {
        public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Inventory> Inventories => Set<Inventory>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<InventoryTag> InventoryTags => Set<InventoryTag>();
        public DbSet<InventoryAccess> InventoryAccesses => Set<InventoryAccess>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Like> Likes => Set<Like>();
        public DbSet<CustomIdFormat> CustomIdFormats => Set<CustomIdFormat>();
        public DbSet<CustomIdElement> CustomIdElements => Set<CustomIdElement>();
        public DbSet<InventorySequence> InventorySequence => Set<InventorySequence>();
        public DbSet<InventoryApiToken> InventoryApiTokens => Set<InventoryApiToken>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}