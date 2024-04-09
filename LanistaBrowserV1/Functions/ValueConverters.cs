using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanistaBrowserV1.Functions
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is TimeSpan timeSpan)
            {
                if (timeSpan.Days > 0)
                {
                    return $"{timeSpan.Days} day{(timeSpan.Days > 1 ? "s" : "")}, {timeSpan.Hours} hour{(timeSpan.Hours > 1 ? "s" : "")}";
                }
                else if (timeSpan.Hours > 0)
                {
                    return $"{timeSpan.Hours} hour{(timeSpan.Hours > 1 ? "s" : "")}, {timeSpan.Minutes} minute{(timeSpan.Minutes > 1 ? "s" : "")}";
                }
                else if (timeSpan.Minutes > 0)
                {
                    return $"{timeSpan.Minutes} minute{(timeSpan.Minutes > 1 ? "s" : "")}, {timeSpan.Seconds} second{(timeSpan.Seconds > 1 ? "s" : "")}";
                }
                else if (timeSpan.Seconds > 0)
                {
                    return $"{timeSpan.Seconds} second{(timeSpan.Seconds > 1 ? "s" : "")}";
                }
                else
                {
                    return timeSpan.ToString(@"hh\:mm\:ss");
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DateTimeRemoveSecondsConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is DateTime timeSpan)
            {
                return timeSpan.ToString("dd/MMM/yy HH:mm");
            }
            return string.Empty;
        }

        public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolInverterConverter : IValueConverter
    {
        public object Convert(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return false;
        }

        public object ConvertBack(object? value, Type? targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}