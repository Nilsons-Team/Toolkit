using System;
using System.Collections.Generic;

namespace Toolkit_Client.Models;

public partial class AppStorePage
{
    public const int SHORT_DESCRIPTION_MAX_LENGTH = 256;
    public const int LONG_DESCRIPTION_MAX_LENGTH = 4096;

    public long Id { get; set; }

    public string ShortDescription { get; set; } = null!;

    public string LongDescription { get; set; } = null!;

    public virtual App IdNavigation { get; set; } = null!;

    public override string ToString()
    {
        return string.Format("Id = {0}", Id);
    }
}
