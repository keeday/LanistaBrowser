using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaWebView;
using DryIoc.FastExpressionCompiler.LightExpression;
using LanistaBrowserV1.Classes;
using LanistaBrowserV1.Functions;
using LanistaBrowserV1.ViewModels;
using LanistaBrowserV1.Views;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using WebViewCore.Events;

namespace LanistaBrowserV1.UserControls
{
    public partial class LanistaWebView : UserControl
    {
        private readonly Timer urlCheckTimer;

        public string Url { get; set; } = "https://beta.lanista.se/";
        public string gladiatorName { get; set; } = string.Empty;
        public string lastLanistaUrl = string.Empty;

        private string loadedTacticName = string.Empty;
        private int currentLevel = 0;

        private Tactic? selectedTactic;

        public LanistaWebView()
        {
            InitializeComponent();
            urlCheckTimer = new Timer(async _ =>
            {
                await CheckCurrentUrl();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private async void LanistaBrowser_NavigationCompleted(object sender, WebViewUrlLoadedEventArg e)
        {
            await SetZoom();
        }

        private async void LanistaBrowser_NavigationStarting(object sender, WebViewUrlLoadingEventArg? e)
        {
            await SetZoom();
        }

        public void RefreshView()
        {
            LanistaBrowser.Reload();
        }

        public async Task SetZoom()
        {
            if (DataContext is not MainViewModel viewModel) { return; }

            string vmZoom = viewModel.UserSettings.FirstOrDefault(r => r.SettingName == "DefaultZoom")?.SettingValue ?? "";

            if (string.IsNullOrWhiteSpace(vmZoom))
            {
                Debug.WriteLine("No zoom level set in settings.");
                return;
            }

            string zoomLevel = $"{vmZoom}%";

            await Dispatcher.UIThread.InvokeAsync(async () =>
              {
                  await LanistaBrowser.ExecuteScriptAsync($"document.body.style.zoom='{zoomLevel}'");
              });
        }

        private async Task CheckCurrentUrl()
        {
            try
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    if (LanistaBrowser != null)
                    {
                        string? currentUrl = await LanistaBrowser.ExecuteScriptAsync("window.location.href");

                        if (!string.IsNullOrEmpty(currentUrl))
                        {
                            currentUrl = currentUrl.Trim('"');

                            if (Url != currentUrl)
                            {
                                Url = currentUrl;
                                if (DataContext is MainViewModel viewModel)
                                {
                                    int id = this.Tag as int? ?? 0;

                                    if (id != 0)
                                    {
                                        string? currentTitle = await LanistaBrowser.ExecuteScriptAsync("document.title");
                                        if (currentTitle is null)
                                        {
                                            currentTitle = "Loading...";
                                        }
                                        else
                                        {
                                            currentTitle = currentTitle.Trim('"');
                                            currentTitle = currentTitle.Replace(" | Lanista", "");
                                        }
                                        viewModel.OpenTabs.Where(r => r.ID == id).First().Title = currentTitle;
                                    }
                                }
                                if (currentUrl.Contains("levelup"))
                                {
                                    string? htmlMarkup = await LanistaBrowser.ExecuteScriptAsync("document.documentElement.outerHTML");
                                    LevelUpDetected(htmlMarkup);
                                }
                                else if (currentUrl.Contains("create-avatar"))
                                {
                                    Debug.WriteLine("Create Avatar detected");
                                    string? htmlMarkup = await LanistaBrowser.ExecuteScriptAsync("document.documentElement.outerHTML");
                                    NewCharacterDetected(htmlMarkup);
                                }
                                else if (currentUrl.Contains("wiki"))
                                {
                                    LanistaBrowser.Url = new Uri(lastLanistaUrl);
                                    MainView mainView = this.FindAncestorOfType<MainView>()!;
                                    mainView.GoToWikiPage();
                                }
                                else
                                {
                                    HideAllPanels();
                                }
                                lastLanistaUrl = currentUrl;
                            }
                        }
                    }

#if Debug
  UrlBox.IsVisible = true;
#endif
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void LevelUpDetected(string? htmlMarkup)
        {
            if (!string.IsNullOrEmpty(htmlMarkup))
            {
                htmlMarkup = System.Text.RegularExpressions.Regex.Unescape(htmlMarkup);

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlMarkup);

                var Levelnode = doc.DocumentNode.SelectSingleNode("//a[@href='/game/avatar/me/levelhistory']");

                var nameNode = doc.DocumentNode.SelectSingleNode("//p[@class='-mb-1 font-serif font-semibold']");

                if (Levelnode != null && nameNode != null)
                {
                    string levelText = Levelnode.InnerText;
                    int startIndex = levelText.IndexOf("grad") + "grad".Length;
                    string numberString = levelText.Substring(startIndex).Trim();

                    gladiatorName = nameNode != null ? nameNode.InnerText.Trim() : string.Empty;

                    if (int.TryParse(numberString, out int number))
                    {
                        currentLevel = number;
                    }

                    LoadTheoryCraftSidePanelLevel(currentLevel, gladiatorName);
                }
            }
        }

        private void NewCharacterDetected(string? htmlMarkup)
        {
            if (!string.IsNullOrEmpty(htmlMarkup))
            {
                htmlMarkup = System.Text.RegularExpressions.Regex.Unescape(htmlMarkup);

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlMarkup);

                currentLevel = 1;
                gladiatorName = "New";

                LoadTheoryCraftSidePanelLevel(currentLevel, gladiatorName);
            }
        }

        private async Task SetStatBoxesToZero()
        {
            await LanistaBrowser.ExecuteScriptAsync(Script("Bash�lsa", 0));
            await LanistaBrowser.ExecuteScriptAsync(Script("Styrka", 0));
            await LanistaBrowser.ExecuteScriptAsync(Script("Uth�llighet", 0));
            await LanistaBrowser.ExecuteScriptAsync(Script("Initiativstyrka", 0));
            await LanistaBrowser.ExecuteScriptAsync(Script("Undvika Anfall", 0));

            await LanistaBrowser.ExecuteScriptAsync(Script("Yxor", 0));
            await LanistaBrowser.ExecuteScriptAsync(Script("Sv�rd", 0));
            await LanistaBrowser.ExecuteScriptAsync(Script("Hammare", 0));
            await LanistaBrowser.ExecuteScriptAsync(Script("Stavar", 0));
            await LanistaBrowser.ExecuteScriptAsync(Script("Sk�ldar", 0));
            await LanistaBrowser.ExecuteScriptAsync(Script("Stickvapen", 0));
            await LanistaBrowser.ExecuteScriptAsync(Script("K�ttingvapen", 0));
        }

        private async void LoadTheoryCraftSidePanelLevel(int level, string characterName)
        {
            Debug.WriteLine($"You are level: '{level}'");
            Debug.WriteLine($"Your name is: '{characterName}'");

            if (DataContext is not MainViewModel viewModel) { return; }

            var tactic = viewModel.Tactics.FirstOrDefault(t => t.LoadedCharacterName == characterName);

            if (tactic != null)
            {
                var placedStats = tactic.PlacedStats.Where(ps => ps.Level == level && ps.TacticId == tactic.Id);

                TacticNameTitle.Text = tactic.TacticName;
                TacticDetailTitle.Text = $"{tactic.RaceName} / {tactic.WeaponName}";
                TacticLevelDisplay.Text = $"Level {level}";

                StatsNamePanel.Children.Clear();
                StatsValuePanel.Children.Clear();

                if (placedStats != null && placedStats.Any())
                {
                    NoStatsPlacedMessage.IsVisible = false;
                    SelectTacticPanel.IsVisible = false;

                    await SetStatBoxesToZero();

                    foreach (var stat in placedStats)
                    {
                        TextBlock statNameBlock = new();
                        statNameBlock.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                        statNameBlock.Margin = new Thickness(0, 3, 3, 3);

                        TextBlock statValueBLock = new();
                        statValueBLock.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                        statValueBLock.Margin = new Thickness(3, 3, 0, 3);

                        StatsNamePanel.Children.Add(statNameBlock);
                        StatsValuePanel.Children.Add(statValueBLock);

                        if (stat.StatType == "stats")
                        {
                            string statName = viewModel.ApiConfig.Stats!.FirstOrDefault(s => s.Type == stat.StatId)!.Name!;

                            if (statName == "STAMINA")
                            {
                                await LanistaBrowser.ExecuteScriptAsync(Script("Bash�lsa", stat.StatValue));
                                statNameBlock.Text = statName.ToLower();
                                statValueBLock.Text = stat.StatValue.ToString();
                            }
                            else if (statName == "STRENGTH")
                            {
                                await LanistaBrowser.ExecuteScriptAsync(Script("Styrka", stat.StatValue));
                                statNameBlock.Text = statName.ToLower();
                                statValueBLock.Text = stat.StatValue.ToString();
                            }
                            else if (statName == "ENDURANCE")
                            {
                                await LanistaBrowser.ExecuteScriptAsync(Script("Uth�llighet", stat.StatValue));
                                statNameBlock.Text = statName.ToLower();
                                statValueBLock.Text = stat.StatValue.ToString();
                            }
                            else if (statName == "INITIATIVE")
                            {
                                await LanistaBrowser.ExecuteScriptAsync(Script("Initiativstyrka", stat.StatValue));
                                statNameBlock.Text = statName.ToLower();
                                statValueBLock.Text = stat.StatValue.ToString();
                            }
                            else if (statName == "DODGE")
                            {
                                await LanistaBrowser.ExecuteScriptAsync(Script("Undvika Anfall", stat.StatValue));
                                statNameBlock.Text = statName.ToLower();
                                statValueBLock.Text = stat.StatValue.ToString();
                            }
                        }
                        else if (stat.StatType == "weapon")
                        {
                            string weaponName = viewModel.ApiConfig.WeaponTypes!.FirstOrDefault(w => w.Value == stat.StatId)!.Key!;

                            if (weaponName == "AXE")
                            {
                                await LanistaBrowser.ExecuteScriptAsync(Script("Yxor", stat.StatValue));
                                statNameBlock.Text = weaponName.ToLower();
                                statValueBLock.Text = stat.StatValue.ToString();
                            }
                            else if (weaponName == "SWORD")
                            {
                                await LanistaBrowser.ExecuteScriptAsync(Script("Sv�rd", stat.StatValue));
                                statNameBlock.Text = weaponName.ToLower();
                                statValueBLock.Text = stat.StatValue.ToString();
                            }
                            else if (weaponName == "MACE")
                            {
                                await LanistaBrowser.ExecuteScriptAsync(Script("Hammare", stat.StatValue));
                                statNameBlock.Text = weaponName.ToLower();
                                statValueBLock.Text = stat.StatValue.ToString();
                            }
                            else if (weaponName == "STAVE")
                            {
                                await LanistaBrowser.ExecuteScriptAsync(Script("Stavar", stat.StatValue));
                                statNameBlock.Text = weaponName.ToLower();
                                statValueBLock.Text = stat.StatValue.ToString();
                            }
                            else if (weaponName == "SHIELD")
                            {
                                await LanistaBrowser.ExecuteScriptAsync(Script("Sk�ldar", stat.StatValue));
                                statNameBlock.Text = weaponName.ToLower();
                                statValueBLock.Text = stat.StatValue.ToString();
                            }
                            else if (weaponName == "SPEAR")
                            {
                                await LanistaBrowser.ExecuteScriptAsync(Script("Stickvapen", stat.StatValue));
                                statNameBlock.Text = weaponName.ToLower();
                                statValueBLock.Text = stat.StatValue.ToString();
                            }
                            else if (weaponName == "CHAIN")
                            {
                                await LanistaBrowser.ExecuteScriptAsync(Script("K�ttingvapen", stat.StatValue));
                                statNameBlock.Text = weaponName.ToLower();
                                statValueBLock.Text = stat.StatValue.ToString();
                            }
                        }
                    }
                }
                else
                {
                    NoStatsPlacedMessage.IsVisible = true;
                }

                var equippedItems = tactic.EquippedItems.Where(e => e.TacticId == tactic.Id && e.EquippedLevel == level);

                EquippedItemsPanel.Children.Clear();

                if (equippedItems != null && equippedItems.Any())
                {
                    foreach (var item in equippedItems)
                    {
                        TextBlock itemBlock = new();
                        itemBlock.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
                        itemBlock.Margin = new Thickness(0, 5, 0, 5);

                        string itemName = string.Empty;

                        if (item.EquippedType == "armor")
                        {
                            itemName = viewModel.ArmorList.FirstOrDefault(a => a.Id == item.EquippedId)!.Name!;
                        }
                        else
                        {
                            itemName = viewModel.WeaponList.FirstOrDefault(w => w.Id == item.EquippedId)!.Name!;
                        }

                        itemBlock.Text = $"({item.equippedSlot}) {itemName}";
                        EquippedItemsPanel.Children.Add(itemBlock);
                    }
                }
                else
                {
                    TextBlock noItemBlock = new();
                    noItemBlock.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
                    noItemBlock.Margin = new Thickness(0, 5, 0, 5);
                    noItemBlock.Text = "No item changes on this level.";
                    EquippedItemsPanel.Children.Add(noItemBlock);
                }

                LoadedTacticPanel.IsVisible = true;
            }
            else
            {
                SelectTacticPanel.IsVisible = true;
            }
        }

        private static string Script(string fieldName, int inputValue)
        {
            string script = $@"
                    var ps = document.querySelectorAll('p');
                    for (var i = 0; i < ps.length; i++) {{
                        var textContent = ps[i].innerText.trim();
                        if (textContent.startsWith('{fieldName}')) {{
                            var parentDiv = ps[i].closest('.flex.border-b.py-2.border-gray-400.items-center');
                            if (parentDiv) {{
                                var input = parentDiv.querySelector('input[type=number]');
                                if (input) {{
                                    input.value = {inputValue};
                                    var event = new Event('input', {{
                                        'bubbles': true,
                                        'cancelable': true
                                    }});
                                    input.dispatchEvent(event);
                                    break;
                                }}
                            }}
                        }}
                    }}";

            return script;
        }

        private void IncreaseDisplayedLevel(object sender, RoutedEventArgs e)
        {
            currentLevel++;
            LoadTheoryCraftSidePanelLevel(currentLevel, gladiatorName);
        }

        private void DecreaseDisplayedLevel(object sender, RoutedEventArgs e)
        {
            if (currentLevel > 1)
            {
                currentLevel--;
                LoadTheoryCraftSidePanelLevel(currentLevel, gladiatorName);
            }
        }

        private void ListBoxTactics_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) { return; }

            if (sender is ListBox listBox && listBox.SelectedItem is Tactic tactic)
            {
                SelectTacticButton.IsVisible = true;
                SelectTacticText.IsVisible = false;

                SelectTacticButton.Content = $"Load '{tactic.TacticName}'";
                selectedTactic = tactic;
            }
        }

