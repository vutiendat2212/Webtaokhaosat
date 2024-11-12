using System;
using System.Collections.Generic;

namespace BTLWebKhaoSat.Models;

public partial class QuestionOption
{
    public int QuestionOptionId { get; set; }

    public int? QuestionId { get; set; }

    public string OptionText { get; set; } = null!;

    public virtual Question? Question { get; set; }
}
