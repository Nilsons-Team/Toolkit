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
using static Toolkit_Client.Modules.UserRegistration;
using static Toolkit_Client.Modules.ClientActionStatus;

namespace Toolkit_Client.Pages
{
    /// <summary>
    /// Interaction logic for UserPage.xaml
    /// </summary>
    public partial class UserProfilePage : ToolkitPage
    {
        public enum ActionType
        {
            NONE = 0,
            CHANGE_DETAILS = 1,
            CHANGE_PASSWORD = 2
        }

        public MainWindow ownerWindow;
        public PartnerWindow partnerWindow;
        private User user;

        private ActionType actionType;

        private int currentUserCountryComboBoxIndex;

        public UserProfilePage(MainWindow ownerWindow, User user) : base(ownerWindow.GetLogger())
        {
            InitializeComponent();
            this.ownerWindow = ownerWindow;
            this.partnerWindow = null;
            this.user = user;

            var initThread = new Thread(LoadCountriesIntoComboBox);
            initThread.Start();

            this.actionType = ActionType.NONE;

            UsernameTextBox.MaxLength = User.USERNAME_MAX_LENGTH;
            EmailTextBox.MaxLength    = User.EMAIL_MAX_LENGTH;
            PhoneTextBox.MaxLength    = User.PHONE_MAX_LENGTH;
            
            UpdateFields(updateCountry: false);
        }

        private void UpdateFields(bool updateCountry)
        {
            UsernameTextBlock.Text = user.Username;

            LoginTextBlock.Text    = user.Login;
            UsernameTextBlock.Text = user.Username;
            UsernameTextBox.Text   = user.Username;
            EmailTextBlock.Text    = user.Email;
            EmailTextBox.Text      = user.Email;
            PhoneTextBlock.Text    = string.IsNullOrEmpty(user.Phone) ? "-" : user.Phone;
            PhoneTextBox.Text      = string.IsNullOrEmpty(user.Phone) ? "" : user.Phone;
            if (updateCountry)
                UpdateSelectedCountry();

            RegistrationDatetimeTextBlock.Text = user.RegistrationDatetimeFormatted;
            WalletTextBlock.Text = user.WalletFormatted;
        }

        public void UpdateSelectedCountry()
        {
            for (int i = 0; i < CountryComboBox.Items.Count; i++) {
                ComboBoxItem item = (ComboBoxItem) CountryComboBox.Items[i];
                if (this.user.CountryId == item.Name) {
                    CountryTextBlock.Text = (string)item.Content;
                    CountryComboBox.SelectedIndex = i;
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
                    CountryComboBox.Items.Add(comboItem);

                    if (this.user.CountryId == country.TwoLetterIsoCode) {
                        CountryTextBlock.Text = countryNameFormatted;
                        CountryComboBox.SelectedIndex = i;
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
                        UsernameTextBox.Text = user.Username;
                        EmailTextBox.Text    = user.Email;
                        PhoneTextBox.Text    = string.IsNullOrEmpty(user.Phone) ? "" : user.Phone;
                        CountryComboBox.SelectedIndex = this.currentUserCountryComboBoxIndex;
                    }

                    UsernameTextBox.Visibility          = shouldBeVisible;
                    EmailTextBox.Visibility             = shouldBeVisible;
                    PhoneTextBox.Visibility             = shouldBeVisible;
                    CountryComboBox.Visibility          = shouldBeVisible;

                    UsernameTextBlock.Visibility        = shouldBeCollapsed;
                    EmailTextBlock.Visibility           = shouldBeCollapsed;
                    PhoneTextBlock.Visibility           = shouldBeCollapsed;
                    CountryTextBlock.Visibility         = shouldBeCollapsed;

                    CurrentPasswordTextBlock.Visibility = shouldBeVisible;
                    CurrentPasswordBox.Visibility       = shouldBeVisible;

                    ChangeDetailsButton.Visibility      = shouldBeCollapsed;
                    ChangePasswordButton.Visibility     = shouldBeCollapsed;
                    ConfirmButton.Visibility            = shouldBeVisible;
                    CancelButton.Visibility             = shouldBeVisible;
                    break;
                case ActionType.CHANGE_PASSWORD:
                    if (finishAction)
                        ClearStatus(null, ConfirmStatusTextBlock);

                    CurrentPasswordTextBlock.Visibility = shouldBeVisible;
                    CurrentPasswordBox.Visibility       = shouldBeVisible;

                    NewPasswordTextBlock.Visibility     = shouldBeVisible;
                    NewPasswordBox.Visibility           = shouldBeVisible;

                    ChangeDetailsButton.Visibility      = shouldBeCollapsed;
                    ChangePasswordButton.Visibility     = shouldBeCollapsed;
                    ConfirmButton.Visibility            = shouldBeVisible;
                    CancelButton.Visibility             = shouldBeVisible;
                    break;
                case ActionType.NONE:
                    return;
            }

            logger.Info($"Updated elements based on action '{action}' (finishAction: {finishAction}).");
            this.actionType = (finishAction) ? ActionType.NONE : action;
        }

        private void ChangeDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateElementsBasedOnAction(ActionType.CHANGE_DETAILS, finishAction: false);
        }

