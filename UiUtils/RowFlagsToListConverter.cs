using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace industREAL.CAN.CanViewer.Utils
{
    public class RowFlagsToListConverter : IValueConverter
    {
        public string FieldName { get; set; } = "_flags";

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null) return Array.Empty<object>();

            // Get the _flags field from the row object
            var field = value.GetType().GetField(FieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field is null) return Array.Empty<object>();

            var enumVal = field.GetValue(value);
            if (enumVal is null) return Array.Empty<object>();

            var enumType = enumVal.GetType();
            if (!enumType.IsEnum) return Array.Empty<object>();

            var none = (Enum)Enum.ToObject(enumType, 0);
            var all = Enum.GetValues(enumType).Cast<Enum>();
            var active = new List<string>();

            foreach (var f in all)
                if (!Equals(f, none) && ((Enum)enumVal).HasFlag(f))
                    active.Add(f.ToString());

            return active;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => null;
    }
}
