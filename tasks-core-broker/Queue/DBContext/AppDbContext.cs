
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using TaskQueue.Models;

namespace TaskQueue.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskResult> TaskResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureTaskItemEntity(modelBuilder);
            ConfigureTaskResultEntity(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private void ConfigureTaskItemEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Data)
                      .IsRequired();

                entity.Property(t => t.Ttl)
                      .HasDefaultValue(60000);

                entity.Property(t => t.Status)
                      .HasConversion<int>();
            });
        }

        private void ConfigureTaskResultEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskResult>(entity =>
            {
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Status)
                      .HasConversion<int>();
            });
        }
    }
}
