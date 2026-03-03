using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EDU_TEST.ViewModels
{
    public class EditQuestionViewModel
    {
        public int QuestionId { get; set; }

        public int TestId { get; set; }

        [Required(ErrorMessage = "Введіть текст питання")]
        public string Text { get; set; } = string.Empty;

        // Id варіанту відповіді, який є правильним (має бути рівно один)
        public int? CorrectAnswerOptionId { get; set; }

        public List<EditAnswerOptionViewModel> Answers { get; set; } = new();
    }

    public class EditAnswerOptionViewModel
    {
        public int AnswerOptionId { get; set; }

        [Required(ErrorMessage = "Текст відповіді обов'язковий")]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }
    }
}