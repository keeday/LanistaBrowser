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

namespace LanistaBrowserV1.UserControls
{
    public partial class TheoryCraftingMain : UserControl
    {
        private int selectedTacticId = 0;
        private string selectedTacticName = string.Empty;
        private int selectedTacticLevel = 0;
        private int weaponTypeSelected = 0;
        private int selectedRace = 0;

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
                WeaponSkillTotalTextBlock.Text = totalPlacedStats.Where(s => s.StatId == 5 && s.StatType == "weapon").Sum(s => s.StatValue).ToString();
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
    }
}