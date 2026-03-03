using EDU_TEST.Models;
using EDU_TEST.Data;
using EDU_TEST.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EDU_TEST.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class QuestionController : Controller
{
    private readonly ApplicationDbContext _context;

    public QuestionController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> List(int testId)
    {
        var test = await _context.Tests
            .Include(t => t.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(t => t.Id == testId);

        if (test == null)
            return NotFound();

        return View(test);
    }

    // =========================
    // CREATE QUESTION
    // =========================
    [HttpGet]
    public async Task<IActionResult> Create(int? testId)
    {
        var tests = await _context.Tests.AsNoTracking().ToListAsync();
        ViewBag.Tests = new SelectList(tests, "Id", "Title", testId);

        return View(new Question
        {
            TestId = testId ?? 0
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Question model)
    {
        var tests = await _context.Tests.AsNoTracking().ToListAsync();
        ViewBag.Tests = new SelectList(tests, "Id", "Title", model.TestId);

        if (!ModelState.IsValid)
            return View(model);

        _context.Questions.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("List", new { testId = model.TestId });
    }

    // =========================
    // EDIT QUESTION + OPTIONS
    // =========================
    public async Task<IActionResult> Edit(int id)
    {
        var question = await _context.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            return NotFound();

        var model = new EditQuestionViewModel
        {
            QuestionId = question.Id,
            Text = question.Text,
            TestId = question.TestId,
            CorrectAnswerOptionId = question.Options.FirstOrDefault(o => o.IsCorrect)?.Id,
            Answers = question.Options.Select(o => new EditAnswerOptionViewModel
            {
                AnswerOptionId = o.Id,
                Text = o.Text,
                IsCorrect = o.IsCorrect
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditQuestionViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var question = await _context.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == model.QuestionId);

        if (question == null)
            return NotFound();

        question.Text = model.Text;

        // ✅ Рівно одна правильна відповідь
        if (model.Answers != null && model.Answers.Count > 0)
        {
            if (!model.CorrectAnswerOptionId.HasValue)
            {
                ModelState.AddModelError("", "Потрібно обрати рівно одну правильну відповідь.");
                return View(model);
            }

            foreach (var answerVm in model.Answers)
            {
                var option = question.Options.FirstOrDefault(o => o.Id == answerVm.AnswerOptionId);
                if (option == null) continue;

                option.Text = answerVm.Text;
                option.IsCorrect = (option.Id == model.CorrectAnswerOptionId.Value);
            }

            var correctOpt = question.Options.FirstOrDefault(o => o.Id == model.CorrectAnswerOptionId.Value);
            if (correctOpt != null)
                question.CorrectAnswer = correctOpt.Text;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("List", new { testId = question.TestId });
    }

    // =========================
    // DELETE QUESTION
    // =========================
    public async Task<IActionResult> Delete(int id)
    {
        var question = await _context.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            return NotFound();

        return View(question);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var question = await _context.Questions
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
            return NotFound();

        int testId = question.TestId;

        _context.AnswerOptions.RemoveRange(question.Options);
        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();

        return RedirectToAction("List", new { testId });
    }
}