using Avalonia.Controls;
using Avalonia.Interactivity;
using LanistaBrowserV1.Classes;
using LanistaBrowserV1.Functions;
using LanistaBrowserV1.ViewModels;
using System.Diagnostics;
using System.Linq;

namespace LanistaBrowserV1.UserControls
{
    public partial class TheoryCraftingMain : UserControl
    {
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
                LevelListListBox.ItemsSource = selectedTactic.LevelsWithStats;
            }
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

            SqliteHandler.CreateNewTactic(CreateTacticNameBox.Text, raceId, weaponTypeId);

            Tactic tactic = new Tactic
            {
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