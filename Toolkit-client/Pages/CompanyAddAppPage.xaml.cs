using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

using Toolkit_Client.Windows;
using Toolkit_Client.Models;

using static Toolkit_Client.Modules.InnerElementSearch;
using static Toolkit_Client.Modules.Database;

namespace Toolkit_Client.Pages
{
    /// <summary>
    /// Interaction logic for CompanyAddAppPage.xaml
    /// </summary>
    public partial class CompanyAddAppPage : ToolkitPage
    {
        public class AppTypeComboBoxItem : ComboBoxItem 
        {
            public AppType AppType { get; set; }

            public AppTypeComboBoxItem() { this.AppType = null; }
            public AppTypeComboBoxItem(AppType appType) { this.AppType = appType; }
        }

        private PartnerWindow ownerWindow;
        private Company company;

        private List<Category> dbCategories;
        private List<Tag> dbTags;

        private List<Category> chosenSearchCategories;
        private List<Tag> chosenSearchTags;

        private int visibleSearchCategories = 0;
        private int visibleSearchTags = 0;

        private bool showAllCategorySearchResults = true;
        private bool showAllTagSearchResults = true;

        public CompanyAddAppPage(PartnerWindow ownerWindow, Company company) : base(ownerWindow.GetLogger())
        {
            InitializeComponent();
            this.ownerWindow = ownerWindow;
            this.company = company;

            Thread updateThread = new Thread(() => {
                using (var db = new ToolkitContext()) {
                    dbCategories = db.Categories.ToList();
                    dbTags = db.Tags.ToList();
                    
                    chosenSearchCategories = new List<Category>(dbCategories.Count);
                    chosenSearchTags = new List<Tag>(dbTags.Count);
                    List<AppType> appTypes = db.AppTypes.ToList();
                    LoadAppTypesIntoComboBox(appTypes, this.AppTypeComboBox);
                }
            });
            updateThread.Start();

            NameTextBox.MaxLength              = App.NAME_MAX_LENGTH;
            DeveloperTextBox.MaxLength         = Company.OPERATING_NAME_MAX_LENGTH;
            PublisherTextBox.MaxLength         = Company.OPERATING_NAME_MAX_LENGTH;
            ShortDescriptionTextBox.MaxLength  = Models.AppStorePage.SHORT_DESCRIPTION_MAX_LENGTH;
            LongDescriptionTextBox.MaxLength   = Models.AppStorePage.LONG_DESCRIPTION_MAX_LENGTH;
            DiscoutPercentTextBox.MaxLength    = 3;
            DiscountStartTextBox.MaxLength     = 19;
            DiscountExpireTextBox.MaxLength    = 19;

            PublisherTextBox.Text = string.Format("{0} (Вы)", company.OperatingName);
            PublisherTextBox.IsReadOnly = true;

            ShortDescriptionCharactersTextBlock.Text = string.Format("(0/{0} символов)", ShortDescriptionTextBox.MaxLength);
            LongDescriptionCharactersTextBlock.Text  = string.Format("(0/{0} символов)", LongDescriptionTextBox.MaxLength);
        }

        public void LoadAppTypesIntoComboBox(List<AppType> appTypes, ComboBox comboBox)
        {
            Application.Current.Dispatcher.BeginInvoke(() => {
                AppType type;
                for (int i = 0; i < appTypes.Count; i++) {
                    type = appTypes[i];
                    var comboItem = new AppTypeComboBoxItem();
                    comboItem.AppType = type;
                    comboItem.Content = type.Name;
                    comboBox.Items.Add(comboItem);
                }
                comboBox.SelectedIndex = 0;
            });
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
        }

        private SearchStackPanel GetNewMatchStackPanel(string prefix, ISearchElement match, SearchElementType type)
        {
            string matchTypeName = GetMatchTypeName(type);

            InnerSearchCheckBox matchCheckBox = new InnerSearchCheckBox(match, type)
            {
                Name   = $"{prefix}{matchTypeName}CheckBox_{match.Id}",
                Style  = SearchMatchCheckboxTemplate.Style,
            };

            SearchTextBlock matchTextBlock = new SearchTextBlock()
            {
                Name    = $"{prefix}{matchTypeName}TextBox_{match.Id}",
                Text    = match.Name,
                Margin  = SearchMatchTextBlockTemplate.Margin,
                Style   = SearchMatchTextBlockTemplate.Style
            };

            SearchStackPanel matchStackPanel = new SearchStackPanel(matchCheckBox, matchTextBlock)
            {
                Name               = $"{prefix}{matchTypeName}StackPanel_{match.Id}",
                Margin             = SearchMatchStackPanelTemplate.Margin,
                Orientation        = SearchMatchStackPanelTemplate.Orientation,
                VerticalAlignment  = SearchMatchStackPanelTemplate.VerticalAlignment
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
                        valid                    = true,
                        searchString             = CategorySearchTextBox.Text,
                        searchList               = dbCategories,
                        chosenElements           = chosenSearchCategories,
                        searchResultsTextBlock   = SearchCategoryResultsTextBlock,
                        searchResultsStackPanel  = SearchCategoryResultsStackPanel,
                        visibleSearchElements    = ref visibleSearchCategories
                    };
                case SearchElementType.TAG:
                    return new SearchElementComponents {
                        valid                    = true,
                        searchString             = CategorySearchTextBox.Text,
                        searchList               = dbTags,
                        chosenElements           = chosenSearchTags,
                        searchResultsTextBlock   = SearchTagResultsTextBlock,
                        searchResultsStackPanel  = SearchTagResultsStackPanel,
                        visibleSearchElements    = ref visibleSearchTags
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

        private void AddAppButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AppsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShortDescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShortDescriptionCharactersTextBlock.Text = string.Format("({0}/{1} символов)", ShortDescriptionTextBox.Text.Length, ShortDescriptionTextBox.MaxLength);
        }

        private void LongDescriptionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LongDescriptionCharactersTextBlock.Text = string.Format("({0}/{1} символов)", LongDescriptionTextBox.Text.Length, LongDescriptionTextBox.MaxLength);
        }
    }
}
