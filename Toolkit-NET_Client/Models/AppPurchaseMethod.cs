using System;
using System.Collections.Generic;

namespace Toolkit_NET_Client.Models;

public partial class AppPurchaseMethod
{
    public const int NAME_MAX_LENGTH = 64;

    public long Id { get; set; }

    public long AppId { get; set; }

    public string Name { get; set; } = null!;

    public double Price { get; set; }

    public long Hours { get; set; }

    public long IsPerpetual { get; set; }

    public virtual App App { get; set; } = null!;

    public virtual ICollection<AppKey> AppKeys { get; } = new List<AppKey>();

    public virtual ICollection<UserPurchasedApp> UserPurchasedApps { get; } = new List<UserPurchasedApp>();
}
