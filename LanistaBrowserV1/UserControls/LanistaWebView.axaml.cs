using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaWebView;
using LanistaBrowserV1.ViewModels;
using System;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using WebViewCore.Events;

namespace LanistaBrowserV1.UserControls
{
    public partial class LanistaWebView : UserControl
    {
        private readonly Timer urlCheckTimer;

        public LanistaWebView()
        {
            InitializeComponent();
            urlCheckTimer = new Timer(async _ =>
            {
                await CheckCurrentUrl();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async Task CheckCurrentUrl()
        {
            try
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    MainViewModel? viewModel = DataContext as MainViewModel;
                    WebView? webView = this.FindControl<WebView>("LanistaBrowser");

                    if (webView != null && viewModel != null)
                    {
                        string? currentUrl = await webView.ExecuteScriptAsync("window.location.href");

                        if (!string.IsNullOrEmpty(currentUrl))
                        {
                            currentUrl = currentUrl.Trim('"');
                            if (viewModel.CurrentUrl != currentUrl)
                            {
                                viewModel.CurrentUrl = currentUrl;
                                if (currentUrl.Contains("levelup"))
                                {
                                    string? htmlMarkup = await webView.ExecuteScriptAsync("document.documentElement.outerHTML");
                                    LevelUpDetected(htmlMarkup);
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

                var node = doc.DocumentNode.SelectSingleNode("//a[@href='/game/avatar/me/levelhistory']");

                if (node != null)
                {
                    string text = node.InnerText;
                    int startIndex = text.IndexOf("grad") + "grad".Length;
                    string numberString = text.Substring(startIndex).Trim();
                    if (int.TryParse(numberString, out int number))
                    {
                        LoadTheoryCraftSidePanelLevel(number);
                    }
                }
            }
        }

        private async void LoadTheoryCraftSidePanelLevel(int level)
        {
            Debug.WriteLine($"You are level: '{level}'");

            WebView? webView = this.FindControl<WebView>("LanistaBrowser");

            if (webView != null)
            {
                await webView.ExecuteScriptAsync(Script("Yxor", 10));
                await webView.ExecuteScriptAsync(Script("Sköldar", 10));
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
    }
}