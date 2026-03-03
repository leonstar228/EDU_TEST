using EDU_TEST.Data;
using EDU_TEST.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDU_TEST.Controllers;

public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        int? studentId,
        int? testId,
        string viewMode = "student",
        string sort = "date",
        string dir = "desc",
        string? testQ = null)
    {
        ViewBag.TestQ = testQ ?? "";

        var testsQuery = _context.Tests
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(testQ))
        {
            var s = testQ.Trim().ToLower();
            testsQuery = testsQuery.Where(t => t.Title.ToLower().Contains(s));
        }

        var tests = await testsQuery
            .OrderBy(t => t.Title)
            .ToListAsync();

        ViewBag.Students = await _context.Users
            .AsNoTracking()
            .Where(u => u.Role == "Student")
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        ViewBag.TestsList = await _context.Tests
            .AsNoTracking()
            .OrderBy(t => t.Title)
            .ToListAsync();

        ViewBag.SelectedStudentId = studentId;
        ViewBag.SelectedTestId = testId;
        ViewBag.ViewMode = viewMode;
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;

        IQueryable<TestResult> q = _context.TestResults
            .Include(r => r.Student)
            .Include(r => r.Test);

        if (viewMode == "student")
        {
            if (studentId.HasValue)
                q = q.Where(r => r.StudentId == studentId.Value);
            else
                q = q.Where(r => false);
        }
        else
        {
            if (testId.HasValue)
                q = q.Where(r => r.TestId == testId.Value);
            else
                q = q.Where(r => false);
        }

        bool asc = dir == "asc";

        if (viewMode == "student")
        {
            q = sort switch
            {
                "test" => asc ? q.OrderBy(r => r.Test!.Title) : q.OrderByDescending(r => r.Test!.Title),
                "score" => asc ? q.OrderBy(r => r.Score) : q.OrderByDescending(r => r.Score),
                "date" => asc ? q.OrderBy(r => r.DateTaken) : q.OrderByDescending(r => r.DateTaken),
                _ => q.OrderByDescending(r => r.DateTaken)
            };
        }
        else
        {
            q = sort switch
            {
                "student" => asc
                    ? q.OrderBy(r => r.Student!.LastName).ThenBy(r => r.Student!.FirstName)
                    : q.OrderByDescending(r => r.Student!.LastName).ThenByDescending(r => r.Student!.FirstName),
                "score" => asc ? q.OrderBy(r => r.Score) : q.OrderByDescending(r => r.Score),
                "date" => asc ? q.OrderBy(r => r.DateTaken) : q.OrderByDescending(r => r.DateTaken),
                _ => q.OrderByDescending(r => r.DateTaken)
            };
        }

        ViewBag.Results = await q.AsNoTracking().ToListAsync();

        return View(tests);
    }

    public IActionResult CreateTest()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTest(Test model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _context.Tests.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> EditTest(int id)
    {
        var test = await _context.Tests.FirstOrDefaultAsync(t => t.Id == id);
        if (test == null) return NotFound();

        return View(test);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTest(Test model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _context.Tests.Update(model);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> DeleteTest(int id)
    {
        var test = await _context.Tests.FirstOrDefaultAsync(t => t.Id == id);
        if (test == null) return NotFound();

        _context.Tests.Remove(test);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}