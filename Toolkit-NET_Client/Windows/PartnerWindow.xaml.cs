using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Toolkit_NET_Client.Models;
using Toolkit_NET_Client.Pages;

namespace Toolkit_NET_Client.Windows
{
    /// <summary>
    /// Interaction logic for PartnerWindow.xaml
    /// </summary>
    public partial class PartnerWindow : Window
    {
        private UserProfilePage userPage;
        private User user;
        public Company company;
     
        private SelectedPageData selectedPage;
        private SelectedPageData previousSelectedPage;

        private Style headerTextBlockStyle;
        private Style selectedHeaderTextBlockStyle;

        public PartnerWindow(UserProfilePage userPage, User user, PageType pageToOpen = PageType.COMPANY_APPS)
        {
            InitializeComponent();
            this.userPage = userPage;
            this.user = user;

            Company? ownedCompany = ToolkitApp.FindCompanyByOwnerUserId(user.Id, null);
            bool userHasCompany = (ownedCompany != null);
            this.company = ownedCompany;
            if (userHasCompany) {
                NavigationCompanyTextBlock.Text = ownedCompany.OperatingName;
            } else {
                NavigationAppsTextBlock.Visibility = Visibility.Collapsed;
                pageToOpen = PageType.COMPANY_PROFILE;
            }

            headerTextBlockStyle         = (Style) TryFindResource("HeaderTextBlock");
            selectedHeaderTextBlockStyle = (Style) TryFindResource("SelectedHeaderTextBlock");

            previousSelectedPage.type = PageType.NONE;
            previousSelectedPage.headerTextBlock = null;

            OpenPage(pageToOpen);
        }

        public TextBlock? GetHeaderTextBlockFromPageType(PageType type) {
            switch (type) {
                case PageType.COMPANY_APPS:     return NavigationAppsTextBlock;
                case PageType.COMPANY_PROFILE:  return NavigationCompanyTextBlock;

                case PageType.NONE:
                default:                        return null;
            }
        }

        public void OpenPage(PageType type) {
            if (type == selectedPage.type)
                return;
            
            previousSelectedPage.type = selectedPage.type;
            previousSelectedPage.headerTextBlock = selectedPage.headerTextBlock;

            switch (type) {
                case PageType.COMPANY_APPS:
                    selectedPage.headerTextBlock = NavigationAppsTextBlock;
                    // this.ContentFrame.Navigate(new LibraryPage(this, user));
                    break;
                case PageType.COMPANY_PROFILE:
                    selectedPage.headerTextBlock = NavigationCompanyTextBlock;
                    this.ContentFrame.Navigate(new CompanyProfilePage(this, company));
                    break;
                case PageType.NONE:
                default: 
                    return;
            }

            this.ContentFrame.NavigationService.RemoveBackEntry();
            GC.Collect();

            selectedPage.type = type;
            selectedPage.headerTextBlock.Style = selectedHeaderTextBlockStyle;
            if (previousSelectedPage.headerTextBlock != null)
                previousSelectedPage.headerTextBlock.Style = headerTextBlockStyle;
        }

        private void NavigationAppsTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenPage(PageType.COMPANY_APPS);
        }

        private void NavigationCompanyTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenPage(PageType.COMPANY_PROFILE);
        }

        private void PartnerWindow_Closed(object sender, EventArgs e)
        {
            this.userPage.partnerWindow = null;
            GC.Collect();
        }
    }
}
