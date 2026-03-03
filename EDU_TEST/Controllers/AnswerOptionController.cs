using EDU_TEST.Data;
using EDU_TEST.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDU_TEST.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AnswerOptionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnswerOptionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> List(int questionId)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null)
                return NotFound();

            return View(question);
        }

        [HttpGet]
        public IActionResult Create(int questionId)
        {
            var model = new AnswerOption { QuestionId = questionId };
            return View("~/Views/AnswerOption/Create.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AnswerOption model)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == model.QuestionId);

            if (question == null)
                return NotFound();

            // ✅ Якщо це перший варіант — він ОБОВ'ЯЗКОВО має бути правильним
            if (!model.IsCorrect && (question.Options == null || question.Options.Count == 0))
            {
                ModelState.AddModelError("", "Перший варіант відповіді має бути правильним (0 правильних не допускається). Позначте 'Правильна відповідь'.");
            }

            if (!ModelState.IsValid)
                return View("~/Views/AnswerOption/Create.cshtml", model);

            // ✅ Якщо новий варіант позначено як правильний — знімаємо правильність з інших
            if (model.IsCorrect)
            {
                foreach (var opt in question.Options)
                    opt.IsCorrect = false;

                question.CorrectAnswer = model.Text;
            }
            else
            {
                // ✅ Якщо НЕ правильний — у питання вже має бути правильний
                if (question.Options.All(o => !o.IsCorrect))
                {
                    ModelState.AddModelError("", "Потрібно мати рівно одну правильну відповідь. Позначте один варіант як правильний.");
                    return View("~/Views/AnswerOption/Create.cshtml", model);
                }
            }

            _context.AnswerOptions.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(List), new { questionId = model.QuestionId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var option = await _context.AnswerOptions.FindAsync(id);
            if (option == null)
                return NotFound();

            return View("~/Views/AnswerOption/Edit.cshtml", option);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AnswerOption model)
        {
            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == model.QuestionId);

            if (question == null)
                return NotFound();

            var dbOption = question.Options.FirstOrDefault(o => o.Id == model.Id);
            if (dbOption == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View("~/Views/AnswerOption/Edit.cshtml", model);

            bool wasCorrect = dbOption.IsCorrect;
            bool willBeCorrect = model.IsCorrect;

            // ✅ Не даємо зробити 0 правильних
            if (wasCorrect && !willBeCorrect)
            {
                bool hasAnotherCorrect = question.Options.Any(o => o.Id != dbOption.Id && o.IsCorrect);
                if (!hasAnotherCorrect)
                {
                    ModelState.AddModelError("", "Не можна залишити 0 правильних відповідей. Оберіть інший варіант як правильний.");
                    return View("~/Views/AnswerOption/Edit.cshtml", model);
                }
            }

            dbOption.Text = model.Text;

            if (willBeCorrect)
            {
                foreach (var opt in question.Options)
                    opt.IsCorrect = (opt.Id == dbOption.Id);

                question.CorrectAnswer = dbOption.Text;
            }
            else
            {
                dbOption.IsCorrect = false;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(List), new { questionId = model.QuestionId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var option = await _context.AnswerOptions.FindAsync(id);
            if (option == null)
                return NotFound();

            return View("~/Views/AnswerOption/Delete.cshtml", option);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var option = await _context.AnswerOptions.FindAsync(id);
            if (option == null)
                return NotFound();

            var question = await _context.Questions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == option.QuestionId);

            if (question == null)
                return NotFound();

            int questionId = option.QuestionId;

            // ✅ Не можна видалити останній варіант (бо буде 0 правильних)
            if (question.Options.Count == 1)
            {
                TempData["Error"] = "Не можна видалити останній варіант відповіді. Потрібно мати рівно одну правильну відповідь.";
                return RedirectToAction(nameof(List), new { questionId });
            }

            bool deletingCorrect = option.IsCorrect;

            _context.AnswerOptions.Remove(option);
            await _context.SaveChangesAsync();

            if (deletingCorrect)
            {
                // ✅ Після видалення правильного — автоматично ставимо перший як правильний
                var remaining = await _context.AnswerOptions
                    .Where(o => o.QuestionId == questionId)
                    .OrderBy(o => o.Id)
                    .ToListAsync();

                foreach (var opt in remaining)
                    opt.IsCorrect = false;

                var newCorrect = remaining.FirstOrDefault();
                if (newCorrect != null)
                {
                    newCorrect.IsCorrect = true;

                    var qEntity = await _context.Questions.FirstAsync(q => q.Id == questionId);
                    qEntity.CorrectAnswer = newCorrect.Text;

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(List), new { questionId });
        }
    }
}