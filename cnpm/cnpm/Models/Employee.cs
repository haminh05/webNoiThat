using System;
using System.Collections.Generic;

namespace cnpm.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public int? UserId { get; set; }

    public string FullName { get; set; } = null!;

    public DateOnly DateOfBirth { get; set; }

    public string Gender { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Position { get; set; } = null!;

    public string IdentityCard { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public bool IsActive { get; set; }

    public virtual User User { get; set; } = null!;
}
