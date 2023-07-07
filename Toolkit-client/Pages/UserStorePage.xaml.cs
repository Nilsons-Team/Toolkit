using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

using Toolkit_Client.Models;
using Toolkit_Client.Windows;

using static Toolkit_Client.Modules.Database;
using static Toolkit_Client.Modules.ClientActionStatus;
using static Toolkit_Client.Modules.InnerElementSearch;

using Toolkit_Shared;

namespace Toolkit_Client.Pages
{
    public enum AppSortElementType : int {
        NONE = 0,
        
        NAME = 1,
        PRICE = 2,
        DEVELOPER = 3,
        PUBLISHER = 4,
        RELEASE_DATETIME = 5
    }

    public class AppSortElementComboBoxItem : ComboBoxItem
    {
        public AppSortElementType Element { get; set; }
        public ListSortDirection Direction { get; set; }

        public AppSortElementComboBoxItem(AppSortElementType element, ListSortDirection direction) 
        {
            this.Element = element;
            this.Direction = direction;
        }
    }
    
    public class SearchStackPanel : StackPanel
    {
        public InnerSearchCheckBox CheckBox { get; set; }
        public SearchTextBlock TextBlock { get; set; }

        public SearchStackPanel(InnerSearchCheckBox checkBox, SearchTextBlock textBlock) : base()
        {
            this.CheckBox = checkBox;
            this.CheckBox.ParentSearchStackPanel = this;
            this.TextBlock = textBlock;
            this.TextBlock.ParentSearchStackPanel = this;
            this.Children.Add(checkBox);
            this.Children.Add(textBlock);
        }
    }

    public class InnerSearchCheckBox : CheckBox
    {
        public SearchStackPanel ParentSearchStackPanel { get; set; }
        public ISearchElement SearchElement { get; set; }
        public SearchElementType SearchElementType { get; set; }

        public InnerSearchCheckBox(ISearchElement element, SearchElementType type) : base()
        {
            this.SearchElement = element;
            this.SearchElementType = type;
        }
    }

    public class FilterCheckBox : CheckBox
    {
        public SearchFilterType Filter { get; set; }

        public FilterCheckBox() : base() {
            this.Filter = SearchFilterType.NONE;
        }

        public FilterCheckBox(SearchFilterType filter) : base() 
        {
            this.Filter = filter;
        }
    }

    public class SearchTextBlock : TextBlock
    {
        public SearchStackPanel ParentSearchStackPanel { get; set; }
        public InnerSearchCheckBox LabelingSearchCheckBox
        {
            get { return ParentSearchStackPanel.CheckBox; }
        }
        public ISearchElement? SearchElement
        {
            get { return this.LabelingSearchCheckBox.SearchElement; }
        }
    }

    public class AppButton : Button
    {
        public static readonly DependencyProperty AppIdProperty =
            DependencyProperty.Register("AppId", typeof(long), typeof(AppButton), new PropertyMetadata((long)0, AppIdPropertyChanged));

        public long AppId
        { 
            get { return (long)GetValue(AppIdProperty); }
            set { SetValue(AppIdProperty, value); }
        }

        private void AppIdPropertyChanged(long appId) 
        {

        }

