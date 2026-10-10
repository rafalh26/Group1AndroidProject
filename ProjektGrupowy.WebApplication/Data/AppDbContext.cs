using Microsoft.EntityFrameworkCore;
using ProjektGrupowy.WebApplication.Models;

namespace ProjektGrupowy.WebApplication.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> users { get; set; } = null!;
        public DbSet<Track> Tracks { get; set; } = null!;
        public DbSet<GeoLog> GeoLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("users");
                b.HasKey(u => u.id);
                b.HasIndex(u => u.nick).IsUnique();
                b.Property(u => u.nick).IsRequired();
                b.Property(u => u.pass_hash).IsRequired();
            });

            modelBuilder.Entity<Track>(b =>
            {
                b.ToTable("Tracks");
                b.HasKey(t => t.id);
                b.Property(t => t.nick).IsRequired();
            });

            modelBuilder.Entity<GeoLog>(b =>
            {
                b.ToTable("GeoLogs");
                b.HasKey(g => g.id);
                b.Property(g => g.locationDate).HasColumnName("locationDate");
            });
        }
    }
}
