using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LanistaBrowserV1.Classes;
using LanistaBrowserV1.Functions;
using LanistaBrowserV1.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LanistaBrowserV1.UserControls
{
    public partial class SearchArmor : UserControl
    {
        public ListBoxItem SelectedItem
        {
            get { return (ListBoxItem)ListBoxArmors.SelectedItem!; }
        }

        private List<Armor> armors = new();

        private string? currentSortColumn;
        private bool isAscending;

        public SearchArmor()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object Sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SearchArmor UserControl_Loaded");

            if (DataContext is not MainViewModel viewModel) { return; }

            await UpdateTypeSelection(false, []);

            armors = viewModel.ArmorList;
        }

        public async Task UpdateTypeSelection(bool isExternal, List<string> itemsToSearch)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
             {
                 if (isExternal)
                 {
                     TypeSelectionBox.Items.Clear();

                     foreach (var item in itemsToSearch)
                     {
                         TypeSelectionBox.Items.Add(item);
                     }

                     TypeSelectionBox.SelectedIndex = 0;
                 }
                 else
                 {
                     if (DataContext is not MainViewModel viewModel || viewModel.ApiConfig == null || viewModel.ApiConfig.ArmorTypes == null) { return; }

                     if (TypeSelectionBox.Items.Count == 0)
                     {
                         var allItem = new ComboBoxItem { Content = "all", HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                         TypeSelectionBox.Items.Add(allItem);
                         foreach (var item in viewModel.ApiConfig.ArmorTypes)
                         {
                             var comboBoxItem = new ComboBoxItem { Content = item.Key.ToLower(), HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                             TypeSelectionBox.Items.Add(comboBoxItem);
                         }

                         TypeSelectionBox.SelectedIndex = 0;
                     }
                 }
             });
        }

        private async void TypeSelectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox)
            {
                return;
            }

            await UpdateList();
        }

        private async void ToggleGreaterLesserEqual(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.Content is string item)
            {
                if (item == ">")
                {
                    button.Content = "<";
                }
                else if (item == "<")
                {
                    button.Content = "=";
                }
                else if (item == "=")
                {
                    button.Content = ">";
                }
                await UpdateList();

                var parentDockPanel = button.Parent as DockPanel;

                var textBox = parentDockPanel!.Children.OfType<TextBox>().FirstOrDefault();

                textBox?.Focus();
            }
        }

        private async void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            if (button == null) { return; }

            // Reset TextBoxes to blank
            SearchBox.Text = string.Empty;

            ToggleFavorites.Text = "A";

            SearchWeight.Text = string.Empty;
            SearchBaseBlock.Text = string.Empty;
            SearchMinLevel.Text = string.Empty;
            SearchMaxLevel.Text = string.Empty;

            SearchWeightButton.Content = "=";
            SearchBaseBlockButton.Content = "=";
            SearchMinLevelButton.Content = "=";
            SearchMaxLevelButton.Content = "=";

            TypeSelectionBox.SelectedIndex = 0;

            // Remove current sorting
            currentSortColumn = null;
            isAscending = true;

            // Update the list without sorting
            await UpdateList();

            if (ListBoxArmors.ItemCount > 0)
            {
                ListBoxArmors.SelectedItem = null;
                await Task.Delay(100);
                ListBoxArmors.ScrollIntoView(ListBoxArmors.Items[0]!);
            }

            DetailsChooseItemView.IsVisible = true;
            DetailsScrollViewer.IsVisible = false;
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox || textBox.Text == null)
            {
                return;
            }

            if (!int.TryParse(textBox.Text, out _))
            {
                if (textBox.Text.Length > 0)
                {
                    textBox.Text = textBox.Text.Remove(textBox.Text.Length - 1);
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }

            await UpdateList();
        }

        private async void FavoriteSortingButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (ToggleFavorites.Text == "F")
                {
                    ToggleFavorites.Text = "A";
                }
                else
                {
                    ToggleFavorites.Text = "F";
                }
            });

            await UpdateList();
        }

        private async void SortListButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.Name is string buttonName)
            {
                // Determine the property to sort by based on the button name
                Func<Armor, object> sortProperty = buttonName switch
                {
                    "WeightButton" => w => w.Weight ?? 0,
                    "MinLevelButton" => w => w.RequiredLevel ?? 0,
                    "MaxLevelButton" => w => w.MaxLevel ?? 0,
                    "BaseBlockButton" => w => w.BaseBlock ?? 0,
                    "ArmorNameButton" => w => w.Name ?? string.Empty,
                    "TypeButton" => w => w.TypeName ?? string.Empty,

                    _ => w => 0,
                };

                // Sort the list
                if (sortProperty != null)
                {
                    if (currentSortColumn == buttonName)
                    {
                        // If the current sort column is the same as the clicked button, flip the sort direction
                        isAscending = !isAscending;
                    }
                    else
                    {
                        // If a different column was clicked, sort in ascending order
                        isAscending = true;
                    }

                    // Update the current sort column
                    currentSortColumn = buttonName;

                    // Get the filtered list and sort it
                    var filteredArmors = await UpdateList();

                    if (buttonName == "ArmorNameButton" || buttonName == "TypeButton" || buttonName == "GripButton")
                    {
                        armors = isAscending ? filteredArmors.OrderBy(w => sortProperty(w).ToString()).ToList() : filteredArmors.OrderByDescending(w => sortProperty(w).ToString()).ToList();
                    }
                    else
                    {
                        armors = isAscending ? filteredArmors.OrderBy(sortProperty).ToList() : filteredArmors.OrderByDescending(sortProperty).ToList();
                    }
                    ListBoxArmors.ItemsSource = armors;
                }
            }
        }

        private void ListBoxArmors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is Armor selectedArmor)
            {
                GeneralStackPanel.Children.Clear();
                RequirementsStackPanel.Children.Clear();
                BonusesStackPanel.Children.Clear();

                //General Block
                SelectedItemTitle.Text = selectedArmor.Name ?? string.Empty;
                GeneralStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Type", selectedArmor.TypeName ?? string.Empty));
                GeneralStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Weight", selectedArmor.Weight.ToString() ?? string.Empty));
                GeneralStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Soulbound", selectedArmor.Soulbound.ToString() ?? string.Empty));

                //Requirements Block

                string requiredLevel;
                string minLevel = selectedArmor.RequiredLevel.ToString() ?? string.Empty;
                string maxLevel = selectedArmor.MaxLevel.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(maxLevel))
                {
                    requiredLevel = minLevel;
                }
                else
                {
                    requiredLevel = $"{minLevel} - {maxLevel}";
                }
                RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Level", requiredLevel));

                if (selectedArmor.StrengthRequirementValue != null && selectedArmor.StrengthRequirementValue != 0)
                {
                    RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Strength", selectedArmor.StrengthRequirementValue.ToString() ?? string.Empty));
                }

                if (selectedArmor.SkillRequirementValue != null && selectedArmor.SkillRequirementValue != 0)
                {
                    RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Skill", selectedArmor.SkillRequirementValue.ToString() ?? string.Empty));
                }

                if (!string.IsNullOrEmpty(selectedArmor.RaceRestrictions))
                {
                    RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Race", selectedArmor.RaceRestrictions));
                }
                if (selectedArmor.RequiresLegend == true)
                {
                    RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Requires Legend", ""));
                }
                if (selectedArmor.RequiredRankingPoints != null)
                {
                    RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Ranking", selectedArmor.RequiredRankingPoints.ToString() ?? string.Empty));
                }
                if (selectedArmor.MinPopularity != null)
                {
                    RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Min Popularity", selectedArmor.MinPopularity.ToString() ?? string.Empty));
                }
                if (selectedArmor.MaxPopularity != null)
                {
                    RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Max Popularity", selectedArmor.MaxPopularity.ToString() ?? string.Empty));
                }

                //Bonuses Block
                if (selectedArmor.Bonuses != null)
                {
                    if (selectedArmor.CritRate != null && selectedArmor.CritRate != 0)
                    {
                        string isPositive = selectedArmor.CritRate > 0 ? "+" : "";
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Crit Rate", $"{isPositive}{selectedArmor.CritRate}%"));
                    }
                    if (selectedArmor.MaxCritRate != null && selectedArmor.MaxCritRate != 0)
                    {
                        string isPositive = selectedArmor.MaxCritRate > 0 ? "+" : "";
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Max Crit Rate", $"{isPositive}{selectedArmor.CritRate}%"));
                    }
                    if (selectedArmor.MinCritRate != null && selectedArmor.MinCritRate != 0)
                    {
                        string isPositive = selectedArmor.MinCritRate > 0 ? "+" : "";
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Min Crit Rate", $"{isPositive}{selectedArmor.CritRate}%"));
                    }
                    if (selectedArmor.IncreasedHitRate != null && selectedArmor.IncreasedHitRate != 0)
                    {
                        string isPositive = selectedArmor.IncreasedHitRate > 0 ? "+" : "";
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Hit Rate", $"{isPositive}{selectedArmor.IncreasedHitRate}%"));
                    }
                    foreach (var item in selectedArmor.Bonuses)
                    {
                        string itemType;
                        if (item.Type == null)
                        {
                            itemType = "Experience";
                        }
                        else
                        {
                            itemType = item.Type;
                        }

                        if (item.Additive != null)
                        {
                            string isPositive = item.Additive > 0 ? "+" : "";
                            BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(itemType.ToLowerInvariant()), $"{isPositive}{item.Additive}"));
                        }
                        else if (item.Multiplier != null)
                        {
                            double multiplier = (item.Multiplier.Value - 1) * 100;
                            multiplier = Math.Round(multiplier);
                            string isPositive = multiplier > 0 ? "+" : "";
                            BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(itemType.ToLowerInvariant()), $"{isPositive}{multiplier}%"));
                        }
                    }
                }

                //Show Details
                DetailsChooseItemView.IsVisible = false;
                DetailsScrollViewer.IsVisible = true;
            }
        }

        private async void SearchBoxItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox || textBox.Text == null)
            {
                return;
            }

            await UpdateList();
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.DataContext is Armor item && DataContext is MainViewModel viewModel)
            {
                SqliteHandler.ToggleFavoritedItem(item, viewModel);

                if (button != null && button.Content != null)
                {
                    var textBlock = (TextBlock)button.Content;

                    if (textBlock.IsVisible)
                    {
                        textBlock.IsVisible = false;
                    }
                    else
                    {
                        textBlock.IsVisible = true;
                    }
                }
            }
        }

        public async Task<List<Armor>> UpdateList()
        {
            string searchQuery;
            string typeQuery = string.Empty;
            bool showOnlyFavorited = false;

            if (DataContext is not MainViewModel viewModel) { return []; }

            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                searchQuery = string.Empty;
            }
            else
            {
                searchQuery = SearchBox.Text.ToLower();
            }

            if (TypeSelectionBox.SelectionBoxItem as string == "all")
            {
                typeQuery = string.Empty;
            }
            else
            {
                typeQuery = TypeSelectionBox.SelectionBoxItem as string ?? string.Empty;
            }

            if (ToggleFavorites.Text == "F")
            {
                showOnlyFavorited = true;
            }

            //BaseBlock
            if (!double.TryParse(SearchMinLevel.Text, out double minLevelQuery)) minLevelQuery = double.NaN;
            if (!double.TryParse(SearchMaxLevel.Text, out double maxLevelQuery)) maxLevelQuery = double.NaN;
            if (!double.TryParse(SearchWeight.Text, out double weightQuery)) weightQuery = double.NaN;
            if (!double.TryParse(SearchBaseBlock.Text, out double baseBlockQuery)) baseBlockQuery = double.NaN;

            var filteredArmors = await Dispatcher.UIThread.InvokeAsync(() =>
            viewModel.ArmorList.Where(w =>
            (!showOnlyFavorited || w.IsFavorited) &&
            (w.Name?.ToLower().Contains(searchQuery) ?? false) &&
            (w.TypeName?.ToLower().Contains(typeQuery) ?? false) &&
            (double.IsNaN(weightQuery) ||
                (SearchWeightButton.Content!.ToString() == ">" && w.Weight > weightQuery) ||
                (SearchWeightButton.Content!.ToString() == "<" && w.Weight < weightQuery) ||
                (SearchWeightButton.Content!.ToString() == "=" && w.Weight == weightQuery)) &&
            (double.IsNaN(minLevelQuery) ||
                (SearchMinLevelButton.Content!.ToString() == ">" && w.RequiredLevel > minLevelQuery) ||
                (SearchMinLevelButton.Content!.ToString() == "<" && w.RequiredLevel < minLevelQuery) ||
                (SearchMinLevelButton.Content!.ToString() == "=" && w.RequiredLevel == minLevelQuery)) &&
            (double.IsNaN(maxLevelQuery) ||
                (SearchMaxLevelButton.Content!.ToString() == ">" && w.MaxLevel > maxLevelQuery) ||
                (SearchMaxLevelButton.Content!.ToString() == "<" && w.MaxLevel < maxLevelQuery) ||
                (SearchMaxLevelButton.Content!.ToString() == "=" && w.MaxLevel == maxLevelQuery)) &&
            (double.IsNaN(baseBlockQuery) ||
                (SearchBaseBlockButton.Content!.ToString() == ">" && w.BaseBlock > baseBlockQuery) ||
                (SearchBaseBlockButton.Content!.ToString() == "<" && w.BaseBlock < baseBlockQuery) ||
                (SearchBaseBlockButton.Content!.ToString() == "=" && w.BaseBlock == baseBlockQuery)))
            .ToList()

);

            Func<Armor, object> sortProperty = currentSortColumn switch
            {
                "WeightButton" => w => w.Weight ?? 0,
                "MinLevelButton" => w => w.RequiredLevel ?? 0,
                "MaxLevelButton" => w => w.MaxLevel ?? 0,
                "BaseBlockButton" => w => w.BaseBlock ?? 0,
                "ArmorNameButton" => w => w.Name ?? string.Empty,
                "TypeButton" => w => w.TypeName ?? string.Empty,

                _ => w => 0,
            };

            // Sort the filtered list
            if (sortProperty != null)
            {
                if (currentSortColumn == "WeaponShieldNameButton" || currentSortColumn == "TypeButton" || currentSortColumn == "GripButton")
                {
                    filteredArmors = isAscending ? filteredArmors.OrderBy(w => sortProperty(w).ToString()).ToList() : filteredArmors.OrderByDescending(w => sortProperty(w).ToString()).ToList();
                }
                else
                {
                    filteredArmors = isAscending ? filteredArmors.OrderBy(sortProperty).ToList() : filteredArmors.OrderByDescending(sortProperty).ToList();
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ListBoxArmors.ItemsSource = filteredArmors;
            });

            return filteredArmors;
        }
    }
}