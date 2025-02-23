using GracelineCMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GracelineCMS.Infrastructure.Repository
{
    public class AppDbContext : DbContext
    {
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<AuthCode> AuthCodes { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
