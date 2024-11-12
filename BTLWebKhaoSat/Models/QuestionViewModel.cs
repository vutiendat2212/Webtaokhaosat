using Microsoft.AspNetCore.Mvc;

namespace BTLWebKhaoSat.Models
{
    public class QuestionViewModel
    {
        public string QuestionText { get; set; }
        public int TypeID { get; set; } // 1: Text, 2: Single Choice, 3: Multiple Choice
        public List<string> Options { get; set; } = new List<string>(); // Chỉ dùng cho câu hỏi có lựa chọn
    }
}
