using System;
using System.Collections.Generic;
using project.utils;

namespace project.utils.catalogue;

public partial class catalogueType : CommonsModel<long>
{
    public string code { get; set; } = null!;

    public string name { get; set; } = null!;

    public string? description { get; set; }

    public ICollection<Catalogue> Catalogos { get; set; } = new List<Catalogue>();
}
