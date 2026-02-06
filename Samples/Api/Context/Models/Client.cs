using System;
using System.Collections.Generic;

namespace Api.Context.Models;

public partial class Client
{
    public int ClientId { get; set; }

    public string Name { get; set; } = null!;
}
