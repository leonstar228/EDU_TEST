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

    public IActionResult List(int testId)
    {
        var test = _context.Tests.Include(t => t.Questions).FirstOrDefault(t => t.Id == testId);
        if (test == null) return NotFound();

        return View(test);

    }

    public IActionResult Create(int testId)
    {
        var question = new Question
        {
            TestId = testId
        };
        return View(question);
    }

    [HttpPost]
    public IActionResult Create(Question model)
    {
        if (!ModelState.IsValid) return View(model);

        _context.Questions.Add(model);
        _context.SaveChanges();

        return RedirectToAction("List", new { testId = model.TestId });
    }

    public IActionResult Edit(int id)
    {
        var question = _context.Questions.FirstOrDefault(q => q.Id == id);
        if (question == null)
            return NotFound();

        return View(question);
    }

    [HttpPost]
    public IActionResult Edit(Question model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _context.Questions.Update(model);
        _context.SaveChanges();

        return RedirectToAction("List", new { testId = model.TestId });
    }

    public IActionResult Delete(int id)
    {
        var question = _context.Questions.FirstOrDefault(q => q.Id == id);
        if (question == null)
            return NotFound();

        return View(question);
    }

    [HttpPost]
    public IActionResult DeleteConfirmed(int id)
    {
        var question = _context.Questions.FirstOrDefault(q => q.Id == id);
        if (question == null)
            return NotFound();

        int testId = question.TestId;

        _context.Questions.Remove(question);
        _context.SaveChanges();

        return RedirectToAction("List", new { testId });
    }
}