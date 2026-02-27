using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace industREAL.CAN.CanViewer.Utils
{
    public class EnumFlagsToListConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null) return Array.Empty<object>();
            var enumType = value.GetType();
            if (!enumType.IsEnum) return Array.Empty<object>();

            var vals = Enum.GetValues(enumType).Cast<Enum>();
            var none = (Enum)Enum.ToObject(enumType, 0);

            return vals.Where(v => !Equals(v, none) && ((Enum)value).HasFlag(v)).ToArray();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
    }
}
