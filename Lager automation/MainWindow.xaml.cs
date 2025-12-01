using Lager_automation.Models;
using Lager_automation.ViewModels;
using Lager_automation.Views;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Lager_automation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<SectionTemplate> _racks = new();
        private readonly List<FloorTemplate> _floors = new();
        public FilterViewModel RackFilter { get; } = new();
        public FilterViewModel FloorFilter { get; } = new();

        private bool _isAdjustingAspect;
        private const double AspectRatio = 16.0 / 9.0;

        private const int WM_SIZING = 0x0214;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }


        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();

            // use native WM_SIZING hook for smooth aspect locking
            this.SourceInitialized += OnSourceInitialized;
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            RunButton.IsEnabled = false;
            RunButton.Content = "Running...";

            try
            {
                var manager = new Manager();
                manager.BeginRackingProcess();
            }
            finally
            {
                Application.Current.Shutdown();
            }
        }

        private void AddRackButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddRackWindow { Owner = this };
            if (win.ShowDialog() == true)
            {
                var section = new SectionTemplate
                {
                    Name = win.SectionName,
                    Parts = win.SelectedParts,
                    Criteria = win.SelectedCriteria
                };

                section.ExtractPartNames();  // if you use this to fill Frame/Beam/BackCover

                AddRackToUI(section);
            }
        }
        private void AddFloorButton_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddFloorWIndow { Owner = this };
            if (win.ShowDialog() == true)
            {
                var floor = new FloorTemplate(win.SelectedProperties);

                AddFloorToUI(floor);
            }
        }

        private DockPanel CreateItemRow(ITemplate template, Action<DockPanel> onEdit, Action<DockPanel> onDelete)
        {
            var row = new DockPanel
            {
                Margin = new Thickness(5),
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(40, 40, 40))
            };

            var nameBlock = new TextBlock
            {
                Text = template.Name,
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 16,
                Margin = new Thickness(5)
            };

            var criteriaBlock = new TextBlock
            {
                Text = $"| {template.Criteria} |",
                Foreground = System.Windows.Media.Brushes.Orange,
                FontSize = 16,
                Margin = new Thickness(20, 5, 5, 5)
            };

            // edit button
            var editButton = new Button
            {
                Content = "✎",
                Width = 40,
                Height = 40,
                FontSize = 24,
                Margin = new Thickness(5, 0, 0, 0),
                Background = System.Windows.Media.Brushes.Transparent,
                Foreground = System.Windows.Media.Brushes.White,
                BorderThickness = new Thickness(0)
            };
            editButton.Click += (s, e) => onEdit(row);

            // delete button
            var deleteButton = new Button
            {
                Content = "🗑",
                Width = 40,
                Height = 40,
                FontSize = 20,
                Margin = new Thickness(5, 0, 0, 0),
                Background = System.Windows.Media.Brushes.Transparent,
                Foreground = System.Windows.Media.Brushes.IndianRed,
                BorderThickness = new Thickness(0)
            };
            deleteButton.Click += (s, e) => onDelete(row);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal };
            buttonPanel.Children.Add(editButton);
            buttonPanel.Children.Add(deleteButton);

            DockPanel.SetDock(buttonPanel, Dock.Right);

            row.Children.Add(buttonPanel);
            row.Children.Add(nameBlock);
            row.Children.Add(criteriaBlock);

            return row;
        }

        private void AddRackToUI(SectionTemplate rack)
        {
            _racks.Add(rack);

            var row = CreateItemRow(rack,
                rowPanel => EditRack(rack, rowPanel),
                rowPanel => DeleteRack(rack, rowPanel)
        );

            // Add to the ItemListControl's internal host panel
            RackListControl.ItemsHost.Children.Add(row);
            RackListControl.RegisterDraggable(row);
        }

        private void EditRack(SectionTemplate rack, DockPanel row)
        {
            // 1) Open the same window you use for adding
            var win = new AddRackWindow { Owner = this };

            win.PreselectParts(rack);

            if (win.ShowDialog() != true)
                return; // user cancelled

            // 2) Apply the edited values back to the model
            rack.Name = win.SectionName;
            rack.Parts = win.SelectedParts;
            rack.Criteria = win.SelectedCriteria;
            rack.ExtractPartNames();  // same as in AddRackButton_Click

            // 3) Update the UI row text
            var textBlocks = row.Children.OfType<TextBlock>().ToList();
            if (textBlocks.Count >= 2)
            {
                var nameBlock = textBlocks[0];
                var criteriaBlock = textBlocks[1];

                nameBlock.Text = rack.Name;
                criteriaBlock.Text = $"| {rack.Criteria} |";
            }
        }

        private void DeleteRack(SectionTemplate rack, DockPanel row)
        {
            if (MessageBox.Show($"Ta bort '{rack.Name}'?", "Bekräfta",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _racks.Remove(rack);
                RackListControl.ItemsHost.Children.Remove(row);
            }
        }

        private void AddFloorToUI(FloorTemplate floor)
        {
            // keep backing collection in sync
            _floors.Add(floor);

            var row = CreateItemRow(floor,
                rowPanel => EditFloor(floor, rowPanel),
                rowPanel => DeleteFloor(floor, rowPanel)
            );

            FloorListControl.ItemsHost.Children.Add(row);
            FloorListControl.RegisterDraggable(row);
        }

        private void EditFloor(FloorTemplate floor, DockPanel row)
        {
            var win = new AddFloorWIndow { Owner = this };

            // Pre-fill controls based on the current floor
            win.LoadExistingValues(floor);

            if (win.ShowDialog() != true)
                return; // user cancelled

            // win.SelectedProperties: [0]=height, [1]=tonnage, [2]=criteria
            var props = win.SelectedProperties;

            if (props.Count >= 3)
            {
                if (int.TryParse(props[0], out var h))
                    floor.HeightLimit = h;

                if (int.TryParse(props[1], out var ton))
                    floor.WeightLimitTonageM2 = ton;

                floor.Criteria = props[2];
                floor.Name = $"{floor.Name} mm";
            }

            // Update UI row
            var textBlocks = row.Children.OfType<TextBlock>().ToList();
            if (textBlocks.Count >= 2)
            {
                var nameBlock = textBlocks[0];
                var criteriaBlock = textBlocks[1];

                nameBlock.Text = floor.Name;
                criteriaBlock.Text = $"| {floor.Criteria} |";
            }
        }

        private void DeleteFloor(FloorTemplate floor, DockPanel row)
        {
            if (MessageBox.Show($"Ta bort '{floor.Name}'?", "Bekräfta",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _floors.Remove(floor);
                FloorListControl.ItemsHost.Children.Remove(row);
            }
        }

        private void Window_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (_isAdjustingAspect) return;

            try
            {
                _isAdjustingAspect = true;

                double widthDelta = Math.Abs(e.NewSize.Width - e.PreviousSize.Width);
                double heightDelta = Math.Abs(e.NewSize.Height - e.PreviousSize.Height);
                bool widthDriven = (e.PreviousSize.Width == 0 && e.PreviousSize.Height == 0) || widthDelta >= heightDelta;

                if (widthDriven)
                {
                    // adjust height to keep 16:9
                    Height = Math.Max(1, e.NewSize.Width / AspectRatio);
                }
                else
                {
                    // adjust width to keep 16:9
                    Width = Math.Max(1, e.NewSize.Height * AspectRatio);
                }
            }
            finally
            {
                _isAdjustingAspect = false;
            }
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var src = HwndSource.FromHwnd(hwnd);
            src?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WM_SIZING) return IntPtr.Zero;

            // wParam is the edge being dragged (1..8)
            int edge = wParam.ToInt32();
            var rect = Marshal.PtrToStructure<RECT>(lParam);

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            // keep width/height > 0
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            // compute desired sizes to preserve AspectRatio (width / height)
            int desiredWidth = (int)Math.Round(height * AspectRatio);
            int desiredHeight = (int)Math.Round(width / AspectRatio);

            RECT newRect = rect;

            // Adjust rectangle based on which edge/corner the user is dragging.
            // This modifies only one side so the resize feels natural.
            switch (edge)
            {
                case 1: // WMSZ_LEFT
                case 4: // WMSZ_TOPLEFT
                case 7: // WMSZ_BOTTOMLEFT
                    newRect.Left = newRect.Right - desiredWidth;
                    break;
                case 2: // WMSZ_RIGHT
                case 5: // WMSZ_TOPRIGHT
                case 8: // WMSZ_BOTTOMRIGHT
                    newRect.Right = newRect.Left + desiredWidth;
                    break;
                case 3: // WMSZ_TOP
                    newRect.Top = newRect.Bottom - desiredHeight;
                    break;
                case 6: // WMSZ_BOTTOM
                    newRect.Bottom = newRect.Top + desiredHeight;
                    break;
            }

            // write modified rect back to lParam
            Marshal.StructureToPtr(newRect, lParam, true);
            handled = false; // allow default window procedure to continue with the adjusted rect
            return IntPtr.Zero;
        }

        private void ImportExcel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}