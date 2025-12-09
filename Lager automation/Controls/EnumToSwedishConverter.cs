using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Lager_automation.Models;

namespace Lager_automation.Controls
{
    [ValueConversion(typeof(object), typeof(string))]
    public class EnumToSwedishConverter : IValueConverter
    {
        private static readonly IReadOnlyDictionary<FilterCriteria, string> _map = new Dictionary<FilterCriteria, string>
        {
            { FilterCriteria.Factory, "Fabrik" },
            { FilterCriteria.Customer, "Kund" },
            { FilterCriteria.StackingHeight, "Staplingshöjd" },
            { FilterCriteria.TheRest, "Alla / Resten" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            if (value is FilterCriteria fc && _map.TryGetValue(fc, out var sv))
                return sv;

            // Fallback to enum name
            return value.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string s) return Binding.DoNothing;

            foreach (var kv in _map)
            {
                if (string.Equals(kv.Value, s, StringComparison.CurrentCultureIgnoreCase))
                    return kv.Key;
            }

            // Try parse by enum name as fallback
            if (Enum.TryParse(typeof(FilterCriteria), s, true, out var parsed))
                return parsed!;

            return Binding.DoNothing;
        }
    }
}