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
                Func<Consumable, object> sortProperty = buttonName switch
                {
                    "WeightButton" => w => w.Weight ?? 0,
                    "MinLevelButton" => w => w.RequiredLevel ?? 0,
                    "MaxLevelButton" => w => w.MaxLevel ?? 0,
                    "WeaponShieldNameButton" => w => w.Name ?? string.Empty,

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
            Debug.WriteLine("DataGridWeapons_SelectionChanged");
            if (sender is ListBox listBox && listBox.SelectedItem is Consumable selectedWeapon)
            {
                //    string critInfo = selectedWeapon.CritDamage?.Replace("till", "to") ?? "n/a";

                // ItemMagicTypesBlock.Text = selectedWeapon.MagicTypes;
                // ItemMaxSpellsBlock.Text = selectedWeapon.MaxSpells.ToString();
                //ItemReqAgeBlock.Text = selectedWeapon.minAge

                ItemNameBlock.Text = selectedWeapon.Name;
                //   ItemTypeBlock.Text = $"{selectedWeapon.TypeName} - {selectedWeapon.GrabType}";
                ItemWeightBlock.Text = selectedWeapon.Weight.ToString();
                //    ItemDurabilityBlock.Text = selectedWeapon.Durability.ToString();
                //   ItemStrengthBlock.Text = selectedWeapon.StrengthRequirementValue.ToString();
                //   ItemSkillBlock.Text = selectedWeapon.SkillRequirementValue.ToString();
                ItemSoulboundBlock.Text = selectedWeapon.Soulbound.ToString();

                if (selectedWeapon.RequiredRankingPoints == null)
                {
                    ItemReqRankBlock.Text = "n/a";
                }
                else
                {
                    ItemReqRankBlock.Text = selectedWeapon.RequiredRankingPoints.ToString();
                }

                if (selectedWeapon.MinPopularity == null && selectedWeapon.MaxPopularity == null)
                {
                    ItemPopularityBlock.Text = "n/a";
                }
                else if (selectedWeapon.MinPopularity == null && selectedWeapon.MaxPopularity != null)
                {
                    ItemPopularityBlock.Text = selectedWeapon.MaxPopularity.ToString();
                }
                else if (selectedWeapon.MinPopularity != null && selectedWeapon.MaxPopularity == null)
                {
                    ItemPopularityBlock.Text = selectedWeapon.MinPopularity.ToString();
                }
                else
                {
                    ItemPopularityBlock.Text = selectedWeapon.MinPopularity.ToString() + " - " + selectedWeapon.MaxPopularity.ToString();
                }

                //if (selectedWeapon.RaceRestrictions == null)
                //{
                //    ItemReqRaceBlock.Text = "All";
                //}
                //else
                //{
                //    ItemReqRaceBlock.Text = selectedWeapon.RaceRestrictions;
                //}

                if (selectedWeapon.Bonuses == null || selectedWeapon.Bonuses.Count == 0)
                {
                    ItemBonusesBlock.Text = "None";
                }
                else
                {
                    ItemBonusesBlock.Text = string.Empty;
                }

                if (selectedWeapon.MaxLevel == null)
                {
                    ItemLevelBlock.Text = selectedWeapon.RequiredLevel.ToString();
                }
                else
                {
                    ItemLevelBlock.Text = selectedWeapon.RequiredLevel.ToString() + " - " + selectedWeapon.MaxLevel.ToString();
                }

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

            //if (TypeSelectionBox.SelectionBoxItem as string == "all")
            //{
            //    typeQuery = string.Empty;
            //}
            //else
            //{
            //    typeQuery = TypeSelectionBox.SelectionBoxItem as string ?? string.Empty;
            //}

            if (ToggleFavorites.Text == "F")
            {
                showOnlyFavorited = true;
            }

            //if (!double.TryParse(SearchMinDmg.Text, out double minDmgQuery)) minDmgQuery = double.NaN;
            //if (!double.TryParse(SearchMaxDmg.Text, out double maxDmgQuery)) maxDmgQuery = double.NaN;
            //if (!double.TryParse(SearchDmgRoof.Text, out double dmgRoofQuery)) dmgRoofQuery = double.NaN;
            //if (!double.TryParse(SearchMinLevel.Text, out double minLevelQuery)) minLevelQuery = double.NaN;
            //if (!double.TryParse(SearchMaxLevel.Text, out double maxLevelQuery)) maxLevelQuery = double.NaN;
            //if (!double.TryParse(SearchStrReq.Text, out double strReqQuery)) strReqQuery = double.NaN;
            //if (!double.TryParse(SearchSkillReq.Text, out double skillReqQuery)) skillReqQuery = double.NaN;
            //if (!double.TryParse(SearchWeight.Text, out double weightQuery)) weightQuery = double.NaN;

            var filteredConsumables = await Dispatcher.UIThread.InvokeAsync(() =>
            viewModel.ConsumableList.Where(w =>
            (!showOnlyFavorited || w.IsFavorited) &&
            (w.Name?.ToLower().Contains(searchQuery) ?? false)) // &&
            //(double.IsNaN(weightQuery) ||
            //    (SearchWeightButton.Content!.ToString() == ">" && w.Weight > weightQuery) ||
            //    (SearchWeightButton.Content!.ToString() == "<" && w.Weight < weightQuery) ||
            //    (SearchWeightButton.Content!.ToString() == "=" && w.Weight == weightQuery)) &&
            //(double.IsNaN(minDmgQuery) ||
            //    (SearchMinDmgButton.Content!.ToString() == ">" && w.BaseDamageMin > minDmgQuery) ||
            //    (SearchMinDmgButton.Content!.ToString() == "<" && w.BaseDamageMin < minDmgQuery) ||
            //    (SearchMinDmgButton.Content!.ToString() == "=" && w.BaseDamageMin == minDmgQuery)) &&
            //(double.IsNaN(maxDmgQuery) ||
            //    (SearchMaxDmgButton.Content!.ToString() == ">" && w.BaseDamageMax > maxDmgQuery) ||
            //    (SearchMaxDmgButton.Content!.ToString() == "<" && w.BaseDamageMax < maxDmgQuery) ||
            //    (SearchMaxDmgButton.Content!.ToString() == "=" && w.BaseDamageMax == maxDmgQuery)) &&
            //(double.IsNaN(dmgRoofQuery) ||
            //    (SearchDmgRoofButton.Content!.ToString() == ">" && w.DamageRoof > dmgRoofQuery) ||
            //    (SearchDmgRoofButton.Content!.ToString() == "<" && w.DamageRoof < dmgRoofQuery) ||
            //    (SearchDmgRoofButton.Content!.ToString() == "=" && w.DamageRoof == dmgRoofQuery)) &&
            //(double.IsNaN(minLevelQuery) ||
            //    (SearchMinLevelButton.Content!.ToString() == ">" && w.RequiredLevel > minLevelQuery) ||
            //    (SearchMinLevelButton.Content!.ToString() == "<" && w.RequiredLevel < minLevelQuery) ||
            //    (SearchMinLevelButton.Content!.ToString() == "=" && w.RequiredLevel == minLevelQuery)) &&
            //(double.IsNaN(maxLevelQuery) ||
            //    (SearchMaxLevelButton.Content!.ToString() == ">" && w.MaxLevel > maxLevelQuery) ||
            //    (SearchMaxLevelButton.Content!.ToString() == "<" && w.MaxLevel < maxLevelQuery) ||
            //    (SearchMaxLevelButton.Content!.ToString() == "=" && w.MaxLevel == maxLevelQuery)) &&
            //(double.IsNaN(strReqQuery) ||
            //    (SearchStrReqButton.Content!.ToString() == ">" && w.StrengthRequirementValue > strReqQuery) ||
            //    (SearchStrReqButton.Content!.ToString() == "<" && w.StrengthRequirementValue < strReqQuery) ||
            //    (SearchStrReqButton.Content!.ToString() == "=" && w.StrengthRequirementValue == strReqQuery)) &&
            //(double.IsNaN(skillReqQuery) ||
            //    (SearchSkillReqButton.Content!.ToString() == ">" && w.SkillRequirementValue > skillReqQuery) ||
            //    (SearchSkillReqButton.Content!.ToString() == "<" && w.SkillRequirementValue < skillReqQuery) ||
            //    (SearchSkillReqButton.Content!.ToString() == "=" && w.SkillRequirementValue == skillReqQuery)))
            .ToList()

);

            Func<Consumable, object> sortProperty = currentSortColumn switch
            {
                "WeightButton" => w => w.Weight ?? 0,
                "MinLevelButton" => w => w.RequiredLevel ?? 0,
                "MaxLevelButton" => w => w.MaxLevel ?? 0,
                "WeaponShieldNameButton" => w => w.Name ?? string.Empty,

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