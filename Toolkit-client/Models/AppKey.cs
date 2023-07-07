using System;
using System.Collections.Generic;

namespace Toolkit_Client.Models;

public partial class AppKey
{
    public const int KEY_LENGTH = 29;

    public string Key { get; set; } = null!;

    public long AppId { get; set; }

    public long AppPurchaseMethodId { get; set; }

    public long IssuerUserId { get; set; }

    public long? ActivatedByUserId { get; set; }

    public string? ActivationDatetime { get; set; }

    public long IsDisabled { get; set; }

    public virtual User? ActivatedByUser { get; set; }

    public virtual App App { get; set; } = null!;

    public virtual AppPurchaseMethod AppPurchaseMethod { get; set; } = null!;

    public virtual User IssuerUser { get; set; } = null!;

    public override string ToString()
    {
        return string.Format("Key = \"{0}\", AppId = \"{1}\", AppPurchaseMethodId = {2}, IssuerUserId = {3}", Key, AppId, AppPurchaseMethodId, IssuerUserId);
    }
}
