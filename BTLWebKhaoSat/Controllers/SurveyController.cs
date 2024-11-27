using BTLWebKhaoSat.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http;


namespace BTLWebKhaoSat.Controllers
{
    public class SurveyController : Controller
    {
        private readonly SurveydbContext _context;

        public SurveyController(SurveydbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var surveys = _context.Surveys.ToList();
            return View(surveys);
        }

        public IActionResult MySurveys()
        {
            var userId = int.Parse(HttpContext.Session.GetString("UserID"));
            var surveys = _context.Surveys.Where(s => s.CreatedBy == userId).ToList();
            return View(surveys);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Survey survey, List<QuestionViewModel> questions)
        {
            if (ModelState.IsValid)
            {
                // Bước 1: Lưu khảo sát để lấy SurveyId
                survey.CreatedBy = int.Parse(HttpContext.Session.GetString("UserID"));
                survey.CreatedAt = DateTime.Now;

                _context.Surveys.Add(survey);
                _context.SaveChanges(); // Lưu Survey trước để lấy SurveyId

                // Bước 2: Duyệt qua từng câu hỏi trong form
                foreach (var question in questions)
                {
                    // Kiểm tra trùng lặp trong DbContext hoặc cơ sở dữ liệu trước khi thêm
                    if (!_context.Questions.Any(q => q.QuestionText == question.QuestionText && q.SurveyId == survey.SurveyId))
                    {
                        var newQuestion = new Question
                        {
                            SurveyId = survey.SurveyId,
                            QuestionText = question.QuestionText,
                            TypeId = question.TypeID
                        };

                        _context.Questions.Add(newQuestion);
                        _context.SaveChanges(); // Lưu câu hỏi để có QuestionId

                        // Bước 3: Xử lý các lựa chọn nếu là câu hỏi Single Choice hoặc Multiple Choice
                        if ((question.TypeID == 2 || question.TypeID == 3) && question.Options != null)
                        {
                            foreach (var option in question.Options.Distinct()) // Loại bỏ lựa chọn trùng lặp
                            {
                                if (!_context.QuestionOptions.Any(o => o.OptionText == option && o.QuestionId == newQuestion.QuestionId))
                                {
                                    var newOption = new QuestionOption
                                    {
                                        QuestionId = newQuestion.QuestionId,
                                        OptionText = option
                                    };

                                    _context.QuestionOptions.Add(newOption);
                                }
                            }

                            _context.SaveChanges(); // Lưu tất cả các lựa chọn của câu hỏi hiện tại
                        }
                    }
                }

                return RedirectToAction(nameof(MySurveys)); // Chuyển hướng sau khi hoàn tất
            }

            return View(survey); // Nếu dữ liệu không hợp lệ, trả về view
        }



