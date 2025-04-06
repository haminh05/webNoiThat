using System;
using System.Collections.Generic;

namespace cnpm.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? ReviewDate { get; set; }
    public bool IsVisible { get; set; } = true;

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
