using System.Security.Claims;
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
    
// Показ тесту студенту
public async Task<IActionResult> Start(int testId, int questionIndex = 0, string handler = null)
    {
        // 1. Отримуємо тест разом із питаннями та варіантами відповідей
        var test = await _context.Tests
            .Include(t => t.Questions)
            .ThenInclude(q => q.Options)
            .FirstOrDefaultAsync(t => t.Id == testId);

        if (test == null) return NotFound();

        // 2. Сортуємо питання за ID для стабільного порядку
        var questions = test.Questions.OrderBy(q => q.Id).ToList();

        // 3. ОЧИЩЕННЯ СТАТУСУ ВІДПОВІДІ
        // Якщо ми прийшли сюди через кнопку "Наступне питання" (handler == "next"),
        // видаляємо TempData["LastAnswerCorrect"], щоб нова сторінка була чистою.
        if (handler == "next")
        {
            TempData.Remove("LastAnswerCorrect");
        }

        // 4. ПЕРЕВІРКА НА ЗАВЕРШЕННЯ
        // Якщо індекс дорівнює або більший за кількість питань — фінішуємо
        if (questionIndex >= questions.Count)
        {
            return await FinishTest(testId);
        }

        // 5. ЛОГІКА ЗБЕРЕЖЕННЯ БАЛІВ (TempData)
        // Скидаємо бали в 0 тільки на самому першому питанні, 
        // якщо ще не було зроблено жодної спроби відповіді.
        if (questionIndex == 0 && TempData["LastAnswerCorrect"] == null)
        {
            TempData["CurrentScore"] = 0;
        }

        // Обов'язково тримаємо бали в сесії для наступних запитів
        TempData.Keep("CurrentScore");

        // 6. ПІДГОТОВКА ПОТОЧНОГО ПИТАННЯ
        var question = questions[questionIndex];

        // РАНДОМІЗАЦІЯ ВАРІАНТІВ (щоб щоразу був інший порядок кнопок)
        var rnd = new Random();
        question.Options = question.Options.OrderBy(x => rnd.Next()).ToList();

        // 7. ПЕРЕДАЧА ДАНИХ У VIEW
        ViewBag.QuestionIndex = questionIndex;
        ViewBag.TotalQuestions = questions.Count;

        return View(question);
    }
    // Ми НЕ робимо Keep для LastAnswerCorrect тут, 
    // щоб він зникав сам після одного показу, якщо ми не в циклі перевірки

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
    private async Task<IActionResult> FinishTest(int testId)
    {
        // Отримуємо фінальні бали
        int finalScorePoints = TempData["CurrentScore"] != null ? (int)TempData["CurrentScore"] : 0;
        var totalQuestions = await _context.Questions.CountAsync(q => q.TestId == testId);

        // Виправляємо 0%: додаємо (double), щоб C# не округлював ділення до нуля
        double percentage = totalQuestions > 0
            ? Math.Round(((double)finalScorePoints / totalQuestions) * 100, 2)
            : 0;

        // Пошук UserId (спробуємо різні варіанти назв клеймів)
        var userIdStr = User.FindFirst("UserId")?.Value
                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Auth");

        var result = new TestResult
        {
            TestId = testId,
            StudentId = int.Parse(userIdStr),
            Score = percentage,
            DateTaken = DateTime.Now
        };

        _context.TestResults.Add(result);
        await _context.SaveChangesAsync(); // Чекаємо на запис у базу

        // Очищаємо дані тільки ПІСЛЯ успішного збереження
        TempData.Remove("CurrentScore");
        TempData.Remove("LastAnswerCorrect");

        return RedirectToAction("Result", new { id = result.Id });
    }
    [HttpPost]
    public async Task<IActionResult> SubmitAnswer(int TestId, int QuestionId, int SelectedAnswerId, int QuestionIndex)
    {
        var option = await _context.AnswerOptions.FirstOrDefaultAsync(a => a.Id == SelectedAnswerId);
        bool isCorrect = option != null && option.IsCorrect;

        int currentScore = TempData["CurrentScore"] != null ? (int)TempData["CurrentScore"] : 0;

        if (isCorrect)
        {
            currentScore++;
        }

        TempData["CurrentScore"] = currentScore;
        TempData["LastAnswerCorrect"] = isCorrect;
        TempData.Keep("CurrentScore");

        // ВАЖЛИВО: ми залишаємося на ТОМУ Ж індексі, щоб Start показав повідомлення "Правильно/Неправильно"
        // Але в методі Start ми додали перевірку, щоб він не обнуляв бали
        return RedirectToAction("Start", new { testId = TestId, questionIndex = QuestionIndex });
    }

    // Показ результату
    public async Task<IActionResult> Result(int id)
    {
        var result = await _context.TestResults
            .Include(r => r.Test)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (result == null) return NotFound();
        return View(result);
    }
}