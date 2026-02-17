using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDU_TEST.Models
{

    public class TestResult
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int StudentId { get; set; }

        public virtual User? Student { get; set; }

        [Required]
        [ForeignKey(nameof(Test))]
        public int TestId { get; set; }

        public virtual Test? Test { get; set; }

        [Range(0, 100, ErrorMessage = "Оцінка повинна бути від 0 до 100")]
        public double Score { get; set; }

        public DateTime DateTaken { get; set; } = DateTime.Now;
    }

}