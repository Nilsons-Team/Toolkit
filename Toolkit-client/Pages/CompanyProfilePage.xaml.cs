using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using Toolkit_Client.Models;
using Toolkit_Client.Windows;

using Toolkit_Client.Modules;
using static Toolkit_Client.Modules.Database;
using static Toolkit_Client.Modules.ClientActionStatus;
using static Toolkit_Client.Modules.CompanyRegistration;

namespace Toolkit_Client.Pages
{
    /// <summary>
    /// Interaction logic for CompanyProfilePage.xaml
    /// </summary>
    public partial class CompanyProfilePage : ToolkitPage
    {
        public enum ActionType
        {
            NONE = 0,
            CHANGE_DETAILS = 1,
        }

        private PartnerWindow ownerWindow;
        private Company company;

        private ActionType actionType;

        private int currentUserCountryComboBoxIndex;

        public CompanyProfilePage(PartnerWindow ownerWindow, Company company) : base(ownerWindow.GetLogger())
        {
            InitializeComponent();
            this.ownerWindow = ownerWindow;
            this.company = company;

            var initThread = new Thread(LoadCountriesIntoComboBox);
            initThread.Start();

            this.actionType = ActionType.NONE;

            LegalNameTextBox.MaxLength       = Company.LEGAL_NAME_MAX_LENGTH;
            CompanyFormTextBox.MaxLength     = Company.COMPANY_FORM_MAX_LENGTH;
            OperatingNameTextBox.MaxLength   = Company.OPERATING_NAME_MAX_LENGTH;
            StreetAddressTextBox.MaxLength   = Company.STREET_ADDRESS_MAX_LENGTH;
            CityTextBox.MaxLength            = Company.CITY_MAX_LENGTH;
            StateOrProvinceTextBox.MaxLength = Company.STATE_OR_PROVINCE_MAX_LENGTH;
            PostalCodeTextBox.MaxLength      = Company.POSTAL_CODE_MAX_LENGTH;
            EmailTextBox.MaxLength           = Company.EMAIL_MAX_LENGTH;
            
            UpdateFields(updateCountry: false);
        }

        private void UpdateFields(bool updateCountry)
        {
            LegalNameTextBlock.Text         = company.LegalName;
            LegalNameTextBox.Text           = company.LegalName;
            CompanyFormTextBlock.Text       = company.CompanyForm;
            CompanyFormTextBox.Text         = company.CompanyForm;
            OperatingNameTextBlock.Text     = company.OperatingName;
            OperatingNameTextBox.Text       = company.OperatingName;
            StreetAddressTextBlock.Text     = company.StreetAddress;
            StreetAddressTextBox.Text       = company.StreetAddress;
            CityTextBlock.Text              = company.City;
            CityTextBox.Text                = company.City;
            StateOrProvinceTextBlock.Text   = company.StateOrProvince;
            StateOrProvinceTextBox.Text     = company.StateOrProvince;
            PostalCodeTextBlock.Text        = company.PostalCode;
            PostalCodeTextBox.Text          = company.PostalCode;
            EmailTextBlock.Text             = company.Email;
            EmailTextBox.Text               = company.Email;
            if (updateCountry)
                UpdateSelectedCountry();

            RegistrationDatetimeTextBlock.Text = company.RegistrationDatetimeFormatted;
        }

        public void UpdateSelectedCountry()
        {
            for (int i = 0; i < this.CountryComboBox.Items.Count; i++) {
                ComboBoxItem item = (ComboBoxItem) this.CountryComboBox.Items[i];
                if (company.CountryId == item.Name) {
                    this.CountryTextBlock.Text = (string)item.Content;
                    this.CountryComboBox.SelectedIndex = i;
                    this.currentUserCountryComboBoxIndex = i;
                    break;
                }
            }
        }

        public void LoadCountriesIntoComboBox()
        {
            List<Country> countries;
            using (var db = new ToolkitContext()) {
                countries = db.Countries.ToList();
            }

            Application.Current.Dispatcher.BeginInvoke(() => {
                Country country;
                for (int i = 0; i < countries.Count; i++) {
                    country = countries[i];
                    string countryNameFormatted = string.Format("{0} ({1})", country.CountryName, country.StateName);
                    var comboItem = new ComboBoxItem();
                    comboItem.Name = country.TwoLetterIsoCode;
                    comboItem.Content = countryNameFormatted;
                    this.CountryComboBox.Items.Add(comboItem);

                    if (company.CountryId == country.TwoLetterIsoCode) {
                        this.CountryTextBlock.Text = countryNameFormatted;
                        this.CountryComboBox.SelectedIndex = i;
                        this.currentUserCountryComboBoxIndex = i;
                    }
                }
            });
        }

