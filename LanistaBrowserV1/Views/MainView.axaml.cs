using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using LanistaBrowserV1.Functions;
using LanistaBrowserV1.ViewModels;
using System;
using System.Diagnostics;

namespace LanistaBrowserV1.Views;

public partial class MainView : UserControl
{
    private LanistaApiCall lanistaApiCall = new();
    private MainViewModel? viewModel;

    public MainView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object Sender, RoutedEventArgs e)
    {
        Debug.WriteLine("SearchWeapons UserControl_Loaded");
        DownloadApiData();
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            MainViewModel? viewModel = DataContext as MainViewModel;
            if (viewModel == null)
            {
                return;
            }

            var tabControl = (TabStrip)sender;
            int selectedIndex = tabControl.SelectedIndex;

            switch (selectedIndex)
            {
                case 0:
                    viewModel.IsLanistaVisible = true;
                    break;

                case 1:
                    viewModel.IsSearchItemsVisible = true;
                    break;
            }
        }
    }

    private async void DownloadApiData()
    {
        try
        {
            viewModel = DataContext as MainViewModel;

            if (viewModel != null)
            {
                Debug.WriteLine("Updating Tables...");
                LoadingInfo.Text = "Updating Tables...";
                await SqliteHandler.CreateAndUpdateTables();

                var favoritedWeapons = SqliteHandler.FetchFavoritedWeapons();
                var favoritedArmors = SqliteHandler.FetchFavoritedArmors();
                var favoritedConsumables = SqliteHandler.FetchFavoritedConsumables();

                viewModel.FavoritedArmors = favoritedArmors;
                viewModel.FavoritedWeapons = favoritedWeapons;
                viewModel.FavoritedConsumables = favoritedConsumables;

                Debug.WriteLine("Fetching Config...");
                LoadingInfo.Text = "Fetching Config...";
                var config = await LanistaApiCall.GetConfig();

                Debug.WriteLine("Fetching Weapons...");
                LoadingInfo.Text = "Fetching Weapons...";
                var weapons = await LanistaApiCall.GetWeapons();

                Debug.WriteLine("Fetching Armors...");
                LoadingInfo.Text = "Fetching Armors...";
                var armors = await LanistaApiCall.GetArmors();

                Debug.WriteLine("Fetching Consumables...");
                LoadingInfo.Text = "Fetching Consumables...";
                var consumables = await LanistaApiCall.GetConsumables();

                Debug.WriteLine("Finalizing...");
                LoadingInfo.Text = "Finalizing...";

                for (int i = 0; i < weapons.Count; i++)
                {
                    weapons[i].IsFavorited = favoritedWeapons.Exists(x => x.Id == weapons[i].Id);
                }

                for (int i = 0; i < armors.Count; i++)
                {
                    armors[i].IsFavorited = favoritedArmors.Exists(x => x.Id == armors[i].Id);
                }

                for (int i = 0; i < consumables.Count; i++)
                {
                    consumables[i].IsFavorited = favoritedConsumables.Exists(x => x.Id == consumables[i].Id);
                }

                viewModel.ApiConfig = config;
                viewModel.ConsumableList = consumables;
                viewModel.ArmorList = armors;
                viewModel.WeaponList = weapons;

                LoadingScreen.IsVisible = false;
                MainWindow.IsVisible = true;
            }
            else
            {
                LoadingText.Text = "Error fetching data";
                LoadingText.Foreground = new SolidColorBrush(Colors.Red);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error fetching Weapons;\n\n" + ex.Message);
        }
    }
}