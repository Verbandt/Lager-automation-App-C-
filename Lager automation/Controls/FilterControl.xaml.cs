using Lager_automation.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lager_automation.Controls
{
    public partial class FilterControl : UserControl
    {
        public FilterControl()
        {
            InitializeComponent();
        }

        public FilterViewModel Filter
        {
            get => (FilterViewModel)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty FilterProperty =
        DependencyProperty.Register(
            nameof(Filter),
            typeof(FilterViewModel),
            typeof(FilterControl),
            new PropertyMetadata(null, OnFilterChanged));

        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(FilterControl),
        new PropertyMetadata(string.Empty));

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (FilterControl)d;
            var current = ctrl.DataContext;
            if (current == null || ReferenceEquals(current, e.OldValue))
            {
                ctrl.DataContext = e.NewValue;
            }
        }

    }
}
