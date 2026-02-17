using System.ComponentModel.DataAnnotations;

 namespace EDU_TEST.Models
 {
     public class User
     {
         [Key] public int Id { get; set; }

         [Required(ErrorMessage = "Ім'я обов'язкове")]
         [StringLength(50, ErrorMessage = "Максимум 50 символів")]
         public string FirstName { get; set; } = string.Empty;

         [Required(ErrorMessage = "Прізвище обов'язкове")]
         [StringLength(50, ErrorMessage = "Максимум 50 символів")]
         public string LastName { get; set; } = string.Empty;

         [Required(ErrorMessage = "Email обов'язковий")]
         [EmailAddress(ErrorMessage = "Некоректний формат email")]
         public string Email { get; set; } = string.Empty;

         [Required] public string PasswordHash { get; set; } = string.Empty;

         [Required] [StringLength(20)] public string Role { get; set; } = "Student";

         public DateTime CreatedAt { get; set; } = DateTime.Now;

         public ICollection<TestResult>? TestResults { get; set; }
     }
 }

