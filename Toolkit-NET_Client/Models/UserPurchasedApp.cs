using System;
using System.Collections.Generic;

namespace Toolkit_NET_Client.Models;

public partial class UserPurchasedApp
{
    public long UserId { get; set; }

    public long AppId { get; set; }

    public long AppPurchaseMethodId { get; set; }

    public string PurchaseDatetime { get; set; } = null!;

    public virtual App App { get; set; } = null!;

    public virtual AppPurchaseMethod AppPurchaseMethod { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
