using System;
using System.Threading;
using System.Windows;

using Toolkit_Shared;
using Toolkit_Shared.Network.Packets;

using Toolkit_Client.Modules;
using Toolkit_Client.Windows;

namespace Toolkit_Client
{
    /// <summary>
    /// Interaction logic for ToolkitApp.xaml
    /// </summary>
    public partial class ToolkitApp : Application
    {
        private ColorTheme _CurrentTheme;
        private Logger logger;
        public Client client;

        public ColorTheme CurrentTheme 
        { 
            get { return _CurrentTheme; } 
            set { ChangeColorTheme(value); }
        }

        public static string DbFilePath;

        protected override void OnStartup(StartupEventArgs e)
        {
            InitDbConnection();
            Thread.CurrentThread.Name = "Main";

            logger = new Logger("log.txt");
            client = new Client();

            logger.Info($"App has been started ({DateTime.Now})." +
                         "\n\t\t\t--------------------" +
                         "\n\t\t\t   Toolkit Client" +
                         "\n\t\t\t--------------------");
            
            var loginWindow = new LoginWindow(logger);
            loginWindow.Show();

            client.Connect("127.0.0.1", 1337);
        }

        public void ChangeColorTheme(ColorTheme theme) {
            Resources["ErrorColor"]            = theme.ErrorColor;
            Resources["WarningColor"]          = theme.WarningColor;
            Resources["SuccessColor"]          = theme.SuccessColor;

            Resources["WindowBackgroundColor"] = theme.WindowBackgroundColor;
            Resources["PrimaryColor"]          = theme.PrimaryColor;
            Resources["SecondaryColor"]        = theme.SecondaryColor;
            Resources["ButtonColor"]           = theme.ButtonColor;
            Resources["ButtonMouseOverColor"]  = theme.ButtonMouseOverColor;
            Resources["SelectionBrush"]        = theme.SelectionBrush;
            Resources["TextBoxColor"]          = theme.TextBoxColor;

            _CurrentTheme = theme;
            ClientActionStatus.UpdateColorTheme(theme);
        }

        public static void InitDbConnection() {
            string executablePath = Environment.CurrentDirectory;
            string dbFilePath = executablePath + "\\Toolkit.db";
            DbFilePath = dbFilePath;
        }

        public static int FindFirstFolderFromEnd(string path, string searchFolderName)
        {
            char pathChar;
            char folderChar;
            bool found;
            for (int pathCharIndex = path.Length - 1; pathCharIndex >= 0; pathCharIndex--)
            {
                found = true;

                for (int folderCharIndex = searchFolderName.Length - 1, pathCompareCharIndex = pathCharIndex;
                         folderCharIndex >= 0;
                         folderCharIndex--, pathCompareCharIndex--)
                {
                    pathChar = path[pathCompareCharIndex];
                    folderChar = searchFolderName[folderCharIndex];
                    if (pathChar != folderChar)
                    {
                        found = false;
                        break;
                    }
                }

                if (found) return pathCharIndex + 2;
            }

            return -1;
        }
    }
}
