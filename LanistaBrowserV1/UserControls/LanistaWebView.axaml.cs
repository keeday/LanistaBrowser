using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaWebView;
using LanistaBrowserV1.ViewModels;
using System;
using System.Diagnostics;
using WebViewCore.Events;

namespace LanistaBrowserV1.UserControls
{
    public partial class LanistaWebView : UserControl
    {
        public LanistaWebView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void OnNavigationStarting(object sender, WebViewUrlLoadingEventArg e)
        {
            MainViewModel? viewModel = DataContext as MainViewModel;

            WebView? webView = this.FindControl<WebView>("LanistaBrowser");

            if (webView != null && viewModel != null)
            {
                string? currentUrl = await webView.ExecuteScriptAsync("window.location.href");

                if (!string.IsNullOrEmpty(currentUrl))
                {
                    currentUrl = currentUrl.Trim('"');
                    viewModel.CurrentUrl = currentUrl;
                    Debug.WriteLine(currentUrl);
                }
            }
        }
    }
}