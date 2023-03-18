using System;
using System.Collections.Generic;

namespace Toolkit_NET_Client.Models;

public partial class App
{
    public const int NAME_MAX_LENGTH = 128;

    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public long DeveloperCompanyId { get; set; }

    public long PublisherCompanyId { get; set; }

    public string UploadDatetime { get; set; } = null!;

    public string? ReleaseDatetime { get; set; }

    public long? DiscountPercent { get; set; }

    public string? DiscountStartDatetime { get; set; }

    public string? DiscountExpireDatetime { get; set; }

    public long AppTypeId { get; set; }

    public long AppReleaseStateId { get; set; }

    public virtual ICollection<AppKey> AppKeys { get; } = new List<AppKey>();

    public virtual ICollection<AppPurchaseMethod> AppPurchaseMethods { get; } = new List<AppPurchaseMethod>();

    public virtual AppReleaseState AppReleaseState { get; set; } = null!;

    public virtual AppStorePage? AppStorePage { get; set; }

    public virtual AppType AppType { get; set; } = null!;

    public virtual Company DeveloperCompany { get; set; } = null!;

    public virtual Company PublisherCompany { get; set; } = null!;

    public virtual ICollection<UserPurchasedApp> UserPurchasedApps { get; } = new List<UserPurchasedApp>();

    public virtual ICollection<Category> Categories { get; } = new List<Category>();

    public virtual ICollection<App> DependantApps { get; } = new List<App>();

    public virtual ICollection<App> RequiredApps { get; } = new List<App>();

    public virtual ICollection<Tag> Tags { get; } = new List<Tag>();
}
