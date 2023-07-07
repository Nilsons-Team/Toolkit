using System;
using System.Collections.Generic;

using Toolkit_Client.Modules;

namespace Toolkit_Client.Models;
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

    public string? ReleaseDatetimeFormatted { 
        get { 
            DateTime release;
            bool parsed = DateTime.TryParse(ReleaseDatetime, out release);
            if (!parsed)
                return null;

            return string.Format("{0} {1} {2}", release.Day, Formatting.GetDatetimeMonthName(release.Month), release.Year); 
        }
    }

    public bool HasActiveDiscount() {
        bool hasData = DiscountStartDatetime  != null &&
                       DiscountExpireDatetime != null &&
                       DiscountPercent        != null;

        if (!hasData)
            return false;

        DateTime start;
        bool startParsed = DateTime.TryParse(DiscountStartDatetime, out start);
        if (!startParsed)
            return false;

        DateTime expire;
        bool expireParsed = DateTime.TryParse(DiscountExpireDatetime, out expire);
        if (!expireParsed)
            return false;

        DateTime now = DateTime.UtcNow;
        int startDiff = DateTime.Compare(start, now);
        if (startDiff > 0)
            return false;

        int expireDiff = DateTime.Compare(now, expire);
        if (expireDiff < 0)
            return false;

        return true;
    }

    public double _LowestPrice = double.MaxValue;

    public double _HighestPrice = 0;

    public override string ToString()
    {
        return string.Format("Id = {0}, Name = \"{1}\"", Id, Name);
    }
}
