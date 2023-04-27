using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using Toolkit_NET_Client.Models;

namespace Toolkit_NET_Client.Windows
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();

            var initThread = new Thread(LoadCountriesIntoComboBox);
            initThread.Start();

            this.EmailTextBox.MaxLength = User.EMAIL_MAX_LENGTH;
            this.LoginTextBox.MaxLength = User.LOGIN_MAX_LENGTH;
            this.PasswordBox.MaxLength = User.PASSWORD_MAX_LENGTH;
            this.UsernameTextBox.MaxLength = User.USERNAME_MAX_LENGTH;
            this.ConfirmPasswordBox.MaxLength = User.PASSWORD_MAX_LENGTH;
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
                foreach (var country in countries)
                {
                    string item = string.Format("{0} ({1})", country.CountryName, country.StateName);
                    var comboItem = new ComboBoxItem();
                    comboItem.Name = country.TwoLetterIsoCode;
                    comboItem.Content = item;
                    this.CountryComboBox.Items.Add(comboItem);
                }
            });
        }

        private void AuthPasswordHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = this.LoginTextBox.Text;
            string password = this.PasswordBox.Password;
            string username = this.UsernameTextBox.Text;
            string email = this.EmailTextBox.Text;
            ComboBoxItem selectedItem = (ComboBoxItem)this.CountryComboBox.SelectedItem;
            string countryId;
            if (selectedItem == null)
                countryId = null;
            else
                countryId = selectedItem.Name;

            // Login
            var loginResult = ToolkitApp.IsValidLogin(login);
            if (loginResult != RegisterUserResultType.SUCCESS)
                this.LoginTextBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
            else
                this.LoginTextBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;

            string loginError = ToolkitApp.GetErrorMessageFromRegisterResult(loginResult);
            ToolkitApp.SetStatusError(this.LoginStatusTextBlock, loginError);

            // Password
            var passwordResult = ToolkitApp.IsValidPassword(password);
            var confirmPasswordMatchesOriginal = false;
            if (passwordResult != RegisterUserResultType.SUCCESS)
            {
                this.PasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
            }
            else
            {
                this.PasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;

                // Confirm password
                string confirmPassword = this.ConfirmPasswordBox.Password;
                if (string.IsNullOrWhiteSpace(confirmPassword))
                {
                    ToolkitApp.SetStatusError(this.ConfirmPasswordStatusTextBlock, "Введите пароль.");
                    this.ConfirmPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
                }
                else if (password != confirmPassword)
                {
                    ToolkitApp.SetStatusError(this.ConfirmPasswordStatusTextBlock, "Пароли не совпадают.");
                    this.ConfirmPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
                }
                else
                {
                    confirmPasswordMatchesOriginal = true;
                    ToolkitApp.ClearStatus(this.ConfirmPasswordStatusTextBlock);
                    this.ConfirmPasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;
                }
            }

            string passwordError = ToolkitApp.GetErrorMessageFromRegisterResult(passwordResult);
            ToolkitApp.SetStatusError(this.PasswordStatusTextBlock, passwordError);

            // Username
            var usernameResult = ToolkitApp.IsValidUsername(username);
            if (usernameResult != RegisterUserResultType.SUCCESS)
                this.UsernameTextBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
            else
                this.UsernameTextBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;

            string usernameError = ToolkitApp.GetErrorMessageFromRegisterResult(usernameResult);
            ToolkitApp.SetStatusError(this.UsernameStatusTextBlock, usernameError);

            // Email
            var emailResult = ToolkitApp.IsValidEmail(email);
            if (emailResult != RegisterUserResultType.SUCCESS)
                this.EmailTextBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
            else
                this.EmailTextBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;

            string emailError = ToolkitApp.GetErrorMessageFromRegisterResult(emailResult);
            ToolkitApp.SetStatusError(this.EmailStatusTextBlock, emailError);

            // Country
            var countryResult = ToolkitApp.IsValidCountry(countryId);
            if (countryResult != RegisterUserResultType.SUCCESS)
                this.CountryComboBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
            else
                this.CountryComboBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;

            string countryError = ToolkitApp.GetErrorMessageFromRegisterResult(countryResult);
            ToolkitApp.SetStatusError(this.CountryStatusTextBlock, countryError);

            // Terms
            bool? checkboxChecked = this.AcceptTermsCheckBox.IsChecked;
            bool termsAccepted = (checkboxChecked != null) ? (bool)checkboxChecked : false;
            if (!termsAccepted)
            {
                this.AcceptTermsCheckBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
                ToolkitApp.SetStatusError(this.AcceptTermsStatusTextBlock, "Подтвердите соглашение.");
            }
            else
            {
                this.AcceptTermsCheckBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;
                ToolkitApp.ClearStatus(this.AcceptTermsStatusTextBlock);
            }

            bool canRegister = true;
            canRegister &= !Convert.ToBoolean((int)loginResult);
            canRegister &= !Convert.ToBoolean((int)passwordResult);
            canRegister &= confirmPasswordMatchesOriginal;
            canRegister &= !Convert.ToBoolean((int)usernameResult);
            canRegister &= !Convert.ToBoolean((int)emailResult);
            canRegister &= !Convert.ToBoolean((int)countryResult);
            canRegister &= termsAccepted;

            if (canRegister)
            {
                RegisterUserResult registerResult = ToolkitApp._UncheckedRegisterUser(login, password, username, email, countryId);
                string errorString = ToolkitApp.GetErrorMessageFromRegisterResult(registerResult.result);
                ToolkitApp.SetStatusError(this.RegisterStatusTextBlock, errorString);
            }
        }
    }
}
