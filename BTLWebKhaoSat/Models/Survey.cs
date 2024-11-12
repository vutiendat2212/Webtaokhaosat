using System;
using System.Collections.Generic;

namespace BTLWebKhaoSat.Models;

public partial class Survey
{
    public int SurveyId { get; set; }

    public string Title { get; set; } = null!;

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
