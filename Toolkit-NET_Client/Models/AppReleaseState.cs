using System;
using System.Collections.Generic;

namespace Toolkit_NET_Client.Models;

public partial class AppReleaseState
{
    public const int MAX_LENGTH_NAME = 32;

    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<App> Apps { get; } = new List<App>();
}
