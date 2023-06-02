using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using Toolkit_NET_Client.Models;
using Toolkit_NET_Client.Windows;
using Microsoft.EntityFrameworkCore;

namespace Toolkit_NET_Client.Pages
{
    public class SearchStackPanel : StackPanel
    {
        public SearchCheckBox CheckBox { get; set; }
        public SearchTextBlock TextBlock { get; set; }

        public SearchStackPanel(SearchCheckBox checkBox, SearchTextBlock textBlock) : base()
        {
            this.CheckBox = checkBox;
            this.CheckBox.ParentSearchStackPanel = this;
            this.TextBlock = textBlock;
            this.TextBlock.ParentSearchStackPanel = this;
            this.Children.Add(checkBox);
            this.Children.Add(textBlock);
        }
    }

    public class SearchCheckBox : CheckBox
    {
        public SearchStackPanel ParentSearchStackPanel { get; set; }
        public ISearchElement SearchElement { get; set; }
        public SearchElementType SearchElementType { get; set; }

        public SearchCheckBox(ISearchElement element, SearchElementType type) : base()
        {
            this.SearchElement = element;
            this.SearchElementType = type;
        }
    }

    public class SearchTextBlock : TextBlock
    {
        public SearchStackPanel ParentSearchStackPanel { get; set; }
        public SearchCheckBox LabelingSearchCheckBox
        {
            get { return ParentSearchStackPanel.CheckBox; }
        }
        public ISearchElement? SearchElement
        {
            get { return this.LabelingSearchCheckBox.SearchElement; }
        }
    }

    /// <summary>
    /// Interaction logic for StoreWindow.xaml
    /// </summary>
    public partial class UserStorePage : Page
    {
        private MainWindow ownerWindow;
        private User user;

        private List<App> dbApps;
        private List<Category> dbCategories;
        private List<Tag> dbTags;

        private int lowestSearchPrice = 0;
        private int highestSearchPrice = int.MaxValue;

        private List<Category> chosenSearchCategories;
        private List<Tag> chosenSearchTags;

        private int visibleSearchCategories = 0;
        private int visibleSearchTags = 0;

        private bool showAllCategorySearchResults = true;
        private bool showAllTagSearchResults = true;

        private SearchStackPanel lastCheckedMatchStackPanel;

        public UserStorePage(MainWindow ownerWindow, User user)
        {
            InitializeComponent();
            this.ownerWindow = ownerWindow;
            this.user = user;

            this.lastCheckedMatchStackPanel = null;

            Thread updateThread = new Thread(() => {
                using (var db = new ToolkitContext()) {
                    dbApps = db.Apps
                        .Include(app => app.DeveloperCompany)
                        .Include(app => app.PublisherCompany)
                        .ToList();
                    dbCategories = db.Categories.ToList();
                    dbTags = db.Tags.ToList();
                    chosenSearchCategories = new List<Category>(dbCategories.Count);
                    chosenSearchTags = new List<Tag>(dbTags.Count);
                }
                Dispatcher.BeginInvoke(() => UpdateGameList(this.dbApps));
            });
            updateThread.Start();
        }

        private void UpdateGameList(List<App> apps)
        {
            AppsListView.ItemsSource = apps;
            SearchCategoryResultsTextBlock.Text = apps.Count.ToString();
        }

        private void AppsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PriceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox priceTextBox = (TextBox)sender;
            int price;
            if (priceTextBox.Text != "") {
                bool parsed = int.TryParse(priceTextBox.Text, out price);
                if (parsed) {
                    ToolkitApp.ClearStatus(this.PriceStatusTextBlock);
                    priceTextBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;
                } else {
                    ToolkitApp.SetStatusError(this.PriceStatusTextBlock, "Введите целочисленное значение.");
                    priceTextBox.BorderBrush = ToolkitApp.SolidColorBrush_Error;
                    return;
                }
            } else {
                if      (priceTextBox.Name.StartsWith("L"))  this.lowestSearchPrice = 0;
                else if (priceTextBox.Name.StartsWith("U"))  this.highestSearchPrice = int.MaxValue;
                else                                         Debug.WriteLine("Warning: Unknown sender in PriceTextBox_TextChanged.");

                ToolkitApp.ClearStatus(this.PriceStatusTextBlock);
                priceTextBox.BorderBrush = ToolkitApp.SolidColorBrush_DefaultBorderBrush;
                return;
            }

