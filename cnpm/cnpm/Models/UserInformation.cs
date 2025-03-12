using System;
using System.Collections.Generic;

namespace cnpm.Models;

public partial class UserInformation
{
    public int UserInformationId { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string ShippingAddress { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
