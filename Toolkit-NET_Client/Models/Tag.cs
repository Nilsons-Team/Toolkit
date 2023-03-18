using System;
using System.Collections.Generic;

namespace Toolkit_NET_Client.Models;

public partial class Tag
{
    public const int NAME_MAX_LENGTH = 32;

    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<App> Apps { get; } = new List<App>();
}
