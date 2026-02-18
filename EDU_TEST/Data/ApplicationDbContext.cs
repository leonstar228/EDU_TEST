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


            // --- Seed Data ---
            // Users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FirstName = "Іван", LastName = "Петренко", Email = "ivan@test.com", PasswordHash = "hash1", Role = "Student", CreatedAt = DateTime.UtcNow },
                new User { Id = 2, FirstName = "Марія", LastName = "Коваль", Email = "maria@test.com", PasswordHash = "hash2", Role = "Student", CreatedAt = DateTime.UtcNow }
            );

            // Tests
            modelBuilder.Entity<Test>().HasData(
                new Test { Id = 1, Title = "Географія" },
                new Test { Id = 2, Title = "Математика" },
                new Test { Id = 3, Title = "Історія" },
                new Test { Id = 4, Title = "Інформатика" }
            );

            // Questions
            modelBuilder.Entity<Question>().HasData(
                new Question { Id = 1, Text = "Столиця Франції?", CorrectAnswer = "Париж", TestId = 1 },
                new Question { Id = 2, Text = "Столиця Італії?", CorrectAnswer = "Рим", TestId = 1 },
                new Question { Id = 3, Text = "5 × 6 = ?", CorrectAnswer = "30", TestId = 2 },
                new Question { Id = 4, Text = "12 + 8 = ?", CorrectAnswer = "20", TestId = 2 },
                new Question { Id = 5, Text = "Хто був князем Київської Русі?", CorrectAnswer = "Володимир Великий", TestId = 3 },
                new Question { Id = 6, Text = "Що таке CLR у .NET?", CorrectAnswer = "Common Language Runtime", TestId = 4 }
            );

            // AnswerOptions
            modelBuilder.Entity<AnswerOption>().HasData(
                new AnswerOption { Id = 1, Text = "Париж", IsCorrect = true, QuestionId = 1 },
                new AnswerOption { Id = 2, Text = "Ліон", IsCorrect = false, QuestionId = 1 },
                new AnswerOption { Id = 3, Text = "Марсель", IsCorrect = false, QuestionId = 1 },
                new AnswerOption { Id = 4, Text = "Рим", IsCorrect = true, QuestionId = 2 },
                new AnswerOption { Id = 5, Text = "Мілан", IsCorrect = false, QuestionId = 2 },
                new AnswerOption { Id = 6, Text = "Неаполь", IsCorrect = false, QuestionId = 2 },
                new AnswerOption { Id = 7, Text = "30", IsCorrect = true, QuestionId = 3 },
                new AnswerOption { Id = 8, Text = "25", IsCorrect = false, QuestionId = 3 },
                new AnswerOption { Id = 9, Text = "35", IsCorrect = false, QuestionId = 3 },
                new AnswerOption { Id = 10, Text = "20", IsCorrect = true, QuestionId = 4 },
                new AnswerOption { Id = 11, Text = "18", IsCorrect = false, QuestionId = 4 },
                new AnswerOption { Id = 12, Text = "22", IsCorrect = false, QuestionId = 4 },
                new AnswerOption { Id = 13, Text = "Володимир Великий", IsCorrect = true, QuestionId = 5 },
                new AnswerOption { Id = 14, Text = "Ярослав Мудрий", IsCorrect = false, QuestionId = 5 },
                new AnswerOption { Id = 15, Text = "Святослав", IsCorrect = false, QuestionId = 5 },
                new AnswerOption { Id = 16, Text = "Common Language Runtime", IsCorrect = true, QuestionId = 6 },
                new AnswerOption { Id = 17, Text = ".NET Compiler", IsCorrect = false, QuestionId = 6 },
                new AnswerOption { Id = 18, Text = "Virtual Machine", IsCorrect = false, QuestionId = 6 }
            );
        }
    }
}
