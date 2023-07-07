using System;
using System.Windows;
using System.Windows.Controls;

using Toolkit_Client.Windows;

namespace Toolkit_Client.Pages
{
    /// <summary>
    /// Interaction logic for CompanyUnregisteredProfilePage.xaml
    /// </summary>
    public partial class CompanyUnregisteredProfilePage : ToolkitPage
    {
        private PartnerWindow ownerWindow;

        public CompanyUnregisteredProfilePage(PartnerWindow ownerWindow) : base(ownerWindow.GetLogger())
        {
            InitializeComponent();
            this.ownerWindow = ownerWindow;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ownerWindow.OpenPage(PageType.COMPANY_REGISTRATION_PAGE);
        }
    }
}
