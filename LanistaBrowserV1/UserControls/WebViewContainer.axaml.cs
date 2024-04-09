using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaWebView;
using DryIoc.FastExpressionCompiler.LightExpression;
using LanistaBrowserV1.Classes;
using LanistaBrowserV1.Functions;
using LanistaBrowserV1.ViewModels;
using ManagedBass;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace LanistaBrowserV1.UserControls
{
    public partial class WebViewContainer : UserControl
    {
        //  private List<WebViewModule> webViewModules = [];

        private int maxIdValue = 0;
        private int currentSelectedId = 0;
        private bool isTimerTabVisible = false;

        private WindowNotificationManager? notificationManager;

        private int stream;

        private DispatcherTimer timer = new();

        private SolidColorBrush activatedColor = new SolidColorBrush(Color.FromRgb(53, 53, 53));
        private SolidColorBrush deActivatedColor = new SolidColorBrush(Color.FromRgb(30, 30, 30));

        public WebViewContainer()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            maxIdValue++;
            LanistaWebView newWebView = new();

            Debug.WriteLine("Loaded");

            newWebView.Tag = maxIdValue;
            WebViewGrid.Children.Add(newWebView);

            if (DataContext is MainViewModel viewModel)
            {
                WebViewTab webViewTab = new()
                {
                    ID = maxIdValue,
                    Title = "Loading..."
                };

                viewModel.OpenTabs.Add(webViewTab);
                viewModel.OpenTabs.Where(r => r.ID == maxIdValue).First().IsSelected = true;
                ActivateTab(maxIdValue);
            }

            var topLevel = TopLevel.GetTopLevel(this);
            notificationManager = new WindowNotificationManager(topLevel);

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick!;
            timer.Start();
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                foreach (CustomTimer timer in viewModel.CustomTimers)
                {
                    if (!timer.IsTimerExpired)
                    {
                        timer.TimeLeft = timer.DateTimeValue - DateTime.Now;
                    }
                    if (timer.TimeLeft.TotalSeconds <= 0)
                    {
                        timer.IsTimerExpired = true;

                        if (!timer.IsNotificationSent)
                        {
                            timer.IsNotificationSent = true;
                            await CreateAlertNotification($"Timer '{timer.TimerName}' have expired.");
                        }
                    }
                }
            }
        }

        private async Task CreateAlertNotification(string notificationMessage)
        {
            try
            {
                if (DataContext is not MainViewModel viewModel) { return; }

                double volume = double.Parse(viewModel.UserSettings.FirstOrDefault(x => x.SettingName == "TimerVolume")!.SettingValue) / 10;

                bool isCustomSoundValid = IsCustomNotificationSoundFileValid();

                await Task.Run(() =>
                {
                    if (stream != 0)
                    {
                        Bass.StreamFree(stream);
                    }

                    Bass.Free();

                    if (Bass.Init())
                    {
                        if (isCustomSoundValid)
                        {
                            string filePath = viewModel.UserSettings.FirstOrDefault(x => x.SettingName == "CustomNotificationPath")!.SettingValue;
                            stream = Bass.CreateStream(filePath);
                        }
                        else
                        {
                            stream = Bass.CreateStream("assets/ambient_flute.mp3");
                        }
                        Bass.Volume = volume;
                        Bass.ChannelPlay(stream);
                    }
                    else
                    {
                        Debug.WriteLine("Bass Error: " + Bass.LastError);
                    }
                });

                ShowMessage("Timer Expired", notificationMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        private void ShowMessage(string header, string message)
        {
            if (notificationManager is null) { return; }
            var notification = new Notification(header, message, NotificationType.Information, expiration: System.TimeSpan.Zero); ;

            notificationManager.Show(notification);
        }

        private void ActivateWebView_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && DataContext is MainViewModel viewModel)
            {
                int id = (int)button.Tag!;

                foreach (LanistaWebView webView in WebViewGrid.Children)
                {
                    if ((int)webView.Tag! == id)
                    {
                        webView.IsVisible = true;
                    }
                    else
                    {
                        webView.IsVisible = false;
                    }
                }

                ActivateTab(id);
            }
        }

        private void CloseWebView_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && DataContext is MainViewModel viewModel)
            {
                int id = (int)button.Tag!;

                viewModel.OpenTabs.Remove(viewModel.OpenTabs.First(x => x.ID == id));

                foreach (LanistaWebView webView in WebViewGrid.Children)
                {
                    if ((int)webView.Tag! == id)
                    {
                        WebViewGrid.Children.Remove(webView);
                        break;
                    }
                }
            }
        }

        private void AddWebView_Click(object sender, RoutedEventArgs e)
        {
            maxIdValue++;
            LanistaWebView newWebView = new()
            {
                Tag = maxIdValue
            };
            WebViewGrid.Children.Add(newWebView);

            if (DataContext is MainViewModel viewModel)
            {
                WebViewTab webViewTab = new()
                {
                    ID = maxIdValue,
                    Title = "Loading..."
                };
                viewModel.OpenTabs.Add(webViewTab);
                viewModel.OpenTabs.Where(r => r.ID == maxIdValue).First().IsSelected = true;
                ActivateTab(maxIdValue);
            }
            // CheckTabCount();
        }

        private void ActivateTab(int id)
        {
            if (DataContext is MainViewModel viewModel)
            {
                foreach (var item in viewModel.OpenTabs)
                {
                    if (item.ID == id)
                    {
                        item.IsSelected = true;
                        item.BackgroundColor = activatedColor;
                    }
                    else
                    {
                        item.IsSelected = false;
                        item.BackgroundColor = deActivatedColor;
                    }
                }
            }
        }

        private void BottomRowToggleVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (isTimerTabVisible)
            {
                BottomRowDockPanel.Height = Double.NaN;
                isTimerTabVisible = false;
                BottomRowGrid.IsVisible = false;
                BottomRowToggleTextBlock.Text = "∧∧ Timers and Reminders ∧∧";
            }
            else
            {
                BottomRowDockPanel.Height = 300;
                isTimerTabVisible = true;
                BottomRowGrid.IsVisible = true;
                BottomRowToggleTextBlock.Text = "∨∨ Timers and Reminders ∨∨";

                if (DataContext is MainViewModel viewModel)
                {
                    TimerSettingVolumeValue.Text = viewModel.UserSettings.FirstOrDefault(x => x.SettingName == "TimerVolume")!.SettingValue;

                    if (IsCustomNotificationSoundFileValid())
                    {
                        RemoveCustomSoundButton.IsVisible = true;

                        CustomNotificationPathTextBox.Text = viewModel.UserSettings.FirstOrDefault(x => x.SettingName == "CustomNotificationPath")!.SettingValue;
                    }
                    else
                    {
                        RemoveCustomSoundButton.IsVisible = false;
                        CustomNotificationPathTextBox.Text = "Default";
                    }
                }

                if (CreateTimerDatePicker.SelectedDate == null || CreateTimerTimePicker.SelectedTime == null)
                {
                    CreateTimerDatePicker.SelectedDate = DateTime.Now;
                    CreateTimerTimePicker.SelectedTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute + 30, DateTime.Now.Second);
                }
            }
        }

        private void CreateTimerButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) { return; }

            if (string.IsNullOrWhiteSpace(CreateTimerNameTextBox.Text))
            {
                return;
            }
            if (CreateTimerDatePicker.SelectedDate == null)
            {
                return;
            }
            if (CreateTimerTimePicker.SelectedTime == null)
            {
                return;
            }
            string timerName = CreateTimerNameTextBox.Text;
            CreateTimerNameTextBox.Text = string.Empty;

            TimeSpan timeWithoutSeconds = new TimeSpan(CreateTimerTimePicker.SelectedTime!.Value.Hours, CreateTimerTimePicker.SelectedTime!.Value.Minutes, 0);
            DateTime newDateTime = CreateTimerDatePicker.SelectedDate!.Value.Date.Add(timeWithoutSeconds);

            Debug.WriteLine($"Creating '{timerName}' with DateTime {newDateTime}");

            int timerId = SqliteHandler.CreateNewTimer(timerName, "timer", newDateTime);

            CustomTimer newTimer = new()
            {
                Id = timerId,
                TimerName = timerName,
                TimerType = "timer",
                DateTimeValue = newDateTime,
                TimeLeft = newDateTime - DateTime.Now
            };

            viewModel.CustomTimers.Add(newTimer);

            viewModel.CustomTimers = new ObservableCollection<CustomTimer>(viewModel.CustomTimers.OrderBy(x => x.DateTimeValue));
        }

        private void RemoveTimerButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) { return; }

            if (sender is Button button)
            {
                int timerId = (int)button.Tag!;

                SqliteHandler.DeleteCustomTimer(timerId);

                viewModel.CustomTimers.Remove(viewModel.CustomTimers.First(x => x.Id == timerId));
            }
        }

        private async void TrySoundButton_Click(object sender, RoutedEventArgs e)
        {
            await CreateAlertNotification("Testing Notification");
        }

        private void IncreaseTimerVolumeButton_Click(object sender, RoutedEventArgs e)
        {
            IncreaseOrDecreaseTimerVolume(true);
        }

        private void DecreaseTimerVolumeButton_Click(object sender, RoutedEventArgs e)
        {
            IncreaseOrDecreaseTimerVolume(false);
        }

        private void IncreaseOrDecreaseTimerVolume(bool increaseVolume)
        {
            if (int.TryParse(TimerSettingVolumeValue.Text, out int currentVolume))
            {
                if (DataContext is not MainViewModel viewModel) { return; }

                if (increaseVolume)
                {
                    if (currentVolume < 10)
                    {
                        currentVolume++;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    if (currentVolume > 0)
                    {
                        currentVolume--;
                    }
                    else
                    {
                        return;
                    }
                }

                Debug.WriteLine($"Current Volume: {currentVolume}");

                TimerSettingVolumeValue.Text = currentVolume.ToString();

                viewModel.UserSettings.FirstOrDefault(x => x.SettingName == "TimerVolume")!.SettingValue = currentVolume.ToString();

                SqliteHandler.UpdateSetting("TimerVolume", currentVolume.ToString());
            }
        }

        private void RemoveCustomSoundButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) { return; }
            viewModel.UserSettings.FirstOrDefault(x => x.SettingName == "CustomNotificationPath")!.SettingValue = string.Empty;
            SqliteHandler.UpdateSetting("CustomNotificationPath", string.Empty);
            RemoveCustomSoundButton.IsVisible = false;
            CustomNotificationPathTextBox.Text = "Default";
        }

        private async void AddCustomSoundButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel viewModel) { return; }

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                var soundFileTypes = new FilePickerFileType("Sound Files")
                {
                    Patterns = ["*.mp3", "*.wav", "*.ogg"],
                    AppleUniformTypeIdentifiers = ["public.mp3", "public.wav", "public.ogg"],
                    MimeTypes = ["audio/mpeg", "audio/wav", "audio/ogg"]
                };

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                {
                    Title = "Pick a sound file",
                    AllowMultiple = false,
                    FileTypeFilter = [soundFileTypes],
                });

                if (files != null && files.Count > 0)
                {
                    string filePath = files[0].Path.LocalPath;

                    viewModel.UserSettings.FirstOrDefault(x => x.SettingName == "CustomNotificationPath")!.SettingValue = filePath;

                    if (IsCustomNotificationSoundFileValid())
                    {
                        CustomNotificationPathTextBox.Text = filePath;
                        RemoveCustomSoundButton.IsVisible = true;
                        SqliteHandler.UpdateSetting("CustomNotificationPath", filePath);
                    }
                }
            }
        }

        private bool IsCustomNotificationSoundFileValid()
        {
            if (DataContext is MainViewModel viewModel)
            {
                string customNotificationPath = viewModel.UserSettings.FirstOrDefault(x => x.SettingName == "CustomNotificationPath")!.SettingValue;
                if (string.IsNullOrWhiteSpace(customNotificationPath))
                {
                    return false;
                }
                else
                {
                    if (Path.Exists(customNotificationPath))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}