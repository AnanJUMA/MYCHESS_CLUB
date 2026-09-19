using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MYCHESS_CLUB.Models;
using System.Numerics;

namespace MYCHESS_CLUB.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Player> Players => Set<Player>();

        // Tournament/Entry/Round/Pairing/Game tables get added here in the
        // next phase — this DbContext is the single source of truth for
        // the whole app's data, Identity tables included.

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // A logged-in member can only have one Player profile
            builder.Entity<Player>()
                .HasIndex(p => p.IdentityUserId)
                .IsUnique();
        }
    }
}