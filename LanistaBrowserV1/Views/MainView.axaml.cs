using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using LanistaBrowserV1.Functions;
using LanistaBrowserV1.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net.Http;
using static System.Net.WebRequestMethods;

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

    public void GoToWikiPage()
    {
        TabStrip.SelectedItem = WikiButton;
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

                case 2:
                    viewModel.IsTheoryCraftingMainVisible = true;
                    break;

                case 3:
                    viewModel.IsWikiPageVisible = true;
                    break;

                case 4:
                    viewModel.IsAboutPageVisible = true;
                    break;
            }
        }
    }

    private void ContinueAnyway_Click(object sender, RoutedEventArgs e)
    {
        LoadingScreen.IsVisible = false;
        MainWindow.IsVisible = true;
    }

    private async void DownloadApiData()
    {
        bool noError = true;
        viewModel = DataContext as MainViewModel;

        if (viewModel != null)
        {
            try
            {
                Debug.WriteLine("Fetching Config...");
                LoadingInfo.Text = "Fetching Config...";
                var config = await LanistaApiCall.GetConfig();
                viewModel.ApiConfig = config;
            }
            catch (Exception ex)
            {
                LoadingTitle.Text = "ERROR";
                LoadingTitle.Foreground = new SolidColorBrush(Colors.Red);
                string maintinanceMessage = string.Empty;
                if (ex.Message.Contains("503"))
                {
                    maintinanceMessage = "Lanista is down for maintenance.";

                    ContinueAnywayButton.IsVisible = true;
                }
                LoadingInfo.Text = "Could Not load API Data.\n\n" + ex.Message + "\n\n" + maintinanceMessage;
                noError = false;
            }

            Debug.WriteLine("Updating Tables...");
            LoadingInfo.Text = "Updating Tables...";

            try
            {
                await SqliteHandler.CreateAndUpdateTables();

                var favoritedWeapons = SqliteHandler.FetchFavoritedWeapons();
                var favoritedArmors = SqliteHandler.FetchFavoritedArmors();
                var favoritedConsumables = SqliteHandler.FetchFavoritedConsumables();
                var tactics = SqliteHandler.FetchTactics();

                viewModel.FavoritedArmors = favoritedArmors;
                viewModel.FavoritedWeapons = favoritedWeapons;
                viewModel.FavoritedConsumables = favoritedConsumables;
                viewModel.Tactics = tactics;
            }
            catch (Exception ex)
            {
                LoadingTitle.Text = "ERROR";
                LoadingTitle.Foreground = new SolidColorBrush(Colors.Red);

                LoadingInfo.Text = "Could Not Load Local Data.\n\n" + ex.Message;
                noError = false;
            }

            try
            {
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
                    weapons[i].IsFavorited = viewModel.FavoritedWeapons.Exists(x => x.Id == weapons[i].Id);
                }

                for (int i = 0; i < armors.Count; i++)
                {
                    armors[i].IsFavorited = viewModel.FavoritedArmors.Exists(x => x.Id == armors[i].Id);
                }

                for (int i = 0; i < consumables.Count; i++)
                {
                    consumables[i].IsFavorited = viewModel.FavoritedConsumables.Exists(x => x.Id == consumables[i].Id);
                }

                viewModel.ConsumableList = consumables;
                viewModel.ArmorList = armors;
                viewModel.WeaponList = weapons;
            }
            catch (Exception ex)
            {
                LoadingTitle.Text = "ERROR";
                LoadingTitle.Foreground = new SolidColorBrush(Colors.Red);
                string maintinanceMessage = string.Empty;
                if (ex.Message.Contains("503"))
                {
                    maintinanceMessage = "Lanista is down for maintenance.";

                    ContinueAnywayButton.IsVisible = true;
                }
                LoadingInfo.Text = "Could Not load API Data.\n\n" + ex.Message + "\n\n" + maintinanceMessage;
                noError = false;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; GrandCircus/1.0)");
                    var url = "https://api.github.com/repos/keeday/LanistaBrowser/releases/latest";
                    var response = await client.GetStringAsync(url);
                    var json = JObject.Parse(response);
                    viewModel.LatestRelease = json["tag_name"].ToString();
                }

                if (viewModel.LatestRelease != viewModel.Version)
                {
                    NewVersionBlock.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                LoadingTitle.Text = "ERROR";
                LoadingTitle.Foreground = new SolidColorBrush(Colors.Red);

                Debug.WriteLine(ex.Message);
                LoadingInfo.Text = "Could not fetch latest release.\n\n" + ex.Message;
                noError = false;
            }

            if (noError)
            {
                LoadingScreen.IsVisible = false;
                MainWindow.IsVisible = true;
            }
        }
        else
        {
            LoadingTitle.Text = "ERROR";
            LoadingTitle.Foreground = new SolidColorBrush(Colors.Red);

            LoadingInfo.Text = "Something went horribly wrong...";
        }
    }

    private void Hyperlink_PointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var textBlock = sender as TextBlock;
        if (textBlock != null)
        {
            var url = "https://github.com/keeday/LanistaBrowser/releases/tag/" + viewModel!.LatestRelease;
            OpenUrl(url);
        }
    }

    private void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
        }
    }
}