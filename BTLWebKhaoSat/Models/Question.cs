using System;
using System.Collections.Generic;

namespace BTLWebKhaoSat.Models;

public partial class Question
{
    public int QuestionId { get; set; }

    public int? SurveyId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int TypeId { get; set; }

    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();

    public virtual ICollection<Response> Responses { get; set; } = new List<Response>();

    public virtual Survey? Survey { get; set; }
}
