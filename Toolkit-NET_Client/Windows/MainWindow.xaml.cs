using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Toolkit_NET_Client.Models;
using Toolkit_NET_Client.Pages;

namespace Toolkit_NET_Client.Windows
{
    public enum PageType {
        NONE = 0,

        USER_LIBRARY = 1,
        USER_STORE = 2,
        USER_PROFILE = 3,

        COMPANY_APPS = 4,
        COMPANY_PROFILE = 5
    }

    public struct SelectedPageData {
        public PageType type;
        public TextBlock? headerTextBlock;
    }

    /// <summary>
    /// Логика взаимодействия для NewUserWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public User user;

        private SelectedPageData selectedPage;
        private SelectedPageData previousSelectedPage;

        private Style headerTextBlockStyle;
        private Style selectedHeaderTextBlockStyle;

        public MainWindow(User user, PageType pageToOpen = PageType.USER_STORE)
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
            if (type == selectedPage.type)
                return;
            
            previousSelectedPage.type = selectedPage.type;
            previousSelectedPage.headerTextBlock = selectedPage.headerTextBlock;

            switch (type) {
                case PageType.USER_LIBRARY:
                    selectedPage.headerTextBlock = NavigationLibraryTextBlock;
                    this.ContentFrame.Navigate(new UserLibraryPage(this, user));
                    break;
                case PageType.USER_STORE:
                    selectedPage.headerTextBlock = NavigationStoreTextBlock;
                    this.ContentFrame.Navigate(new UserStorePage(this, user));
                    break;
                case PageType.USER_PROFILE:
                    selectedPage.headerTextBlock = NavigationUsernameTextBlock;
                    this.ContentFrame.Navigate(new UserProfilePage(this, user));
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
