using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Toolkit_NET_Client.Models;

namespace Toolkit_NET_Client.Windows
{
    /// <summary>
    /// Логика взаимодействия для UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
        public enum ActionType
        {
            NONE = 0,
            CHANGE_DETAILS = 1,
            CHANGE_PASSWORD = 2
        }

        private User user;
        private ActionType actionType;

        private int currentUserCountryComboBoxIndex;

        public UserWindow(User user)
        {
            InitializeComponent();
            this.user = user;

            var initThread = new Thread(LoadCountriesIntoComboBox);
            initThread.Start();

            this.actionType = ActionType.NONE;
            
            UpdateFields(updateCountry: false);
        }

        private void UpdateFields(bool updateCountry)
        {
            this.NavigationUsernameTextBlock.Text = user.Username;
            this.UsernameTextBlock.Text           = user.Username;
            this.UsernameHeaderTextBlock.Text     = user.Username;

            this.LoginTextBlock.Text    = user.Login;
            this.UsernameTextBlock.Text = user.Username;
            this.UsernameTextBox.Text   = user.Username;
            this.EmailTextBlock.Text    = user.Email;
            this.EmailTextBox.Text      = user.Email;
            this.PhoneTextBlock.Text    = string.IsNullOrEmpty(user.Phone) ? "-" : user.Phone;
            this.PhoneTextBox.Text      = string.IsNullOrEmpty(user.Phone) ? "" : user.Phone;
            if (updateCountry)
                UpdateSelectedCountry();

            DateTime reg;
            bool datetimeParsed = DateTime.TryParse(user.RegistrationDatetime, out reg);
            string datetimeString = (datetimeParsed) ? $"{reg.Day} {ToolkitApp.GetDatetimeMonthName(reg.Month)} {reg.Year}" : "-";
            this.RegistrationDatetimeTextBlock.Text = datetimeString;
            this.WalletTextBlock.Text = $"{user.Wallet,2} р.";
        }

