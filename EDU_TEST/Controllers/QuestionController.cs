using EDU_TEST.Models;
using EDU_TEST.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace EDU_TEST.Controllers;


[Authorize(Roles = "Admin")]
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
            .FirstOrDefaultAsync(t => t.Id == testId);

        if (test == null)
            return NotFound();

        return View(test);
    }


    public IActionResult Create(int testId)
    {
        return View(new Question { TestId = testId });
    }

    [HttpPost]
    public async Task<IActionResult> Create(Question model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _context.Questions.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("List", new { testId = model.TestId });
    }


    public async Task<IActionResult> Edit(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            return NotFound();

        return View(question);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Question model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _context.Questions.Update(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("List", new { testId = model.TestId });
    }
    
    public async Task<IActionResult> Delete(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            return NotFound();

        return View(question);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question == null)
            return NotFound();

        int testId = question.TestId;

        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();

        return RedirectToAction("List", new { testId });
    }
}