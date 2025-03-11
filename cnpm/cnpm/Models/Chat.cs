using System;
using System.Collections.Generic;

namespace cnpm.Models;

public partial class Chat
{
    public int ChatId { get; set; }

    public int SenderId { get; set; }

    public int? ReceiverId { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? Timestamp { get; set; }

    public virtual User? Receiver { get; set; }

    public virtual User Sender { get; set; } = null!;
}
