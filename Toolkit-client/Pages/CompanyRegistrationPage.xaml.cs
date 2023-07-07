using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using Toolkit_Client.Models;
using Toolkit_Client.Windows;

using static Toolkit_Client.Modules.Database;
using static Toolkit_Client.Modules.ClientActionStatus;
using static Toolkit_Client.Modules.CompanyRegistration;

namespace Toolkit_Client.Pages
{
    /// <summary>
    /// Interaction logic for CompanyRegistrationPage.xaml
    /// </summary>
    public partial class CompanyRegistrationPage : ToolkitPage
    {
        private PartnerWindow ownerWindow;

        public CompanyRegistrationPage(PartnerWindow ownerWindow) : base (ownerWindow.GetLogger())
        {
            InitializeComponent();
            this.ownerWindow = ownerWindow;

            var initThread = new Thread(LoadCountriesIntoComboBox);
            initThread.Start();

            this.LegalNameTextBox.MaxLength       = Company.LEGAL_NAME_MAX_LENGTH;
            this.CompanyFormTextBox.MaxLength     = Company.COMPANY_FORM_MAX_LENGTH;
            this.OperatingNameTextBox.MaxLength   = Company.OPERATING_NAME_MAX_LENGTH;
            this.StreetAddressTextBox.MaxLength   = Company.STREET_ADDRESS_MAX_LENGTH;
            this.CityTextBox.MaxLength            = Company.CITY_MAX_LENGTH;
            this.StateOrProvinceTextBox.MaxLength = Company.STATE_OR_PROVINCE_MAX_LENGTH;
            this.PostalCodeTextBox.MaxLength      = Company.POSTAL_CODE_MAX_LENGTH;
            this.EmailTextBox.MaxLength           = Company.EMAIL_MAX_LENGTH;
        }

        public void LoadCountriesIntoComboBox()
        {
            List<Country> countries;
            using (var db = new ToolkitContext()) {
                countries = db.Countries.ToList();
            }

            Application.Current.Dispatcher.BeginInvoke(() => {
                foreach (var country in countries) {
                    string item = string.Format("{0} ({1})", country.CountryName, country.StateName);
                    var comboItem = new ComboBoxItem();
                    comboItem.Name = country.TwoLetterIsoCode;
                    comboItem.Content = item;
                    this.CountryComboBox.Items.Add(comboItem);
                }
            });
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string legalName       = this.LegalNameTextBox.Text;
            string companyForm     = this.CompanyFormTextBox.Text;
            string operatingName   = this.OperatingNameTextBox.Text;
            string streetAddress   = this.StreetAddressTextBox.Text;
            string city            = this.CityTextBox.Text;
            string stateOrProvince = this.StateOrProvinceTextBox.Text;
            string postalCode      = this.PostalCodeTextBox.Text;
            string email           = this.EmailTextBox.Text;
            ComboBoxItem selectedItem = (ComboBoxItem)this.CountryComboBox.SelectedItem;
            string countryId;
            if (selectedItem == null)
                countryId = null;
            else
                countryId = selectedItem.Name;

            // LegalName
            var legalNameResult = IsValidLegalName(legalName);
            string legalNameError = GetErrorMessageFromRegisterCompanyResult(legalNameResult);
            SetStatusError(LegalNameTextBox, LegalNameStatusTextBlock, legalNameError);

            // CompanyForm
            var companyFormResult = IsValidCompanyForm(companyForm);
            string companyFormError = GetErrorMessageFromRegisterCompanyResult(companyFormResult);
            SetStatusError(CompanyFormTextBox, CompanyFormStatusTextBlock, companyFormError);

            // OperatingName
            var operatingNameResult = IsValidOperatingName(operatingName);
            string operatingNameError = GetErrorMessageFromRegisterCompanyResult(operatingNameResult);
            SetStatusError(OperatingNameTextBox, OperatingNameStatusTextBlock, operatingNameError);

            // StreetAddress
            var streetAddressResult = IsValidStreetAddress(streetAddress);
            string streetAddressError = GetErrorMessageFromRegisterCompanyResult(streetAddressResult);
            SetStatusError(StreetAddressTextBox, StreetAddressStatusTextBlock, streetAddressError);

            // City
            var cityResult = IsValidCity(city);
            string cityError = GetErrorMessageFromRegisterCompanyResult(cityResult);
            SetStatusError(CityTextBox, CityStatusTextBlock, cityError);

            // StateOrProvince
            var stateOrProvinceResult = IsValidStateOrProvince(stateOrProvince);
            string stateOrProvinceError = GetErrorMessageFromRegisterCompanyResult(stateOrProvinceResult);
            SetStatusError(StateOrProvinceTextBox, StateOrProvinceStatusTextBlock, stateOrProvinceError);

            // PostalCode
            var postalCodeResult = IsValidPostalCode(postalCode);
            string postalCodeError = GetErrorMessageFromRegisterCompanyResult(postalCodeResult);
            SetStatusError(PostalCodeTextBox, PostalCodeStatusTextBlock, postalCodeError);

            // Country
            var countryResult = IsValidCountry(countryId);
            string countryError = GetErrorMessageFromRegisterCompanyResult(countryResult);
            SetStatusError(CountryComboBox, CountryStatusTextBlock, countryError);

            // Email
            var emailResult = IsValidEmail(email);
            string emailError = GetErrorMessageFromRegisterCompanyResult(emailResult);
            SetStatusError(EmailTextBox, EmailStatusTextBlock, emailError);

            bool canRegister = true;
            canRegister &= !Convert.ToBoolean((int)legalNameResult);
            canRegister &= !Convert.ToBoolean((int)companyFormResult);
            canRegister &= !Convert.ToBoolean((int)operatingNameResult);
            canRegister &= !Convert.ToBoolean((int)streetAddressResult);
            canRegister &= !Convert.ToBoolean((int)cityResult);
            canRegister &= !Convert.ToBoolean((int)stateOrProvinceResult);
            canRegister &= !Convert.ToBoolean((int)postalCodeResult);
            canRegister &= !Convert.ToBoolean((int)countryResult);
            canRegister &= !Convert.ToBoolean((int)emailResult);

            if (!canRegister)
                return;

            RegisterCompanyResult registerResult = _UncheckedRegisterCompany(
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

            if (registerResult.result == RegisterCompanyResultType.SUCCESS) {
                SetStatusSuccess(null, RegisterStatusTextBlock, "Вы успешно зарегистрировались.");
                RegisterButton.Visibility = Visibility.Collapsed;
                ProfileButton.Visibility = Visibility.Visible;
                this.ownerWindow.UpdateCompany(registerResult.company);
            } else {
                string errorString = GetErrorMessageFromRegisterCompanyResult(registerResult.result);
                SetStatusError(null, RegisterStatusTextBlock, errorString);
            }
        }
    }
}
