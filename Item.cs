using System;
using System.Collections.Generic;

namespace TodoApi;
 [Serializable]
public  class Item
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public sbyte? IsComplete { get; set; }
}
