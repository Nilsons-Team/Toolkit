using System;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Toolkit_Client.Models;

using static Toolkit_Client.Modules.Database;

namespace Toolkit_Client.Modules
{
    public class CompanyRegistration
    {
        public enum RegisterCompanyResultType
        {
            SUCCESS = 0,

            FAIL_EMPTY_LEGAL_NAME = 1,
            FAIL_LONG_LEGAL_NAME = 2,

            FAIL_EMPTY_COMPANY_FORM = 3,
            FAIL_LONG_COMPANY_FORM = 4,

            FAIL_EMPTY_OPERATING_NAME = 5,
            FAIL_LONG_OPERATING_NAME = 6,

            FAIL_EMPTY_STREET_ADDRESS = 7,
            FAIL_LONG_STREET_ADDRESS = 8,

            FAIL_EMPTY_CITY = 9,
            FAIL_LONG_CITY = 10,

            FAIL_EMPTY_STATE_OR_PROVINCE = 11,
            FAIL_LONG_STATE_OR_PROVINCE = 12,

            FAIL_EMPTY_POSTAL_CODE = 13,
            FAIL_LONG_POSTAL_CODE = 14,

            FAIL_EMPTY_COUNTRY = 15,
            FAIL_INCORRECT_COUNTRY_CODE = 16,
            FAIL_COUNTRY_NOT_FOUND = 17,

            FAIL_EMPTY_EMAIL = 18,
            FAIL_LONG_EMAIL = 19,
            FAIL_INCORRECT_EMAIL = 20,

            FAIL_ALREADY_REGISTERED = 21,
            FAIL_DATABASE_FAILURE = 22
        }

        public struct RegisterCompanyResult
        {
            public Company? company;
            public RegisterCompanyResultType result;
        }

        public static string GetErrorMessageFromRegisterCompanyResult(RegisterCompanyResultType result)
        {
            switch (result)
            {
                case RegisterCompanyResultType.FAIL_EMPTY_LEGAL_NAME:        return "Введите юридическое название.";
                case RegisterCompanyResultType.FAIL_LONG_LEGAL_NAME:         return $"Юридическое название должно содержать максимум {Company.LEGAL_NAME_MAX_LENGTH} символов.";

                case RegisterCompanyResultType.FAIL_EMPTY_COMPANY_FORM:      return "Введите форму компании.";
                case RegisterCompanyResultType.FAIL_LONG_COMPANY_FORM:       return $"Форма компании должна содержать максимум {Company.COMPANY_FORM_MAX_LENGTH} символов.";
                
                case RegisterCompanyResultType.FAIL_EMPTY_OPERATING_NAME:    return "Введите отображаемое название.";
                case RegisterCompanyResultType.FAIL_LONG_OPERATING_NAME:     return $"Отображамое название должно содержать максимум {Company.OPERATING_NAME_MAX_LENGTH} символов.";

                case RegisterCompanyResultType.FAIL_EMPTY_STREET_ADDRESS:    return "Введите улицу, номер дома и квартиры/офиса.";
                case RegisterCompanyResultType.FAIL_LONG_STREET_ADDRESS:     return $"Улица, номер дома и квартиры/офиса должны содержать максимум {Company.STREET_ADDRESS_MAX_LENGTH} символов.";

                case RegisterCompanyResultType.FAIL_EMPTY_CITY:              return "Введите город.";
                case RegisterCompanyResultType.FAIL_LONG_CITY:               return $"Город должен содержать максимум {Company.CITY_MAX_LENGTH} символов.";

                case RegisterCompanyResultType.FAIL_EMPTY_STATE_OR_PROVINCE: return "Введите регион/область";
                case RegisterCompanyResultType.FAIL_LONG_STATE_OR_PROVINCE:  return $"Регион/область должны содержать максимум {Company.STATE_OR_PROVINCE_MAX_LENGTH} символов.";

                case RegisterCompanyResultType.FAIL_EMPTY_POSTAL_CODE:       return "Введите почтовый индекс.";
                case RegisterCompanyResultType.FAIL_LONG_POSTAL_CODE:        return $"Почтовый индекс должен содержать максимум {Company.POSTAL_CODE_MAX_LENGTH} символов.";

                case RegisterCompanyResultType.FAIL_EMPTY_COUNTRY:           return "Выберите страну.";
                case RegisterCompanyResultType.FAIL_INCORRECT_COUNTRY_CODE:  return "Данный код страны некорректный. Пожалуйста, выберите корректную страну.";
                case RegisterCompanyResultType.FAIL_COUNTRY_NOT_FOUND:       return "Данная страна не поддерживается. Пожалуйста, выберите поддерживаемую страну.";

                case RegisterCompanyResultType.FAIL_EMPTY_EMAIL:             return "Введите эл. почту.";
                case RegisterCompanyResultType.FAIL_LONG_EMAIL:              return $"Эл. почта должна содержать максимум {Company.EMAIL_MAX_LENGTH} символов.";
                case RegisterCompanyResultType.FAIL_INCORRECT_EMAIL:         return "Данная эл. почта некорректна. Пожалуйста, введите корректную эл. почту.";

                case RegisterCompanyResultType.FAIL_ALREADY_REGISTERED:      return "Компания с данным юридическим названием или отображаемым именем уже зарегистрирована.";
                case RegisterCompanyResultType.FAIL_DATABASE_FAILURE:        return "Произошла неизвестная ошибка при регистрации компании. Пожалуйста, попробуйте еще раз или обратитесь в поддержку.";
                
                default:                                                     return null;
            }
        }