        private void ChangeDetailsFinish(bool cancelChange)
        {
            string password = CurrentPasswordBox.Password;
            string email    = EmailTextBox.Text;
            string username = UsernameTextBox.Text;
            string phone    = PhoneTextBox.Text;
            ComboBoxItem selectedCountry = (ComboBoxItem) CountryComboBox.SelectedItem;
            string countryId = selectedCountry.Name;

            bool shouldCancelChange = cancelChange;
            bool phoneNotChanged = true;
            if (!shouldCancelChange) {
                phoneNotChanged = (this.user.Phone == phone || (this.user.Phone == null && string.IsNullOrEmpty(phone)));
                bool noChangesMade = true;
                noChangesMade &= (this.user.Email == email);
                noChangesMade &= (this.user.Username == username);
                noChangesMade &= phoneNotChanged;
                noChangesMade &= (this.user.CountryId == countryId);
                noChangesMade &= (string.IsNullOrEmpty(password));
                shouldCancelChange = noChangesMade;
            }

            ToolkitContext db = null;
            if (shouldCancelChange)
            {
                ClearStatus(CurrentPasswordBox, ConfirmStatusTextBlock);
                goto ChangeDetailsEnd;
            }

            bool passwordMatches = (this.user.Password == Encryption.GetHashSHA256(password));
            var passwordError = (passwordMatches) ? null : "Неверный пароль.";
            SetStatusError(CurrentPasswordBox, ConfirmStatusTextBlock, passwordError);
            if (!passwordMatches)
                return;

            var emailResult = IsValidEmail(email);
            string emailError = GetErrorMessageFromRegisterUserResult(emailResult);
            SetStatusError(EmailTextBox, ConfirmStatusTextBlock, emailError);
            if (emailResult != RegisterUserResultType.SUCCESS)
                return;

            var usernameResult = IsValidUsername(username);
            string usernameError = GetErrorMessageFromRegisterUserResult(usernameResult);
            SetStatusError(UsernameTextBox, ConfirmStatusTextBlock, usernameError);
            if (usernameResult != RegisterUserResultType.SUCCESS)
                return;

            if (phoneNotChanged == false) {
                var phoneResult = IsValidPhone(phone);
                string phoneError = GetErrorMessageFromRegisterUserResult(phoneResult);
                SetStatusError(PhoneTextBox, ConfirmStatusTextBlock, phoneError);
                if (phoneResult != RegisterUserResultType.SUCCESS)
                    return;
            }

            var countryResult = IsValidCountry(countryId);
            string countryError = GetErrorMessageFromRegisterUserResult(countryResult);
            SetStatusError(CountryComboBox, ConfirmStatusTextBlock, countryError);
            if (countryResult != RegisterUserResultType.SUCCESS)
                return;

            db = new ToolkitContext();
            User? user = FindUserById(this.user.Id, db);
            if (user == null) {
                logger.Warning("User was not found in database, which was not expected. Changes would not be saved." +
                               $"\n\t... User login was: '{this.user.Login}'.");
                SetStatusError(null, ConfirmStatusTextBlock, "Не удалось найти пользователя.");
                return;
            }

            user.Username = username;
            user.Email = email;
            user.Phone = phone;
            user.CountryId = countryId;
            this.user = user;
            this.ownerWindow.user = user;

            this.UsernameTextBlock.Text = username;
            this.ownerWindow.NavigationUsernameTextBlock.Text = username;
            UpdateFields(true);

            db.SaveChanges();
            db.DisposeAsync();

            logger.Info("User account information was updated.");
            SetStatusSuccess(null, ConfirmStatusTextBlock, "Данные успешно изменены.");

        ChangeDetailsEnd:
            this.CurrentPasswordBox.Password = "";
            UpdateElementsBasedOnAction(ActionType.CHANGE_DETAILS, finishAction: true);
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateElementsBasedOnAction(ActionType.CHANGE_PASSWORD, finishAction: false);
        }

