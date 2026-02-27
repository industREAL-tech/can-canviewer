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
    public class BoolToBrushConverter : IValueConverter
    {
        public IBrush TrueBrush { get; set; } = new SolidColorBrush(Color.Parse("#2D8C3C"));  // green
        public IBrush FalseBrush { get; set; } = new SolidColorBrush(Color.Parse("#8C2D2D")); // red

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is true ? TrueBrush : FalseBrush;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