        public static RegisterCompanyResultType CanRegisterCompany(
            string legalName, 
            string companyForm, 
            string operatingName, 
            string streetAddress, 
            string city,
            string stateOrProvince,
            string postalCode,
            string countryId,
            string email
        )
        {
            var legalNameResult = IsValidLegalName(legalName);
            if (legalNameResult != RegisterCompanyResultType.SUCCESS)
                return legalNameResult;

            var companyFormResult = IsValidCompanyForm(companyForm);
            if (companyFormResult != RegisterCompanyResultType.SUCCESS)
                return companyFormResult;

            var operatingNameResult = IsValidOperatingName(operatingName);
            if (operatingNameResult != RegisterCompanyResultType.SUCCESS)
                return operatingNameResult;

            var streetAddressResult = IsValidStreetAddress(streetAddress);
            if (streetAddressResult != RegisterCompanyResultType.SUCCESS)
                return streetAddressResult;

            var cityResult = IsValidCity(city);
            if (cityResult != RegisterCompanyResultType.SUCCESS)
                return cityResult;

            var stateOrProvinceResult = IsValidStateOrProvince(stateOrProvince);
            if (stateOrProvinceResult != RegisterCompanyResultType.SUCCESS)
                return stateOrProvinceResult;

            var postalCodeResult = IsValidPostalCode(postalCode);
            if (postalCodeResult != RegisterCompanyResultType.SUCCESS)
                return postalCodeResult;

            var countryResult = IsValidCountry(countryId);
            if (countryResult != RegisterCompanyResultType.SUCCESS)
                return countryResult;

            var emailResult = IsValidEmail(email);
            if (emailResult != RegisterCompanyResultType.SUCCESS)
                return emailResult;

            var registeredResult = IsCompanyAlreadyRegistered(legalName, operatingName);
            if (registeredResult != RegisterCompanyResultType.SUCCESS)
                return registeredResult;

            return RegisterCompanyResultType.SUCCESS;
        }

        public static RegisterCompanyResultType IsValidLegalName(string legalName) 
        {
            if (string.IsNullOrWhiteSpace(legalName))
                return RegisterCompanyResultType.FAIL_EMPTY_LEGAL_NAME;

            if (legalName.Length > Company.LEGAL_NAME_MAX_LENGTH)
                return RegisterCompanyResultType.FAIL_LONG_LEGAL_NAME;

            return RegisterCompanyResultType.SUCCESS;
        }

        public static RegisterCompanyResultType IsValidCompanyForm(string companyForm) 
        {
            if (string.IsNullOrWhiteSpace(companyForm))
                return RegisterCompanyResultType.FAIL_EMPTY_COMPANY_FORM;

            if (companyForm.Length > Company.COMPANY_FORM_MAX_LENGTH)
                return RegisterCompanyResultType.FAIL_LONG_COMPANY_FORM;

            return RegisterCompanyResultType.SUCCESS;
        }

        public static RegisterCompanyResultType IsValidOperatingName(string operatingName) 
        {
            if (string.IsNullOrWhiteSpace(operatingName))
                return RegisterCompanyResultType.FAIL_EMPTY_OPERATING_NAME;

            if (operatingName.Length > Company.OPERATING_NAME_MAX_LENGTH)
                return RegisterCompanyResultType.FAIL_LONG_OPERATING_NAME;

            return RegisterCompanyResultType.SUCCESS;
        }

        public static RegisterCompanyResultType IsValidStreetAddress(string streetAddress) 
        {
            if (string.IsNullOrWhiteSpace(streetAddress))
                return RegisterCompanyResultType.FAIL_EMPTY_STREET_ADDRESS;

            if (streetAddress.Length > Company.STREET_ADDRESS_MAX_LENGTH)
                return RegisterCompanyResultType.FAIL_LONG_STREET_ADDRESS;

            return RegisterCompanyResultType.SUCCESS;
        }

        public static RegisterCompanyResultType IsValidCity(string city) 
        {
            if (string.IsNullOrWhiteSpace(city))
                return RegisterCompanyResultType.FAIL_EMPTY_CITY;

            if (city.Length > Company.CITY_MAX_LENGTH)
                return RegisterCompanyResultType.FAIL_LONG_CITY;

            return RegisterCompanyResultType.SUCCESS;
        }

