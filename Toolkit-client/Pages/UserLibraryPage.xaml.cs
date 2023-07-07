using System;
using System.Linq;
using System.Windows.Controls;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

using Toolkit_Client.Models;
using Toolkit_Client.Windows;

namespace Toolkit_Client.Pages
{
    public partial class UserLibraryPage : ToolkitPage
    {
        public class AppTextBlock : TextBlock
        {
            public App App { get; set; }

            public AppTextBlock(App app)
            {
                this.App = app;
            }
        }

        private MainWindow ownerWindow;
        private User user;

        private List<App> dbApps;
        private List<App> dbUserPurchasedApps;

        public UserLibraryPage(MainWindow ownerWindow, User user) : base(ownerWindow.GetLogger())
        {
            InitializeComponent();
            this.ownerWindow = ownerWindow;
            this.user = user;

            using (var db = new ToolkitContext()) {
                var dbUsers = db.Users.Include(user => user.UserPurchasedApps).ToList();
                foreach (var dbUser in dbUsers) {
                    if (dbUser.Id == user.Id) {
                        this.user = dbUser;
                        break;
                    }
                }

                dbApps = db.Apps.ToList();
                dbUserPurchasedApps = new List<App>(this.user.UserPurchasedApps.Count);
                foreach (var app in dbApps) {
                    foreach (var purchasedApp in this.user.UserPurchasedApps) {
                        if (app.Id == purchasedApp.AppId)
                            dbUserPurchasedApps.Add(app);
                    }
                }
                UpdateGameList(dbUserPurchasedApps);
            }
        }

        private bool IsPurchasedApp(App app) {
            foreach (var purchasedApp in user.UserPurchasedApps) {
                return app.Id == purchasedApp.AppId;
            }
            return false;
        }

        private void UpdateGameList(List<App> apps) 
        {
            AppsListView.Items.Clear();

            foreach (var app in apps) {
                var textBlock = new AppTextBlock(app)
                {
                    Text = app.Name,
                    Padding = AppTextBlockTemplate.Padding,
                    Style = AppTextBlockTemplate.Style
                };
                AppsListView.Items.Add(textBlock);
            }
        }

        private void NameSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filteredApps = dbUserPurchasedApps.Where(AppHasFilters).OrderBy(app => app.Name).ToList();
            UpdateGameList(filteredApps);
        }

        private bool AppHasFilters(App app)
        {
            string searchName = NameSearchTextBox.Text;
            if (searchName != "" && !app.Name.ToLower().Contains(searchName.ToLower())) 
                return false;

            return true;
        }

        private void AppsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AppsListView.SelectedItem == null)
                return;

            var textBlock = (AppTextBlock) AppsListView.SelectedItem;
            AppTitleTextBlock.Text = textBlock.App.Name;
        }
    }
}