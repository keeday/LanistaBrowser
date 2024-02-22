using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using LanistaBrowserV1.Classes;
using LanistaBrowserV1.Functions;
using LanistaBrowserV1.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace LanistaBrowserV1.UserControls
{
    public partial class SearchWeapons : UserControl
    {
        private List<Weapon> weapons = new();

        private string? currentSortColumn;
        private bool isAscending;

        public SearchWeapons()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object Sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SearchWeapons UserControl_Loaded");

            UpdateTypeSelection();

            if (DataContext is not MainViewModel viewModel) { return; }

            weapons = viewModel.WeaponList;
        }

        private void UpdateTypeSelection()
        {
            if (DataContext is not MainViewModel viewModel || viewModel.ApiConfig == null || viewModel.ApiConfig.WeaponTypes == null) { return; }

            if (TypeSelectionBox.Items.Count == 0)
            {
                var allItem = new ComboBoxItem { Content = "all", HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                TypeSelectionBox.Items.Add(allItem);
                foreach (var item in viewModel.ApiConfig.WeaponTypes)
                {
                    var comboBoxItem = new ComboBoxItem { Content = item.Key.ToLower(), HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                    TypeSelectionBox.Items.Add(comboBoxItem);
                }

                TypeSelectionBox.SelectedIndex = 0;
            }
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.DataContext is Weapon item && DataContext is MainViewModel viewModel)
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

        private async void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            if (button == null) { return; }

            // Reset TextBoxes to blank
            SearchBox.Text = string.Empty;
            SearchWeight.Text = string.Empty;
            SearchMinDmg.Text = string.Empty;
            SearchMaxDmg.Text = string.Empty;
            SearchDmgRoof.Text = string.Empty;
            SearchMinLevel.Text = string.Empty;
            SearchMaxLevel.Text = string.Empty;
            SearchStrReq.Text = string.Empty;
            SearchSkillReq.Text = string.Empty;

            ToggleFavorites.Text = "A";

            // Reset Buttons to "="
            SearchWeightButton.Content = "=";
            SearchMinDmgButton.Content = "=";
            SearchMaxDmgButton.Content = "=";
            SearchDmgRoofButton.Content = "=";
            SearchMinLevelButton.Content = "=";
            SearchMaxLevelButton.Content = "=";
            SearchStrReqButton.Content = "=";
            SearchSkillReqButton.Content = "=";

            // Reset ComboBoxes to index 0
            TypeSelectionBox.SelectedIndex = 0;
            GripTypeSelection.SelectedIndex = 0;

            // Remove current sorting
            currentSortColumn = null;
            isAscending = true;

            // Update the list without sorting
            await UpdateList();

            if (ListBoxWeapons.ItemCount > 0)
            {
                ListBoxWeapons.SelectedItem = null;
                await Task.Delay(100);
                ListBoxWeapons.ScrollIntoView(ListBoxWeapons.Items[0]!);
            }

            DetailsChooseItemView.IsVisible = true;
            DetailsScrollViewer.IsVisible = false;
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
                Func<Weapon, object> sortProperty = buttonName switch
                {
                    "WeightButton" => w => w.Weight ?? 0,
                    "MinDmgButton" => w => w.BaseDamageMin ?? 0,
                    "MaxDmgButton" => w => w.BaseDamageMax ?? 0,
                    "DmdRoofButton" => w => w.DamageRoof ?? 0,
                    "MinLevelButton" => w => w.RequiredLevel ?? 0,
                    "MaxLevelButton" => w => w.MaxLevel ?? 0,
                    "StrReqButton" => w => w.StrengthRequirementValue ?? 0,
                    "SkillReqButton" => w => w.SkillRequirementValue ?? 0,
                    "WeaponShieldNameButton" => w => w.Name ?? string.Empty,
                    "TypeButton" => w => w.TypeName ?? string.Empty,
                    "GripButton" => w => w.GrabType ?? string.Empty,

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
                    var filteredWeapons = await UpdateList();

                    if (buttonName == "WeaponShieldNameButton" || buttonName == "TypeButton" || buttonName == "GripButton")
                    {
                        weapons = isAscending ? filteredWeapons.OrderBy(w => sortProperty(w).ToString()).ToList() : filteredWeapons.OrderByDescending(w => sortProperty(w).ToString()).ToList();
                    }
                    else
                    {
                        weapons = isAscending ? filteredWeapons.OrderBy(sortProperty).ToList() : filteredWeapons.OrderByDescending(sortProperty).ToList();
                    }
                    ListBoxWeapons.ItemsSource = weapons;
                }
            }
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

        private async void SearchBoxItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox || textBox.Text == null)
            {
                return;
            }

            await UpdateList();
        }

        private async void TypeSelectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox)
            {
                return;
            }

            await UpdateList();
        }

        private async Task<List<Weapon>> UpdateList()
        {
            string searchQuery;
            string typeQuery;
            string grabQuery;
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

            if (GripTypeSelection.SelectionBoxItem as string == "1h/2h")
            {
                grabQuery = string.Empty;
            }
            else
            {
                grabQuery = GripTypeSelection.SelectionBoxItem as string ?? string.Empty;
            }

            if (ToggleFavorites.Text == "F")
            {
                showOnlyFavorited = true;
            }

            if (!double.TryParse(SearchMinDmg.Text, out double minDmgQuery)) minDmgQuery = double.NaN;
            if (!double.TryParse(SearchMaxDmg.Text, out double maxDmgQuery)) maxDmgQuery = double.NaN;
            if (!double.TryParse(SearchDmgRoof.Text, out double dmgRoofQuery)) dmgRoofQuery = double.NaN;
            if (!double.TryParse(SearchMinLevel.Text, out double minLevelQuery)) minLevelQuery = double.NaN;
            if (!double.TryParse(SearchMaxLevel.Text, out double maxLevelQuery)) maxLevelQuery = double.NaN;
            if (!double.TryParse(SearchStrReq.Text, out double strReqQuery)) strReqQuery = double.NaN;
            if (!double.TryParse(SearchSkillReq.Text, out double skillReqQuery)) skillReqQuery = double.NaN;
            if (!double.TryParse(SearchWeight.Text, out double weightQuery)) weightQuery = double.NaN;

            var filteredWeapons = await Dispatcher.UIThread.InvokeAsync(() =>
            viewModel.WeaponList.Where(w =>
            (!showOnlyFavorited || w.IsFavorited) &&
            (w.Name?.ToLower().Contains(searchQuery) ?? false) &&
            (w.TypeName?.ToLower().Contains(typeQuery) ?? false) &&
            (w.GrabType?.ToLower().Contains(grabQuery) ?? false) &&
            (double.IsNaN(weightQuery) ||
                (SearchWeightButton.Content!.ToString() == ">" && w.Weight > weightQuery) ||
                (SearchWeightButton.Content!.ToString() == "<" && w.Weight < weightQuery) ||
                (SearchWeightButton.Content!.ToString() == "=" && w.Weight == weightQuery)) &&
            (double.IsNaN(minDmgQuery) ||
                (SearchMinDmgButton.Content!.ToString() == ">" && w.BaseDamageMin > minDmgQuery) ||
                (SearchMinDmgButton.Content!.ToString() == "<" && w.BaseDamageMin < minDmgQuery) ||
                (SearchMinDmgButton.Content!.ToString() == "=" && w.BaseDamageMin == minDmgQuery)) &&
            (double.IsNaN(maxDmgQuery) ||
                (SearchMaxDmgButton.Content!.ToString() == ">" && w.BaseDamageMax > maxDmgQuery) ||
                (SearchMaxDmgButton.Content!.ToString() == "<" && w.BaseDamageMax < maxDmgQuery) ||
                (SearchMaxDmgButton.Content!.ToString() == "=" && w.BaseDamageMax == maxDmgQuery)) &&
            (double.IsNaN(dmgRoofQuery) ||
                (SearchDmgRoofButton.Content!.ToString() == ">" && w.DamageRoof > dmgRoofQuery) ||
                (SearchDmgRoofButton.Content!.ToString() == "<" && w.DamageRoof < dmgRoofQuery) ||
                (SearchDmgRoofButton.Content!.ToString() == "=" && w.DamageRoof == dmgRoofQuery)) &&
            (double.IsNaN(minLevelQuery) ||
                (SearchMinLevelButton.Content!.ToString() == ">" && w.RequiredLevel > minLevelQuery) ||
                (SearchMinLevelButton.Content!.ToString() == "<" && w.RequiredLevel < minLevelQuery) ||
                (SearchMinLevelButton.Content!.ToString() == "=" && w.RequiredLevel == minLevelQuery)) &&
            (double.IsNaN(maxLevelQuery) ||
                (SearchMaxLevelButton.Content!.ToString() == ">" && w.MaxLevel > maxLevelQuery) ||
                (SearchMaxLevelButton.Content!.ToString() == "<" && w.MaxLevel < maxLevelQuery) ||
                (SearchMaxLevelButton.Content!.ToString() == "=" && w.MaxLevel == maxLevelQuery)) &&
            (double.IsNaN(strReqQuery) ||
                (SearchStrReqButton.Content!.ToString() == ">" && w.StrengthRequirementValue > strReqQuery) ||
                (SearchStrReqButton.Content!.ToString() == "<" && w.StrengthRequirementValue < strReqQuery) ||
                (SearchStrReqButton.Content!.ToString() == "=" && w.StrengthRequirementValue == strReqQuery)) &&
            (double.IsNaN(skillReqQuery) ||
                (SearchSkillReqButton.Content!.ToString() == ">" && w.SkillRequirementValue > skillReqQuery) ||
                (SearchSkillReqButton.Content!.ToString() == "<" && w.SkillRequirementValue < skillReqQuery) ||
                (SearchSkillReqButton.Content!.ToString() == "=" && w.SkillRequirementValue == skillReqQuery))).ToList()

);

            Func<Weapon, object> sortProperty = currentSortColumn switch
            {
                "WeightButton" => w => w.Weight ?? 0,
                "MinDmgButton" => w => w.BaseDamageMin ?? 0,
                "MaxDmgButton" => w => w.BaseDamageMax ?? 0,
                "DmgRoofButton" => w => w.DamageRoof ?? 0,
                "MinLevelButton" => w => w.RequiredLevel ?? 0,
                "MaxLevelButton" => w => w.MaxLevel ?? 0,
                "StrReqButton" => w => w.StrengthRequirementValue ?? 0,
                "SkillReqButton" => w => w.SkillRequirementValue ?? 0,
                "WeaponShieldNameButton" => w => w.Name ?? string.Empty,
                "TypeButton" => w => w.TypeName ?? string.Empty,
                "GripButton" => w => w.GrabType ?? string.Empty,

                _ => w => 0,
            };

            // Sort the filtered list
            if (sortProperty != null)
            {
                if (currentSortColumn == "WeaponShieldNameButton" || currentSortColumn == "TypeButton" || currentSortColumn == "GripButton")
                {
                    filteredWeapons = isAscending ? filteredWeapons.OrderBy(w => sortProperty(w).ToString()).ToList() : filteredWeapons.OrderByDescending(w => sortProperty(w).ToString()).ToList();
                }
                else
                {
                    filteredWeapons = isAscending ? filteredWeapons.OrderBy(sortProperty).ToList() : filteredWeapons.OrderByDescending(sortProperty).ToList();
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ListBoxWeapons.ItemsSource = filteredWeapons;
            });

            return filteredWeapons;
        }

        private void ListBoxWeapons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("DataGridWeapons_SelectionChanged");
            if (sender is ListBox listBox && listBox.SelectedItem is Weapon selectedWeapon)
            {
                string critInfo = selectedWeapon.CritDamage?.Replace("till", "to") ?? "n/a";

                // ItemMagicTypesBlock.Text = selectedWeapon.MagicTypes;
                // ItemMaxSpellsBlock.Text = selectedWeapon.MaxSpells.ToString();
                //ItemReqAgeBlock.Text = selectedWeapon.minAge

                ItemNameBlock.Text = selectedWeapon.Name;
                ItemTypeBlock.Text = $"{selectedWeapon.TypeName} - {selectedWeapon.GrabType}";
                ItemWeightBlock.Text = selectedWeapon.Weight.ToString();
                ItemDurabilityBlock.Text = selectedWeapon.Durability.ToString();
                ItemStrengthBlock.Text = selectedWeapon.StrengthRequirementValue.ToString();
                ItemSkillBlock.Text = selectedWeapon.SkillRequirementValue.ToString();
                ItemReqLegendBlock.Text = selectedWeapon.RequiresLegend.ToString();
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

                if (selectedWeapon.RaceRestrictions == null)
                {
                    ItemReqRaceBlock.Text = "All";
                }
                else
                {
                    ItemReqRaceBlock.Text = selectedWeapon.RaceRestrictions;
                }

                if (selectedWeapon.Bonuses == null || selectedWeapon.Bonuses.Count == 0)
                {
                    ItemBonusesBlock.Text = "None";
                }
                else
                {
                    ItemBonusesBlock.Text = string.Empty;

                    foreach (var item in selectedWeapon.Bonuses)
                    {
                        if (item.Additive != null)
                        {
                            ItemBonusesBlock.Text += $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Type!.ToLowerInvariant())}: {item.Additive}\n";
                        }
                        else if (item.Multiplier != null)
                        {
                            double multiplier = (item.Multiplier.Value - 1) * 100;
                            multiplier = Math.Round(multiplier);
                            ItemBonusesBlock.Text += $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Type!.ToLowerInvariant())}: {multiplier}%\n";
                        }
                    }
                }

                if (selectedWeapon.MaxLevel == null)
                {
                    ItemLevelBlock.Text = selectedWeapon.RequiredLevel.ToString();
                }
                else
                {
                    ItemLevelBlock.Text = selectedWeapon.RequiredLevel.ToString() + " - " + selectedWeapon.MaxLevel.ToString();
                }

                if (selectedWeapon.IsShield == true)
                {
                    ItemDualWieldBlock.Text = "n/a";
                    ItemDamageBlock.Text = "n/a";
                    ItemCritBlock.Text = "n/a";
                    ItemAbsorptionBlock.Text = selectedWeapon.Absorption.ToString();
                    ItemActionsBlock.Text = "2";
                }
                else
                {
                    ItemDamageBlock.Text = $"{selectedWeapon.BaseDamageMin} - {selectedWeapon.BaseDamageMax} (max {selectedWeapon.DamageRoof})";
                    ItemCritBlock.Text = critInfo;
                    ItemAbsorptionBlock.Text = "n/a";
                    ItemActionsBlock.Text = selectedWeapon.Actions.ToString();

                    if (selectedWeapon.IsTwoHanded == true)
                    {
                        ItemDualWieldBlock.Text = "n/a";
                    }
                    else
                    {
                        ItemDualWieldBlock.Text = selectedWeapon.CanDualWield.ToString();
                    }
                }

                DetailsChooseItemView.IsVisible = false;
                DetailsScrollViewer.IsVisible = true;
            }
        }

        private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }

        private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
        }
    }
}