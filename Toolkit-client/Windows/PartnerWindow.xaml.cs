using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

using Toolkit_Client.Models;
using Toolkit_Client.Pages;

using static Toolkit_Client.Modules.Database;

using Toolkit_Shared;

namespace Toolkit_Client.Windows
{
    /// <summary>
    /// Interaction logic for PartnerWindow.xaml
    /// </summary>
    public partial class PartnerWindow : ToolkitWindow
    {
        private User user;
        public UserProfilePage userPage;
        public Company company;
     
        private SelectedPageData selectedPage;
        private SelectedPageData previousSelectedPage;

        private Style headerTextBlockStyle;
        private Style selectedHeaderTextBlockStyle;

        public PartnerWindow(Logger logger, UserProfilePage userPage, User user, PageType pageToOpen = PageType.COMPANY_APPS) : base(logger)
        {
            InitializeComponent();
            this.userPage = userPage;
            this.user = user;

            Company? ownedCompany = FindCompanyByOwnerUserId(user.Id, null);
            bool userHasCompany = (ownedCompany != null);
            this.company = ownedCompany;
            if (userHasCompany) {
                NavigationCompanyTextBlock.Text = ownedCompany.OperatingName;
            } else {
                NavigationAppsTextBlock.Visibility = Visibility.Collapsed;
                pageToOpen = PageType.COMPANY_UNREGISTERED_PROFILE;
            }

            NavigationCompanyRegistrationTextBlock.Visibility = Visibility.Collapsed;

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

            NavigationCompanyAddAppTextBlock.Visibility = Visibility.Collapsed;

            switch (type) {
                case PageType.COMPANY_APPS:
                    selectedPage.headerTextBlock = NavigationAppsTextBlock;
                    this.ContentFrame.Navigate(new CompanyAppsPage(this, company));
                    break;
                case PageType.COMPANY_PROFILE:
                    selectedPage.headerTextBlock = NavigationCompanyTextBlock;
                    if (company == null)
                        return;
                    
                    this.ContentFrame.Navigate(new CompanyProfilePage(this, company));
                    break;
                case PageType.COMPANY_UNREGISTERED_PROFILE:
                    selectedPage.headerTextBlock = NavigationCompanyTextBlock;
                    this.ContentFrame.Navigate(new CompanyUnregisteredProfilePage(this));
                    break;
                case PageType.COMPANY_REGISTRATION_PAGE:
                    NavigationCompanyTextBlock.Visibility = Visibility.Collapsed;
                    NavigationCompanyRegistrationTextBlock.Visibility = Visibility.Visible;
                    selectedPage.headerTextBlock = NavigationCompanyRegistrationTextBlock;
                    this.ContentFrame.Navigate(new CompanyRegistrationPage(this));
                    break;
                case PageType.COMPANY_ADD_APP:
                    NavigationCompanyAddAppTextBlock.Visibility = Visibility.Visible;
                    selectedPage.headerTextBlock = NavigationCompanyAddAppTextBlock;
                    this.ContentFrame.Navigate(new CompanyAddAppPage(this, company));
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

        public void UpdateCompany(Company company) {
            this.company = company;
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