            if      (priceTextBox.Name.StartsWith("L"))  this.lowestSearchPrice = price;
            else if (priceTextBox.Name.StartsWith("U"))  this.highestSearchPrice = price;
            else                                         Debug.WriteLine("Warning: Unknown sender in PriceTextBox_TextChanged.");

        }

        private void FilterSearchResults()
        {

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

        private void SearchFilterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SearchCheckBox checkbox = (SearchCheckBox)sender;

            lastCheckedMatchStackPanel = checkbox.ParentSearchStackPanel;

            SearchElementType matchType = checkbox.SearchElementType;
            if (matchType == SearchElementType.CATEGORY) {
                MoveMatchFromResultsToChosen(
                    matchType,
                    checkbox.ParentSearchStackPanel,
                    SearchCategoryResultsStackPanel,
                    ChosenCategoriesStackPanel,
                    ref visibleSearchCategories
                );
            } else if (matchType == SearchElementType.TAG) {
                MoveMatchFromResultsToChosen(
                    matchType,
                    checkbox.ParentSearchStackPanel,
                    SearchTagResultsStackPanel,
                    ChosenTagsStackPanel,
                    ref visibleSearchTags
                );
            } else {

            }
        }

        private SearchStackPanel GetNewMatchStackPanel(string prefix, ISearchElement match, SearchElementType type)
        {
            string matchTypeName = GetMatchTypeName(type);

            SearchCheckBox matchCheckBox = new SearchCheckBox(match, type)
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
            SearchElementType matchType,
            SearchStackPanel searchMatchStackPanel,
            StackPanel searchResultsStackPanel,
            StackPanel chosenMatchesStackPanel,
            ref int visibleMatchesCount
        )
        {
            ISearchElement match = searchMatchStackPanel.CheckBox.SearchElement;
            if (matchType == SearchElementType.CATEGORY) {
                Category? dbCategory = ToolkitApp.FindCategoryById(match.Id, null);
                if (dbCategory == null)
                    return;

                chosenSearchCategories.Add(dbCategory);
            } else if (matchType == SearchElementType.TAG) {
                Tag? dbTag = ToolkitApp.FindTagById(match.Id, null);
                if (dbTag == null)
                    return;

                chosenSearchTags.Add(dbTag);
            } else
                return;

            SearchStackPanel matchStackPanel = GetNewMatchStackPanel("Chosen", match, matchType);
            matchStackPanel.CheckBox.IsChecked = true;
            matchStackPanel.CheckBox.Unchecked += ChosenSearchFilterCheckBox_Unchecked;

            chosenMatchesStackPanel.Children.Add(matchStackPanel);

            searchMatchStackPanel.Visibility = Visibility.Collapsed;
            searchMatchStackPanel.CheckBox.IsChecked = false;

            visibleMatchesCount -= 1;
            if (visibleMatchesCount < 1)
                searchResultsStackPanel.Visibility = Visibility.Collapsed;
        }

