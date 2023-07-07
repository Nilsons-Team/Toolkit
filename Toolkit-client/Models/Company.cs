using System;
using System.Collections.Generic;

using Toolkit_Client.Modules;

namespace Toolkit_Client.Models;

public partial class Company
{
    public const int LEGAL_NAME_MAX_LENGTH = 128;
    public const int COMPANY_FORM_MAX_LENGTH = 64;
    public const int OPERATING_NAME_MAX_LENGTH = 128;
    public const int STREET_ADDRESS_MAX_LENGTH = 128;
    public const int CITY_MAX_LENGTH = 64;
    public const int STATE_OR_PROVINCE_MAX_LENGTH = 32;
    public const int POSTAL_CODE_MAX_LENGTH = 16;
    public const int EMAIL_MAX_LENGTH = 256;

    public long Id { get; set; }

    public string LegalName { get; set; } = null!;

    public string CompanyForm { get; set; } = null!;

    public string OperatingName { get; set; } = null!;

    public string StreetAddress { get; set; } = null!;

    public string City { get; set; } = null!;

    public string StateOrProvince { get; set; } = null!;

    public string PostalCode { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string RegistrationDatetime { get; set; } = null!;

    public string CountryId { get; set; } = null!;

    public long OwnerUserId { get; set; }

    public virtual ICollection<App> AppDeveloperCompanies { get; } = new List<App>();

    public virtual ICollection<App> AppPublisherCompanies { get; } = new List<App>();

    public virtual Country Country { get; set; } = null!;

    public virtual User OwnerUser { get; set; } = null!;

    public string RegistrationDatetimeFormatted { 
        get { 
            DateTime reg;
            bool parsed = DateTime.TryParse(RegistrationDatetime, out reg);
            if (!parsed)
                return null;

            return string.Format("{0} {1} {2}", reg.Day, Formatting.GetDatetimeMonthName(reg.Month), reg.Year); 
        }
    }

    public override string ToString()
    {
        return string.Format("Id = {0}, OperatingName = \"{1}\"", Id, OperatingName);
    }
}
