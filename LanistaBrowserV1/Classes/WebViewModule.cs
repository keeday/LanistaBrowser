using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using LanistaBrowserV1.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanistaBrowserV1.Classes
{
    public class WebViewTab : ObservableObject
    {
        private SolidColorBrush _backgroundColor = new();
        private bool _isSelected = false;
        private string _title = "Loading...";

        public int ID { get; set; }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public SolidColorBrush BackgroundColor
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class CustomTimer : ObservableObject
    {
        private string _timerName = string.Empty;
        private string _timerType = string.Empty;
        private DateTime _dateTimeValue;
        private TimeSpan _timeLeft;
        private bool _isTimerExpired = false;
        private bool _isTimerAcknowledged = false;
        private bool _isNotificationSent = false;
        public int Id { get; set; }

        public string TimerName
        {
            get => _timerName;
            set => SetProperty(ref _timerName, value);
        }

        public string TimerType
        {
            get => _timerType;
            set => SetProperty(ref _timerType, value);
        }

        public DateTime DateTimeValue
        {
            get => _dateTimeValue;
            set => SetProperty(ref _dateTimeValue, value);
        }

        public TimeSpan TimeLeft
        {
            get => _timeLeft;
            set => SetProperty(ref _timeLeft, value);
        }

        public bool IsTimerExpired
        {
            get => _isTimerExpired;
            set => SetProperty(ref _isTimerExpired, value);
        }

        public bool IsTimerAcknowledged
        {
            get => _isTimerAcknowledged;
            set => SetProperty(ref _isTimerAcknowledged, value);
        }

        public bool IsNotificationSent
        {
            get => _isNotificationSent;
            set => SetProperty(ref _isNotificationSent, value);
        }
    }

    public class UserSettings : ObservableObject
    {
        private string _settingName = string.Empty;
        private string _settingValue = string.Empty;

        public int Id { get; set; }

        public string SettingName
        {
            get => _settingName;
            set => SetProperty(ref _settingName, value);
        }

        public string SettingValue
        {
            get => _settingValue;
            set => SetProperty(ref _settingValue, value);
        }
    }
}