using EDU_TEST.Data;
using EDU_TEST.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDU_TEST.Controllers;

[Authorize(Roles = "Student")]
public class TestPassingController : Controller
{
    private readonly ApplicationDbContext _context;

    public TestPassingController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Показ тесту студенту
    public async Task<IActionResult> Start(int testId)
    {
        var test = await _context.Tests
            .Include(t => t.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(t => t.Id == testId);

        if (test == null)
            return NotFound();

        return View(test);
    }

    // Прийом відповідей
    [HttpPost]
    public async Task<IActionResult> Submit(TestResultViewModel model)
    {
        int score = 0;

        foreach (var answer in model.Answers)
        {
            bool isCorrect = await _context.AnswerOptions
                .AnyAsync(a => a.Id == answer.SelectedAnswerId && a.IsCorrect);

            if (isCorrect)
                score++;
        }

        var result = new TestResult
        {
            StudentId = model.StudentId,
            TestId = model.TestId,
            Score = score,
            DateTaken = DateTime.Now
        };

        _context.TestResults.Add(result);
        await _context.SaveChangesAsync();

        return RedirectToAction("Result", new { id = result.Id });
    }

    // Показ результату
    public async Task<IActionResult> Result(int id)
    {
        var result = await _context.TestResults
            .Include(r => r.Test)
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (result == null)
            return NotFound();

        return View(result);
    }
}