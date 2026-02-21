using EDU_TEST.Data;
using EDU_TEST.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDU_TEST.Controllers
{
    [Authorize(Roles = "Admin")]
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

        public IActionResult Create(int questionId)
        {
            return View(new AnswerOption { QuestionId = questionId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(AnswerOption model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.AnswerOptions.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("List", new { questionId = model.QuestionId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var option = await _context.AnswerOptions.FindAsync(id);
            if (option == null)
                return NotFound();

            return View(option);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AnswerOption model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.AnswerOptions.Update(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("List", new { questionId = model.QuestionId });
        }
        
        public async Task<IActionResult> Delete(int id)
        {
            var option = await _context.AnswerOptions.FindAsync(id);
            if (option == null)
                return NotFound();

            return View(option);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var option = await _context.AnswerOptions.FindAsync(id);
            if (option == null)
                return NotFound();

            int questionId = option.QuestionId;

            _context.AnswerOptions.Remove(option);
            await _context.SaveChangesAsync();

            return RedirectToAction("List", new { questionId });
        }
    }
}