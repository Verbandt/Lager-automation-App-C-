using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lager_automation.Helpers
{
    public static class Placeholder
    {
        public static readonly DependencyProperty DefaultTextProperty =
            DependencyProperty.RegisterAttached(
                "DefaultText",
                typeof(string),
                typeof(Placeholder),
                new PropertyMetadata("", OnDefaultChanged));

        public static void SetDefaultText(DependencyObject d, string value)
            => d.SetValue(DefaultTextProperty, value);

        public static string GetDefaultText(DependencyObject d)
            => (string)d.GetValue(DefaultTextProperty);

        private static void OnDefaultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox tb)
                return;

            tb.Loaded -= Apply;
            tb.Loaded += Apply;

            tb.GotFocus -= Remove;
            tb.GotFocus += Remove;

            tb.LostFocus -= Apply;
            tb.LostFocus += Apply;

            tb.DataContextChanged -= DataContextChanged;
            tb.DataContextChanged += DataContextChanged;
        }

        private static void DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var tb = (TextBox)sender;

            // When template selector swaps templates,
            // the DataContext is the FIRST thing to change.
            // Apply placeholder AFTER new binding applies.
            tb.Dispatcher.InvokeAsync(() => Apply(tb, null));
        }

        private static void Apply(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;

            // Only show placeholder when real text is empty
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = GetDefaultText(tb);
                tb.Foreground = Brushes.Gray;
            }
        }

        private static void Remove(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;

            if (tb.Foreground == Brushes.Gray)
            {
                tb.Text = "";
                tb.Foreground = Brushes.White;
            }
        }
    }
}
