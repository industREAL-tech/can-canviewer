using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace industREAL.CAN.CanViewer.Utils
{
    public class HexBytesConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is byte[] bytes && bytes.Length > 0)
            {
                // format as "AA BB CC ..."
                char[] chars = new char[bytes.Length * 3 - 1];
                int k = 0;
                for (int i = 0; i < bytes.Length; i++)
                {
                    byte b = bytes[i];
                    chars[k++] = GetHex((b >> 4) & 0xF);
                    chars[k++] = GetHex(b & 0xF);
                    if (i != bytes.Length - 1)
                        chars[k++] = ' ';
                }
                return new string(chars);
            }
            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // One-way display only
            return null;
        }

        private static char GetHex(int v) => (char)(v < 10 ? ('0' + v) : ('A' + (v - 10)));
    }
}