        public void UpdateSelectedCountry()
        {
            for (int i = 0; i < this.CountryComboBox.Items.Count; i++)
            {
                ComboBoxItem item = (ComboBoxItem)this.CountryComboBox.Items[i];
                if (this.user.CountryId == item.Name)
                {
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
            using (var db = new ToolkitContext())
            {
                countries = db.Countries.ToList();
            }

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                Country country;
                for (int i = 0; i < countries.Count; i++)
                {
                    country = countries[i];
                    string countryNameFormatted = string.Format("{0} ({1})", country.CountryName, country.StateName);
                    var comboItem = new ComboBoxItem();
                    comboItem.Name = country.TwoLetterIsoCode;
                    comboItem.Content = countryNameFormatted;
                    this.CountryComboBox.Items.Add(comboItem);

                    if (this.user.CountryId == country.TwoLetterIsoCode)
                    {
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
            if (finishAction)
            {
                shouldBeVisible = Visibility.Collapsed;
                shouldBeCollapsed = Visibility.Visible;
            }

            switch (action)
            {
                case ActionType.CHANGE_DETAILS:
                    if (finishAction)
                    {
                        ToolkitApp.ClearStatus(this.ConfirmStatusTextBlock);
                        this.UsernameTextBox.Text = user.Username;
                        this.EmailTextBox.Text = user.Email;
                        this.PhoneTextBox.Text = string.IsNullOrEmpty(user.Phone) ? "" : user.Phone;
                        this.CountryComboBox.SelectedIndex = this.currentUserCountryComboBoxIndex;
                    }

                    this.UsernameTextBox.Visibility = shouldBeVisible;
                    this.EmailTextBox.Visibility = shouldBeVisible;
                    this.PhoneTextBox.Visibility = shouldBeVisible;
                    this.CountryComboBox.Visibility = shouldBeVisible;

                    this.UsernameTextBlock.Visibility = shouldBeCollapsed;
                    this.EmailTextBlock.Visibility = shouldBeCollapsed;
                    this.PhoneTextBlock.Visibility = shouldBeCollapsed;
                    this.CountryTextBlock.Visibility = shouldBeCollapsed;

                    this.CurrentPasswordTextBlock.Visibility = shouldBeVisible;
                    this.CurrentPasswordBox.Visibility = shouldBeVisible;

                    this.ChangeDetailsButton.Visibility = shouldBeCollapsed;
                    this.ChangePasswordButton.Visibility = shouldBeCollapsed;
                    this.ConfirmButton.Visibility = shouldBeVisible;
                    this.CancelButton.Visibility = shouldBeVisible;
                    break;
                case ActionType.CHANGE_PASSWORD:
                    if (finishAction)
                        ToolkitApp.ClearStatus(this.ConfirmStatusTextBlock);

                    this.CurrentPasswordTextBlock.Visibility = shouldBeVisible;
                    this.CurrentPasswordBox.Visibility = shouldBeVisible;

                    this.NewPasswordTextBlock.Visibility = shouldBeVisible;
                    this.NewPasswordBox.Visibility = shouldBeVisible;

                    this.ChangeDetailsButton.Visibility = shouldBeCollapsed;
                    this.ChangePasswordButton.Visibility = shouldBeCollapsed;
                    this.ConfirmButton.Visibility = shouldBeVisible;
                    this.CancelButton.Visibility = shouldBeVisible;
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
            string password = this.CurrentPasswordBox.Password;
            string email = this.EmailTextBox.Text;
            string username = this.UsernameTextBox.Text;
            string phone = this.PhoneTextBox.Text;
            ComboBoxItem selectedCountry = (ComboBoxItem)this.CountryComboBox.SelectedItem;
            string countryId = selectedCountry.Name;

            bool shouldCancelChange = cancelChange;
            bool phoneNotChanged = true;
            if (!shouldCancelChange)
            {
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
                goto ChangeDetailsEnd;

            bool passwordMatches = (this.user.Password == ToolkitApp.GetHashSHA256(password));
            if (!passwordMatches)
            {
                this.CurrentPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
                ToolkitApp.SetStatusError(this.ConfirmStatusTextBlock, "Неверный пароль.");
                return;
            } 
            else
                this.CurrentPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;

            var emailResult = ToolkitApp.IsValidEmail(email);
            if (emailResult != RegisterUserResultType.SUCCESS)
            {
                this.EmailTextBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
                string emailError = ToolkitApp.GetErrorMessageFromRegisterResult(emailResult);
                ToolkitApp.SetStatusError(this.ConfirmStatusTextBlock, emailError);
                return;
            }
            else
                this.EmailTextBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;

            var usernameResult = ToolkitApp.IsValidUsername(username);
            if (usernameResult != RegisterUserResultType.SUCCESS)
            {
                this.UsernameTextBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
                string usernameError = ToolkitApp.GetErrorMessageFromRegisterResult(usernameResult);
                ToolkitApp.SetStatusError(this.ConfirmStatusTextBlock, usernameError);
                return;
            }
            else
                this.UsernameTextBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;

            if (!phoneNotChanged)
            {
                var phoneResult = ToolkitApp.IsValidPhone(username);
                if (phoneResult != RegisterUserResultType.SUCCESS)
                {
                    this.PhoneTextBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
                    string phoneError = ToolkitApp.GetErrorMessageFromRegisterResult(phoneResult);
                    ToolkitApp.SetStatusError(this.ConfirmStatusTextBlock, phoneError);
                    return;
                }
                else
                    this.PhoneTextBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;
            }

            var countryResult = ToolkitApp.IsValidCountry(countryId);
            if (countryResult != RegisterUserResultType.SUCCESS)
            {
                this.CountryComboBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
                string countryError = ToolkitApp.GetErrorMessageFromRegisterResult(countryResult);
                ToolkitApp.SetStatusError(this.ConfirmStatusTextBlock, countryError);
                return;
            }
            else
                this.CountryComboBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;

            db = new ToolkitContext();
            User? user = ToolkitApp.FindUser(this.user.Id, db);
            if (user == null)
            {
                ToolkitApp.SetStatusError(this.ConfirmStatusTextBlock, "Не удалось найти пользователя.");
                return;
            }

            user.Username = username;
            user.Email = email;
            user.Phone = phone;
            user.CountryId = countryId;
            this.user = user;

            this.UsernameTextBlock.Text = username;
            this.UsernameHeaderTextBlock.Text = username;

            ToolkitApp.SetStatusSuccess(this.ConfirmStatusTextBlock, "Данные успешно изменены.");

        ChangeDetailsEnd:
            if (shouldCancelChange)
            {
                ToolkitApp.ClearStatus(this.ConfirmStatusTextBlock);
                this.CurrentPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;
            }

            this.CurrentPasswordBox.Password = "";

            UpdateElementsBasedOnAction(ActionType.CHANGE_DETAILS, finishAction: true);

            if (!shouldCancelChange)
            {
                db.SaveChanges();
                db.DisposeAsync();
            }
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateElementsBasedOnAction(ActionType.CHANGE_PASSWORD, finishAction: false);
        }

        private void ChangePasswordFinish(bool cancelChange)
        {
            string oldPassword = this.CurrentPasswordBox.Password;
            string newPassword = this.NewPasswordBox.Password;

            bool shouldCancelChange = cancelChange;
            if (!shouldCancelChange)
            {
                bool noChangesMade = true;
                noChangesMade &= string.IsNullOrEmpty(oldPassword);
                noChangesMade &= string.IsNullOrEmpty(newPassword);
                shouldCancelChange = noChangesMade;
            }
            ToolkitContext db = null;
            if (shouldCancelChange)
                goto ChangePasswordEnd;

            bool oldPasswordMatches = (this.user.Password == ToolkitApp.GetHashSHA256(oldPassword));
            if (!oldPasswordMatches)
            {
                this.CurrentPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
                this.NewPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;
                ToolkitApp.SetStatusError(this.ConfirmStatusTextBlock, "Неверный пароль.");
                return;
            }

            var passwordResult = ToolkitApp.IsValidPassword(newPassword);
            bool validPassword = passwordResult == RegisterUserResultType.SUCCESS;
            string passwordError = ToolkitApp.GetErrorMessageFromRegisterResult(passwordResult);
            ToolkitApp.SetStatusError(this.ConfirmStatusTextBlock, passwordError);
            if (!validPassword)
            {
                this.CurrentPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;
                this.NewPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
                return;
            }

            bool newPasswordMatchesOld = (oldPassword == newPassword);
            if (newPasswordMatchesOld)
            {
                this.CurrentPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;
                this.NewPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
                ToolkitApp.SetStatusError(this.ConfirmStatusTextBlock, "Пароль совпадает с существующим.");
                return;
            }

            this.CurrentPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;
            this.NewPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;

            db = new ToolkitContext();
            User? user = ToolkitApp.FindUser(this.user.Id, db);
            if (user == null)
            {
                ToolkitApp.SetStatusError(this.ConfirmStatusTextBlock, "Не удалось найти пользователя.");
                return;
            }

            user.Password = ToolkitApp.GetHashSHA256(newPassword);
            this.user = user;

            ToolkitApp.SetStatusError(this.ConfirmStatusTextBlock, "Пароль успешно изменен.");

        ChangePasswordEnd:
            if (shouldCancelChange)
            {
                ToolkitApp.ClearStatus(this.ConfirmStatusTextBlock);
                this.CurrentPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;
            }

            this.CurrentPasswordBox.Password = "";
            this.NewPasswordBox.Password = "";

            UpdateElementsBasedOnAction(ActionType.CHANGE_PASSWORD, finishAction: true);

            if (!shouldCancelChange)
            {
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

            double funds;
            bool parsed = double.TryParse(fundsString, out funds);
            if (!parsed)
            {
                this.AddFundsTextBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
                ToolkitApp.SetStatusError(this.AddFundsStatusTextBlock, "Данная сумма некорректна. Пожалуйста, введите корректую сумму.");
                return;
            }
            else
                this.AddFundsTextBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;

            using (var db = new ToolkitContext())
            {
                User user = null;
                foreach (var dbUser in db.Users)
                {
                    if (dbUser.Id == this.user.Id)
                    {
                        user = dbUser;
                        break;
                    }
                }

                if (user == null)
                {
                    ToolkitApp.SetStatusError(this.AddFundsStatusTextBlock, "Не удалось найти пользователя.");
                    return;
                }

                user.Wallet += funds;
                this.user = user;

                this.WalletTextBlock.Text = this.WalletTextBlock.Text = $"{user.Wallet,2} р.";

                ToolkitApp.SetStatusSuccess(this.AddFundsStatusTextBlock, $"Счет успешно пополнен на {funds} р.");

                db.SaveChanges();
            }
        }

        private void NavigationStoreTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var storeWindow = new StoreWindow(this.user);
            storeWindow.Show();
            this.Close();
        }

        private void NavigationLibraryTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var libraryWindow = new LibraryWindow(this.user);
            libraryWindow.Show();
            this.Close();
        }
    }
}
