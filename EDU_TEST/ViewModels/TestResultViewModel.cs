namespace EDU_TEST.Models;

public class TestResultViewModel
{
    public int TestId { get; set; }
    public int StudentId { get; set; }
    public List<UserAnswer> Answers { get; set; } = new();
}

public class UserAnswer
{
    public int QuestionId { get; set; }
    public int SelectedAnswerId { get; set; }
}