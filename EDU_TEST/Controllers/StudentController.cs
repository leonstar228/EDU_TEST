using EDU_TEST.Data;
using EDU_TEST.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EDU_TEST.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ ДОДАНО: пошук тестів (q) + збереження вкладки (tab)
        // tab: "available" або "myresults"
        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? tab)
        {
            var userIdStr = User.FindFirst("UserId")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Auth");

            int userId = int.Parse(userIdStr);

            ViewBag.SearchQuery = q ?? "";
            ViewBag.ActiveTab = string.IsNullOrWhiteSpace(tab) ? "available" : tab;

            IQueryable<Test> testsQuery = _context.Tests
                .Include(t => t.Questions)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var s = q.Trim().ToLower();
                testsQuery = testsQuery.Where(t => t.Title.ToLower().Contains(s));
            }

            var availableTests = await testsQuery
                .OrderBy(t => t.Title)
                .ToListAsync();

            ViewBag.MyResults = await _context.TestResults
                .Include(r => r.Test)
                .Where(r => r.StudentId == userId)
                .OrderByDescending(r => r.DateTaken)
                .AsNoTracking()
                .ToListAsync();

            return View(availableTests);
        }

        [HttpGet]
        public async Task<IActionResult> TakeTest(int id)
        {
            var test = await _context.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null)
                return NotFound();

            return View(test);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTest(int testId, Dictionary<int, string> answers)
        {
            var userIdStr = User.FindFirstValue("UserId")
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Auth");

            int userId = int.Parse(userIdStr);

            var questions = await _context.Questions
                .Where(q => q.TestId == testId)
                .AsNoTracking()
                .ToListAsync();

            int correctCount = 0;

            foreach (var q in questions)
            {
                if (answers.ContainsKey(q.Id) &&
                    (answers[q.Id] ?? "").Trim() == (q.CorrectAnswer ?? "").Trim())
                {
                    correctCount++;
                }
            }

            double score = questions.Count > 0
                ? (double)correctCount / questions.Count * 100
                : 0;

            var result = new TestResult
            {
                StudentId = userId,
                TestId = testId,
                Score = Math.Round(score, 2),
                DateTaken = DateTime.Now
            };

            _context.TestResults.Add(result);
            await _context.SaveChangesAsync();

            // ✅ ПІСЛЯ ЗДАЧІ: повертаємось в кабінет на вкладку результатів
            return RedirectToAction("Index", new { tab = "myresults" });
        }

        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            var userIdStr = User.FindFirstValue("UserId")
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Auth");

            int userId = int.Parse(userIdStr);

            var results = await _context.TestResults
                .Include(tr => tr.Test)
                .Where(tr => tr.StudentId == userId)
                .OrderByDescending(tr => tr.DateTaken)
                .AsNoTracking()
                .ToListAsync();

            return View(results);
        }

        [HttpGet]
        public async Task<IActionResult> ResultSummary(int testId)
        {
            var userIdStr = User.FindFirstValue("UserId")
                           ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Auth");

            int userId = int.Parse(userIdStr);

            var result = await _context.TestResults
                .Include(r => r.Test)
                .Include(r => r.Test.Questions)
                .ThenInclude(q => q.Options)
                .Where(r => r.StudentId == userId && r.TestId == testId)
                .OrderByDescending(r => r.DateTaken)
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            return View(result);
        }
    }
}