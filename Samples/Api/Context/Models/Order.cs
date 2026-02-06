using System;
using System.Collections.Generic;

namespace Api.Context.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int Status { get; set; }

    public int PaymentType { get; set; }

    public int ClientId { get; set; }

    public DateTime CreatedAt { get; set; }

    public decimal Total { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
