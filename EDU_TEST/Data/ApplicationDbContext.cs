using EDU_TEST.Models;
using Microsoft.EntityFrameworkCore;

namespace EDU_TEST.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<AnswerOption> AnswerOptions { get; set; }
        public DbSet<TestResult> TestResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.Email)
                    .IsRequired();

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.Role)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(u => u.PasswordHash)
                    .IsRequired();
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasMany(t => t.Questions)
                    .WithOne(q => q.Test)
                    .HasForeignKey(q => q.TestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(q => q.Id);

                entity.Property(q => q.Text)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(q => q.CorrectAnswer)
                    .IsRequired();

                entity.HasMany(q => q.Options)
                    .WithOne(o => o.Question)
                    .HasForeignKey(o => o.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

       
            modelBuilder.Entity<AnswerOption>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.Text)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(o => o.IsCorrect)
                    .IsRequired();
            });

           
            modelBuilder.Entity<TestResult>(entity =>
            {
                entity.HasKey(tr => tr.Id);

                entity.Property(tr => tr.Score)
                    .IsRequired();

                entity.HasOne(tr => tr.Student)
                    .WithMany(u => u.TestResults)
                    .HasForeignKey(tr => tr.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tr => tr.Test)
                    .WithMany()
                    .HasForeignKey(tr => tr.TestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
