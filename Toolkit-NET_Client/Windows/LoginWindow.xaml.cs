using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;

using Toolkit_NET_Client.Models;

namespace Toolkit_NET_Client.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            this.LoginTextBox.MaxLength = User.LOGIN_MAX_LENGTH;
            this.PasswordBox.MaxLength = User.PASSWORD_MAX_LENGTH;

            using (var db = new ToolkitContext())
            {
                db.Countries.FirstOrDefaultAsync();
            }
        }

        private void ForgotPasswordHyperlink_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RegistrationHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            this.Close();
        }

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            string login = this.LoginTextBox.Text;
            string password = this.PasswordBox.Password;

            // Login
            var loginResult = (string.IsNullOrWhiteSpace(login)) ? AuthUserResultType.FAIL_EMPTY_LOGIN : AuthUserResultType.SUCCESS;
            if (loginResult != AuthUserResultType.SUCCESS)
                this.LoginTextBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
            else
                this.LoginTextBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;

            string loginError = ToolkitApp.GetErrorMessageFromAuthResult(loginResult);
            ToolkitApp.SetStatusError(this.LoginStatusTextBlock, loginError);

            // Password
            var passwordResult = (string.IsNullOrWhiteSpace(password)) ? AuthUserResultType.FAIL_EMPTY_PASSWORD : AuthUserResultType.SUCCESS;
            if (passwordResult != AuthUserResultType.SUCCESS)
                this.PasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
            else
                this.PasswordBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;

            string passwordError = ToolkitApp.GetErrorMessageFromAuthResult(passwordResult);
            ToolkitApp.SetStatusError(this.PasswordStatusTextBlock, passwordError);

            bool canAuth = true;
            canAuth &= !Convert.ToBoolean((int)loginResult);
            canAuth &= !Convert.ToBoolean((int)passwordResult);

            if (canAuth)
            {
                AuthUserResult authResult = ToolkitApp.AuthUser(login, password);
                string authError = ToolkitApp.GetErrorMessageFromAuthResult(authResult.result);
                ToolkitApp.SetStatusError(this.StatusTextBlock, authError);

                if (authResult.result == AuthUserResultType.SUCCESS)
                {
                    var storeWindow = new StoreWindow(authResult.user);
                    storeWindow.Show();
                    this.Close();
                }
            }
            else
            {
                ToolkitApp.ClearStatus(this.StatusTextBlock);
            }
        }
    }
}
