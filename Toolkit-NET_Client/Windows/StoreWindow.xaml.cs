using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Toolkit_NET_Client.Models;

namespace Toolkit_NET_Client.Windows
{
    /// <summary>
    /// Interaction logic for StoreWindow.xaml
    /// </summary>
    public partial class StoreWindow : Window
    {
        private List<App> appsList;
        private User user;

        public StoreWindow(User user)
        {
            InitializeComponent();
            this.user = user;

            this.UsernameTextBlock.Text = user.Username;

            using (var db = new ToolkitContext())
            {
                appsList = db.Apps.ToList();
            }

            UpdateGameList(this.appsList);
        }

        private void UpdateGameList(IEnumerable<App> apps)
        {
            this.AppsListView.Items.Clear();

            foreach (var app in apps)
            {
                var appItem = new TextBlock();
                appItem.Text = app.Name;
                appItem.Padding = new Thickness(15, 2, 2, 2);
                appItem.Style = (Style)FindResource("DefaultTextBlock");
                appItem.Name = string.Format("App_{0}", app.Id);
                this.AppsListView.Items.Add(appItem);
            }
        }

        private void HasDiscountCheckbox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void AppPurchaseTypeCheckbox1_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void LibraryTextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var libraryWindow = new LibraryWindow(this.user);
            libraryWindow.Show();
            this.Close();
        }

        private void UsernameTextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var userWindow = new UserWindow(this.user);
            userWindow.Show();
            this.Close();
        }

        private void AppsListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}