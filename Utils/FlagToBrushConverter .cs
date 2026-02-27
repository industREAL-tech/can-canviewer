using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace industREAL.CAN.CanViewer.Utils
{
    public class FlagToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Works with CanFlags enum or its ToString()
            var key = value?.ToString() ?? "";
            return key switch
            {
                "BRS" => new SolidColorBrush(Color.Parse("#3A7BD5")), // blue
                "FD" => new SolidColorBrush(Color.Parse("#2D8C3C")), // green
                "RTR" => new SolidColorBrush(Color.Parse("#B75C3A")), // orange/red
                "IDE" => new SolidColorBrush(Color.Parse("#7B61FF")), // purple
                "ESI" => new SolidColorBrush(Color.Parse("#C9912C")), // amber
                _ => new SolidColorBrush(Color.Parse("#444A50"))  // default
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
    }

    /// <summary>
    /// Foreground (text) color to ensure readability on the pill background.
    /// </summary>
    public sealed class FlagToForegroundBrushConverter : IValueConverter
    {
        private static readonly IBrush LightText = Brushes.White;
        private static readonly IBrush DarkText = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x26)); // #111826

        private static readonly HashSet<string> DarkOnes = new(StringComparer.OrdinalIgnoreCase)
        {
            "EXT" // light background -> dark text
        };

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string key && DarkOnes.Contains(key))
                return DarkText;
            return LightText; // default
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
