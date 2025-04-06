﻿using System;
using System.Collections.Generic;

namespace cnpm.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string ShippingAddress { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public DateTime? OrderDate { get; set; }

    public decimal TotalPrice { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual User User { get; set; } = null!;
}
