using EDU_TEST.Data;
using EDU_TEST.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data;
namespace EDU_TEST.Controllers;

public class AdminController : Controller
{

    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var tests = _context.Tests.ToList();
        return View(tests);
    }
    
    public IActionResult CreateTest()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CreateTest(Test model)
    {
        if (!ModelState.IsValid)
            return View(model);

        _context.Tests.Add(model);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }


    public IActionResult EditTest(int id)
    {
        var test = _context.Tests.FirstOrDefault(t => t.Id == id);
        if (test == null) return NotFound();

        return View(test);
    }

    [HttpPost]
    public IActionResult EditTest(Test model)
    {
        if (!ModelState.IsValid) return View(model);

        _context.Tests.Update(model);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    public IActionResult DeleteTest(int id)
    {
        var test = _context.Tests.FirstOrDefault(t => t.Id == id);
        if (test == null) return NotFound();
        
        _context.Tests.Remove(test);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }
}