using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace Lager_automation.Views
{
    /// <summary>
    /// Interaction logic for InputDataTemplateWindow.xaml
    /// </summary>
    public partial class InputDataTemplateWindow : Window
    {
        private readonly DataTable _table;
        private ICollectionView _view;

        // column -> allowed values
        private readonly Dictionary<string, HashSet<string>> _filters = new();

        // column -> all values (cached once)
        private readonly Dictionary<string, List<string>> _allValues = new();

        public InputDataTemplateWindow(DataTable table)
        {
            InitializeComponent();

            _table = table;

            // Cache original values for filter UI
            CacheAllColumnValues();

            // Build rows as DataRowView list
            var rows = table.DefaultView.Cast<DataRowView>().ToList();

            _view = CollectionViewSource.GetDefaultView(rows);
            _view.Filter = RowPassesFilter;

            TemplateDataGrid.ItemsSource = _view;

            // 🔹 IMPORTANT: generate columns manually
            GenerateColumns();
        }



        private void CacheAllColumnValues()
        {
            _allValues.Clear();

            foreach (DataColumn col in _table.Columns)
            {
                _allValues[col.ColumnName] = _table.AsEnumerable()
                    .Select(r => r[col]?.ToString() ?? "")
                    .Distinct()
                    .OrderBy(v => v)
                    .ToList();
            }
        }

        private bool RowPassesFilter(object item)
        {
            if (item is not DataRowView row)
                return false;

            foreach (var kv in _filters)
            {
                var column = kv.Key;
                var allowed = kv.Value;

                if (allowed.Count == 0)
                    return false;

                var value = row[column]?.ToString() ?? "";
                if (!allowed.Contains(value))
                    return false;
            }

            return true;
        }




        private void RefreshFilter()
        {
            _view.Refresh();
        }

        private void OpenFilterPopup(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            string column = (string)button.Tag;

            if (!_filters.ContainsKey(column))
                _filters[column] = new HashSet<string>(_allValues[column]);

            var values = _allValues[column];
            var panel = new StackPanel { Background = Brushes.White };

            // buttons
            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

            var selectAll = new Button { Content = "Select all", Margin = new Thickness(0, 0, 5, 0) };
            var unselectAll = new Button { Content = "Unselect all" };

            btnPanel.Children.Add(selectAll);
            btnPanel.Children.Add(unselectAll);
            panel.Children.Add(btnPanel);
            panel.Children.Add(new Separator());

            var checkBoxes = new List<CheckBox>();

            foreach (var v in values)
            {
                var cb = new CheckBox
                {
                    Content = v,
                    IsChecked = _filters[column].Contains(v),
                    Margin = new Thickness(5, 2, 5, 2)
                };

                cb.Checked += (_, __) =>
                {
                    _filters[column].Add(v);
                    RefreshFilter();
                };

                cb.Unchecked += (_, __) =>
                {
                    _filters[column].Remove(v);
                    RefreshFilter();
                };

                checkBoxes.Add(cb);
                panel.Children.Add(cb);
            }

            selectAll.Click += (_, __) =>
            {
                _filters[column].Clear();
                foreach (var v in values)
                    _filters[column].Add(v);

                foreach (var cb in checkBoxes)
                    cb.IsChecked = true;

                RefreshFilter();
            };

            unselectAll.Click += (_, __) =>
            {
                _filters[column].Clear();

                foreach (var cb in checkBoxes)
                    cb.IsChecked = false;

                RefreshFilter();
            };

            new Popup
            {
                PlacementTarget = button,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                IsOpen = true,
                Child = new Border
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.White,
                    Child = new ScrollViewer
                    {
                        Content = panel,
                        MaxHeight = 300,
                        Width = 220
                    }
                }
            };
        }


        private void GenerateColumns()
        {
            TemplateDataGrid.Columns.Clear();

            foreach (DataColumn col in _table.Columns)
            {
                TemplateDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = CreateFilterHeader(col.ColumnName),
                    Binding = new Binding($"[{col.ColumnName}]"),
                    SortMemberPath = col.ColumnName
                });
            }
        }


        private object CreateFilterHeader(string columnName)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            panel.Children.Add(new TextBlock
            {
                Text = columnName,
                Margin = new Thickness(0, 0, 4, 0),
                VerticalAlignment = VerticalAlignment.Center
            });

            var button = new Button
            {
                Content = "▼",
                Width = 18,
                Height = 18,
                Padding = new Thickness(0),
                Margin = new Thickness(2, 0, 0, 0),
                Tag = columnName
            };

            button.Click += OpenFilterPopup;

            panel.Children.Add(button);

            return panel;
        }



    }
}
