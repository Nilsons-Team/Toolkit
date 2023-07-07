using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Toolkit_Client.Models;
using Toolkit_Client.Pages;

using Toolkit_Shared;

namespace Toolkit_Client.Windows
{
    public enum PageType {
        NONE = 0,

        USER_LIBRARY = 1,
        USER_STORE = 2,
        USER_PROFILE = 3,

        APP_STORE_PAGE = 4,

        COMPANY_APPS = 5,
        COMPANY_PROFILE = 6,
        COMPANY_UNREGISTERED_PROFILE = 7,
        COMPANY_REGISTRATION_PAGE = 8,

        COMPANY_ADD_APP = 9
    }

    public struct SelectedPageData {
        public PageType type;
        public TextBlock? headerTextBlock;
    }

    /// <summary>
    /// Логика взаимодействия для NewUserWindow.xaml
    /// </summary>
    public partial class MainWindow : ToolkitWindow
    {
        public User user;
        public long appId;

        private SelectedPageData selectedPage;
        private SelectedPageData previousSelectedPage;

        private Style headerTextBlockStyle;
        private Style selectedHeaderTextBlockStyle;

        public MainWindow(Logger logger, User user, PageType pageToOpen = PageType.USER_STORE) : base(logger)
        {
            InitializeComponent();
            this.user = user;

            NavigationUsernameTextBlock.Text = user.Username;

            headerTextBlockStyle         = (Style) TryFindResource("HeaderTextBlock");
            selectedHeaderTextBlockStyle = (Style) TryFindResource("SelectedHeaderTextBlock");

            previousSelectedPage.type = PageType.NONE;
            previousSelectedPage.headerTextBlock = null;

            OpenPage(pageToOpen);
        }

        public TextBlock? GetHeaderTextBlockFromPageType(PageType type) {
            switch (type) {
                case PageType.USER_LIBRARY:  return NavigationLibraryTextBlock;
                case PageType.USER_STORE:    return NavigationStoreTextBlock;
                case PageType.USER_PROFILE:  return NavigationUsernameTextBlock;

                case PageType.NONE:
                default:                     return null;
            }
        }

        public void OpenPage(PageType type) {
            if (type == selectedPage.type && type != PageType.APP_STORE_PAGE)
                return;
            
            previousSelectedPage.type = selectedPage.type;
            previousSelectedPage.headerTextBlock = selectedPage.headerTextBlock;

            NavigationAppTextBlock.Visibility = Visibility.Collapsed;

            switch (type) {
                case PageType.USER_LIBRARY:
                    selectedPage.headerTextBlock = NavigationLibraryTextBlock;
                    ContentFrame.Navigate(new UserLibraryPage(this, user));
                    break;
                case PageType.USER_STORE:
                    selectedPage.headerTextBlock = NavigationStoreTextBlock;
                    ContentFrame.Navigate(new UserStorePage(this, user));
                    break;
                case PageType.USER_PROFILE:
                    selectedPage.headerTextBlock = NavigationUsernameTextBlock;
                    ContentFrame.Navigate(new UserProfilePage(this, user));
                    break;
                case PageType.APP_STORE_PAGE:
                    selectedPage.headerTextBlock = NavigationAppTextBlock;
                    NavigationAppTextBlock.Visibility = Visibility.Visible;
                    ContentFrame.Navigate(new Pages.AppStorePage(this, user, appId));
                    break;
                case PageType.NONE:
                default: 
                    return;
            }

            logger.Info($"Switched to page '{type}' from '{previousSelectedPage.type}'.");

            ContentFrame.NavigationService.RemoveBackEntry();
            GC.Collect();

            selectedPage.type = type;
            selectedPage.headerTextBlock.Style = selectedHeaderTextBlockStyle;
            if (previousSelectedPage.headerTextBlock != null)
                previousSelectedPage.headerTextBlock.Style = headerTextBlockStyle;
        }
        
        private void NavigationStoreTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenPage(PageType.USER_STORE);
        }

        private void NavigationLibraryTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenPage(PageType.USER_LIBRARY);
        }

        private void NavigationUsernameTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenPage(PageType.USER_PROFILE);
        }
    }
}
