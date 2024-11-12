using System;
using System.Collections.Generic;

namespace BTLWebKhaoSat.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int? RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Response> Responses { get; set; } = new List<Response>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<Survey> Surveys { get; set; } = new List<Survey>();
}
