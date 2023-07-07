using Toolkit_Client;
using Toolkit_Client.Models;

namespace Toolkit_Client_Tests
{
    public class CompanyTests
    {
        [Theory]
        [InlineData(null, null, null, null, null, null, null, null, null)]
        [InlineData("", "", "", "", "", "", "", "", "")]
        [InlineData("", "CompanyForm", "OperatingName", "StreetAddress", "City", "StateOrProvince", "PostalCode", "ru", "test@gmail.com")]
        [InlineData("LegalName", "", "OperatingName", "StreetAddress", "City", "StateOrProvince", "PostalCode", "ru", "test@gmail.com")]
        [InlineData("LegalName", "CompanyForm", "", "StreetAddress", "City", "StateOrProvince", "PostalCode", "ru", "test@gmail.com")]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "", "City", "StateOrProvince", "PostalCode", "ru", "test@gmail.com")]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "", "StateOrProvince", "PostalCode", "ru", "test@gmail.com")]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "City", "", "PostalCode", "ru", "test@gmail.com")]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "City", "StateOrProvince", "", "ru", "test@gmail.com")]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "City", "StateOrProvince", "PostalCode", "", "test@gmail.com")]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "City", "StateOrProvince", "PostalCode", "ru", "")]
        public void RegisterEmptyDataTest(
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
            ToolkitApp.InitDbConnection();
            var result = ToolkitApp.CanRegisterCompany(
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

            Assert.True(result != RegisterCompanyResultType.SUCCESS);
        }

        [Fact]
        public void RegisterExistingDataTest()
        {
            ToolkitApp.InitDbConnection();
            Company dbCompany;
            using (var db = new ToolkitContext()) {
                dbCompany = db.Companies.FirstOrDefault();
            }

            var result = ToolkitApp.CanRegisterCompany(
                dbCompany.LegalName, 
                dbCompany.CompanyForm, 
                dbCompany.OperatingName, 
                dbCompany.StreetAddress, 
                dbCompany.City, 
                dbCompany.StateOrProvince, 
                dbCompany.PostalCode, 
                dbCompany.CountryId, 
                dbCompany.Email
            );

            Assert.True(result != RegisterCompanyResultType.SUCCESS);
        }

        [Theory]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "City", "StateOrProvince", "PostalCode", "ru", "123")]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "City", "StateOrProvince", "PostalCode", "ru", "@")]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "City", "StateOrProvince", "PostalCode", "ru", "123@")]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "City", "StateOrProvince", "PostalCode", "ru", "gmail.com")]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "City", "StateOrProvince", "PostalCode", "ru", "@gmail.com")]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "City", "StateOrProvince", "PostalCode", "ru", "@gmail.com123")]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "City", "StateOrProvince", "PostalCode", "ru", "1@1")]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "City", "StateOrProvince", "PostalCode", "ru", "1@1.1")]
        public void RegisterBadEmailDataTest(
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
            ToolkitApp.InitDbConnection();
            var result = ToolkitApp.CanRegisterCompany(
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

            Assert.True(result != RegisterCompanyResultType.SUCCESS);
        }

        [Theory]
        [InlineData("LegalName", "CompanyForm", "OperatingName", "StreetAddress", "City", "StateOrProvince", "PostalCode", "ru", "test@gmail.com")]
        public void RegisterCorrectDataTest(
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
            ToolkitApp.InitDbConnection();
            var result = ToolkitApp.CanRegisterCompany(
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

            Assert.True(result == RegisterCompanyResultType.SUCCESS);
        }
    }
}
