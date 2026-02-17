
﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDU_TEST.Models;

public class AnswerOption
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Текст варіанту відповіді обов'язковий")]
    [StringLength(500, MinimumLength = 1, 
        ErrorMessage = "Текст має бути від 1 до 500 символів")]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    [Required]
    [ForeignKey(nameof(Question))]
    public int QuestionId { get; set; }

    public virtual Question? Question { get; set; } 

}