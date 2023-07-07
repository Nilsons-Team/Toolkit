using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Microsoft.EntityFrameworkCore;

using Toolkit_Client.Windows;
using Toolkit_Client.Models;

using static Toolkit_Client.Modules.ClientActionStatus;
using static Toolkit_Client.Modules.Database;

namespace Toolkit_Client.Pages
{
    /// <summary>
    /// Interaction logic for AppStorePage.xaml
    /// </summary>
    public partial class AppStorePage : ToolkitPage
    {
        public struct PurchaseMethodData
        {
            public TextBlock NameTextBlock;
            public TextBlock PriceTextBlock;
            public AppPurchaseMethodButton Button;
            public TextBlock StatusTextBlock;
        }

        public class AppPurchaseMethodButton : Button
        {
            public AppPurchaseMethod PurchaseMethod { get; set; }
            public TextBlock NameTextBlock { get; set; }
            public TextBlock PriceTextBlock { get; set; }
            public TextBlock StatusTextBlock { get; set; }

            public AppPurchaseMethodButton(AppPurchaseMethod purchaseMethod) {
                this.PurchaseMethod = purchaseMethod;
            }
        }

        private MainWindow ownerWindow;
        private App app;
        private User user;
        private Models.AppStorePage appStorePage;

        public AppStorePage(MainWindow ownerWindow, User user, long appId) : base(ownerWindow.GetLogger())
        {
            InitializeComponent();
            this.ownerWindow = ownerWindow;
            this.user = user;
            using (var db = new ToolkitContext()) {
                var dbApps = db.Apps
                    .Include(app => app.Categories)
                    .Include(app => app.Tags)
                    .Include(app => app.DeveloperCompany)
                    .Include(app => app.PublisherCompany)
                    .Include(app => app.AppPurchaseMethods)
                    .ToList();

                foreach (var app in dbApps) {
                    if (app.Id == appId) {
                        this.app = app;
                        break;
                    }
                }
                ownerWindow.NavigationAppTextBlock.Text = app.Name;

                foreach (var dbUser in db.Users.Include(user => user.UserPurchasedApps).ToList()) {
                    if (dbUser.Id == user.Id) {
                        this.user = dbUser;
                        break;
                    }
                }

                var appStorePage = FindAppStorePageByAppId(appId, db);
                if (appStorePage != null)
                    this.appStorePage = appStorePage;
            }

            UpdateAppInfo();
        }

        public PurchaseMethodData GetNewPurchaseMethodData(int index, AppPurchaseMethod purchaseMethod) {
            PurchaseMethodData data;

            var nameTextBlock = new TextBlock() 
            {
                Text                = purchaseMethod.Name,
                FontSize            = PurchaseMethodNameTemplate.FontSize,
                HorizontalAlignment = PurchaseMethodNameTemplate.HorizontalAlignment,
                Margin              = PurchaseMethodNameTemplate.Margin,
                Style               = PurchaseMethodNameTemplate.Style
            };
            Grid.SetRow(nameTextBlock, index);
            Grid.SetColumn(nameTextBlock, 0);

            var priceTextBlock = new TextBlock() 
            {
                Text                = string.Format("{0} руб.", purchaseMethod.Price),
                VerticalAlignment   = PurchaseMethodPriceTemplate.VerticalAlignment,
                HorizontalAlignment = PurchaseMethodPriceTemplate.HorizontalAlignment,
                Background          = PurchaseMethodPriceTemplate.Background,
                Padding             = PurchaseMethodPriceTemplate.Padding,
                Style               = PurchaseMethodPriceTemplate.Style
            };
            Grid.SetRow(priceTextBlock, index);
            Grid.SetColumn(priceTextBlock, 1);

            var button = new AppPurchaseMethodButton(purchaseMethod)
            {
                Content           = PurchaseMethodButtonTemplate.Content,
                VerticalAlignment = PurchaseMethodButtonTemplate.VerticalAlignment,
                Style             = PurchaseMethodButtonTemplate.Style
            };
            button.Click += PurchaseMethodButton_Click;
            Grid.SetRow(button, index);
            Grid.SetColumn(button, 2);

            var statusTextBlock = new TextBlock() 
            {
                VerticalAlignment   = PurchaseMethodStatusTemplate.VerticalAlignment,
                HorizontalAlignment = PurchaseMethodStatusTemplate.HorizontalAlignment,
                Visibility          = PurchaseMethodStatusTemplate.Visibility,
                Style               = PurchaseMethodStatusTemplate.Style
            };
            Grid.SetRow(statusTextBlock, index + 1);
            Grid.SetColumn(statusTextBlock, 1);
            Grid.SetColumnSpan(statusTextBlock, 2);

            button.NameTextBlock   = nameTextBlock;
            button.PriceTextBlock  = priceTextBlock;
            button.StatusTextBlock = statusTextBlock;

            data.NameTextBlock     = nameTextBlock;
            data.PriceTextBlock    = priceTextBlock;
            data.Button            = button;
            data.StatusTextBlock   = statusTextBlock;

            return data;
        }

