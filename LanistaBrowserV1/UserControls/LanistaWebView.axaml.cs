using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaWebView;
using LanistaBrowserV1.ViewModels;
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
        public string gladiatorName { get; set; } = string.Empty;

        private string loadedTacticName = string.Empty;
        private int currentLevel = 0;

        public LanistaWebView()
        {
            InitializeComponent();
            urlCheckTimer = new Timer(async _ =>
            {
                await CheckCurrentUrl();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private async Task CheckCurrentUrl()
        {
            try
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    MainViewModel? viewModel = DataContext as MainViewModel;

                    if (LanistaBrowser != null && viewModel != null)
                    {
                        string? currentUrl = await LanistaBrowser.ExecuteScriptAsync("window.location.href");

                        if (!string.IsNullOrEmpty(currentUrl))
                        {
                            currentUrl = currentUrl.Trim('"');
                            if (viewModel.CurrentUrl != currentUrl)
                            {
                                viewModel.CurrentUrl = currentUrl;
                                if (currentUrl.Contains("levelup"))
                                {
                                    string? htmlMarkup = await LanistaBrowser.ExecuteScriptAsync("document.documentElement.outerHTML");
                                    LevelUpDetected(htmlMarkup);
                                }
                                else if (currentUrl.Contains("create-avatar"))
                                {
                                    Debug.WriteLine("Create Avatar detected");
                                    //Load new tactic here. Also load special js script for create avatar.
                                }
                                else
                                {
                                    if (LoadedTacticPanel != null)
                                    {
                                        LoadedTacticPanel.IsVisible = false;
                                    }
                                }
                            }
                        }
                    }
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
                    foreach (var stat in placedStats)
                    {
                        TextBlock statNameBlock = new();
                        statNameBlock.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                        statNameBlock.Margin = new Thickness(0, 3, 3, 3);

                        TextBlock statValueBLock = new();
                        statValueBLock.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                        statValueBLock.Margin = new Thickness(3, 3, 0, 3);

                        NoStatsPlacedMessage.IsVisible = false;
                        StatsNamePanel.Children.Add(statNameBlock);
                        StatsValuePanel.Children.Add(statValueBLock);

                        if (stat.StatType == "stats")
                        {
                            string statName = viewModel.ApiConfig.Stats!.FirstOrDefault(s => s.Type == stat.StatId)!.Name!;

                            if (statName == "STAMINA")
                            {
                                await LanistaBrowser.ExecuteScriptAsync(Script("Bashälsa", stat.StatValue));
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
                                await LanistaBrowser.ExecuteScriptAsync(Script("Uthållighet", stat.StatValue));
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
                                await LanistaBrowser.ExecuteScriptAsync(Script("Svärd", stat.StatValue));
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
                                await LanistaBrowser.ExecuteScriptAsync(Script("Sköldar", stat.StatValue));
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
                                await LanistaBrowser.ExecuteScriptAsync(Script("Kättingvapen", stat.StatValue));
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
                //No tactic loaded or this character.
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
    }
}