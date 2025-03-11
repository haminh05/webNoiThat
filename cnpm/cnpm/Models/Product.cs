using System;
using System.Collections.Generic;

namespace cnpm.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public int? CategoryId { get; set; }

    public string? ImagePath { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ProductDetail? ProductDetail { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