        private static void AppIdPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) 
        {
            ((AppButton)d).AppIdPropertyChanged((long)args.NewValue);
        }
    }

    /// <summary>
    /// Interaction logic for StoreWindow.xaml
    /// </summary>
    public partial class UserStorePage : ToolkitPage
    {
        private MainWindow ownerWindow;
        private User user;

        private List<App> dbApps;
        private List<Category> dbCategories;
        private List<Tag> dbTags;

        private List<App> filteredApps;
        
        private int lowestSearchPrice = 0;
        private int highestSearchPrice = int.MaxValue;

        private List<Category> chosenSearchCategories;
        private List<Tag> chosenSearchTags;

        private int visibleSearchCategories = 0;
        private int visibleSearchTags = 0;

        private bool showAllCategorySearchResults = true;
        private bool showAllTagSearchResults = true;

        private SearchFilterType searchFilters = SearchFilterType.NONE;

        private AppType software = null;
        private AppType demo = null;
        private AppType dlc = null;

        public UserStorePage(MainWindow ownerWindow, User user) : base(ownerWindow.GetLogger())
        {
            InitializeComponent();
            this.ownerWindow = ownerWindow;
            this.user = user;

            Thread updateThread = new Thread(() => {
                using (var db = new ToolkitContext()) {
                    dbApps = db.Apps
                        .Include(app => app.DeveloperCompany)
                        .Include(app => app.PublisherCompany)
                        .Include(app => app.Categories)
                        .Include(app => app.Tags)
                        .Include(app => app.AppPurchaseMethods)
                        .Include(app => app.AppReleaseState)
                        .ToList();
                    dbCategories = db.Categories.ToList();
                    dbTags = db.Tags.ToList();
                    
                    foreach (var type in db.AppTypes) {
                        if (software == null && type.Name == "Программное обеспечение")
                            software = type;

                        if (demo == null     && type.Name == "Демо")
                            demo = type;

                        if (dlc == null      && type.Name == "Дополнение")
                            dlc = type;
                    }
                    
                    chosenSearchCategories = new List<Category>(dbCategories.Count);
                    chosenSearchTags = new List<Tag>(dbTags.Count);

                    var sortElementTypes = new List<AppSortElementType>
                    {
                        AppSortElementType.NAME,
                        AppSortElementType.PRICE,
                        AppSortElementType.DEVELOPER,
                        AppSortElementType.PUBLISHER,
                        AppSortElementType.RELEASE_DATETIME
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

        public static string GetAppSortElementTypeName(AppSortElementType sortElementType) {
            switch (sortElementType) {
                case AppSortElementType.NAME:              return "Название";
                case AppSortElementType.PRICE:             return "Цена";
                case AppSortElementType.DEVELOPER:         return "Разработчик";
                case AppSortElementType.PUBLISHER:         return "Издатель";
                case AppSortElementType.RELEASE_DATETIME:  return "Дата выхода";
            
                case AppSortElementType.NONE:
                default:                                   return null;
            }
        }

        private void UpdateSortElementTypes(List<AppSortElementType> sortElementTypes, ComboBox comboBox) 
        {
            Application.Current.Dispatcher.BeginInvoke(() => {
                AppSortElementType element;
                for (int i = 0; i < sortElementTypes.Count; i++) {
                    element = sortElementTypes[i];

                    string elementAscending  = string.Format("{0} - {1}", GetAppSortElementTypeName(element), GetSortDirectionName(ListSortDirection.Ascending));
                    string elementDescending = string.Format("{0} - {1}", GetAppSortElementTypeName(element), GetSortDirectionName(ListSortDirection.Descending));

                    var elementAscendingItem = new AppSortElementComboBoxItem(element, ListSortDirection.Ascending);
                    elementAscendingItem.Content = elementAscending;
                    var elementDescendingItem = new AppSortElementComboBoxItem(element, ListSortDirection.Descending);
                    elementDescendingItem.Content = elementDescending;

                    comboBox.Items.Add(elementAscendingItem);
                    comboBox.Items.Add(elementDescendingItem);
                }
            });
        }

        private void PriceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox priceTextBox = (TextBox)sender;
            int price;
            if (priceTextBox.Text != "") {
                bool parsed = int.TryParse(priceTextBox.Text, out price);
                if (parsed) {
                    ClearStatus(priceTextBox, PriceStatusTextBlock);
                } else {
                    SetStatusError(priceTextBox, PriceStatusTextBlock, "Введите целочисленное значение.");
                    return;
                }
            } else {
                if      (priceTextBox.Name.StartsWith("L"))  this.lowestSearchPrice = 0;
                else if (priceTextBox.Name.StartsWith("U"))  this.highestSearchPrice = int.MaxValue;
                else                                         Debug.WriteLine("Warning: Unknown sender in PriceTextBox_TextChanged.");

                ClearStatus(priceTextBox, PriceStatusTextBlock);
                return;
            }

            if      (priceTextBox.Name.StartsWith("L"))  this.lowestSearchPrice = price;
            else if (priceTextBox.Name.StartsWith("U"))  this.highestSearchPrice = price;
            else                                         Debug.WriteLine("Warning: Unknown sender in PriceTextBox_TextChanged.");

            FilterSearchResults();
        }

        private bool HasFilter(SearchFilterType filters, SearchFilterType filter) {
            return Convert.ToBoolean(filters & filter);
        }

        private bool AppHasFilters(App app) {
            string searchName = NameSearchTextBox.Text;
            if (searchName != "" && !app.Name.ToLower().Contains(searchName.ToLower())) 
                return false;
            
            if (app.AppReleaseState.Name != "Выпущено")
                return false;

            bool priceInRange = false;
            bool hasPerpetualPurchase = false;
            bool hasSubscriptionPurchase = false;
            bool hasDiscount = app.HasActiveDiscount();
            long discountPercent = (hasDiscount) ? (long)app.DiscountPercent : 0;

            foreach (var purchaseMethod in app.AppPurchaseMethods) {
                var finalPrice = purchaseMethod.Price;
                if (hasDiscount)
                    finalPrice = finalPrice * (1 / discountPercent);

                if (app._LowestPrice > finalPrice) app._LowestPrice = finalPrice;
                if (app._HighestPrice < finalPrice) app._HighestPrice = finalPrice;

                bool isPerpetual = (purchaseMethod.IsPerpetual == 1);
                if (!hasPerpetualPurchase && isPerpetual)
                    hasPerpetualPurchase = true;

                if (!hasSubscriptionPurchase && !isPerpetual)
                    hasSubscriptionPurchase = true;

                if (finalPrice >= lowestSearchPrice && finalPrice <= highestSearchPrice) {
                    priceInRange = true;
                }
            }
                
            if (!priceInRange)
                return false;

            {
                bool shouldHaveDiscount = HasFilter(searchFilters, SearchFilterType.HAS_DISCOUNT);
                if (shouldHaveDiscount && !hasDiscount)
                    return false;

                bool shouldHavePerpetualPurchase = HasFilter(searchFilters, SearchFilterType.PERPETUAL_PURCHASE);
                if (shouldHavePerpetualPurchase && !hasPerpetualPurchase)
                    return false;

                bool shouldHaveSubscriptionPurchase = HasFilter(searchFilters, SearchFilterType.SUBSCRIPTION_PURCHASE);
                if (shouldHaveSubscriptionPurchase && !hasSubscriptionPurchase)
                    return false;

                bool shouldBeSoftware = HasFilter(searchFilters, SearchFilterType.IS_SOFTWARE);
                bool isSoftware = app.AppType.Id == software.Id;
                if (shouldBeSoftware && !isSoftware)
                    return false;

                bool shouldBeDemo = HasFilter(searchFilters, SearchFilterType.IS_DEMO);
                bool isDemo = app.AppType.Id == demo.Id;
                if (shouldBeDemo && !isDemo)
                    return false;

                bool shouldBeDLC = HasFilter(searchFilters, SearchFilterType.IS_DLC);
                bool isDLC = app.AppType.Id == dlc.Id;
                if (shouldBeDLC && !isDLC)
                    return false;
            }

            bool hasSelectedCategories = true;
            List<Category> appCategories = app.Categories.ToList();
            foreach (var selectedCategory in chosenSearchCategories) {
                bool hasSelectedCategory = false;
                foreach (var appCategory in appCategories) {
                    if (selectedCategory.Id == appCategory.Id) {
                        hasSelectedCategory = true;
                        break;
                    }
                }

                if (!hasSelectedCategory) {
                    hasSelectedCategories = false;
                    break;
                }
            }

            if (!hasSelectedCategories)
                return false;

            bool hasSelectedTags = true;
            List<Tag> appTags = app.Tags.ToList();
            foreach (var selectedTag in chosenSearchTags) {
                bool hasSelectedTag = false;
                foreach (var appTag in appTags) {
                    if (selectedTag.Id == appTag.Id) {
                        hasSelectedTag = true;
                        break;
                    }
                }

                if (!hasSelectedTag) {
                    hasSelectedTags = false;
                    break;
                }
            }

            if (!hasSelectedTags)
                return false;

            return true;
        }

        private void FilterSearchResults()
        {
            filteredApps = dbApps.Where(AppHasFilters).OrderBy(app => app.Name).ToList();
            UpdateGameList(filteredApps);
        }

        private void ChosenSearchFilterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            string[] nameChunks = checkbox.Name.Split("_");

            int matchId;
            string matchIdString = nameChunks[1];
            bool parsedId = int.TryParse(matchIdString, out matchId);
            if (!parsedId)
                return;

            string name = nameChunks[0];
            if (name.Contains("Category")) {
                foreach (var category in chosenSearchCategories) {
                    if (category.Id == matchId) {
                        chosenSearchCategories.Remove(category);
                        break;
                    }
                }
            } else if (name.Contains("Tag")) {
                foreach (var tag in chosenSearchTags) {
                    if (tag.Id == matchId) {
                        chosenSearchTags.Remove(tag);
                        break;
                    }
                }
            } else
                return;

            StackPanel checkboxStackPanel = (StackPanel)checkbox.Parent;
            StackPanel outerStackPanel = (StackPanel)checkboxStackPanel.Parent;
            outerStackPanel.Children.Remove(checkboxStackPanel);

            FilterSearchResults();
        }

        public string GetMatchTypeName(SearchElementType type)
        {
            switch (type) {
                case SearchElementType.NONE:      return "NONE";
                case SearchElementType.CATEGORY:  return "Category";
                case SearchElementType.TAG:       return "Tag";
                default:                          return null;
            }
        }

        private void InnerSearchCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            InnerSearchCheckBox checkbox = (InnerSearchCheckBox) sender;

            SearchElementType matchType = checkbox.SearchElementType;
            if (matchType == SearchElementType.CATEGORY) {
                MoveMatchFromResultsToChosen(
                    matchType,
                    checkbox.ParentSearchStackPanel,
                    SearchCategoryResultsStackPanel,
                    ChosenCategoriesStackPanel
                );
            } else if (matchType == SearchElementType.TAG) {
                MoveMatchFromResultsToChosen(
                    matchType,
                    checkbox.ParentSearchStackPanel,
                    SearchTagResultsStackPanel,
                    ChosenTagsStackPanel
                );
            }

            FilterSearchResults();
        }

        private void SearchFilterCheckBox_CheckedOrUnchecked(object sender, RoutedEventArgs e)
        {
            FilterCheckBox checkbox = (FilterCheckBox) sender;
            searchFilters ^= checkbox.Filter;
            FilterSearchResults();
        }

        private SearchStackPanel GetNewMatchStackPanel(string prefix, ISearchElement match, SearchElementType type)
        {
            string matchTypeName = GetMatchTypeName(type);

            InnerSearchCheckBox matchCheckBox = new InnerSearchCheckBox(match, type)
            {
                Name = $"{prefix}{matchTypeName}CheckBox_{match.Id}",
                Style = SearchMatchCheckboxTemplate.Style,
            };

            SearchTextBlock matchTextBlock = new SearchTextBlock()
            {
                Name = $"{prefix}{matchTypeName}TextBox_{match.Id}",
                Text = match.Name,
                Margin = SearchMatchTextBlockTemplate.Margin,
                Style = SearchMatchTextBlockTemplate.Style
            };

            SearchStackPanel matchStackPanel = new SearchStackPanel(matchCheckBox, matchTextBlock)
            {
                Name = $"{prefix}{matchTypeName}StackPanel_{match.Id}",
                Margin = SearchMatchStackPanelTemplate.Margin,
                Orientation = SearchMatchStackPanelTemplate.Orientation,
                VerticalAlignment = SearchMatchStackPanelTemplate.VerticalAlignment
            };

            return matchStackPanel;
        }

        private void MoveMatchFromResultsToChosen(
            SearchElementType elementType,
            SearchStackPanel searchMatchStackPanel,
            StackPanel searchResultsStackPanel,
            StackPanel chosenMatchesStackPanel
        )
        {
            SearchElementComponents comps = GetSearchElementComponents(elementType);
            ISearchElement element = searchMatchStackPanel.CheckBox.SearchElement;
            if (elementType == SearchElementType.CATEGORY) {
                Category? dbCategory = FindCategoryById(element.Id, null);
                if (dbCategory == null)
                    return;

                chosenSearchCategories.Add(dbCategory);
            } else if (elementType == SearchElementType.TAG) {
                Tag? dbTag = FindTagById(element.Id, null);
                if (dbTag == null)
                    return;

                chosenSearchTags.Add(dbTag);
            } else
                return;

            SearchStackPanel matchStackPanel = GetNewMatchStackPanel("Chosen", element, elementType);
            matchStackPanel.CheckBox.IsChecked = true;
            matchStackPanel.CheckBox.Unchecked += ChosenSearchFilterCheckBox_Unchecked;

            chosenMatchesStackPanel.Children.Add(matchStackPanel);

            searchMatchStackPanel.Visibility = Visibility.Collapsed;
            searchMatchStackPanel.CheckBox.IsChecked = false;

            comps.visibleSearchElements -= 1;
            if (comps.visibleSearchElements < 1)
                searchResultsStackPanel.Visibility = Visibility.Collapsed;
        }

        private void ClearResults(SearchElementType elementType, StackPanel searchResultsStackPanel) {
            List<StackPanel> panelsToRemove = new List<StackPanel>(searchResultsStackPanel.Children.Count);
            foreach (UIElement resultsElement in searchResultsStackPanel.Children) {
                if (resultsElement is not SearchStackPanel) continue;
                SearchStackPanel stackPanel = (SearchStackPanel) resultsElement;
                if (stackPanel.CheckBox.SearchElementType == elementType)
                    panelsToRemove.Add(stackPanel);
            }

            foreach (StackPanel panel in panelsToRemove)
                searchResultsStackPanel.Children.Remove(panel);
        }

        private void ShowAllInnerSearchElements(SearchElementType elementType) 
        {
            SearchElementComponents comps = GetSearchElementComponents(elementType);
            ClearResults(elementType, comps.searchResultsStackPanel);
            
            IEnumerable<ISearchElement> chosenElements = comps.chosenElements;
            List<ISearchElement> matches = comps.searchList.Where(match => {
                foreach (var chosenMatch in chosenElements)
                    if (chosenMatch.Id == match.Id)
                        return false;

                return true;
            }).OrderBy(match => match.Name).ToList();
            int matchesCount = matches.Count();

            comps.searchResultsStackPanel.Visibility = Visibility.Visible;
            if (matchesCount < 1) {
                comps.searchResultsTextBlock.Text = "Результатов не найдено.";
                return;
            } else {
                comps.searchResultsTextBlock.Text = "Результаты:";
            }

            for (int matchIdx = 0; matchIdx < matchesCount; matchIdx++) {
                ISearchElement match = matches[matchIdx];

                SearchStackPanel matchStackPanel = GetNewMatchStackPanel("Result", match, elementType);
                matchStackPanel.CheckBox.IsChecked = false;
                matchStackPanel.CheckBox.Checked += InnerSearchCheckBox_Checked;

                comps.searchResultsStackPanel.Children.Add(matchStackPanel);
                comps.visibleSearchElements += 1;
            }
        }

        private void HideAllInnerSearchElements(SearchElementType elementType) 
        {
            SearchElementComponents comps = GetSearchElementComponents(elementType);
            comps.searchResultsTextBlock.IsEnabled = true;
            InnerSearchShowResults(elementType);
        }

        public ref struct SearchElementComponents {
            public bool valid;
            public string searchString;
            public IEnumerable<ISearchElement> searchList;
            public IEnumerable<ISearchElement> chosenElements;
            public TextBlock searchResultsTextBlock;
            public StackPanel searchResultsStackPanel;
            public ref int visibleSearchElements;
        }

        private SearchElementComponents GetSearchElementComponents(SearchElementType type) {
            switch (type) {
                case SearchElementType.CATEGORY:
                    return new SearchElementComponents {
                        valid                   = true,
                        searchString            = CategorySearchTextBox.Text,
                        searchList              = dbCategories,
                        chosenElements          = chosenSearchCategories,
                        searchResultsTextBlock  = SearchCategoryResultsTextBlock,
                        searchResultsStackPanel = SearchCategoryResultsStackPanel,
                        visibleSearchElements   = ref visibleSearchCategories
                    };
                case SearchElementType.TAG:
                    return new SearchElementComponents {
                        valid                   = true,
                        searchString            = CategorySearchTextBox.Text,
                        searchList              = dbTags,
                        chosenElements          = chosenSearchTags,
                        searchResultsTextBlock  = SearchTagResultsTextBlock,
                        searchResultsStackPanel = SearchTagResultsStackPanel,
                        visibleSearchElements   = ref visibleSearchTags
                    };
                case SearchElementType.NONE:
                default:
                    return new SearchElementComponents {
                        valid = false
                    };
            }
        }

        private void InnerSearchShowResults(SearchElementType elementType)
        {
            SearchElementComponents comps = GetSearchElementComponents(elementType);
            ClearResults(elementType, comps.searchResultsStackPanel);

            if (comps.searchString == "") {
                comps.visibleSearchElements = 0;
                comps.searchResultsStackPanel.Visibility = Visibility.Collapsed;
                return;
            }

            string searchString = comps.searchString;
            IEnumerable<ISearchElement> chosenElements = comps.chosenElements;
            List<ISearchElement> matches = comps.searchList.Where(match => {
                if (!match.Name.ToLower().Contains(searchString.ToLower())) return false;
                foreach (var chosenMatch in chosenElements)
                    if (chosenMatch.Id == match.Id)
                        return false;

                return true;
            }).OrderBy(match => match.Name).ToList();
            int matchesCount = matches.Count();

            comps.searchResultsStackPanel.Visibility = Visibility.Visible;
            if (matchesCount < 1) {
                comps.searchResultsTextBlock.Text = "Результатов не найдено.";
                return;
            } else {
                comps.searchResultsTextBlock.Text = "Результаты:";
            }

            var matchesMaxCount = (matchesCount < 5) ? matchesCount : 5;
            for (int matchIdx = 0; matchIdx < matchesMaxCount; matchIdx++) {
                ISearchElement match = matches[matchIdx];

                SearchStackPanel matchStackPanel = GetNewMatchStackPanel("Result", match, elementType);
                matchStackPanel.CheckBox.IsChecked = false;
                matchStackPanel.CheckBox.Checked += InnerSearchCheckBox_Checked;

                comps.searchResultsStackPanel.Children.Add(matchStackPanel);
            }
            comps.visibleSearchElements = matchesMaxCount;
        }

        private void InnerSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox searchTextBox = (TextBox)sender;

            SearchElementType elementType = SearchElementType.NONE;;
            if (searchTextBox.Name == this.CategorySearchTextBox.Name) {
                elementType = SearchElementType.CATEGORY;
            } else if (searchTextBox.Name == this.TagSearchTextBox.Name) {
                elementType = SearchElementType.TAG;
            }

            InnerSearchShowResults(elementType);
        }

        private void ShowAllSearchMatchesButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (button.Name == this.ShowAllSearchCategoriesButton.Name) {
                if (showAllCategorySearchResults) {
                    ShowAllInnerSearchElements(SearchElementType.CATEGORY);
                } else {
                    HideAllInnerSearchElements(SearchElementType.CATEGORY);
                }

                this.ShowAllSearchCategoriesButton.Content = (showAllCategorySearchResults) ? "Скрыть все" : "Показать все";
                this.CategorySearchTextBox.IsEnabled = !showAllCategorySearchResults;
                showAllCategorySearchResults = !showAllCategorySearchResults;

            } else if (button.Name == this.ShowAllSearchTagsButton.Name) {
                if (showAllTagSearchResults) {
                    ShowAllInnerSearchElements(SearchElementType.TAG);
                } else {
                    HideAllInnerSearchElements(SearchElementType.TAG);
                }

                this.ShowAllSearchTagsButton.Content = (showAllTagSearchResults) ? "Скрыть все" : "Показать все";
                this.TagSearchTextBox.IsEnabled = !showAllTagSearchResults;
                showAllTagSearchResults = !showAllTagSearchResults;
            }

        }

        private void NameSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterSearchResults();
        }

        private void AppButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (AppButton) sender;
            ownerWindow.appId = button.AppId;
            ownerWindow.OpenPage(PageType.APP_STORE_PAGE);
        }

        private void SortSearchResults(AppSortElementType element, ListSortDirection direction)
        {
            if (filteredApps == null)
                return;

            if (direction == ListSortDirection.Ascending) {
                switch (element) {
                    case AppSortElementType.NAME:
                        filteredApps = filteredApps.OrderBy(app => app.Name).ToList();
                        break;
                    case AppSortElementType.PRICE:
                        filteredApps = filteredApps.OrderBy(app => app._LowestPrice).ToList();
                        break;
                    case AppSortElementType.DEVELOPER:
                        filteredApps = filteredApps.OrderBy(app => app.DeveloperCompany.OperatingName).ToList();
                        break;
                    case AppSortElementType.PUBLISHER:
                        filteredApps = filteredApps.OrderBy(app => app.PublisherCompany.OperatingName).ToList();
                        break;
                    case AppSortElementType.RELEASE_DATETIME:
                        filteredApps = filteredApps.OrderBy(app => DateTime.Parse(app.ReleaseDatetime)).ToList();
                        break;
                }
            } else if (direction == ListSortDirection.Descending) {
                switch (element) {
                    case AppSortElementType.NAME:
                        filteredApps = filteredApps.OrderByDescending(app => app.Name).ToList();
                        break;
                    case AppSortElementType.PRICE:
                        filteredApps = filteredApps.OrderByDescending(app => app._LowestPrice).ToList();
                        break;
                    case AppSortElementType.DEVELOPER:
                        filteredApps = filteredApps.OrderByDescending(app => app.DeveloperCompany.OperatingName).ToList();
                        break;
                    case AppSortElementType.PUBLISHER:
                        filteredApps = filteredApps.OrderByDescending(app => app.PublisherCompany.OperatingName).ToList();
                        break;
                    case AppSortElementType.RELEASE_DATETIME:
                        filteredApps = filteredApps.OrderByDescending(app => DateTime.Parse(app.ReleaseDatetime)).ToList();
                        break;
                }
            }

            UpdateGameList(filteredApps);
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var elementItem = (AppSortElementComboBoxItem) SortComboBox.SelectedItem;
            SortSearchResults(elementItem.Element, elementItem.Direction);
        }
    }
}