        public IActionResult AnswerSurvey(int id)
        {
            var survey = _context.Surveys
                .Include(s => s.Questions)
                .ThenInclude(q => q.QuestionOptions) // Bao gồm các tùy chọn của câu hỏi
                .FirstOrDefault(s => s.SurveyId == id);

            if (survey == null)
            {
                return NotFound();
            }

            return View(survey);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AnswerSurvey(int id, List<ResponseViewModel> responses)
        {
            if (ModelState.IsValid)
            {
                foreach (var response in responses)
                {
                    var newResponse = new Response
                    {
                        QuestionId = response.QuestionID,
                        AnswerText = response.AnswerText,
                        UserId = int.Parse(HttpContext.Session.GetString("UserID")),
                        SubmittedAt = DateTime.Now
                    };

                    _context.Responses.Add(newResponse);
                }

                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            var survey = _context.Surveys
                .Include(s => s.Questions)
                .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefault(s => s.SurveyId == id);

            return View(survey);
        }



        // Hiển thị danh sách câu trả lời của một khảo sát
        public IActionResult Responses(int id)
        {
            var survey = _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Responses)
                        .ThenInclude(r => r.User) // Bao gồm thông tin người trả lời
                .FirstOrDefault(s => s.SurveyId == id);

            if (survey == null || survey.CreatedBy != int.Parse(HttpContext.Session.GetString("UserID")))
            {
                return NotFound();
            }

            return View(survey);
        }

      

        // Xuất dữ liệu khảo sát ra file Excel
        public IActionResult ExportToExcel(int id)
        {
            var survey = _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Responses)
                        .ThenInclude(r => r.User)
                .FirstOrDefault(s => s.SurveyId == id);

            if (survey == null || survey.CreatedBy != int.Parse(HttpContext.Session.GetString("UserID")))
            {
                return NotFound();
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Survey Responses");
                worksheet.Cell(1, 1).Value = "Question";
                worksheet.Cell(1, 2).Value = "Answer";
                worksheet.Cell(1, 3).Value = "Answered By";

                int row = 2;

                foreach (var question in survey.Questions)
                {
                    foreach (var response in question.Responses)
                    {
                        worksheet.Cell(row, 1).Value = question.QuestionText;
                        worksheet.Cell(row, 2).Value = response.AnswerText;
                        worksheet.Cell(row, 3).Value = response.User.Username; // Tên người trả lời
                        row++;
                    }
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SurveyResponses.xlsx");
                }
            }
        }



        // Xóa khảo sát
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var survey = _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Responses)
                .Include(s => s.Questions)
                    .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefault(s => s.SurveyId == id);

            if (survey == null)
            {
                return NotFound();
            }

            // Chỉ cho phép Admin hoặc người tạo khảo sát xóa
            if (HttpContext.Session.GetString("RoleId") != "1" &&
                survey.CreatedBy != int.Parse(HttpContext.Session.GetString("UserID")))
            {
                return Unauthorized();
            }

            foreach (var question in survey.Questions)
            {
                _context.Responses.RemoveRange(question.Responses);
                _context.QuestionOptions.RemoveRange(question.QuestionOptions);
            }

            _context.Questions.RemoveRange(survey.Questions);
            _context.Surveys.Remove(survey);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFirstSurvey()
        {
            // Lấy khảo sát đầu tiên trong bảng Surveys
            var firstSurvey = _context.Surveys
                .Include(s => s.Questions)
                .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefault();

            if (firstSurvey == null)
            {
                return NotFound("No surveys available to delete.");
            }

            // Xóa các câu hỏi và các lựa chọn liên quan trước
            foreach (var question in firstSurvey.Questions)
            {
                _context.QuestionOptions.RemoveRange(question.QuestionOptions);
                _context.Questions.Remove(question);
            }

            // Xóa khảo sát
            _context.Surveys.Remove(firstSurvey);
            _context.SaveChanges();

            return RedirectToAction("Index"); // Chuyển hướng về trang Index sau khi xóa
        }

        [HttpGet]
        public IActionResult EditFirstSurvey()
        {
            var firstSurvey = _context.Surveys
                .OrderBy(s => s.SurveyId) // Lấy khảo sát đầu tiên dựa trên thời gian tạo
                .FirstOrDefault();

            if (firstSurvey == null)
            {
                return NotFound("No surveys available to edit.");
            }

            // Chuyển hướng đến action EditSurvey với ID của khảo sát đầu tiên
            return RedirectToAction("EditSurvey", new { id = firstSurvey.SurveyId });
        }

        [HttpGet]
        public IActionResult EditSurvey(int id)
        {
            var survey = _context.Surveys
            .Include(s => s.Questions)
            .ThenInclude(q => q.QuestionOptions)
            .FirstOrDefault(s => s.SurveyId == id);

            if (survey == null)
            {
                return NotFound("Survey not found.");
            }

            var questions = survey.Questions.Select(q => new QuestionViewModel
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                TypeID = q.TypeId,
                Options = q.QuestionOptions.Select(o => o.OptionText).ToList()
            }).ToList();

            ViewBag.Questions = questions;

            return View(survey);
        }



    }
}
