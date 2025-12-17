using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
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

        private readonly Dictionary<string, Button> _filterButtons = new();
        private const string FilterOffIcon = "⏷";
        private const string FilterOnIcon = "▼";

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

            // Ensure filter exists
            if (!_filters.ContainsKey(column))
                _filters[column] = new HashSet<string>(_allValues[column]);

            // 🔒 Transaction copy (popup-local state)
            var tempFilter = new HashSet<string>(_filters[column]);
            var values = _allValues[column];

            // =========================
            // TOP PANEL
            // =========================
            var topPanel = new StackPanel { Background = Brushes.White };

            var searchBox = new TextBox
            {
                Margin = new Thickness(5),
                Height = 24,
                ToolTip = "Sök värden..."
            };
            topPanel.Children.Add(searchBox);

            var clearFilterBtn = new Button
            {
                Content = "Rensa filter",
                Margin = new Thickness(5)
            };
            topPanel.Children.Add(clearFilterBtn);

            var selectAllCheckBox = new CheckBox
            {
                Content = "Markera alla",
                Margin = new Thickness(5, 2, 5, 5),
                FontWeight = FontWeights.SemiBold
            };
            topPanel.Children.Add(selectAllCheckBox);

            topPanel.Children.Add(new Separator());

            // =========================
            // CHECKBOX LIST
            // =========================
            var checkboxPanel = new StackPanel();
            var checkBoxes = new List<CheckBox>();

            bool updatingSelectAll = false;

            void SetMarkeraAlla(bool? state)
            {
                updatingSelectAll = true;
                selectAllCheckBox.IsChecked = state;
                updatingSelectAll = false;
            }

            foreach (var v in values)
            {
                var cb = new CheckBox
                {
                    Content = v,
                    Margin = new Thickness(5, 2, 5, 2),
                    IsChecked = tempFilter.Contains(v)
                };

                cb.Checked += (_, __) =>
                {
                    tempFilter.Add(v);
                    SetMarkeraAlla(tempFilter.Count == values.Count);
                };

                cb.Unchecked += (_, __) =>
                {
                    tempFilter.Remove(v);
                    SetMarkeraAlla(false);
                };

                checkBoxes.Add(cb);
                checkboxPanel.Children.Add(cb);
            }

            SetMarkeraAlla(tempFilter.Count == values.Count);

            selectAllCheckBox.Checked += (_, __) =>
            {
                if (updatingSelectAll) return;

                tempFilter.Clear();
                foreach (var v in values)
                    tempFilter.Add(v);

                foreach (var cb in checkBoxes)
                    cb.IsChecked = true;
            };

            selectAllCheckBox.Unchecked += (_, __) =>
            {
                if (updatingSelectAll) return;

                tempFilter.Clear();
                foreach (var cb in checkBoxes)
                    cb.IsChecked = false;
            };

            searchBox.TextChanged += (_, __) =>
            {
                string search = searchBox.Text.Trim();

                foreach (var cb in checkBoxes)
                {
                    var text = cb.Content?.ToString() ?? "";
                    bool visible =
                        string.IsNullOrEmpty(search) ||
                        text.Contains(search, StringComparison.OrdinalIgnoreCase);

                    cb.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

                    // 🔹 NEW: auto-check visible items (transaction-only)
                    if (visible)
                    {
                        cb.IsChecked = true;
                        tempFilter.Add(text);
                    }
                }

                // Update "Markera alla" checkbox correctly
                SetMarkeraAlla(tempFilter.Count == values.Count);
            };

          
            // =========================
            // BOTTOM PANEL
            // =========================
            var okBtn = new Button
            {
                Content = "OK",
                Width = 70,
                Margin = new Thickness(0, 0, 5, 0)
            };

            var cancelBtn = new Button
            {
                Content = "Avbryt",
                Width = 70
            };

            var bottomPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5)
            };

            bottomPanel.Children.Add(okBtn);
            bottomPanel.Children.Add(cancelBtn);

            // =========================
            // ROOT GRID
            // =========================
            var rootGrid = new Grid
            {
                Width = 260,
                Background = Brushes.White
            };

            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid.SetRow(topPanel, 0);
            rootGrid.Children.Add(topPanel);

            var scrollViewer = new ScrollViewer
            {
                Content = checkboxPanel,
                MaxHeight = 250
            };
            Grid.SetRow(scrollViewer, 1);
            rootGrid.Children.Add(scrollViewer);

            Grid.SetRow(bottomPanel, 2);
            rootGrid.Children.Add(bottomPanel);

            // =========================
            // POPUP
            // =========================
            bool applyChanges = false;
            Popup popup = null!;

            // 🔹 SHARED OK LOGIC (used by OK + Enter)
            Action applyOk = () =>
            {
                applyChanges = true;

                tempFilter.Clear();
                foreach (var cb in checkBoxes)
                {
                    if (cb.Visibility == Visibility.Visible && cb.IsChecked == true)
                        tempFilter.Add(cb.Content?.ToString() ?? "");
                }

                popup.IsOpen = false;
            };

            clearFilterBtn.Click += (_, __) =>
            {
                // Reset selection
                tempFilter.Clear();
                foreach (var v in values)
                    tempFilter.Add(v);

                // Reset UI
                searchBox.Text = "";
                SetMarkeraAlla(true);

                foreach (var cb in checkBoxes)
                {
                    cb.Visibility = Visibility.Visible;
                    cb.IsChecked = true;
                }

                // 🔹 Apply immediately (same as OK)
                applyOk();
            };

            okBtn.Click += (_, __) => applyOk();

            cancelBtn.Click += (_, __) =>
            {
                applyChanges = false;
                popup.IsOpen = false;
            };

            popup = new Popup
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
                    Child = rootGrid
                }
            };

            // 🔑 ENTER = OK, ESC = Cancel (robust)
            rootGrid.AddHandler(
                Keyboard.PreviewKeyDownEvent,
                new KeyEventHandler((s, e2) =>
                {
                    if (e2.Key == Key.Enter)
                    {
                        applyOk();      // EXACT same logic as OK
                        e2.Handled = true;
                    }
                    else if (e2.Key == Key.Escape)
                    {
                        applyChanges = false;
                        popup.IsOpen = false;
                        e2.Handled = true;
                    }
                }),
                true
            );

            // =========================
            // COMMIT / ROLLBACK
            // =========================
            popup.Closed += (_, __) =>
            {
                if (!applyChanges)
                    return;

                _filters[column] = new HashSet<string>(tempFilter);
                RefreshFilter();
                UpdateFilterIcon(column);
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
            var grid = new Grid();

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var text = new TextBlock
            {
                Text = columnName,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(text, 0);
            grid.Children.Add(text);

            var button = new Button
            {
                Content = "▼",
                Width = 18,
                Height = 18,
                Padding = new Thickness(0),
                Tag = columnName,
                Margin = new Thickness(6, 0, 2, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            button.Click += OpenFilterPopup;

            Grid.SetColumn(button, 1);
            grid.Children.Add(button);

            return grid;
        }

        private void UpdateFilterIcon(string column)
        {
            if (!_filterButtons.TryGetValue(column, out var button))
                return;

            // no filter or full selection → normal icon
            if (!_filters.ContainsKey(column) ||
                _filters[column].Count == _allValues[column].Count)
            {
                button.Content = FilterOffIcon;
            }
            else
            {
                // active filter
                button.Content = FilterOnIcon;
            }
        }


    }
}
