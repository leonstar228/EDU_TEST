using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDU_TEST.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Текст питання обов'язковий")]
        [StringLength(500, ErrorMessage = "Максимум 500 символів")]
        public string Text { get; set; } = string.Empty;

        [Required(ErrorMessage = "Правильна відповідь обов'язкова")]
        public string CorrectAnswer { get; set; } = string.Empty;

        public ICollection<AnswerOption>? Options { get; set; }

        [Required]
        [ForeignKey(nameof(Test))]
        public int TestId { get; set; }

        public virtual Test? Test { get; set; }
    }

}