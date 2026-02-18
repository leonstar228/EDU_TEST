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

    
}