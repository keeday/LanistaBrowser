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

                LevelListPanel.IsVisible = true;
                ContentStackPanel.IsVisible = false;
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

        private void ListBoxSelectLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                ContentStackPanel.IsVisible = true;
                UpdateButtons();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Text != null)
            {
                textBox.Text = System.Text.RegularExpressions.Regex.Replace(textBox.Text, "[^0-9]", "");
                SaveStatsButton.IsVisible = true;
                UpdateStatsToSpend();
            }
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

            if (pointsSpent < maxPointsToSpend)
            {
                var box = MessageBoxManager
              .GetMessageBoxStandard("Warning", "You have more points to spend, are you sure you want to save?",
                  ButtonEnum.YesNo);

                var result = await box.ShowAsync();

                if (result.Equals(ButtonResult.No))
                {
                    return;
                }
            }
            else if (pointsSpent > maxPointsToSpend)
            {
                var box = MessageBoxManager
              .GetMessageBoxStandard("Warning", "You have more points spent than allowed, are you sure you want to save?",
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
            if (viewModel.ApiConfig.WeaponSkills == null) { return; }

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

            foreach (var weaponType in viewModel.ApiConfig.WeaponSkills)
            {
                if (weaponType.Name != null)
                {
                    var comboBoxItem = new ComboBoxItem { Content = weaponType.Name.ToLower(), HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                    CreateTacticWeaponBox.Items.Add(weaponType.Name.ToLower());
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
            int weaponTypeId = viewModel.ApiConfig.WeaponSkills!.First(w => w.Name!.ToLower() == CreateTacticWeaponBox.SelectionBoxItem as string).Type;

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

        private void ItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                List<string> selectedItemTypes = new();

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

                OpenSearchItemWindow(selectedItemCategory, selectedItemTypes);
            }
        }

        private void CloseSearchItemButton_Click(object sender, RoutedEventArgs e)
        {
            CloseSearchItem();
        }

        private void CloseSearchItem()
        {
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
            }
            if (sender is ListBox listBoxArmor && listBoxArmor.SelectedItem is Armor armor)
            {
                selectedArmor = armor;
                EquipItemButton.IsVisible = true;
            }
        }

        private void EquipSelectedItemButton_Click(object sender, RoutedEventArgs e)
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
            CloseSearchItem();
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (DataContext is MainViewModel viewModel)
            {
                var equippedItems = viewModel.Tactics.First(t => t.Id == selectedTacticId).EquippedItems.OrderByDescending(r => r.EquippedLevel);

                int mainHandId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "mainhand")?.EquippedId ?? 0;
                int shieldHandId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "shieldhand")?.EquippedId ?? 0;
                int headId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "head")?.EquippedId ?? 0;
                int shouldersId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "shoulders")?.EquippedId ?? 0;
                int chestId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "chest")?.EquippedId ?? 0;
                int handsId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "hands")?.EquippedId ?? 0;
                int legsId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "legs")?.EquippedId ?? 0;
                int feetId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "feet")?.EquippedId ?? 0;
                int backId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "back")?.EquippedId ?? 0;
                int neckId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "neck")?.EquippedId ?? 0;
                int fingerId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "finger")?.EquippedId ?? 0;
                int amuletId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "amulet")?.EquippedId ?? 0;
                int braceletId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "bracelet")?.EquippedId ?? 0;
                int trinketId = equippedItems.FirstOrDefault(item => item.TacticId == selectedTacticId && item.EquippedLevel <= selectedTacticLevel && item.equippedSlot == "trinket")?.EquippedId ?? 0;

                MainHandButton.Content = mainHandId != 0 ? viewModel.WeaponList.First(w => w.Id == mainHandId).Name : "None";
                ShieldHandButton.Content = shieldHandId != 0 ? viewModel.WeaponList.First(w => w.Id == shieldHandId).Name : "None";
                HeadButton.Content = headId != 0 ? viewModel.ArmorList.First(a => a.Id == headId).Name : "None";
                ShouldersButton.Content = shouldersId != 0 ? viewModel.ArmorList.First(a => a.Id == shouldersId).Name : "None";
                ChestButton.Content = chestId != 0 ? viewModel.ArmorList.First(a => a.Id == chestId).Name : "None";
                HandsButton.Content = handsId != 0 ? viewModel.ArmorList.First(a => a.Id == handsId).Name : "None";
                LegsButton.Content = legsId != 0 ? viewModel.ArmorList.First(a => a.Id == legsId).Name : "None";
                FeetButton.Content = feetId != 0 ? viewModel.ArmorList.First(a => a.Id == feetId).Name : "None";
                BackButton.Content = backId != 0 ? viewModel.ArmorList.First(a => a.Id == backId).Name : "None";
                NeckButton.Content = neckId != 0 ? viewModel.ArmorList.First(a => a.Id == neckId).Name : "None";
                FingerButton.Content = fingerId != 0 ? viewModel.ArmorList.First(a => a.Id == fingerId).Name : "None";
                AmuletButton.Content = amuletId != 0 ? viewModel.ArmorList.First(a => a.Id == amuletId).Name : "None";
                BraceletButton.Content = braceletId != 0 ? viewModel.ArmorList.First(a => a.Id == braceletId).Name : "None";
                TrinketButton.Content = trinketId != 0 ? viewModel.ArmorList.First(a => a.Id == trinketId).Name : "None";
            }
        }
    }
}