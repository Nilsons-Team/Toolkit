using System;
using System.Collections.Generic;

namespace Toolkit_NET_Client.Models;

public partial class User
{
    public const int LOGIN_MAX_LENGTH = 64;
    public const int PASSWORD_MIN_LENGTH = 8;
    public const int PASSWORD_MAX_LENGTH = 64;
    public const int PASSWORD_HASH_LENGTH = 64;
    public const int USERNAME_MAX_LENGTH = 64;
    public const int EMAIL_MAX_LENGTH = 256;
    public const int PHONE_MIN_LENGTH = 11;
    public const int PHONE_MAX_LENGTH = 16;

    public long Id { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public double Wallet { get; set; }

    public string RegistrationDatetime { get; set; } = null!;

    public string CountryId { get; set; } = null!;

    public virtual ICollection<AppKey> AppKeyActivatedByUsers { get; } = new List<AppKey>();

    public virtual ICollection<AppKey> AppKeyIssuerUsers { get; } = new List<AppKey>();

    public virtual ICollection<Company> Companies { get; } = new List<Company>();

    public virtual Country Country { get; set; } = null!;

    public virtual ICollection<UserPurchasedApp> UserPurchasedApps { get; } = new List<UserPurchasedApp>();
}
