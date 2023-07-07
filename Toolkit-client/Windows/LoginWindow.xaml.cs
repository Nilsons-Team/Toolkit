using System;
using System.Windows;

using Microsoft.EntityFrameworkCore;

using Toolkit_Client.Models;
using Toolkit_Client.Modules;

using static Toolkit_Client.Modules.UserAuth;
using static Toolkit_Client.Modules.ClientActionStatus;

using Toolkit_Shared;
using Toolkit_Shared.Network.Packets;

namespace Toolkit_Client.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : ToolkitWindow
    {
        public LoginWindow(Logger logger) : base(logger)
        {
            InitializeComponent();

            LoginTextBox.MaxLength = User.LOGIN_MAX_LENGTH;
            PasswordBox.MaxLength = User.PASSWORD_MAX_LENGTH;

            using (var db = new ToolkitContext()) {
                db.Countries.FirstOrDefaultAsync();
            }

            ColorTheme.InitializeThemes();
            var app = (ToolkitApp) Application.Current;
            app.ChangeColorTheme(ColorTheme.LightColorTheme);
        }

        private void RegistrationHyperlink_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new RegistrationWindow(logger);
            registrationWindow.Show();
            this.Close();
        }

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            string login    = LoginTextBox.Text;
            string password = PasswordBox.Password;

            var loginResult = (string.IsNullOrWhiteSpace(login)) ? AuthUserResultType.FAIL_EMPTY_LOGIN : AuthUserResultType.SUCCESS;
            string loginError = GetErrorMessageFromAuthResult(loginResult);
            SetStatusError(LoginTextBox, LoginStatusTextBlock, loginError);

            var passwordResult = (string.IsNullOrWhiteSpace(password)) ? AuthUserResultType.FAIL_EMPTY_PASSWORD : AuthUserResultType.SUCCESS;
            string passwordError = GetErrorMessageFromAuthResult(passwordResult);
            SetStatusError(PasswordBox, PasswordStatusTextBlock, passwordError);

            bool canAuth = true;
            canAuth &= !Convert.ToBoolean((int)loginResult);
            canAuth &= !Convert.ToBoolean((int)passwordResult);

            if (canAuth) {
                AuthUserResult authResult = AuthUser(login, password);
                string authError = GetErrorMessageFromAuthResult(authResult.result);
                SetStatusError(null, StatusTextBlock, authError);

                if (authResult.result == AuthUserResultType.SUCCESS) {
                    // logger.Info($"User logged in as '{authResult.user.Login}'.");

                    ToolkitApp app = (ToolkitApp) Application.Current;
                    UserAuthPacket packet = new UserAuthPacket()
                    {
                        packetType = PacketType.USER_AUTH_PACKET,
                        Login = login,
                        Password = Encryption.GetHashSHA256(password)
                    };
                    app.client.SendPacket(packet.packetType, packet);

                    /*
                    var newUserWindow = new MainWindow(logger, authResult.user);
                    newUserWindow.Show();
                    this.Close();
                    */
                }
            } else {
                ClearStatus(null, StatusTextBlock);
            }
        }
    }
}
