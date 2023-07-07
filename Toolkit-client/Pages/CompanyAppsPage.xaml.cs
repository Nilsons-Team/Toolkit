using System;
using System.Linq;
using System.Threading;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

using Toolkit_Client.Models;
using Toolkit_Client.Windows;

using static Toolkit_Client.Modules.InnerElementSearch;

namespace Toolkit_Client.Pages
{
    /// <summary>
    /// Interaction logic for CompanyAppsPage.xaml
    /// </summary>
    public partial class CompanyAppsPage : ToolkitPage
    {
        public enum CompanyAppSortElementType : int {
            NONE = 0,
        
            NAME = 1,
            APP_TYPE = 2,
            APP_RELEASE_STATE = 3,
            
            UPLOAD_DATETIME = 4,
            RELEASE_DATETIME = 5,

            DISCOUNT_PERCENT = 6,
            DISCOUNT_START_DATETIME = 7,
            DISCOUNT_EXPIRE_DATETIME = 8
        }

        public class CompanyAppSortElementComboBoxItem : ComboBoxItem
        {
            public CompanyAppSortElementType Element { get; set; }
            public ListSortDirection Direction { get; set; }

            public CompanyAppSortElementComboBoxItem(CompanyAppSortElementType element, ListSortDirection direction) 
            {
                this.Element = element;
                this.Direction = direction;
            }
        }

        private PartnerWindow ownerWindow;
        private Company company;

        private List<App> dbCompanyApps;
        private List<App> filteredApps;

        public CompanyAppsPage(PartnerWindow ownerWindow, Company company) : base(ownerWindow.GetLogger())
        {
            InitializeComponent();
            this.ownerWindow = ownerWindow;
            this.company = company;

            Thread updateThread = new Thread(() => {
                using (var db = new ToolkitContext()) {
                    dbCompanyApps = db.Apps
                        .Include(app => app.AppType)
                        .Include(app => app.DeveloperCompany)
                        .Include(app => app.PublisherCompany)
                        .Include(app => app.Categories)
                        .Include(app => app.Tags)
                        .Include(app => app.AppPurchaseMethods)
                        .Include(app => app.AppReleaseState)
                        .ToList();

                    var sortElementTypes = new List<CompanyAppSortElementType>
                    {
                        CompanyAppSortElementType.NAME,
                        CompanyAppSortElementType.APP_TYPE,
                        CompanyAppSortElementType.APP_RELEASE_STATE,
                        CompanyAppSortElementType.UPLOAD_DATETIME,
                        CompanyAppSortElementType.RELEASE_DATETIME,
                        CompanyAppSortElementType.DISCOUNT_PERCENT,
                        CompanyAppSortElementType.DISCOUNT_START_DATETIME,
                        CompanyAppSortElementType.DISCOUNT_EXPIRE_DATETIME
                    };
                    UpdateSortElementTypes(sortElementTypes, SortComboBox);
                }
                Dispatcher.BeginInvoke(FilterSearchResults);
            });
            updateThread.Start();
        }

        private void UpdateGameList(List<App> apps)
        {
            AppsListView.ItemsSource = apps;
            SearchResultsTextBlock.Text = apps.Count.ToString();
        }

        private bool AppContainsName(App app) {
            string searchName = NameSearchTextBox.Text;
            if (searchName != "" && !app.Name.ToLower().Contains(searchName.ToLower())) 
                return false;

            return true;
        }

        private void FilterSearchResults()
        {
            filteredApps = dbCompanyApps.Where(AppContainsName).OrderBy(app => app.Name).ToList();
            UpdateGameList(filteredApps);
        }

        private void AddAppButton_Click(object sender, RoutedEventArgs e)
        {
            ownerWindow.OpenPage(PageType.COMPANY_ADD_APP);
        }

        public static string GetCompanyAppSortElementTypeName(CompanyAppSortElementType sortElementType) {
            switch (sortElementType) {
                case CompanyAppSortElementType.NAME:                     return "Название";
                case CompanyAppSortElementType.APP_TYPE:                 return "Тип";
                case CompanyAppSortElementType.APP_RELEASE_STATE:        return "Статус";

                case CompanyAppSortElementType.UPLOAD_DATETIME:          return "Дата загрузки";
                case CompanyAppSortElementType.RELEASE_DATETIME:         return "Дата выхода";

                case CompanyAppSortElementType.DISCOUNT_PERCENT:         return "Скидка";
                case CompanyAppSortElementType.DISCOUNT_START_DATETIME:  return "Старт скидки";
                case CompanyAppSortElementType.DISCOUNT_EXPIRE_DATETIME: return "Конец скидки";

                case CompanyAppSortElementType.NONE:
                default:                                                 return null;
            }
        }

        private void UpdateSortElementTypes(List<CompanyAppSortElementType> sortElementTypes, ComboBox comboBox) 
        {
            Application.Current.Dispatcher.BeginInvoke(() => {
                CompanyAppSortElementType element;
                for (int i = 0; i < sortElementTypes.Count; i++) {
                    element = sortElementTypes[i];

                    string elementAscending  = string.Format("{0} - {1}", GetCompanyAppSortElementTypeName(element), GetSortDirectionName(ListSortDirection.Ascending));
                    string elementDescending = string.Format("{0} - {1}", GetCompanyAppSortElementTypeName(element), GetSortDirectionName(ListSortDirection.Descending));

                    var elementAscendingItem = new CompanyAppSortElementComboBoxItem(element, ListSortDirection.Ascending);
                    elementAscendingItem.Content = elementAscending;
                    var elementDescendingItem = new CompanyAppSortElementComboBoxItem(element, ListSortDirection.Descending);
                    elementDescendingItem.Content = elementDescending;

                    comboBox.Items.Add(elementAscendingItem);
                    comboBox.Items.Add(elementDescendingItem);
                }
            });
        }