        private void UpdateElementsBasedOnAction(ActionType action, bool finishAction)
        {
            Visibility shouldBeVisible = Visibility.Visible;
            Visibility shouldBeCollapsed = Visibility.Collapsed;
            if (finishAction) {
                shouldBeVisible = Visibility.Collapsed;
                shouldBeCollapsed = Visibility.Visible;
            }

            switch (action) {
                case ActionType.CHANGE_DETAILS:
                    if (finishAction) {
                        ClearStatus(null, ConfirmStatusTextBlock);
                        CountryComboBox.SelectedIndex = currentUserCountryComboBoxIndex;
                        UpdateFields(updateCountry: false);
                    }

                    LegalNameTextBox.Visibility          = shouldBeVisible;
                    CompanyFormTextBox.Visibility        = shouldBeVisible;
                    OperatingNameTextBox.Visibility      = shouldBeVisible;
                    StreetAddressTextBox.Visibility      = shouldBeVisible;
                    CityTextBox.Visibility               = shouldBeVisible;
                    StateOrProvinceTextBox.Visibility    = shouldBeVisible;
                    PostalCodeTextBox.Visibility         = shouldBeVisible;
                    EmailTextBox.Visibility              = shouldBeVisible;
                    CountryComboBox.Visibility           = shouldBeVisible;

                    LegalNameTextBlock.Visibility        = shouldBeCollapsed;
                    CompanyFormTextBlock.Visibility      = shouldBeCollapsed;
                    OperatingNameTextBlock.Visibility    = shouldBeCollapsed;
                    StreetAddressTextBlock.Visibility    = shouldBeCollapsed;
                    CityTextBlock.Visibility             = shouldBeCollapsed;
                    StateOrProvinceTextBlock.Visibility  = shouldBeCollapsed;
                    PostalCodeTextBlock.Visibility       = shouldBeCollapsed;
                    EmailTextBlock.Visibility            = shouldBeCollapsed;
                    CountryTextBlock.Visibility          = shouldBeCollapsed;

                    CurrentPasswordTextBlock.Visibility  = shouldBeVisible;
                    CurrentPasswordBox.Visibility        = shouldBeVisible;

                    ChangeDetailsButton.Visibility       = shouldBeCollapsed;
                    ConfirmButton.Visibility             = shouldBeVisible;
                    CancelButton.Visibility              = shouldBeVisible;
                    break;
                case ActionType.NONE:
                    return;
            }

            this.actionType = (finishAction) ? ActionType.NONE : action;
        }

        private void ChangeDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateElementsBasedOnAction(ActionType.CHANGE_DETAILS, finishAction: false);
        }

        private void ChangeDetailsFinish(bool cancelChange)
        {
            string password        = CurrentPasswordBox.Password;
            string legalName       = LegalNameTextBox.Text;
            string companyForm     = CompanyFormTextBox.Text;
            string operatingName   = OperatingNameTextBox.Text;
            string streetAddress   = StreetAddressTextBox.Text;
            string city            = CityTextBox.Text;
            string stateOrProvince = StateOrProvinceTextBox.Text;
            string postalCode      = PostalCodeTextBox.Text;
            string email           = EmailTextBox.Text;
            ComboBoxItem selectedCountry = (ComboBoxItem) CountryComboBox.SelectedItem;
            string countryId = selectedCountry.Name;

            bool shouldCancelChange = cancelChange;
            if (!cancelChange) {
                bool noChangesMade = true;
                noChangesMade &= (company.LegalName       == legalName);
                noChangesMade &= (company.CompanyForm     == companyForm);
                noChangesMade &= (company.OperatingName   == operatingName);
                noChangesMade &= (company.StreetAddress   == streetAddress);
                noChangesMade &= (company.City            == city);
                noChangesMade &= (company.StateOrProvince == stateOrProvince);
                noChangesMade &= (company.PostalCode      == postalCode);
                noChangesMade &= (company.Email           == email);
                noChangesMade &= (company.CountryId       == countryId);
                noChangesMade &= (string.IsNullOrEmpty(password));
                shouldCancelChange = noChangesMade;
            }

            ToolkitContext db = null;
            if (shouldCancelChange)
                goto ChangeDetailsEnd;

            db = new ToolkitContext();
            User? ownerUser = FindUserById(company.OwnerUserId, db);

            bool passwordMatches = (ownerUser.Password == Encryption.GetHashSHA256(password));
            var passwordError = (passwordMatches) ? null : "Неверный пароль.";
            SetStatusError(CurrentPasswordBox, ConfirmStatusTextBlock, passwordError);
            if (!passwordMatches)
                return;

            var emailResult = IsValidEmail(email);
            var emailError = GetErrorMessageFromRegisterCompanyResult(emailResult);
            SetStatusError(EmailTextBox, ConfirmStatusTextBlock, emailError);
            if (emailResult != RegisterCompanyResultType.SUCCESS)
                return;

            var countryResult = IsValidCountry(countryId);
            string countryError = GetErrorMessageFromRegisterCompanyResult(countryResult);
            SetStatusError(CountryComboBox, ConfirmStatusTextBlock, countryError);
            if (countryResult != RegisterCompanyResultType.SUCCESS)
                return;

            Company? dbCompany = FindCompanyByOwnerUserId(company.OwnerUserId, db);
            if (company == null) {
                SetStatusError(null, ConfirmStatusTextBlock, "Не удалось найти компанию.");
                return;
            }

            dbCompany.LegalName       = legalName;
            dbCompany.CompanyForm     = companyForm;
            dbCompany.OperatingName   = operatingName;
            dbCompany.StreetAddress   = streetAddress;
            dbCompany.City            = city;
            dbCompany.StateOrProvince = stateOrProvince;
            dbCompany.PostalCode      = postalCode;
            dbCompany.Email           = email;
            dbCompany.CountryId       = countryId;
            this.company = dbCompany;
            this.ownerWindow.company = dbCompany;

            ownerWindow.NavigationCompanyTextBlock.Text = operatingName;

            SetStatusSuccess(null, ConfirmStatusTextBlock, "Данные успешно изменены.");

        ChangeDetailsEnd:
            if (shouldCancelChange) {
                ClearStatus(CurrentPasswordBox, ConfirmStatusTextBlock);
            }

            this.CurrentPasswordBox.Password = "";
            UpdateElementsBasedOnAction(ActionType.CHANGE_DETAILS, finishAction: true);

            if (!shouldCancelChange) {
                db.SaveChanges();
                db.DisposeAsync();
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.actionType == ActionType.CHANGE_DETAILS)
                ChangeDetailsFinish(cancelChange: false);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.actionType == ActionType.CHANGE_DETAILS)
                ChangeDetailsFinish(cancelChange: true);
        }
    }
}
