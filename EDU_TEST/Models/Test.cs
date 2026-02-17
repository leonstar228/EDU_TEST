using System.ComponentModel.DataAnnotations;

namespace EDU_TEST.Models
{

    public class Test
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва тесту обов'язкова")]
        [StringLength(200, ErrorMessage = "Максимум 200 символів")]
        public string Title { get; set; } = string.Empty;

        public ICollection<Question>? Questions { get; set; }
    }

}