        private void SelectTacticButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) { return; }
            if (selectedTactic != null)
            {
                foreach (var tac in viewModel.Tactics)
                {
                    if (tac.LoadedCharacterName == gladiatorName)
                    {
                        tac.LoadedCharacterName = string.Empty;
                    }
                }

                if (gladiatorName == "New")
                {
                    viewModel.Tactics.FirstOrDefault(t => t.Id == selectedTactic.Id)!.LoadedCharacterName = "New";
                    SqliteHandler.UpdateLoadedTactic(selectedTactic.Id, "");
                }
                else
                {
                    viewModel.Tactics.FirstOrDefault(t => t.Id == selectedTactic.Id)!.LoadedCharacterName = gladiatorName;
                    SqliteHandler.UpdateLoadedTactic(selectedTactic.Id, gladiatorName);
                }
                HideAllPanels();
                LoadTheoryCraftSidePanelLevel(currentLevel, gladiatorName);
            }
        }

        private void SelectNewTacticButton_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            SelectTacticPanel.IsVisible = true;
        }

        private void HideAllPanels()
        {
            LoadedTacticPanel.IsVisible = false;
            SelectTacticPanel.IsVisible = false;

            SelectTacticButton.IsVisible = false;
            SelectTacticText.IsVisible = true;
        }
    }
}