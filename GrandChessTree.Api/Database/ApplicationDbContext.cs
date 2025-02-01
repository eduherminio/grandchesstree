using GrandChessTree.Api.D10Search;
using Microsoft.EntityFrameworkCore;

namespace GrandChessTree.Api.Database
{

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        public DbSet<D10SearchItem> D10SearchItems { get; set; }
        public DbSet<D10SearchTask> D10SearchTasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<D10SearchTask>()
                .HasOne(t => t.SearchItem)
                .WithMany(i => i.SearchTasks)
                .HasForeignKey(t => t.SearchItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
