using System;
using System.Globalization;
using System.Windows.Data;
using Lager_automation.Models;

namespace Lager_automation.Controls
{
    // Takes [SelectedCriteria, InputValue] and returns a help string
    public class CriteriaHelpConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 1) return string.Empty;

            // values[0] expected to be FilterCriteria (enum) or string
            // values[1] optional input value
            var criteriaObj = values.Length > 0 ? values[0] : null;
            var inputValue = values.Length > 1 ? values[1]?.ToString() ?? string.Empty : string.Empty;

            // safe enum parse
            FilterCriteria? criteria = null;
            if (criteriaObj is FilterCriteria fc) criteria = fc;
            else if (criteriaObj != null && Enum.TryParse(typeof(FilterCriteria), criteriaObj.ToString(), out var parsed))
                criteria = (FilterCriteria)parsed;

            if (criteria == null) return string.Empty;

            switch (criteria.Value)
            {
                case FilterCriteria.Factory:
                    return $"Fabrikens namn. (exempel: VS, VV)";
                case FilterCriteria.Customer:
                    return "Kundens namn. (exempel: BS8CA, BP2TD)";
                case FilterCriteria.StackingHeight:
                    return "Max antal emb per stapel";
                // Extend with other cases from your enum
                default:
                    return $"Enter value for {criteria.Value}. Current: '{inputValue}'";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}