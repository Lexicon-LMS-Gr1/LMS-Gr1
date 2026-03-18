using Domain.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ActivityType> ActivityTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Student → Course (en student går en kurs)
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Course)
                .WithMany(c => c.Students)
                .HasForeignKey(u => u.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            // 2. Course → Teacher (en kurs har en lärare)
            builder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany()
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Module → Course
            builder.Entity<Module>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Modules)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. Activity → Module
            builder.Entity<Activity>()
                .HasOne(a => a.Module)
                .WithMany(m => m.Activities)
                .HasForeignKey(a => a.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // 5. Activity → ActivityType
            builder.Entity<Activity>()
                .HasOne(a => a.ActivityType)
                .WithMany(at => at.Activities)
                .HasForeignKey(a => a.ActivityTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}