        public static RegisterCompanyResultType IsValidStateOrProvince(string stateOrProvince) 
        {
            if (string.IsNullOrWhiteSpace(stateOrProvince))
                return RegisterCompanyResultType.FAIL_EMPTY_STATE_OR_PROVINCE;

            if (stateOrProvince.Length > Company.STATE_OR_PROVINCE_MAX_LENGTH)
                return RegisterCompanyResultType.FAIL_LONG_STATE_OR_PROVINCE;

            return RegisterCompanyResultType.SUCCESS;
        }

        public static RegisterCompanyResultType IsValidPostalCode(string postalCode) 
        {
            if (string.IsNullOrWhiteSpace(postalCode))
                return RegisterCompanyResultType.FAIL_EMPTY_POSTAL_CODE;

            if (postalCode.Length > Company.POSTAL_CODE_MAX_LENGTH)
                return RegisterCompanyResultType.FAIL_LONG_POSTAL_CODE;

            return RegisterCompanyResultType.SUCCESS;
        }

        public static RegisterCompanyResultType IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RegisterCompanyResultType.FAIL_EMPTY_EMAIL;

            if (email.Length > User.EMAIL_MAX_LENGTH)
                return RegisterCompanyResultType.FAIL_LONG_EMAIL;

            if (!Validation.IsCorrectEmail(email))
                return RegisterCompanyResultType.FAIL_INCORRECT_EMAIL;

            return RegisterCompanyResultType.SUCCESS;
        }

        public static RegisterCompanyResultType IsValidCountry(string countryId)
        {
            if (string.IsNullOrWhiteSpace(countryId))
                return RegisterCompanyResultType.FAIL_EMPTY_COUNTRY;

            if (countryId.Length != Country.TWO_LETTER_ISO_CODE_LENGTH)
                return RegisterCompanyResultType.FAIL_INCORRECT_COUNTRY_CODE;

            return RegisterCompanyResultType.SUCCESS;
        }

        public static RegisterCompanyResultType IsCompanyAlreadyRegistered(string legalName, string operatingName) 
        {
            using (var db = new ToolkitContext()) {
                foreach (Company company in db.Companies)
                    if (company.LegalName == legalName || company.OperatingName == operatingName)
                        return RegisterCompanyResultType.FAIL_ALREADY_REGISTERED;
            }

            return RegisterCompanyResultType.SUCCESS;
        }

        public static RegisterCompanyResult RegisterCompany(
            string legalName, 
            string companyForm, 
            string operatingName, 
            string streetAddress, 
            string city,
            string stateOrProvince,
            string postalCode,
            string countryId,
            string email
        )
        {
            RegisterCompanyResult result;
            result.company = null;
            RegisterCompanyResultType canRegister = CanRegisterCompany(
                legalName, 
                companyForm, 
                operatingName, 
                streetAddress, 
                city, 
                stateOrProvince, 
                postalCode, 
                countryId, 
                email
            );
            result.result = canRegister;
            if (canRegister != RegisterCompanyResultType.SUCCESS)
                return result;

            result = _UncheckedRegisterCompany(
                legalName, 
                companyForm, 
                operatingName, 
                streetAddress, 
                city, 
                stateOrProvince, 
                postalCode, 
                countryId, 
                email
            );

            return result;
        }

        public static RegisterCompanyResult _UncheckedRegisterCompany(
            string legalName, 
            string companyForm, 
            string operatingName, 
            string streetAddress, 
            string city,
            string stateOrProvince,
            string postalCode,
            string countryId,
            string email
        )
        {
            RegisterCompanyResult result;
            result.company = null;
            result.result = RegisterCompanyResultType.SUCCESS;

            using (var db = new ToolkitContext()) {
                foreach (var dbCompany in db.Companies) {
                    if (dbCompany.LegalName == legalName || dbCompany.OperatingName == operatingName) {
                        result.result = RegisterCompanyResultType.FAIL_ALREADY_REGISTERED;
                        return result;
                    }
                }

                Company company = new Company()
                {
                    LegalName       = legalName, 
                    CompanyForm     = companyForm, 
                    OperatingName   = operatingName, 
                    StreetAddress   = streetAddress, 
                    City            = city,
                    StateOrProvince = stateOrProvince,
                    PostalCode      = postalCode,
                    CountryId       = countryId,
                    Email           = email
                };

                db.Companies.Add(company);

                try {
                    result.company = company;
                    db.SaveChanges();
                } catch (DbUpdateException dbUpdateException) {
                    var innerException = dbUpdateException.InnerException;
                    if (innerException == null) {
                        Console.WriteLine("Unknown DB failure: there was no `InnerException` in `DbUpdateException`.");
                        result.result = RegisterCompanyResultType.FAIL_DATABASE_FAILURE;
                        return result;
                    }

                    if (innerException is SqliteException) {
                        SqliteException sqliteException = (SqliteException)innerException;
                        if (sqliteException.SqliteExtendedErrorCode == (int)SQLiteErrorCode.SQLITE_CONSTRAINT_FOREIGNKEY) {
                            result.result = RegisterCompanyResultType.FAIL_COUNTRY_NOT_FOUND;
                            return result;
                        }
                    }
                }
            }

            return result;
        }
    }
}
