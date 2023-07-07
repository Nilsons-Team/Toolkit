using System;
using System.Collections.Generic;

namespace Toolkit_Client.Models;

public partial class UserPurchasedApp
{
    public long UserId { get; set; }

    public long AppId { get; set; }

    public long AppPurchaseMethodId { get; set; }

    public string PurchaseDatetime { get; set; } = null!;

    public virtual App App { get; set; } = null!;

    public virtual AppPurchaseMethod AppPurchaseMethod { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public override string ToString()
    {
        return string.Format("UserId = {0}, AppId = {1}, AppPurchaseMethodId = {2}, PurchaseDatetime = \"{3}\"", UserId, AppId, AppPurchaseMethodId, PurchaseDatetime);
    }
}
