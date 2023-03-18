using System;
using System.Collections.Generic;

namespace Toolkit_NET_Client.Models;

public partial class Country
{
    public const int TWO_LETTER_ISO_CODE_LENGTH = 2;
    public const int COUNTRY_NAME_MAX_LENGTH = 64;
    public const int STATE_NAME_MAX_LENGTH = 64;

    public string TwoLetterIsoCode { get; set; } = null!;

    public string CountryName { get; set; } = null!;

    public string StateName { get; set; } = null!;

    public virtual ICollection<Company> Companies { get; } = new List<Company>();

    public virtual ICollection<User> Users { get; } = new List<User>();
}