        private void ChangePasswordFinish(bool cancelChange)
        {
            string oldPassword = CurrentPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;

            bool shouldCancelChange = cancelChange;
            if (!shouldCancelChange) {
                bool noChangesMade = true;
                noChangesMade &= string.IsNullOrEmpty(oldPassword);
                noChangesMade &= string.IsNullOrEmpty(newPassword);
                shouldCancelChange = noChangesMade;
            }

            ToolkitContext db = null;
            if (shouldCancelChange)
                goto ChangePasswordEnd;

            bool oldPasswordMatches = (this.user.Password == Encryption.GetHashSHA256(oldPassword));
            if (!oldPasswordMatches) {
                ResetBorderBrushColor(NewPasswordBox);
                SetStatusError(CurrentPasswordBox, ConfirmStatusTextBlock, "Неверный пароль.");
                return;
            }

            var passwordResult = IsValidPassword(newPassword);
            string passwordError = GetErrorMessageFromRegisterUserResult(passwordResult);
            SetStatusError(NewPasswordBox, ConfirmStatusTextBlock, passwordError);
            bool validPassword = passwordResult == RegisterUserResultType.SUCCESS;
            if (!validPassword) {
                ResetBorderBrushColor(CurrentPasswordBox);
                return;
            }

            bool newPasswordMatchesOld = (oldPassword == newPassword);
            if (newPasswordMatchesOld) {
                ResetBorderBrushColor(CurrentPasswordBox);
                SetStatusError(NewPasswordBox, ConfirmStatusTextBlock, "Пароль совпадает с существующим.");
                return;
            }

            ResetBorderBrushColor(CurrentPasswordBox);
            ResetBorderBrushColor(NewPasswordBox);

            db = new ToolkitContext();
            User? user = FindUserById(this.user.Id, db);
            if (user == null) {
                logger.Warning("User was not found in database, which was not expected. Changes would not be saved." +
                               $"\n\t... User login was: '{this.user.Login}'.");
                SetStatusError(null, ConfirmStatusTextBlock, "Не удалось найти пользователя.");
                return;
            }

            user.Password = Encryption.GetHashSHA256(newPassword);
            this.user = user;

            SetStatusError(null, ConfirmStatusTextBlock, "Пароль успешно изменен.");

        ChangePasswordEnd:
            if (shouldCancelChange) {
                ClearStatus(CurrentPasswordBox, ConfirmStatusTextBlock);
            }

            this.CurrentPasswordBox.Password = "";
            this.NewPasswordBox.Password = "";
            UpdateElementsBasedOnAction(ActionType.CHANGE_PASSWORD, finishAction: true);

            if (!shouldCancelChange) {
                db.SaveChanges();
                db.DisposeAsync();
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.actionType == ActionType.CHANGE_DETAILS)
                ChangeDetailsFinish(cancelChange: false);
            else if (this.actionType == ActionType.CHANGE_PASSWORD)
                ChangePasswordFinish(cancelChange: false);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.actionType == ActionType.CHANGE_DETAILS)
                ChangeDetailsFinish(cancelChange: true);
            else if (this.actionType == ActionType.CHANGE_PASSWORD)
                ChangePasswordFinish(cancelChange: true);
        }

        private void AddFundsButton_Click(object sender, RoutedEventArgs e)
        {
            string fundsString = this.AddFundsTextBox.Text;

            double funds = 0;
            bool parsed = double.TryParse(fundsString, out funds);
            if (!parsed || funds < 0) {
                SetStatusError(AddFundsTextBox, AddFundsStatusTextBlock, "Данная сумма некорректна. Пожалуйста, введите корректую сумму.");
                return;
            }

            using (var db = new ToolkitContext()) {
                User user = null;
                foreach (var dbUser in db.Users) {
                    if (dbUser.Id == this.user.Id) {
                        user = dbUser;
                        break;
                    }
                }

                if (user == null) {
                    logger.Warning("User was not found in database, which was not expected. Changes would not be saved." +
                               $"\n\t... User login was: '{this.user.Login}'.");
                    SetStatusError(AddFundsTextBox, AddFundsStatusTextBlock, "Не удалось найти пользователя.");
                    return;
                }

                user.Wallet += funds;
                this.user = user;
                this.ownerWindow.user = user;

                WalletTextBlock.Text = user.WalletFormatted;

                db.SaveChanges();

                logger.Info("Funds were added to user account.");
                SetStatusSuccess(AddFundsTextBox, AddFundsStatusTextBlock, $"Счет успешно пополнен на {funds} р.");
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из аккаунта?", "Toolkit - Выход из аккаунта", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes) {
                logger.Info($"User logged out from account '{this.user.Login}'.");
                var authWindow = new LoginWindow(logger);
                authWindow.Show();
                this.ownerWindow.Close();
            }
        }

        private void PartnershipButton_Click(object sender, RoutedEventArgs e)
        {
            if (partnerWindow == null) {
                partnerWindow = new PartnerWindow(logger, this, user);
                partnerWindow.Owner = this.ownerWindow;
                partnerWindow.Show();
            } else {
                partnerWindow.Focus();
            }
        }
    }
}
