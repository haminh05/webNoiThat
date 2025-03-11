using System;
using System.Collections.Generic;

namespace cnpm.Models;

public partial class Report
{
    public int ReportId { get; set; }

    public string ReportType { get; set; } = null!;

    public int? UserId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Content { get; set; }

    public virtual User? User { get; set; }
}
