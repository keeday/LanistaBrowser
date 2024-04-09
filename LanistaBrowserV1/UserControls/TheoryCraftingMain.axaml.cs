using Avalonia.Controls;
using Avalonia.Interactivity;
using LanistaBrowserV1.Classes;
using LanistaBrowserV1.Functions;
using LanistaBrowserV1.ViewModels;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace LanistaBrowserV1.UserControls
{
    public partial class TheoryCraftingMain : UserControl
    {
        private int selectedTacticId = 0;
        private string selectedTacticName = string.Empty;
        private int selectedTacticLevel = 0;
        private int weaponTypeSelected = 0;
        private int selectedRace = 0;

        private Weapon? selectedWeapon = new();
        private Armor? selectedArmor = new();
        private string weaponTypeNameSelected = string.Empty;
        private string equippingSlot = string.Empty;

        private string selectedRaceName = string.Empty;

        private List<string> equipmentSlots = ["mainhand", "shieldhand", "head", "shoulders", "chest", "hands", "legs", "feet", "back", "neck", "finger", "amulet", "bracelet", "trinket"];

        private Bonuses? loadedRaceBonuses = new();

        private List<Weapon> loadedWeapons = [];
        private List<Armor> loadedArmor = [];

        private bool isEquippingItems = true;

        private string selectedItemCategory = string.Empty;

        public TheoryCraftingMain()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object Sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SearchArmor UserControl_Loaded");

            if (DataContext is not MainViewModel viewModel) { return; }
        }

        private void ListBoxTactics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) { return; }

            if (sender is ListBox listBox && listBox.SelectedItem is Tactic selectedTactic)
            {
                LevelListListBox.ItemsSource = selectedTactic.Levels;

                AddLevelButton.IsVisible = true;
                selectedTacticId = selectedTactic.Id;
                selectedTacticName = selectedTactic.TacticName;
                weaponTypeSelected = selectedTactic.WeaponSkillID;
                weaponTypeNameSelected = viewModel.ApiConfig.WeaponTypes!.First(w => w.Value == selectedTactic.WeaponSkillID).Key.ToLower();
                selectedRace = selectedTactic.RaceID;

                selectedRaceName = viewModel.ApiConfig.Races!.First(r => r.Type == selectedTactic.RaceID).Name!.ToLower();

                LevelListPanel.IsVisible = true;
                DeleteTacticButton.IsVisible = true;
                ContentStackPanel.IsVisible = false;
                SummaryStackPanel.IsVisible = false;

                loadedRaceBonuses = viewModel.ApiConfig.Races!.FirstOrDefault(r => r.Type == selectedTactic.RaceID)!.Bonuses!;

                StaminaPercentage.Text = ReturnPercentageValue(0, loadedRaceBonuses);
                StrengthPercentage.Text = ReturnPercentageValue(1, loadedRaceBonuses);
                EndurancePercentage.Text = ReturnPercentageValue(3, loadedRaceBonuses);
                InitiativePercentage.Text = ReturnPercentageValue(4, loadedRaceBonuses);
                DodgePercentage.Text = ReturnPercentageValue(7, loadedRaceBonuses);
                WeaponPercentage.Text = ReturnPercentageWeaponValue(weaponTypeSelected, loadedRaceBonuses);
                ShieldPercentage.Text = ReturnPercentageWeaponValue(6, loadedRaceBonuses);
            }
            else
            {
                AddLevelButton.IsVisible = false;
                selectedTacticName = string.Empty;
                selectedTacticId = 0;
                weaponTypeSelected = 0;
                selectedRace = 0;
            }
        }

        private string ReturnPercentageValue(int type, Bonuses loadedBonuses)
        {
            if (loadedBonuses.Stats == null) { return "Err"; }

            return ((loadedBonuses.Stats.First(r => r.Type == type).Value - 1) < 0 ? "" : "+") + (loadedBonuses.Stats.First(r => r.Type == type).Value - 1).ToString("P0");
        }

        private string ReturnPercentageWeaponValue(int type, Bonuses loadedBonuses)
        {
            if (loadedBonuses.WeaponSkills == null) { return "Err"; }

            return ((loadedBonuses.WeaponSkills.First(r => r.Type == type).Value - 1) < 0 ? "" : "+") + (loadedBonuses.WeaponSkills.First(r => r.Type == type).Value - 1).ToString("P0");
        }

        private async void ListBoxSelectLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
             {
                 if (DataContext is not MainViewModel viewModel) { return; }

                 if (sender is ListBox listBox && listBox.SelectedItem is TacticsLevels selectedLevel)
                 {
                     selectedTacticLevel = selectedLevel.Level;

                     List<TacticPlacedStat> placedStats = viewModel.Tactics.First(t => t.Id == selectedTacticId).PlacedStats.Where(s => s.Level == selectedTacticLevel).ToList();
                     List<TacticPlacedStat> totalPlacedStats = viewModel.Tactics.First(t => t.Id == selectedTacticId).PlacedStats.Where(s => s.Level < selectedTacticLevel).ToList();

                     StaminaTextBox.Text = string.Empty;
                     StrengthTextBox.Text = string.Empty;
                     EnduranceTextBox.Text = string.Empty;
                     InitiativeTextBox.Text = string.Empty;
                     DodgeTextBox.Text = string.Empty;
                     WeaponSkillTextBox.Text = string.Empty;
                     ShieldSkillTextBox.Text = string.Empty;

                     StaminaTotalTextBlock.Text = "0";
                     StrengthTotalTextBlock.Text = "0";
                     EnduranceTotalTextBlock.Text = "0";
                     InitiativeTotalTextBlock.Text = "0";
                     DodgeTotalTextBlock.Text = "0";
                     WeaponSkillTotalTextBlock.Text = "0";
                     ShieldSkillTotalTextBlock.Text = "0";

                     StaminaTotalTextBlock.Text = totalPlacedStats.Where(s => s.StatId == 0 && s.StatType == "stats").Sum(s => s.StatValue).ToString();
                     StrengthTotalTextBlock.Text = totalPlacedStats.Where(s => s.StatId == 1 && s.StatType == "stats").Sum(s => s.StatValue).ToString();
                     EnduranceTotalTextBlock.Text = totalPlacedStats.Where(s => s.StatId == 3 && s.StatType == "stats").Sum(s => s.StatValue).ToString();
                     InitiativeTotalTextBlock.Text = totalPlacedStats.Where(s => s.StatId == 4 && s.StatType == "stats").Sum(s => s.StatValue).ToString();
                     DodgeTotalTextBlock.Text = totalPlacedStats.Where(s => s.StatId == 7 && s.StatType == "stats").Sum(s => s.StatValue).ToString();
                     WeaponSkillTotalTextBlock.Text = totalPlacedStats.Where(s => s.StatId != 6 && s.StatType == "weapon").Sum(s => s.StatValue).ToString();
                     ShieldSkillTotalTextBlock.Text = totalPlacedStats.Where(s => s.StatId == 6 && s.StatType == "weapon").Sum(s => s.StatValue).ToString();

                     foreach (var stat in placedStats)
                     {
                         if (stat.StatType == "stats")
                         {
                             if (stat.StatId == 0) { StaminaTextBox.Text = stat.StatValue.ToString(); }
                             if (stat.StatId == 1) { StrengthTextBox.Text = stat.StatValue.ToString(); }
                             if (stat.StatId == 3) { EnduranceTextBox.Text = stat.StatValue.ToString(); }
                             if (stat.StatId == 4) { InitiativeTextBox.Text = stat.StatValue.ToString(); }
                             if (stat.StatId == 7) { DodgeTextBox.Text = stat.StatValue.ToString(); }
                         }

                         if (stat.StatType == "weapon")
                         {
                             if (stat.StatId == 6)
                             {
                                 ShieldSkillTextBox.Text = stat.StatValue.ToString();
                             }
                             else
                             {
                                 WeaponSkillTextBox.Text = stat.StatValue.ToString();
                             }
                         }
                     }

                     LevelContentTitle.Text = $"Level {selectedTacticLevel} | {selectedRaceName} | {weaponTypeNameSelected}";
                     ContentStackPanel.IsVisible = true;
                     SummaryStackPanel.IsVisible = true;
                 }
             });
            await UpdateLoadedArmorAndWeapons();
            await UpdateButtons();
            await CalculateStats();
        }

        private async Task UpdateLoadedArmorAndWeapons()
        {
            if (DataContext is not MainViewModel viewModel) { return; }

            await Task.Run(() =>
             {
                 loadedArmor.Clear();
                 loadedWeapons.Clear();

                 var equippedTacticItems = viewModel.Tactics.First(t => t.Id == selectedTacticId).EquippedItems.Where(e => e.TacticId == selectedTacticId).OrderByDescending(r => r.EquippedLevel);

                 foreach (var item in equipmentSlots)
                 {
                     var equippedItem = equippedTacticItems.Where(e => e.equippedSlot == item && e.EquippedLevel <= selectedTacticLevel).FirstOrDefault();

                     if (equippedItem != null && equippedItem.EquippedType == "armor")
                     {
                         loadedArmor.Add(viewModel.ArmorList.First(a => a.Id == equippedItem.EquippedId));
                     }
                     else if (equippedItem != null && equippedItem.equippedSlot == "mainhand")
                     {
                         loadedWeapons.Add(viewModel.WeaponList.First(w => w.Id == equippedItem.EquippedId));
                     }
                     else if (equippedItem != null && equippedItem.equippedSlot == "shieldhand")
                     {
                         TacticEquippedItem? mainHandWeapon = equippedTacticItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "mainhand");

                         bool mainHandIsTwoHanded = false;

                         if (mainHandWeapon != null && mainHandWeapon.EquippedType == "2h")
                         {
                             mainHandIsTwoHanded = true;
                         }

                         if (!mainHandIsTwoHanded)
                         {
                             loadedWeapons.Add(viewModel.WeaponList.First(w => w.Id == equippedItem.EquippedId));
                         }
                     }
                 }
             });
        }

        private async void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
             {
                 if (sender is TextBox textBox && textBox.Text != null)
                 {
                     textBox.Text = System.Text.RegularExpressions.Regex.Replace(textBox.Text, "[^0-9]", "");
                     SaveStatsButton.IsVisible = true;
                     UpdateStatsToSpend();
                 }
             });

            await CalculateStats();
        }

        private void UpdateStatsToSpend()
        {
            int maxPointsToSpend = 20;
            if (selectedTacticLevel == 1)
            {
                maxPointsToSpend = 150;
            }

            int.TryParse(StaminaTextBox.Text, out int staminaPoints);
            int.TryParse(StrengthTextBox.Text, out int strengthPoints);
            int.TryParse(EnduranceTextBox.Text, out int endurancePoints);
            int.TryParse(InitiativeTextBox.Text, out int initiativePoints);
            int.TryParse(DodgeTextBox.Text, out int dodgePoints);
            int.TryParse(WeaponSkillTextBox.Text, out int weaponSkillPoints);
            int.TryParse(ShieldSkillTextBox.Text, out int shieldSkillPoints);

            int pointsSpent = staminaPoints + strengthPoints + endurancePoints + initiativePoints + dodgePoints + weaponSkillPoints + shieldSkillPoints;

            PointsToSpendBlock.Text = $"{pointsSpent} / {maxPointsToSpend} Points Spent";

            if (pointsSpent > maxPointsToSpend)
            {
                PointsToSpendBlock.Foreground = Avalonia.Media.Brushes.Red;
            }
            else if (pointsSpent == maxPointsToSpend)
            {
                PointsToSpendBlock.Foreground = Avalonia.Media.Brushes.LimeGreen;
            }
            else
            {
                PointsToSpendBlock.Foreground = Avalonia.Media.Brushes.Yellow;
            }
        }

        private async void SaveStatsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) { return; }

            int maxPointsToSpend = 20;
            if (selectedTacticLevel == 1)
            {
                maxPointsToSpend = 150;
            }

            int.TryParse(StaminaTextBox.Text, out int staminaPoints);
            int.TryParse(StrengthTextBox.Text, out int strengthPoints);
            int.TryParse(EnduranceTextBox.Text, out int endurancePoints);
            int.TryParse(InitiativeTextBox.Text, out int initiativePoints);
            int.TryParse(DodgeTextBox.Text, out int dodgePoints);
            int.TryParse(WeaponSkillTextBox.Text, out int weaponSkillPoints);
            int.TryParse(ShieldSkillTextBox.Text, out int shieldSkillPoints);

            int pointsSpent = staminaPoints + strengthPoints + endurancePoints + initiativePoints + dodgePoints + weaponSkillPoints + shieldSkillPoints;

            if (pointsSpent > maxPointsToSpend)
            {
                var box = MessageBoxManager
            .GetMessageBoxStandard("Warning", "You have spent more points than allowed, are you sure you want to save?",
                             ButtonEnum.YesNo);

                var result = await box.ShowAsync();

                if (result.Equals(ButtonResult.No))
                {
                    return;
                }
            }

            SqliteHandler.UpdatePlacedStats(selectedTacticId, selectedTacticLevel, "stats", 0, staminaPoints);
            SqliteHandler.UpdatePlacedStats(selectedTacticId, selectedTacticLevel, "stats", 1, strengthPoints);
            SqliteHandler.UpdatePlacedStats(selectedTacticId, selectedTacticLevel, "stats", 3, endurancePoints);
            SqliteHandler.UpdatePlacedStats(selectedTacticId, selectedTacticLevel, "stats", 4, initiativePoints);
            SqliteHandler.UpdatePlacedStats(selectedTacticId, selectedTacticLevel, "stats", 7, dodgePoints);
            SqliteHandler.UpdatePlacedStats(selectedTacticId, selectedTacticLevel, "weapon", weaponTypeSelected, weaponSkillPoints);
            SqliteHandler.UpdatePlacedStats(selectedTacticId, selectedTacticLevel, "weapon", 6, shieldSkillPoints);

            List<string> placedStastsString = [];

            if (staminaPoints > 0) { placedStastsString.Add("Stamina +" + staminaPoints.ToString()); }
            if (strengthPoints > 0) { placedStastsString.Add("Strength +" + strengthPoints.ToString()); }
            if (endurancePoints > 0) { placedStastsString.Add("Endurance +" + endurancePoints.ToString()); }
            if (initiativePoints > 0) { placedStastsString.Add("Initiative +" + initiativePoints.ToString()); }
            if (dodgePoints > 0) { placedStastsString.Add("Dodge +" + dodgePoints.ToString()); }
            if (weaponSkillPoints > 0) { placedStastsString.Add("Weapon Skill +" + weaponSkillPoints.ToString()); }
            if (shieldSkillPoints > 0) { placedStastsString.Add("Shield Skill +" + shieldSkillPoints.ToString()); }

            SqliteHandler.UpdateLevelStatsInfo(selectedTacticId, selectedTacticLevel, string.Join(", ", placedStastsString));

            var tactic = viewModel.Tactics.First(t => t.Id == selectedTacticId);
            var levelToUpdate = tactic.Levels.FirstOrDefault(l => l.TacticId == selectedTacticId && l.Level == selectedTacticLevel);
            if (levelToUpdate != null)
            {
                levelToUpdate.PlacedStatsString = string.Join(", ", placedStastsString);
            }
            var statsToRemove = tactic.PlacedStats.Where(t => t.TacticId == selectedTacticId && t.Level == selectedTacticLevel).ToList();
            foreach (var stat in statsToRemove)
            {
                tactic.PlacedStats.Remove(stat);
            }
            if (staminaPoints > 0) { tactic.PlacedStats.Add(new TacticPlacedStat { TacticId = selectedTacticId, Level = selectedTacticLevel, StatType = "stats", StatId = 0, StatValue = staminaPoints }); }
            if (strengthPoints > 0) { tactic.PlacedStats.Add(new TacticPlacedStat { TacticId = selectedTacticId, Level = selectedTacticLevel, StatType = "stats", StatId = 1, StatValue = strengthPoints }); }
            if (endurancePoints > 0) { tactic.PlacedStats.Add(new TacticPlacedStat { TacticId = selectedTacticId, Level = selectedTacticLevel, StatType = "stats", StatId = 3, StatValue = endurancePoints }); }
            if (initiativePoints > 0) { tactic.PlacedStats.Add(new TacticPlacedStat { TacticId = selectedTacticId, Level = selectedTacticLevel, StatType = "stats", StatId = 4, StatValue = initiativePoints }); }
            if (dodgePoints > 0) { tactic.PlacedStats.Add(new TacticPlacedStat { TacticId = selectedTacticId, Level = selectedTacticLevel, StatType = "stats", StatId = 7, StatValue = dodgePoints }); }
            if (weaponSkillPoints > 0) { tactic.PlacedStats.Add(new TacticPlacedStat { TacticId = selectedTacticId, Level = selectedTacticLevel, StatType = "weapon", StatId = weaponTypeSelected, StatValue = weaponSkillPoints }); }
            if (shieldSkillPoints > 0) { tactic.PlacedStats.Add(new TacticPlacedStat { TacticId = selectedTacticId, Level = selectedTacticLevel, StatType = "weapon", StatId = 6, StatValue = shieldSkillPoints }); }
        }

        private void AddLevelButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) { return; }

            var tactic = viewModel.Tactics.First(t => t.Id == selectedTacticId);
            int maxLevel = tactic.Levels.Any() ? tactic.Levels.Max(l => l.Level) : 0;

            SqliteHandler.AddNewLevel(selectedTacticId, maxLevel + 1);

            string levelAsName = "Level " + (maxLevel + 1).ToString();
            tactic.Levels.Add(new TacticsLevels { TacticId = selectedTacticId, Level = maxLevel + 1, LevelAsString = levelAsName, PlacedStatsString = "" });
        }

        private void LoadComboBoxes()
        {
            if (DataContext is not MainViewModel viewModel) { return; }
            if (viewModel.ApiConfig == null) { return; }
            if (viewModel.ApiConfig.Races == null) { return; }
            if (viewModel.ApiConfig.WeaponTypes == null) { return; }

            CreateTacticRaceBox.Items.Clear();
            CreateTacticWeaponBox.Items.Clear();
            CreateTacticNameBox.Text = string.Empty;

            foreach (var race in viewModel.ApiConfig.Races)
            {
                if (race.Name != null)
                {
                    var comboBoxItem = new ComboBoxItem { Content = race.Name.ToLower(), HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                    CreateTacticRaceBox.Items.Add(comboBoxItem);
                }
            }

            foreach (var weaponType in viewModel.ApiConfig.WeaponTypes)
            {
                if (weaponType.Key != null)
                {
                    var comboBoxItem = new ComboBoxItem { Content = weaponType.Key.ToLower(), HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                    CreateTacticWeaponBox.Items.Add(weaponType.Key.ToLower());
                }
            }
        }

        private void TacticCancelButton_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTacticDockPanel.IsVisible = false;
            Debug.WriteLine("Closing Window");
        }

        private void TacticCreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel)
            {
                CreateTacticWarningBlock.Text = "Something went horrible wrong!";
                return;
            }
            if (string.IsNullOrWhiteSpace(CreateTacticNameBox.Text))
            {
                CreateTacticWarningBlock.Text = "Please enter a name for the tactic";
                CreateTacticWarningBlock.IsVisible = true;
                return;
            }
            if (string.IsNullOrWhiteSpace(CreateTacticRaceBox.SelectionBoxItem as string))
            {
                CreateTacticWarningBlock.Text = "Please select a race";
                CreateTacticWarningBlock.IsVisible = true;
                return;
            }
            if (string.IsNullOrWhiteSpace(CreateTacticWeaponBox.SelectionBoxItem as string))
            {
                CreateTacticWarningBlock.Text = "Please select a weapon type";
                CreateTacticWarningBlock.IsVisible = true;
                return;
            }

            int raceId = viewModel.ApiConfig.Races!.First(r => r.Name!.ToLower() == CreateTacticRaceBox.SelectionBoxItem as string).Type;
            int weaponTypeId = viewModel.ApiConfig.WeaponTypes!.First(w => w.Key!.ToLower() == CreateTacticWeaponBox.SelectionBoxItem as string).Value;

            int newId = SqliteHandler.CreateNewTactic(CreateTacticNameBox.Text, raceId, weaponTypeId);

            Tactic tactic = new Tactic
            {
                Id = newId,
                TacticName = CreateTacticNameBox.Text,
                RaceID = raceId,
                WeaponSkillID = weaponTypeId,
                RaceName = (string)CreateTacticRaceBox.SelectionBoxItem,
                WeaponName = (string)CreateTacticWeaponBox.SelectionBoxItem
            };

            viewModel.Tactics.Add(tactic);

            CreateNewTacticDockPanel.IsVisible = false;
        }

        private void OpenNewTacticWindow_Click(object sender, RoutedEventArgs e)
        {
            LoadComboBoxes();
            CreateNewTacticDockPanel.IsVisible = true;
            Debug.WriteLine("Opening Window");
        }

        private async void ItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                List<string> selectedItemTypes = [];

                string type = button.Name!.Replace("Button", "");
                if (type == "MainHand")
                {
                    selectedItemCategory = "weapon";
                    equippingSlot = "mainhand";
                    selectedItemTypes.Add(weaponTypeNameSelected);
                }
                else if (type == "ShieldHand")
                {
                    selectedItemCategory = "weapon";
                    equippingSlot = "shieldhand";
                    selectedItemTypes.Add(weaponTypeNameSelected);
                    selectedItemTypes.Add("shield");
                }
                else
                {
                    selectedItemCategory = "armor";
                    equippingSlot = type.ToLower();
                    selectedItemTypes.Add(type.ToLower());
                }

                if (isEquippingItems)
                {
                    OpenSearchItemWindow(selectedItemCategory, selectedItemTypes);
                }
                else
                {
                    await RemoveEquippedItem();
                    await UpdateLoadedArmorAndWeapons();
                    await CalculateStats();
                }
            }
        }

        private async Task RemoveEquippedItem()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
               {
                   if (DataContext is MainViewModel viewModel)
                   {
                       var tactic = viewModel.Tactics.First(t => t.Id == selectedTacticId);

                       var equipmentToRemove = tactic.EquippedItems.Where(e => e.TacticId == selectedTacticId && e.EquippedLevel == selectedTacticLevel && e.equippedSlot == equippingSlot).ToList();

                       foreach (var item in equipmentToRemove)
                       {
                           tactic.EquippedItems.Remove(item);
                       }

                       int equippedItemsOnLevel = tactic.EquippedItems.Count(e => e.TacticId == selectedTacticId && e.EquippedLevel == selectedTacticLevel);
                       if (equippedItemsOnLevel == 0)
                       {
                           viewModel.Tactics.First(t => t.Id == selectedTacticId).Levels.First(l => l.Level == selectedTacticLevel).EquippedItemOnLevel = "";
                       }
                       SqliteHandler.DeleteEquippedItem(selectedTacticId, selectedTacticLevel, equippingSlot);
                   }
               });

            await UpdateButtons();
        }

        private void CloseSearchItemButton_Click(object sender, RoutedEventArgs e)
        {
            CloseSearchItem();
        }

        private void CloseSearchItem()
        {
            EquipItemButton.IsVisible = false;
            SearchItemsParentPanel.IsVisible = false;
            SearchItemsDockPanel.Children.Clear();
        }

        private async void OpenSearchItemWindow(string selectedItemCategory, List<string> selectedItemType)
        {
            selectedWeapon = null;
            selectedArmor = null;
            SearchItemsDockPanel.Children.Clear();

            if (selectedItemCategory == "weapon")
            {
                SearchWeapons searchWeapons = new();

                searchWeapons.ListBoxWeapons.SelectionChanged += SelectionChanged!;

                await searchWeapons.UpdateTypeSelection(true, selectedItemType);

                if (selectedItemType.Contains("shield"))
                {
                    await searchWeapons.SetGripList("1h");
                }

                SearchItemsParentPanel.IsVisible = true;

                SearchItemsDockPanel.Children.Add(searchWeapons);

                await searchWeapons.UpdateList();
            }
            else if (selectedItemCategory == "armor")
            {
                SearchArmor searchArmor = new();

                searchArmor.ListBoxArmors.SelectionChanged += SelectionChanged!;

                await searchArmor.UpdateTypeSelection(true, selectedItemType);

                SearchItemsParentPanel.IsVisible = true;

                SearchItemsDockPanel.Children.Add(searchArmor);

                await searchArmor.UpdateList();
            }
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBoxWeapon && listBoxWeapon.SelectedItem is Weapon weapon)
            {
                selectedWeapon = weapon;
                EquipItemButton.IsVisible = true;
                EquipItemButton.Content = "Equip " + weapon.Name;
            }
            if (sender is ListBox listBoxArmor && listBoxArmor.SelectedItem is Armor armor)
            {
                selectedArmor = armor;
                EquipItemButton.IsVisible = true;
                EquipItemButton.Content = "Equip " + armor.Name;
            }
        }

        private async void EquipSelectedItemButton_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (selectedWeapon != null && selectedItemCategory == "weapon")
                {
                    string equippedType = string.Empty;

                    if (selectedWeapon.Type == 6)
                    {
                        equippedType = "shield";
                    }
                    else if (selectedWeapon.IsTwoHanded == true)
                    {
                        equippedType = "2h";
                    }
                    else
                    {
                        equippedType = "1h";
                    }

                    SqliteHandler.UpdateEquippedItem(selectedTacticId, selectedTacticLevel, equippedType, selectedWeapon.Id, equippingSlot);

                    if (DataContext is MainViewModel viewModel)
                    {
                        var tactic = viewModel.Tactics.First(t => t.Id == selectedTacticId);

                        var equipmentToRemove = tactic.EquippedItems.Where(e => e.TacticId == selectedTacticId && e.EquippedLevel == selectedTacticLevel && e.equippedSlot == equippingSlot).ToList();

                        foreach (var item in equipmentToRemove)
                        {
                            tactic.EquippedItems.Remove(item);
                        }

                        tactic.EquippedItems.Add(new TacticEquippedItem { TacticId = selectedTacticId, EquippedType = equippedType, EquippedId = selectedWeapon.Id, EquippedLevel = selectedTacticLevel, equippedSlot = equippingSlot });
                        viewModel.Tactics.First(t => t.Id == selectedTacticId).Levels.First(l => l.Level == selectedTacticLevel).EquippedItemOnLevel = "✦";
                    }
                }
                if (selectedArmor != null && selectedItemCategory == "armor")
                {
                    SqliteHandler.UpdateEquippedItem(selectedTacticId, selectedTacticLevel, "armor", selectedArmor.Id, equippingSlot);

                    if (DataContext is MainViewModel viewModel)
                    {
                        var tactic = viewModel.Tactics.First(t => t.Id == selectedTacticId);

                        var equipmentToRemove = tactic.EquippedItems.Where(e => e.TacticId == selectedTacticId && e.EquippedLevel == selectedTacticLevel && e.equippedSlot == equippingSlot).ToList();

                        foreach (var item in equipmentToRemove)
                        {
                            tactic.EquippedItems.Remove(item);
                        }

                        tactic.EquippedItems.Add(new TacticEquippedItem { TacticId = selectedTacticId, EquippedType = "armor", EquippedId = selectedArmor.Id, EquippedLevel = selectedTacticLevel, equippedSlot = equippingSlot });
                        viewModel.Tactics.First(t => t.Id == selectedTacticId).Levels.First(l => l.Level == selectedTacticLevel).EquippedItemOnLevel = "✦";
                    }
                }
            });

            CloseSearchItem();
            await UpdateLoadedArmorAndWeapons();
            await UpdateButtons();
            await CalculateStats();
        }

        private async Task UpdateButtons()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
             {
                 if (DataContext is MainViewModel viewModel)
                 {
                     var equippedItems = viewModel.Tactics.First(t => t.Id == selectedTacticId).EquippedItems.OrderByDescending(r => r.EquippedLevel);
                     var equippedItemsOnLevel = viewModel.Tactics.First(t => t.Id == selectedTacticId).EquippedItems.Where(r => r.EquippedLevel == selectedTacticLevel);

                     var equipmentButtons = new Dictionary<string, Button>
                     {
                        { "mainhand", MainHandButton },
                        { "shieldhand", ShieldHandButton },
                        { "head", HeadButton },
                        { "shoulders", ShouldersButton },
                        { "chest", ChestButton },
                        { "hands", HandsButton },
                        { "legs", LegsButton },
                        { "feet", FeetButton },
                        { "back", BackButton },
                        { "neck", NeckButton },
                        { "finger", FingerButton },
                        { "amulet", AmuletButton },
                        { "bracelet", BraceletButton },
                        { "trinket", TrinketButton }
                     };

                     foreach (var equipmentType in equipmentSlots)
                     {
                         int equipmentId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == equipmentType)?.EquippedId ?? 0;

                         if (equipmentId != 0)
                         {
                             if (equipmentType == "mainhand")
                             {
                                 equipmentButtons[equipmentType].Content = viewModel.WeaponList.First(w => w.Id == equipmentId).Name;
                             }
                             else if (equipmentType == "shieldhand")
                             {
                                 TacticEquippedItem? mainHandWeapon = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "mainhand");

                                 bool mainHandIsTwoHanded = false;

                                 if (mainHandWeapon != null && mainHandWeapon.EquippedType == "2h")
                                 {
                                     mainHandIsTwoHanded = true;
                                 }

                                 if (mainHandIsTwoHanded)
                                 {
                                     equipmentButtons[equipmentType].Content = "/";
                                 }
                                 else
                                 {
                                     equipmentButtons[equipmentType].Content = viewModel.WeaponList.First(w => w.Id == equipmentId).Name;
                                 }
                             }
                             else
                             {
                                 equipmentButtons[equipmentType].Content = viewModel.ArmorList.First(a => a.Id == equipmentId).Name;
                             }

                             // Check if the item is in the equippedItemsOnLevel list
                             if (equippedItemsOnLevel.Any(item => item.EquippedId == equipmentId && item.equippedSlot == equipmentType))
                             {
                                 // Change the button's border color to yellow
                                 equipmentButtons[equipmentType].BorderBrush = new SolidColorBrush(Colors.DarkGoldenrod);
                             }
                             else
                             {
                                 // Reset the button's border color
                                 equipmentButtons[equipmentType].BorderBrush = new SolidColorBrush(Colors.Transparent);
                             }
                         }
                         else
                         {
                             equipmentButtons[equipmentType].Content = "None";
                             // Reset the button's border color
                             equipmentButtons[equipmentType].BorderBrush = new SolidColorBrush(Colors.Transparent);
                         }
                     }
                 }
             });
        }

        private void ToggleEquippingRemovingButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEquippingItems)
            {
                isEquippingItems = false;
                ToggleEquippingRemovingButton.Content = "Removing Items (Click To Toggle)";
                ToggleEquippingRemovingButton.BorderBrush = new SolidColorBrush(Colors.Red);
            }
            else
            {
                isEquippingItems = true;
                ToggleEquippingRemovingButton.Content = "Equipping Items (Click To Toggle)";
                ToggleEquippingRemovingButton.BorderBrush = new SolidColorBrush(Colors.LimeGreen);
            }
        }

        private async Task CalculateStats()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (loadedRaceBonuses == null || loadedRaceBonuses.Stats == null) { return; }
                if (loadedRaceBonuses == null || loadedRaceBonuses.WeaponSkills == null) { return; }

                if (DataContext is not MainViewModel viewModel) { return; }

                double staminaRB = (ReturnTotalStatValue(StaminaTotalTextBlock, StaminaTextBox) * loadedRaceBonuses.Stats.First(r => r.Type == 0).Value);
                StaminaRB.Text = staminaRB.ToString("F1");
                double strengthRB = (ReturnTotalStatValue(StrengthTotalTextBlock, StrengthTextBox) * loadedRaceBonuses.Stats.First(r => r.Type == 1).Value);
                StrengthRB.Text = strengthRB.ToString("F1");
                double enduranceRB = (ReturnTotalStatValue(EnduranceTotalTextBlock, EnduranceTextBox) * loadedRaceBonuses.Stats.First(r => r.Type == 3).Value);
                EnduranceRB.Text = enduranceRB.ToString("F1");
                double initiativeRB = (ReturnTotalStatValue(InitiativeTotalTextBlock, InitiativeTextBox) * loadedRaceBonuses.Stats.First(r => r.Type == 4).Value);
                InitiativeRB.Text = initiativeRB.ToString("F1");
                double dodgeRB = (ReturnTotalStatValue(DodgeTotalTextBlock, DodgeTextBox) * loadedRaceBonuses.Stats.First(r => r.Type == 7).Value);
                DodgeRB.Text = dodgeRB.ToString("F1");
                double weaponRB = (ReturnTotalStatValue(WeaponSkillTotalTextBlock, WeaponSkillTextBox) * loadedRaceBonuses.WeaponSkills.First(r => r.Type == weaponTypeSelected).Value);
                WeaponRB.Text = weaponRB.ToString("F1");
                double shieldRB = (ReturnTotalStatValue(ShieldSkillTotalTextBlock, ShieldSkillTextBox) * loadedRaceBonuses.WeaponSkills.First(r => r.Type == 6).Value);
                ShieldRB.Text = shieldRB.ToString("F1");

                //weaponTypeSelected

                string weaponTypeName = viewModel.ApiConfig.WeaponTypes!.First(w => w.Value == weaponTypeSelected).Key;

                double staminaEQ = ReturnRaceBonusAndEquippedStats("STAMINA", staminaRB);
                StaminaEQ.Text = staminaEQ.ToString("F1");
                double strengthEQ = ReturnRaceBonusAndEquippedStats("STRENGTH", strengthRB);
                StrengthEQ.Text = strengthEQ.ToString("F1");
                double enduranceEQ = ReturnRaceBonusAndEquippedStats("ENDURANCE", enduranceRB);
                EnduranceEQ.Text = enduranceEQ.ToString("F1");
                double initiativeEQ = ReturnRaceBonusAndEquippedStats("INITIATIVE", initiativeRB);
                InitiativeEQ.Text = initiativeEQ.ToString("F1");
                double dodgeEQ = ReturnRaceBonusAndEquippedStats("DODGE", dodgeRB);
                DodgeEQ.Text = dodgeEQ.ToString("F1");
                double weaponEQ = ReturnRaceBonusAndEquippedStats(weaponTypeName, weaponRB);
                WeaponEQ.Text = weaponEQ.ToString("F1");
                double shieldEQ = ReturnRaceBonusAndEquippedStats("SHIELD", shieldRB);
                ShieldEQ.Text = shieldEQ.ToString("F1");

                BonusSourcesStackPanel.Children.Clear();

                Border initBorder = new()
                {
                    Height = 2,
                    Background = new SolidColorBrush(Colors.White),
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    Margin = new Avalonia.Thickness(5, 10, 10, 5),
                    CornerRadius = new Avalonia.CornerRadius(1),
                };

                BonusSourcesStackPanel.Children.Add(initBorder);

                foreach (var weapon in loadedWeapons)
                {
                    if (weapon.Bonuses == null || weapon.Bonuses.Count == 0) { continue; }

                    StackPanel baseStackPanel = new();
                    baseStackPanel.Margin = new Avalonia.Thickness(10, 10, 0, 10);
                    TextBlock weaponName = new()
                    {
                        FontWeight = FontWeight.Bold,
                        Text = weapon.Name,
                        FontSize = 16
                    };
                    baseStackPanel.Children.Add(weaponName);

                    foreach (var stat in weapon.Bonuses)
                    {
                        if (stat.Additive != null)
                        {
                            TextBlock additiveBlock = new();
                            additiveBlock.Text = stat.Type!.ToLower() + ": ";
                            additiveBlock.Text += stat.Additive > 0 ? $"+{stat.Additive}" : stat.Additive.ToString();
                            baseStackPanel.Children.Add(additiveBlock);
                        }
                        if (stat.Multiplier != null)
                        {
                            TextBlock multiplierBlock = new();
                            multiplierBlock.Text = stat.Type!.ToLower() + ": ";
                            multiplierBlock.Text += stat.Multiplier.HasValue && stat.Multiplier > 1 ? $"+{Math.Round((stat.Multiplier.Value - 1) * 100, 1)}%" : $"{Math.Round((stat.Multiplier.GetValueOrDefault() - 1) * 100, 1)}%";
                            baseStackPanel.Children.Add(multiplierBlock);
                        }
                    }
                    BonusSourcesStackPanel.Children.Add(baseStackPanel);

                    TextBlock requirementsTitleText = new()
                    {
                        Text = "Requirements",
                        FontWeight = FontWeight.Bold,
                        Margin = new Avalonia.Thickness(10, 0, 0, 0)
                    };
                    BonusSourcesStackPanel.Children.Add(requirementsTitleText);

                    BonusSourcesStackPanel.Children.Add(GetWeaponRequirements(weapon));

                    Border border = new()
                    {
                        Height = 2,
                        Background = new SolidColorBrush(Colors.White),
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                        Margin = new Avalonia.Thickness(10, 15, 10, 5),
                        CornerRadius = new Avalonia.CornerRadius(1),
                    };

                    BonusSourcesStackPanel.Children.Add(border);
                }

                foreach (var armor in loadedArmor)
                {
                    if (armor.Bonuses == null || armor.Bonuses.Count == 0) { continue; }

                    StackPanel baseStackPanel = new();
                    baseStackPanel.Margin = new Avalonia.Thickness(10, 10, 0, 10);
                    TextBlock armorName = new();
                    armorName.Text = armor.Name;
                    armorName.FontWeight = FontWeight.Bold;
                    baseStackPanel.Children.Add(armorName);

                    foreach (var stat in armor.Bonuses)
                    {
                        if (stat.Additive != null)
                        {
                            TextBlock additiveBlock = new();
                            additiveBlock.Text = stat.Type!.ToLower() + ": ";
                            additiveBlock.Text += stat.Additive > 0 ? $"+{stat.Additive}" : stat.Additive.ToString();
                            baseStackPanel.Children.Add(additiveBlock);
                        }
                        if (stat.Multiplier != null)
                        {
                            TextBlock multiplierBlock = new();
                            multiplierBlock.Text = stat.Type!.ToLower() + ": ";
                            multiplierBlock.Text += stat.Multiplier > 1 ? $"+{(stat.Multiplier - 1) * 100}%" : $"{(stat.Multiplier - 1) * 100}%";
                            baseStackPanel.Children.Add(multiplierBlock);
                        }
                    }
                    BonusSourcesStackPanel.Children.Add(baseStackPanel);
                }
            });
        }

        private static StackPanel GetWeaponRequirements(Weapon selectedWeapon)
        {
            StackPanel weaponRequirements = new();

            string requiredLevel;
            string minLevel = selectedWeapon.RequiredLevel.ToString() ?? string.Empty;
            string maxLevel = selectedWeapon.MaxLevel.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(maxLevel))
            {
                requiredLevel = minLevel;
            }
            else
            {
                requiredLevel = $"{minLevel} - {maxLevel}";
            }
            weaponRequirements.Children.Add(BuildInfoDockPanel("Level", requiredLevel));

            if (selectedWeapon.StrengthRequirementValue != null && selectedWeapon.StrengthRequirementValue != 0)
            {
                weaponRequirements.Children.Add(BuildInfoDockPanel("Strength", selectedWeapon.StrengthRequirementValue.ToString() ?? string.Empty));
            }

            if (selectedWeapon.SkillRequirementValue != null && selectedWeapon.SkillRequirementValue != 0)
            {
                weaponRequirements.Children.Add(BuildInfoDockPanel(selectedWeapon.TypeName ?? "Skill", selectedWeapon.SkillRequirementValue.ToString() ?? string.Empty));
            }

            if (!string.IsNullOrEmpty(selectedWeapon.RaceRestrictions))
            {
                weaponRequirements.Children.Add(BuildInfoDockPanel("Race", selectedWeapon.RaceRestrictions));
            }
            if (selectedWeapon.RequiresLegend == true)
            {
                weaponRequirements.Children.Add(BuildInfoDockPanel("Requires Legend", ""));
            }
            if (selectedWeapon.RequiredRankingPoints != null)
            {
                weaponRequirements.Children.Add(BuildInfoDockPanel("Ranking", selectedWeapon.RequiredRankingPoints.ToString() ?? string.Empty));
            }
            if (selectedWeapon.MinPopularity != null)
            {
                weaponRequirements.Children.Add(BuildInfoDockPanel("Min Popularity", selectedWeapon.MinPopularity.ToString() ?? string.Empty));
            }
            if (selectedWeapon.MaxPopularity != null)
            {
                weaponRequirements.Children.Add(BuildInfoDockPanel("Max Popularity", selectedWeapon.MaxPopularity.ToString() ?? string.Empty));
            }

            return weaponRequirements;
        }

        private static DockPanel BuildInfoDockPanel(string title, string content)
        {
            DockPanel dockPanel = new();
            TextBlock titleText = new();
            TextBlock contentText = new();

            dockPanel.Margin = new Avalonia.Thickness(10, 0, 0, 0);

            if (title.Contains("Requires Legend"))
            {
                titleText.Text = $"{title}";
            }
            else
            {
                titleText.Text = $"{title}: ";
            }

            titleText.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;

            contentText.Text = content;
            contentText.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;

            dockPanel.Children.Add(titleText);
            dockPanel.Children.Add(contentText);

            return dockPanel;
        }

        private static double ReturnTotalStatValue(TextBlock block, TextBox box)
        {
            return (int.TryParse(box.Text, out int staminaValue) ? staminaValue : 0) + (int.TryParse(block.Text, out int totalStaminaValue) ? totalStaminaValue : 0);
        }

        private double ReturnRaceBonusAndEquippedStats(string bonusType, double baseStat)
        {
            double totalAfterAdditive = 0;
            double totalAfterMultiplier = 0;

            foreach (var armor in loadedArmor)
            {
                if (armor.Bonuses == null) { continue; }

                foreach (var stat in armor.Bonuses)
                {
                    if (stat.Type == bonusType)
                    {
                        if (stat.Additive != null)
                        {
                            totalAfterAdditive += (double)stat.Additive;
                        }
                        if (stat.Multiplier != null)
                        {
                            totalAfterMultiplier += baseStat * ((double)stat.Multiplier - 1);
                        }
                    }
                }
            }

            foreach (var weapon in loadedWeapons)
            {
                if (weapon.Bonuses == null) { continue; }

                foreach (var stat in weapon.Bonuses)
                {
                    if (stat.Type == bonusType)
                    {
                        if (stat.Additive != null)
                        {
                            totalAfterAdditive += (double)stat.Additive;
                        }
                        if (stat.Multiplier != null)
                        {
                            totalAfterMultiplier += baseStat * ((double)stat.Multiplier - 1);
                        }
                    }
                }
            }

            return (baseStat + totalAfterAdditive + totalAfterMultiplier);
        }

        private async void DeleteTacticButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) { return; }

            string selectedTacticName = viewModel.Tactics.First(t => t.Id == selectedTacticId).TacticName;

            var box = MessageBoxManager
        .GetMessageBoxStandard("Warning", $"You are about to delete '{selectedTacticName}'!\n\nAre you sure? \n\nThis is permanent, and can't be reversed!",
                         ButtonEnum.YesNo);

            var result = await box.ShowAsync();

            if (!result.Equals(ButtonResult.Yes))
            {
                return;
            }

            SqliteHandler.DeleteTactic(selectedTacticId);
            viewModel.Tactics.Remove(viewModel.Tactics.First(t => t.Id == selectedTacticId));

            selectedTacticId = 0;

            LevelListPanel.IsVisible = false;
            DeleteTacticButton.IsVisible = false;
            ContentStackPanel.IsVisible = false;
            SummaryStackPanel.IsVisible = false;
        }
    }
}