        private void SortSearchResults(CompanyAppSortElementType element, ListSortDirection direction)
        {
            if (filteredApps == null)
                return;

            if (direction == ListSortDirection.Ascending) {
                switch (element) {
                    case CompanyAppSortElementType.NAME:
                        filteredApps = filteredApps.OrderBy(app => app.Name).ToList();
                        break;
                    case CompanyAppSortElementType.APP_TYPE:
                        filteredApps = filteredApps.OrderBy(app => app.AppType.Name).ToList();
                        break;
                    case CompanyAppSortElementType.APP_RELEASE_STATE:
                        filteredApps = filteredApps.OrderBy(app => app.AppReleaseState.Name).ToList();
                        break;
                    case CompanyAppSortElementType.UPLOAD_DATETIME:
                        filteredApps = filteredApps.OrderBy(app => DateTime.Parse(app.UploadDatetime)).ToList();
                        break;
                    case CompanyAppSortElementType.RELEASE_DATETIME:
                        filteredApps = filteredApps.OrderBy(app => {
                            DateTime release;
                            bool parsed = DateTime.TryParse(app.ReleaseDatetime, out release);
                            if (!parsed)
                                return DateTime.MinValue;

                            return release;
                        }).ToList();
                        break;
                    case CompanyAppSortElementType.DISCOUNT_PERCENT:
                        filteredApps = filteredApps.OrderBy(app => app.DiscountPercent).ToList();
                        break;
                    case CompanyAppSortElementType.DISCOUNT_START_DATETIME:
                        filteredApps = filteredApps.OrderBy(app => {
                            DateTime start;
                            bool parsed = DateTime.TryParse(app.DiscountStartDatetime, out start);
                            if (!parsed)
                                return DateTime.MinValue;

                            return start;
                        }).ToList();
                        break;
                    case CompanyAppSortElementType.DISCOUNT_EXPIRE_DATETIME:
                        filteredApps = filteredApps.OrderBy(app => {
                            DateTime expire;
                            bool parsed = DateTime.TryParse(app.DiscountExpireDatetime, out expire);
                            if (!parsed)
                                return DateTime.MinValue;

                            return expire;
                        }).ToList();
                        break;
                }
            } else if (direction == ListSortDirection.Descending) {
                switch (element) {
                    case CompanyAppSortElementType.NAME:
                        filteredApps = filteredApps.OrderByDescending(app => app.Name).ToList();
                        break;
                    case CompanyAppSortElementType.APP_TYPE:
                        filteredApps = filteredApps.OrderByDescending(app => app.AppType.Name).ToList();
                        break;
                    case CompanyAppSortElementType.APP_RELEASE_STATE:
                        filteredApps = filteredApps.OrderByDescending(app => app.AppReleaseState.Name).ToList();
                        break;
                    case CompanyAppSortElementType.UPLOAD_DATETIME:
                        filteredApps = filteredApps.OrderByDescending(app => DateTime.Parse(app.UploadDatetime)).ToList();
                        break;
                    case CompanyAppSortElementType.RELEASE_DATETIME:
                        filteredApps = filteredApps.OrderByDescending(app => {
                            DateTime release;
                            bool parsed = DateTime.TryParse(app.ReleaseDatetime, out release);
                            if (!parsed)
                                return DateTime.MinValue;

                            return release;
                        }).ToList();
                        break;
                    case CompanyAppSortElementType.DISCOUNT_PERCENT:
                        filteredApps = filteredApps.OrderByDescending(app => app.DiscountPercent).ToList();
                        break;
                    case CompanyAppSortElementType.DISCOUNT_START_DATETIME:
                        filteredApps = filteredApps.OrderByDescending(app => {
                            DateTime start;
                            bool parsed = DateTime.TryParse(app.DiscountStartDatetime, out start);
                            if (!parsed)
                                return DateTime.MinValue;

                            return start;
                        }).ToList();
                        break;
                    case CompanyAppSortElementType.DISCOUNT_EXPIRE_DATETIME:
                        filteredApps = filteredApps.OrderByDescending(app => {
                            DateTime expire;
                            bool parsed = DateTime.TryParse(app.DiscountExpireDatetime, out expire);
                            if (!parsed)
                                return DateTime.MinValue;

                            return expire;
                        }).ToList();
                        break;
                }
            }

            UpdateGameList(filteredApps);
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var elementItem = (CompanyAppSortElementComboBoxItem) SortComboBox.SelectedItem;
            SortSearchResults(elementItem.Element, elementItem.Direction);
        }

        private void NameSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterSearchResults();
        }

        private void AppButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (AppButton) sender;
            MainWindow mainWindow = ownerWindow.userPage.ownerWindow;
            mainWindow.appId = button.AppId;
            mainWindow.OpenPage(PageType.APP_STORE_PAGE);
        }
    }
}