        private void InnerSearchShowResults(
            SearchElementType matchType,
            string? searchString,
            IEnumerable<ISearchElement> searchList,
            IEnumerable<ISearchElement> chosenMatches,
            TextBlock searchResultsTextBlock,
            StackPanel searchResultsStackPanel,
            bool showAllResults,
            int matchesToShow,
            ref int visibleMatches
        )
        {
            if (searchString == null) {
                matchesToShow = (showAllResults) ? searchList.Count() : 0;
            } else if (searchString == "") {
                searchResultsStackPanel.Visibility = Visibility.Collapsed;
                return;
            }

            List<StackPanel> panelsToRemove = new List<StackPanel>(matchesToShow);
            foreach (UIElement resultsElement in searchResultsStackPanel.Children) {
                if (resultsElement is not SearchStackPanel) continue;
                SearchStackPanel stackPanel = (SearchStackPanel) resultsElement;
                if (stackPanel.CheckBox.SearchElementType == matchType)
                    panelsToRemove.Add(stackPanel);
            }

            foreach (StackPanel panel in panelsToRemove)
                searchResultsStackPanel.Children.Remove(panel);

            List<ISearchElement> matches = null;
            int matchesCount;
            if (!showAllResults) {
                if (matchesToShow > 0) {
                    matches = searchList.Where(match => {
                        if (!match.Name.ToLower().Contains(searchString.ToLower())) return false;
                        foreach (var chosenMatch in chosenMatches)
                            if (chosenMatch.Id == match.Id)
                                return false;

                        return true;
                    }).OrderBy(match => match.Name).ToList();
                    matchesCount = matches.Count();
                } else
                    matchesCount = 0;
            } else {
                matches = searchList.OrderBy(match => match.Name).ToList();
                matchesCount = matches.Count();
            }

            searchResultsStackPanel.Visibility = Visibility.Visible;
            if (matchesCount < 1) {
                searchResultsTextBlock.Text = "Результатов не найдено.";
                return;
            } else {
                searchResultsTextBlock.Text = "Результаты:";
            }

            if (!showAllResults)
                return;

            var matchesMaxCount = (matchesCount < matchesToShow) ? matchesCount : matchesToShow;
            for (int matchIdx = 0; matchIdx < matchesMaxCount; matchIdx++) {
                ISearchElement match = matches[matchIdx];

                SearchStackPanel matchStackPanel = GetNewMatchStackPanel("Result", match, matchType);
                matchStackPanel.CheckBox.IsChecked = false;
                matchStackPanel.CheckBox.Checked += SearchFilterCheckBox_Checked;

                searchResultsStackPanel.Children.Add(matchStackPanel);
            }
            visibleMatches = matchesMaxCount;
        }

        private void InnerSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox searchTextBox = (TextBox)sender;

            if (searchTextBox.Name == this.CategorySearchTextBox.Name) {
                InnerSearchShowResults(
                    SearchElementType.CATEGORY,
                    searchTextBox.Text,
                    dbCategories,
                    chosenSearchCategories,
                    SearchCategoryResultsTextBlock,
                    SearchCategoryResultsStackPanel,
                    false,
                    5,
                    ref visibleSearchCategories
                );
            } else if (searchTextBox.Name == this.TagSearchTextBox.Name) {
                InnerSearchShowResults(
                    SearchElementType.TAG,
                    searchTextBox.Text,
                    dbTags,
                    chosenSearchTags,
                    SearchTagResultsTextBlock,
                    SearchTagResultsStackPanel,
                    false,
                    5,
                    ref visibleSearchTags
                );
            }
        }

        private void InnerSearchShowAllResults(
            SearchElementType matchType,
            IEnumerable<ISearchElement> searchList,
            IEnumerable<ISearchElement> chosenMatches,
            TextBlock searchResultsTextBlock,
            StackPanel searchResultsStackPanel,
            ref int visibleMatches
        ) {

        }

        private void ShowAllSearchMatchesButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (button.Name == this.ShowAllSearchCategoriesButton.Name) {
                var matchesToShow = (showAllCategorySearchResults) ? dbCategories.Count : 0;
                InnerSearchShowResults(
                    SearchElementType.CATEGORY,
                    null,
                    dbCategories,
                    chosenSearchCategories,
                    SearchCategoryResultsTextBlock,
                    SearchCategoryResultsStackPanel,
                    showAllCategorySearchResults,
                    matchesToShow,
                    ref visibleSearchCategories
                );

                this.ShowAllSearchCategoriesButton.Content = (showAllCategorySearchResults) ? "Скрыть все" : "Показать все";
                this.CategorySearchTextBox.IsEnabled = !showAllCategorySearchResults;
                showAllCategorySearchResults = !showAllCategorySearchResults;

            } else if (button.Name == this.ShowAllSearchTagsButton.Name) {
                var matchesToShow = (showAllTagSearchResults) ? dbTags.Count : 0;
                InnerSearchShowResults(
                    SearchElementType.TAG,
                    null,
                    dbTags,
                    chosenSearchTags,
                    SearchTagResultsTextBlock,
                    SearchTagResultsStackPanel,
                    showAllTagSearchResults,
                    matchesToShow,
                    ref visibleSearchTags
                );

                showAllTagSearchResults = !showAllTagSearchResults;
                this.ShowAllSearchTagsButton.Content = (showAllTagSearchResults) ? "Скрыть все" : "Показать все";
                this.TagSearchTextBox.IsEnabled = !showAllTagSearchResults;
            }

        }
    }
}