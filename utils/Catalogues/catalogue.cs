using System;
using System.Collections.Generic;
using project.utils;

namespace project.utils.catalogue;

public partial class Catalogue : CommonsModel<long>
{
    public string name { get; set; } = null!;
    public string? description { get; set; }
    public long catalogueTypeId { get; set; }
    public long? catalogueParentId { get; set; }
    public Catalogue? catalogueParent { get; set; }
    public catalogueType catalogueType { get; set; } = null!;


}
