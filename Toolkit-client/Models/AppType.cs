﻿using System;
using System.Collections.Generic;

namespace Toolkit_Client.Models;

public partial class AppType
{
    public const int NAME_MAX_LENGTH = 64;

    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<App> Apps { get; } = new List<App>();

    public override string ToString()
    {
        return string.Format("Id = {0}, Name = \"{1}\"", Id, Name);
    }
}
