using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Department> Department => Set<Department>();

        public DbSet<Category> Category { get; set; } = null!;

        public DbSet<Idea> Idea { get; set; } = null!;

        public DbSet<React> React { get; set; } = null!;

        public DbSet<Comment> Comment { get; set; } = null!;

        public DbSet<FileOnFileSystem> FileOnFileSystem { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Department>()
                .HasIndex(u => u.Name)
                .IsUnique();

            builder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<React>()
                .HasOne(c => c.User)
                .WithMany(u => u.Reacts)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Idea>()
                .HasOne(c => c.User)
                .WithMany(u => u.Ideas)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ApplicationUser>()
               .HasOne(c => c.Department)
               .WithMany(u => u.Users)
               .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasMany(e => e.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        }
    }
}