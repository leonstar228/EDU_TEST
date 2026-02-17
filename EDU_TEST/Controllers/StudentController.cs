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

        public async Task<IActionResult> Index()
        {
            var tests = await _context.Tests
            .Include(t => t.Questions)
            .ToListAsync();
            return View(tests);
        }

        [HttpGet]
        public async Task<IActionResult> TakeTest(int id)
        {
            var test = await _context.Tests
            .Include(t => t.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(t => t.Id == id);

            if (test == null) return NotFound();

            return View(test);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitTest(int testId, Dictionary<int, string>
                answers)
        {
            var userIdStr = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Auth");
            int userId = int.Parse(userIdStr);

            var questions = await _context.Questions
            .Where(q => q.TestId == testId)
            .ToListAsync();

            int correctCount = 0;
            foreach (var q in questions)
            {
                if (answers.ContainsKey(q.Id) && answers[q.Id].Trim() == q.CorrectAnswer.Trim())
                {
                    correctCount++;
                }
            }

            double score = (questions.Count > 0) ? ((double)correctCount / questions.Count) * 100 : 0;

            var result = new TestResult
            {
                StudentId = userId,
                TestId = testId,
                Score = Math.Round(score, 2),
                DateTaken = DateTime.Now
            };

            _context.TestResults.Add(result);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyResults");
        }

        public async Task<IActionResult> MyResults()
        {
            var userIdStr = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Auth");
            int userId = int.Parse(userIdStr);

            var results = await _context.TestResults
            .Include(tr => tr.Test)
            .Where(tr => tr.StudentId == userId)
            .OrderByDescending(tr => tr.DateTaken)
            .ToListAsync();

            return View(results);
        }
    }
}