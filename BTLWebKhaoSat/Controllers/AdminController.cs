using BTLWebKhaoSat.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTLWebKhaoSat.Controllers
{
    public class AdminController : Controller
    {
        private readonly SurveydbContext _context;

        public AdminController(SurveydbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var surveys = _context.Surveys.Include("Questions").ToList();
            return View(surveys);
        }

        [HttpPost]
        public IActionResult DeleteSurvey(int id)
        {
            var survey = _context.Surveys.Find(id);
            _context.Surveys.Remove(survey);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
