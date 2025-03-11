using System;
using System.Collections.Generic;

namespace cnpm.Models;

public partial class ProductDetail
{
    public int ProductDetailId { get; set; }

    public int ProductId { get; set; }

    public string Material { get; set; } = null!;

    public decimal Weight { get; set; }

    public decimal Length { get; set; }

    public string Origin { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
