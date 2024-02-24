using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LanistaBrowserV1.Classes;
using LanistaBrowserV1.Functions;
using LanistaBrowserV1.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace LanistaBrowserV1.UserControls
{
    public partial class SearchConsumables : UserControl
    {
        private List<Consumable> consumables = new();

        private string? currentSortColumn;
        private bool isAscending;

        public SearchConsumables()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object Sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SearchConsumables UserControl_Loaded");

            if (DataContext is not MainViewModel viewModel) { return; }

            consumables = viewModel.ConsumableList;
        }

        private async void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            if (button == null) { return; }

            // Reset TextBoxes to blank
            SearchBox.Text = string.Empty;

            ToggleFavorites.Text = "A";

            SearchMinLevel.Text = string.Empty;
            SearchMaxLevel.Text = string.Empty;

            TypeSelection.SelectedIndex = 0;

            SearchMinLevelButton.Content = "=";
            SearchMaxLevelButton.Content = "=";

            // Remove current sorting
            currentSortColumn = null;
            isAscending = true;

            // Update the list without sorting
            await UpdateList();

            if (ListBoxConsumables.ItemCount > 0)
            {
                ListBoxConsumables.SelectedItem = null;
                await Task.Delay(100);
                ListBoxConsumables.ScrollIntoView(ListBoxConsumables.Items[0]!);
            }

            DetailsChooseItemView.IsVisible = true;
            DetailsScrollViewer.IsVisible = false;
        }

        private async void TypeSelectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox)
            {
                return;
            }

            await UpdateList();
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

        private async void SortListButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.Name is string buttonName)
            {
                // Determine the property to sort by based on the button name
                Func<Consumable, object> sortProperty = buttonName switch
                {
                    "TypeButton" => w => w.ForLiveBattle.ToString() ?? string.Empty,
                    "MinLevelButton" => w => w.RequiredLevel ?? 0,
                    "MaxLevelButton" => w => w.MaxLevel ?? 0,
                    "ConsumablesNameButton" => w => w.Name ?? string.Empty,

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
                    var filteredConsumables = await UpdateList();

                    if (buttonName == "ConsumableNameButton" || buttonName == "TypeButton" || buttonName == "GripButton")
                    {
                        consumables = isAscending ? filteredConsumables.OrderBy(w => sortProperty(w).ToString()).ToList() : filteredConsumables.OrderByDescending(w => sortProperty(w).ToString()).ToList();
                    }
                    else
                    {
                        consumables = isAscending ? filteredConsumables.OrderBy(sortProperty).ToList() : filteredConsumables.OrderByDescending(sortProperty).ToList();
                    }
                    ListBoxConsumables.ItemsSource = consumables;
                }
            }
        }

        private void ListBoxConsumables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is Consumable selectedConsumable)
            {
                GeneralStackPanel.Children.Clear();
                RequirementsStackPanel.Children.Clear();
                BonusesStackPanel.Children.Clear();

                //General Block
                SelectedItemTitle.Text = selectedConsumable.Name ?? string.Empty;

                if (selectedConsumable.Cooldown != null)
                {
                    string str;
                    if (selectedConsumable.Cooldown > 86400)
                    {
                        TimeSpan time = TimeSpan.FromSeconds(selectedConsumable.Cooldown.Value);
                        str = string.Format("{0} d {1} h {2} m", time.Days, time.Hours, time.Minutes);
                    }
                    else
                    {
                        TimeSpan time = TimeSpan.FromSeconds(selectedConsumable.Cooldown.Value);
                        int totalHours = (int)time.TotalHours;
                        int remainingMinutes = time.Minutes;
                        str = string.Format("{0} h {1} m", totalHours, remainingMinutes);
                    }

                    GeneralStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Cooldown", str));
                }

                if (selectedConsumable.Duration != null)
                {
                    string str;
                    if (selectedConsumable.Duration > 1440)
                    {
                        TimeSpan time = TimeSpan.FromMinutes(selectedConsumable.Duration.Value);
                        str = string.Format("{0} d {1} h {2} m", time.Days, time.Hours, time.Minutes);
                    }
                    else
                    {
                        TimeSpan time = TimeSpan.FromMinutes(selectedConsumable.Duration.Value);
                        int totalHours = (int)time.TotalHours;
                        int remainingMinutes = time.Minutes;
                        str = string.Format("{0} h {1} m", totalHours, remainingMinutes);
                    }
                    GeneralStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Duration", str));
                }
                if (selectedConsumable.ActiveRounds != null)
                {
                    GeneralStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Active Rounds", selectedConsumable.ActiveRounds.ToString() ?? string.Empty));
                }

                GeneralStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("For Live Battles", selectedConsumable.ForLiveBattle.ToString() ?? string.Empty));

                GeneralStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Soulbound", selectedConsumable.Soulbound.ToString() ?? string.Empty));

                //Requirements Block

                string requiredLevel;
                string minLevel = selectedConsumable.RequiredLevel.ToString() ?? string.Empty;
                string maxLevel = selectedConsumable.MaxLevel.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(maxLevel))
                {
                    requiredLevel = minLevel;
                }
                else
                {
                    requiredLevel = $"{minLevel} - {maxLevel}";
                }
                RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Level", requiredLevel));

                if (selectedConsumable.StrengthRequirementValue != null && selectedConsumable.StrengthRequirementValue != 0)
                {
                    RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Strength", selectedConsumable.StrengthRequirementValue.ToString() ?? string.Empty));
                }

                if (selectedConsumable.SkillRequirementValue != null && selectedConsumable.SkillRequirementValue != 0)
                {
                    RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Skill", selectedConsumable.SkillRequirementValue.ToString() ?? string.Empty));
                }

                if (!string.IsNullOrEmpty(selectedConsumable.RaceRestrictions))
                {
                    RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Race", selectedConsumable.RaceRestrictions));
                }

                if (selectedConsumable.RequiredRankingPoints != null)
                {
                    RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Ranking", selectedConsumable.RequiredRankingPoints.ToString() ?? string.Empty));
                }
                if (selectedConsumable.MinPopularity != null)
                {
                    RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Min Popularity", selectedConsumable.MinPopularity.ToString() ?? string.Empty));
                }
                if (selectedConsumable.MaxPopularity != null)
                {
                    RequirementsStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Max Popularity", selectedConsumable.MaxPopularity.ToString() ?? string.Empty));
                }

                //Bonuses Block
                if (selectedConsumable.Bonuses != null)
                {
                    if (selectedConsumable.CritRate != null && selectedConsumable.CritRate != 0)
                    {
                        string isPositive = selectedConsumable.CritRate > 0 ? "+" : "";
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Crit Rate", $"{isPositive}{selectedConsumable.CritRate}%"));
                    }
                    if (selectedConsumable.MaxCritRate != null && selectedConsumable.MaxCritRate != 0)
                    {
                        string isPositive = selectedConsumable.MaxCritRate > 0 ? "+" : "";
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Max Crit Rate", $"{isPositive}{selectedConsumable.CritRate}%"));
                    }
                    if (selectedConsumable.MinCritRate != null && selectedConsumable.MinCritRate != 0)
                    {
                        string isPositive = selectedConsumable.MinCritRate > 0 ? "+" : "";
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Min Crit Rate", $"{isPositive}{selectedConsumable.CritRate}%"));
                    }
                    if (selectedConsumable.IncreasedHitRate != null && selectedConsumable.IncreasedHitRate != 0)
                    {
                        string isPositive = selectedConsumable.IncreasedHitRate > 0 ? "+" : "";
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Hit Rate", $"{isPositive}{selectedConsumable.IncreasedHitRate}%"));
                    }
                    if (selectedConsumable.RestoreHp != null)
                    {
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Healing", $"{selectedConsumable.RestoreHp} ({selectedConsumable.RestoreHpChance}% chance to success.)"));
                    }
                    if (selectedConsumable.Damage != null)
                    {
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Offensive Damage", $"{selectedConsumable.Damage} ({selectedConsumable.DamageChance}% chance to success.)"));
                    }

                    if (selectedConsumable.ReducedHitRate != null && selectedConsumable.ReducedHitRateChance != null)
                    {
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Reduce Enemy Hitrate", $"{selectedConsumable.ReducedHitRate} ({selectedConsumable.ReducedHitRateChance}% chance to success.)"));
                    }
                    if (selectedConsumable.Xp != null && selectedConsumable.Xp != 0)
                    {
                        string isPositive = selectedConsumable.Xp > 0 ? "+" : "";
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("XP", isPositive + selectedConsumable.Xp.ToString() ?? string.Empty));
                    }
                    if (selectedConsumable.GivenCoins != null && selectedConsumable.GivenCoins != 0)
                    {
                        string isPositive = selectedConsumable.GivenCoins > 0 ? "+" : "";
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Coins", isPositive + selectedConsumable.GivenCoins.ToString() ?? string.Empty));
                    }
                    if (selectedConsumable.Popularity != null && selectedConsumable.Popularity != 0)
                    {
                        string isPositive = selectedConsumable.Popularity > 0 ? "+" : "";
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Popularity", isPositive + selectedConsumable.Popularity.ToString() ?? string.Empty));
                    }
                    if (selectedConsumable.Rounds != null && selectedConsumable.Rounds != 0)
                    {
                        string isPositive = selectedConsumable.Rounds > 0 ? "+" : "";
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Popularity", isPositive + selectedConsumable.Rounds.ToString() ?? string.Empty));
                    }
                    if (selectedConsumable.Undead == true)
                    {
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel("Become Undead", $"{selectedConsumable.Undead} ({selectedConsumable.UndeadChance} chance to success.)"));
                    }

                    foreach (var item in selectedConsumable.Bonuses)
                    {
                        BonusesStackPanel.Children.Add(InfoDockPanel.BuildInfoDockPanel(item.BonusableName!, $"{item.BonusValueDisplay}"));
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
            if (button.DataContext is Consumable item && DataContext is MainViewModel viewModel)
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

        private async Task<List<Consumable>> UpdateList()
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

            if (TypeSelection.SelectionBoxItem as string == "all")
            {
                typeQuery = string.Empty;
            }
            else
            {
                typeQuery = TypeSelection.SelectionBoxItem as string ?? string.Empty;
            }

            if (ToggleFavorites.Text == "F")
            {
                showOnlyFavorited = true;
            }

            if (!double.TryParse(SearchMinLevel.Text, out double minLevelQuery)) minLevelQuery = double.NaN;
            if (!double.TryParse(SearchMaxLevel.Text, out double maxLevelQuery)) maxLevelQuery = double.NaN;

            var filteredConsumables = await Dispatcher.UIThread.InvokeAsync(() =>
            viewModel.ConsumableList.Where(w =>
            (!showOnlyFavorited || w.IsFavorited) &&
            (w.Name?.ToLower().Contains(searchQuery) ?? false) &&
            (w.ForLiveBattle.ToString()?.ToLower().Contains(searchQuery) ?? false) &&
            (double.IsNaN(minLevelQuery) ||
                (SearchMinLevelButton.Content!.ToString() == ">" && w.RequiredLevel > minLevelQuery) ||
                (SearchMinLevelButton.Content!.ToString() == "<" && w.RequiredLevel < minLevelQuery) ||
                (SearchMinLevelButton.Content!.ToString() == "=" && w.RequiredLevel == minLevelQuery)) &&
            (double.IsNaN(maxLevelQuery) ||
                (SearchMaxLevelButton.Content!.ToString() == ">" && w.MaxLevel > maxLevelQuery) ||
                (SearchMaxLevelButton.Content!.ToString() == "<" && w.MaxLevel < maxLevelQuery) ||
                (SearchMaxLevelButton.Content!.ToString() == "=" && w.MaxLevel == maxLevelQuery)))
            .ToList()

);

            Func<Consumable, object> sortProperty = currentSortColumn switch
            {
                "TypeButton" => w => w.Weight ?? 0,
                "MinLevelButton" => w => w.RequiredLevel ?? 0,
                "MaxLevelButton" => w => w.MaxLevel ?? 0,
                "ConsumablesNameButton" => w => w.Name ?? string.Empty,

                _ => w => 0,
            };

            // Sort the filtered list
            if (sortProperty != null)
            {
                if (currentSortColumn == "WeaponShieldNameButton" || currentSortColumn == "TypeButton" || currentSortColumn == "GripButton")
                {
                    filteredConsumables = isAscending ? filteredConsumables.OrderBy(w => sortProperty(w).ToString()).ToList() : filteredConsumables.OrderByDescending(w => sortProperty(w).ToString()).ToList();
                }
                else
                {
                    filteredConsumables = isAscending ? filteredConsumables.OrderBy(sortProperty).ToList() : filteredConsumables.OrderByDescending(sortProperty).ToList();
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ListBoxConsumables.ItemsSource = filteredConsumables;
            });

            return filteredConsumables;
        }
    }
}