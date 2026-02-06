using System;
using System.Collections.Generic;

namespace Api.Context.Models;

public partial class Stock
{
    public int ProductInStockId { get; set; }

    public int ProductId { get; set; }

    public int Stock1 { get; set; }

    public virtual Product Product { get; set; } = null!;
}
