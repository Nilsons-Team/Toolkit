using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using Toolkit_NET_Client.Models;
using Toolkit_NET_Client.Windows;

namespace Toolkit_NET_Client.Pages
{
    public partial class UserLibraryPage : Page
    {
        private MainWindow ownerWindow;
        private User user;

        public UserLibraryPage(MainWindow ownerWindow, User user)
        {
            InitializeComponent();
            this.ownerWindow = ownerWindow;
            this.user = user;
        }

        private void AppsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void AppSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}