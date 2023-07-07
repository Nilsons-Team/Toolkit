using System;
using System.Collections.Generic;

using Toolkit_Client.Modules;

namespace Toolkit_Client.Models;

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

    public string RegistrationDatetimeFormatted { 
        get { 
            DateTime reg;
            bool parsed = DateTime.TryParse(RegistrationDatetime, out reg);
            if (!parsed)
                return null;

            return string.Format("{0} {1} {2}", reg.Day, Formatting.GetDatetimeMonthName(reg.Month), reg.Year); 
        }
    }

    public string WalletFormatted {
        get {
            return $"{Wallet,2} р.";
        }
    }

    public override string ToString()
    {
        return string.Format("Id = {0}, Login = \"{1}\", Username = \"{2}\"", Id, Login, Username);
    }
}
