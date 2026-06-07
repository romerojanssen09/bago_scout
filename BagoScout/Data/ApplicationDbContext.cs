using BagoScout.Models;
using Microsoft.EntityFrameworkCore;

namespace BagoScout.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<UserSkill> UserSkills { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<JobPreference> JobPreferences { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<EmailMessage> EmailMessages { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Skill entity
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.HasIndex(e => e.SkillName).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure UserSkill entity
            modelBuilder.Entity<UserSkill>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.SkillId }).IsUnique();
                entity.Property(e => e.AddedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure UserPreference entity
            modelBuilder.Entity<UserPreference>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.SkillId }).IsUnique();
                entity.Property(e => e.AddedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Job entity
            modelBuilder.Entity<Job>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Configure Application entity
            modelBuilder.Entity<Application>(entity =>
            {
                entity.HasIndex(e => new { e.JobId, e.SeekerId }).IsUnique();
                entity.Property(e => e.AppliedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Status).HasDefaultValue("Pending");
            });

            // Configure EmailMessage entity
            modelBuilder.Entity<EmailMessage>(entity =>
            {
                entity.Property(e => e.SentAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsRead).HasDefaultValue(false);
            });

            // Configure Education entity
            modelBuilder.Entity<Education>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Experience entity
            modelBuilder.Entity<Experience>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsCurrentJob).HasDefaultValue(false);
            });

            // Configure Message entity
            modelBuilder.Entity<Message>(entity =>
            {
                entity.Property(e => e.SentAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsRead).HasDefaultValue(false);
            });

            // Configure JobPreference entity
            modelBuilder.Entity<JobPreference>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Seed default skills
            modelBuilder.Entity<Skill>().HasData(
                new Skill { SkillId = 1, SkillName = "Web Development", CreatedAt = DateTime.UtcNow },
                new Skill { SkillId = 2, SkillName = "Mobile Development", CreatedAt = DateTime.UtcNow },
                new Skill { SkillId = 3, SkillName = "Graphic Design", CreatedAt = DateTime.UtcNow },
                new Skill { SkillId = 4, SkillName = "Data Entry", CreatedAt = DateTime.UtcNow },
                new Skill { SkillId = 5, SkillName = "Customer Service", CreatedAt = DateTime.UtcNow },
                new Skill { SkillId = 6, SkillName = "Sales", CreatedAt = DateTime.UtcNow },
                new Skill { SkillId = 7, SkillName = "Marketing", CreatedAt = DateTime.UtcNow },
                new Skill { SkillId = 8, SkillName = "Accounting", CreatedAt = DateTime.UtcNow }
            );
        }
    }
}
