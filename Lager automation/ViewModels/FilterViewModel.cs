using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Lager_automation.Models;

namespace Lager_automation.ViewModels
{
    public class FilterViewModel: INotifyPropertyChanged, IDataErrorInfo
    {
        private bool _useFilter;
        private FilterCriteria _selectedCriteria;
        private string _inputValue = string.Empty;

        private static readonly Regex FactoryRegex = new(@"^[A-Z]{2}$", RegexOptions.Compiled);

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool UseFilter
        {
            get => _useFilter;
            set { _useFilter = value; Notify(); }
        }

        public FilterCriteria SelectedCriteria
        {
            get => _selectedCriteria;
            set
            {
                if (value == _selectedCriteria)
                    return;

                _selectedCriteria = value;

                if (value == FilterCriteria.TheRest)
                {
                    InputValue = "";
                }

                Notify(nameof(SelectedCriteria));
                Notify(nameof(InputValue));
            }
        }

        public string InputValue
        {
            get => _inputValue;
            set
            {
                var newValue = value ?? string.Empty;

                // Normalize for factory codes when appropriate. Do not perform normalization inside validation.
                if (SelectedCriteria == FilterCriteria.Factory)
                    newValue = newValue.ToUpperInvariant();

                if (newValue == _inputValue) // avoid redundant notifications and revalidation loops
                    return;

                _inputValue = newValue;
                Notify(nameof(InputValue));
            }
        }

        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                if (columnName == nameof(InputValue))
                {
                    switch (SelectedCriteria)
                    {
                        case FilterCriteria.Factory:

                            if (string.IsNullOrWhiteSpace(_inputValue))
                                return "Kan inte vara tom.";

                            // Validate using a derived value; do NOT assign to the property here.
                            var candidate = _inputValue.ToUpperInvariant();
                            if (!FactoryRegex.IsMatch(candidate))
                                return "Fabriks kod måste bestå av två bokstäver.";
                            break;

                        case FilterCriteria.Customer:
                            if (string.IsNullOrWhiteSpace(_inputValue))
                                return "Kan inte vara tom.";
                            if (_inputValue.Length != 5)
                                return "Kundens namn måste vara 5 tecken långt.";
                            break;

                        case FilterCriteria.StackingHeight:
                            if (!int.TryParse(_inputValue, out int val))
                                return "Staplingshöjd måste vara ett heltal.";
                            if (val < 1 || val > 15)
                                return "Staplingshöjd måste vara mellan 1 och 15.";
                            break;
                    }
                }
                return string.Empty;
            }
        }


        private void Notify([CallerMemberName] string? prop = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
