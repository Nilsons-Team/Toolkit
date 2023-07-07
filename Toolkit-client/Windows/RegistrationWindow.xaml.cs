using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using Toolkit_Client.Models;

using static Toolkit_Client.Modules.UserRegistration;
using static Toolkit_Client.Modules.ClientActionStatus;

using Toolkit_Shared;

namespace Toolkit_Client.Windows
{
    /// <summary>
    /// Логика взаимодействия для RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : ToolkitWindow
    {
        public RegistrationWindow(Logger logger) : base(logger)
        {
            InitializeComponent();

            var initThread = new Thread(LoadCountriesIntoComboBox);
            initThread.Start();

            EmailTextBox.MaxLength       = User.EMAIL_MAX_LENGTH;
            LoginTextBox.MaxLength       = User.LOGIN_MAX_LENGTH;
            PasswordBox.MaxLength        = User.PASSWORD_MAX_LENGTH;
            UsernameTextBox.MaxLength    = User.USERNAME_MAX_LENGTH;
            ConfirmPasswordBox.MaxLength = User.PASSWORD_MAX_LENGTH;
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

        private void AuthPasswordHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow(logger);
            loginWindow.Show();
            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login    = LoginTextBox.Text;
            string password = PasswordBox.Password;
            string username = UsernameTextBox.Text;
            string email    = EmailTextBox.Text;
            ComboBoxItem selectedItem = (ComboBoxItem) CountryComboBox.SelectedItem;
            string countryId;
            if (selectedItem == null)
                countryId = null;
            else
                countryId = selectedItem.Name;

            // Login
            var loginResult = IsValidLogin(login);
            string loginError = GetErrorMessageFromRegisterUserResult(loginResult);
            SetStatusError(LoginTextBox, LoginStatusTextBlock, loginError);

            // Password
            var passwordResult = IsValidPassword(password);
            var confirmPasswordMatchesOriginal = false;
            if (passwordResult == RegisterUserResultType.SUCCESS) {
                // Confirm password
                string confirmPassword = ConfirmPasswordBox.Password;
                if (string.IsNullOrWhiteSpace(confirmPassword)) {
                    SetStatusError(ConfirmPasswordBox, ConfirmPasswordStatusTextBlock, "Введите пароль.");
                } else if (password != confirmPassword) {
                    SetStatusError(ConfirmPasswordBox, ConfirmPasswordStatusTextBlock, "Пароли не совпадают.");
                } else {
                    confirmPasswordMatchesOriginal = true;
                    ClearStatus(ConfirmPasswordBox, ConfirmPasswordStatusTextBlock);
                }
            }

            string passwordError = GetErrorMessageFromRegisterUserResult(passwordResult);
            SetStatusError(PasswordBox, PasswordStatusTextBlock, passwordError);

            // Username
            var usernameResult = IsValidUsername(username);
            string usernameError = GetErrorMessageFromRegisterUserResult(usernameResult);
            SetStatusError(UsernameTextBox, UsernameStatusTextBlock, usernameError);

            // Email
            var emailResult = IsValidEmail(email);
            string emailError = GetErrorMessageFromRegisterUserResult(emailResult);
            SetStatusError(EmailTextBox, EmailStatusTextBlock, emailError);

            // Country
            var countryResult = IsValidCountry(countryId);
            string countryError = GetErrorMessageFromRegisterUserResult(countryResult);
            SetStatusError(CountryComboBox, CountryStatusTextBlock, countryError);

            // Terms
            bool? checkboxChecked = AcceptTermsCheckBox.IsChecked;
            bool termsAccepted = (checkboxChecked != null) ? (bool)checkboxChecked : false;
            if (!termsAccepted) {
                SetStatusError(AcceptTermsCheckBox, AcceptTermsStatusTextBlock, "Подтвердите соглашение.");
            } else {
                ClearStatus(AcceptTermsCheckBox, AcceptTermsStatusTextBlock);
            }

            bool canRegister = true;
            canRegister &= !Convert.ToBoolean((int)loginResult);
            canRegister &= !Convert.ToBoolean((int)passwordResult);
            canRegister &= confirmPasswordMatchesOriginal;
            canRegister &= !Convert.ToBoolean((int)usernameResult);
            canRegister &= !Convert.ToBoolean((int)emailResult);
            canRegister &= !Convert.ToBoolean((int)countryResult);
            canRegister &= termsAccepted;

            if (!canRegister)
                return;

            RegisterUserResult registerResult = _UncheckedRegisterUser(login, password, username, email, countryId, phone: null);
            if (registerResult.result == RegisterUserResultType.SUCCESS) {
                this.RegisterButton.IsEnabled = false;
                SetStatusSuccess(null, RegisterStatusTextBlock, "Вы успешно зарегистрировались.");
            } else {
                string errorString = GetErrorMessageFromRegisterUserResult(registerResult.result);
                SetStatusError(null, RegisterStatusTextBlock, errorString);
            }
        }
    }
}
