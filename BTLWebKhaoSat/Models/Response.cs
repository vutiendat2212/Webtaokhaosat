using System;
using System.Collections.Generic;

namespace BTLWebKhaoSat.Models;

public partial class Response
{
    public int ResponseId { get; set; }

    public int? QuestionId { get; set; }

    public int? UserId { get; set; }

    public string? AnswerText { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public virtual Question? Question { get; set; }

    public virtual User? User { get; set; }
}
