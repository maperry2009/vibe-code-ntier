using Microsoft.EntityFrameworkCore;

namespace NameDemo.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<GuestName> GuestNames => Set<GuestName>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuestName>(entity =>
        {
            entity.ToTable("guest_names");
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name).HasMaxLength(200).IsRequired();
            entity.Property(g => g.LastName).HasMaxLength(200).IsRequired();
            entity.Property(g => g.CreatedAt).IsRequired();
        });
    }
}