        private void UpdateAppPurchaseMethods() {
            PurchaseMethodsGrid.Children.Clear();

            int index = 0;
            var methods = app.AppPurchaseMethods.ToList();
            for (int methodIdx = 0; methodIdx < methods.Count; methodIdx++) {
                AppPurchaseMethod method = methods[methodIdx];
                PurchaseMethodData data = GetNewPurchaseMethodData(index, method);

                PurchaseMethodsGrid.RowDefinitions.Add(new RowDefinition());
                PurchaseMethodsGrid.RowDefinitions.Add(new RowDefinition());

                PurchaseMethodsGrid.Children.Add(data.NameTextBlock);
                PurchaseMethodsGrid.Children.Add(data.PriceTextBlock);
                PurchaseMethodsGrid.Children.Add(data.Button);
                PurchaseMethodsGrid.Children.Add(data.StatusTextBlock);
                
                foreach (var purchasedApp in user.UserPurchasedApps) {
                    if (purchasedApp.AppId == app.Id && purchasedApp.AppPurchaseMethodId == method.Id) {
                        SetStatusSuccess(null, data.Button.StatusTextBlock, "В библиотеке.");
                        data.Button.IsEnabled = false;
                        break;
                    }
                }

                index += 2;
            }
        }

        private void UpdateAppInfo() {
            AppNameTextBlock.Text           = app.Name;
            DeveloperTextBlock.Text         = app.DeveloperCompany.OperatingName;
            PublisherTextBlock.Text         = app.PublisherCompany.OperatingName;
            ShortDescriptionTextBlock.Text  = appStorePage.ShortDescription;
            DescriptionTextBlock.Text       = appStorePage.LongDescription;

            var builder = new StringBuilder();
            var categories = app.Categories.ToList();
            if (categories.Count < 1) {
                CategoriesTextBlock.Text = "-";
            } else {
                builder.Append(categories[0].Name);
                builder.Append(", ");
                for (int i = 1; i < categories.Count; i++) {
                    builder.Append(categories[i].Name);
                    builder.Append(", ");
                }
                CategoriesTextBlock.Text = builder.ToString();
            }
            builder.Clear();

            var tags = app.Tags.ToList();
            if (tags.Count < 1) {
                TagsTextBlock.Text = "-";
            } else {
                builder.Append(tags[0].Name);
                builder.Append(", ");
                for (int i = 1; i < tags.Count; i++) {
                    builder.Append(tags[i].Name);
                    builder.Append(", ");
                }
                TagsTextBlock.Text = builder.ToString();
            }

            ReleaseDateTextBlock.Text = app.ReleaseDatetimeFormatted;
            UpdateAppPurchaseMethods();
        }

        private void PurchaseMethodButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (AppPurchaseMethodButton) sender;
            bool hasDiscount = app.HasActiveDiscount();
            long discountPercent = (hasDiscount) ? (long)app.DiscountPercent : 0;
            var finalPrice = button.PurchaseMethod.Price;
            if (hasDiscount)
                finalPrice = finalPrice * (1 / discountPercent);

            if (user.Wallet < finalPrice) {
                SetStatusError(null, button.StatusTextBlock, "Недостаточно средств.");
            } else {
                using (var db = new ToolkitContext()) {
                    foreach (var dbUser in db.Users.Include(user => user.UserPurchasedApps)) {
                        if (user.Id == dbUser.Id) {
                            user = dbUser;
                            break;
                        }
                    }

                    var purchasedApp = new UserPurchasedApp()
                    {
                        UserId = user.Id,
                        AppId = button.PurchaseMethod.AppId,
                        AppPurchaseMethodId = button.PurchaseMethod.Id
                    };
                    user.UserPurchasedApps.Add(purchasedApp);
                    user.Wallet -= finalPrice;

                    db.SaveChanges();
                }
                SetStatusSuccess(null, button.StatusTextBlock, "Успешно приобретено.");
                button.IsEnabled = false;
            }
        }
    }
}
