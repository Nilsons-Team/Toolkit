using System;
using System.Windows;
using System.Windows.Input;
using Toolkit_NET_Client.Models;

namespace Toolkit_NET_Client.Windows
{
    /// <summary>
    /// Логика взаимодействия для AppWindow.xaml
    /// </summary>
    public partial class AppWindow : Window
    {
        private User user;
        private int appId;

        public AppWindow(User user, int appId)
        {
            InitializeComponent();
            this.user = user;
            this.appId = appId;

            this.NavigationUsernameTextBlock.Text = user.Username;
        }

        private void NavigationLibraryTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // var libraryWindow = new LibraryWindow(this.user);
            // libraryWindow.Show();
            // this.Close();
        }

        private void NavigationUsernameTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // var userWindow = new UserWindow(this.user);
            // userWindow.Show();
            // this.Close();
        }

        private void NavigationStoreTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // var storeWindow = new StoreWindow(this.user);
            // storeWindow.Show();
            // this.Close();
        }
    }
}
