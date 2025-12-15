using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lager_automation.Controls
{
    public partial class ItemListControl : UserControl
    {
        private UIElement? _draggedItem;
        private int _sourceIndex = -1;
        private Border? _placeholder;
        private int _currentTargetIndex = -1;
        private readonly Dictionary<UIElement, TranslateTransform> _transforms = new();
        private bool _isSourceRemoved; // track if we removed the source from the panel during drag
        private bool _dropHandled;

        public bool IsAddButtonEnabled
        {
            get => AddButton.IsEnabled;
            set => AddButton.IsEnabled = value;
        }

        public ItemListControl()
        {
            InitializeComponent();

            // handle drag operations that target the panel itself
            // ItemsPanel.DragOver += ItemsPanel_DragOver;
            // ItemsPanel.Drop += ItemsPanel_Drop;
            // ItemsPanel.DragLeave += ItemsPanel_DragLeave;

            ItemsPanel.AllowDrop = true;
            this.AddHandler(UIElement.DragOverEvent, new DragEventHandler(ItemsPanel_DragOver), true);
            this.AddHandler(UIElement.DropEvent, new DragEventHandler(ItemsPanel_Drop), true);

        }

        // Title property for header text
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(ItemListControl), new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        // Expose the internal ItemsPanel so callers can add UI elements
        public StackPanel ItemsHost => ItemsPanel;

        public event RoutedEventHandler? AddClicked;

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddClicked?.Invoke(this, e);
        }

        public void RegisterDraggable(UIElement element)
        {
            if (element == null) return;

            element.PreviewMouseLeftButtonDown += Item_PreviewMouseLeftButtonDown;
            element.PreviewMouseMove += Item_PreviewMouseMove;
            element.PreviewMouseLeftButtonUp += Item_PreviewMouseLeftButtonUp;

            // ensure each child has a transform we can animate
            EnsureTransform(element);
        }

        private void EnsureTransform(UIElement el)
        {
            if (!_transforms.ContainsKey(el))
            {
                var tt = new TranslateTransform(0, 0);
                el.RenderTransform = tt;
                _transforms[el] = tt;
            }
        }

        private void Item_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If clicking anywhere inside a Button → do NOT drag
            if (IsClickInsideButton(e.OriginalSource as DependencyObject))
                return;

            _draggedItem = sender as UIElement;
            if (_draggedItem == null) return;

            _sourceIndex = ItemsPanel.Children.IndexOf(_draggedItem);
            _currentTargetIndex = _sourceIndex;
            _isSourceRemoved = false;

            double h = Math.Max(1, (_draggedItem as FrameworkElement)?.ActualHeight ?? 40);
            _placeholder = new Border
            {
                Height = h,
                Background = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255)),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Margin = (_draggedItem as FrameworkElement)?.Margin ?? new Thickness(5)
            };
        }

        private void Item_PreviewMouseMove(object? sender, MouseEventArgs e)
        {
            if (_draggedItem == null || e.LeftButton != MouseButtonState.Pressed) return;

            // prevent double execution — this is the main fix
            if (_placeholder != null && ItemsPanel.Children.Contains(_placeholder))
                return;

            // start drag if moved enough
            var pos = e.GetPosition(this);
            // use a small threshold to avoid accidental drags
            if (Math.Abs(pos.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(pos.Y) < SystemParameters.MinimumVerticalDragDistance) return;

            // insert placeholder at source position if not already present
            if (_placeholder != null && !ItemsPanel.Children.Contains(_placeholder))
            {
                // remove the actual element from the panel and keep it aside while dragging
                if (ItemsPanel.Children.Contains(_draggedItem))
                {
                    ItemsPanel.Children.Remove(_draggedItem);
                    _isSourceRemoved = true;
                }

                ItemsPanel.Children.Insert(_sourceIndex, _placeholder);

                // reduce opacity of dragged item so user sees placeholder (even though removed this helps if it's visible elsewhere)
                if (_draggedItem is FrameworkElement fe) fe.Opacity = 0.6;
            }

            // Start the WPF DragDrop. This blocks until drop completes.
            var data = new DataObject("Item", _draggedItem);
            // reset flag before starting drag
            _dropHandled = false;

            DragDrop.DoDragDrop(_draggedItem, data, DragDropEffects.Move);

            // 👉 When we get here, the drag is finished.
            // If NO drop inside our control happened, we must restore the item.
            if (!_dropHandled && _draggedItem != null)
            {
                // treat like a cancelled drag: put it back where it came from
                if (_isSourceRemoved)
                {
                    int safeIndex = Math.Max(0, Math.Min(ItemsPanel.Children.Count, _sourceIndex));
                    ItemsPanel.Children.Insert(safeIndex, _draggedItem);
                }

                CleanupDrag(cancelled: false); // we already reinserted, so not "cancelled" anymore
            }
        }

        private void Item_PreviewMouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            // if user releases without dropping on panel, ensure cleanup
            CleanupDrag(cancelled: true);
        }

        private void ItemsPanel_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Item")) return;
            e.Effects = DragDropEffects.Move;

            var pos = e.GetPosition(ItemsPanel);

            // determine index based on vertical midpoint of children
            int targetIndex = 0;
            double y = pos.Y;

            // FAILSAFE: If mouse is outside panel, snap placeholder
            if (pos.Y < 0)
            {
                MovePlaceholderTo(0);
                _currentTargetIndex = 0;
                return;
            }

            if (pos.Y > ItemsPanel.ActualHeight)
            {
                int bottomIndex = ItemsPanel.Children.Count - (ItemsPanel.Children.Contains(_placeholder) ? 1 : 0);
                MovePlaceholderTo(bottomIndex);
                _currentTargetIndex = bottomIndex;
                return;
            }

            for (int i = 0; i < ItemsPanel.Children.Count; i++)
            {
                var child = ItemsPanel.Children[i] as FrameworkElement;
                if (child == null || child == _placeholder) continue;
                double top = child.TranslatePoint(new Point(0, 0), ItemsPanel).Y;
                double mid = top + child.ActualHeight / 2;
                if (y > mid) targetIndex = i + 1;
            }

            // clamp
            targetIndex = Math.Max(0, Math.Min(ItemsPanel.Children.Count - (ItemsPanel.Children.Contains(_placeholder) ? 1 : 0), targetIndex));

            if (targetIndex != _currentTargetIndex)
            {
                MovePlaceholderTo(targetIndex);
                _currentTargetIndex = targetIndex;
            }
        }

        private void MovePlaceholderTo(int newIndex)
        {
            if (_placeholder == null) return;
            int oldIndex = ItemsPanel.Children.IndexOf(_placeholder);
            if (oldIndex == newIndex) return;

            // compute direction
            bool movingDown = newIndex > oldIndex;

            // remove placeholder then insert at newIndex (account for removal)
            ItemsPanel.Children.Remove(_placeholder);
            ItemsPanel.Children.Insert(newIndex, _placeholder);

            // animate shifted children between oldIndex and newIndex
            int start = Math.Min(oldIndex, newIndex);
            int end = Math.Max(oldIndex, newIndex);

            double delta = _placeholder.Height;

            for (int i = 0; i < ItemsPanel.Children.Count; i++)
            {
                var child = ItemsPanel.Children[i] as UIElement;
                if (child == null || child == _placeholder || child == _draggedItem) continue;
                EnsureTransform(child);

                // determine if child should move temporarily
                if (i >= start && i <= end)
                {
                    // if movingDown, children that were between oldIndex+1..newIndex shift up by -delta
                    // if movingUp, children that were between newIndex..oldIndex-1 shift down by +delta
                    double to = 0;
                    if (movingDown)
                    {
                        // children that now occupy earlier slots need to move up
                        if (i >= start && i < newIndex) to = -delta;
                    }
                    else
                    {
                        if (i > newIndex && i <= end) to = delta;
                    }

                    AnimateTranslateY(_transforms[child], to);
                }
                else
                {
                    AnimateTranslateY(_transforms[child], 0);
                }
            }
        }

        private void AnimateTranslateY(TranslateTransform tt, double to)
        {
            var anim = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(160),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            tt.BeginAnimation(TranslateTransform.YProperty, anim);
        }

        private void ItemsPanel_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Item")) { CleanupDrag(cancelled: true); return; }

            var source = e.Data.GetData("Item") as UIElement;
            if (source == null) { CleanupDrag(cancelled: true); return; }

            // final target index is where the placeholder is
            int insertIndex = _placeholder != null ? ItemsPanel.Children.IndexOf(_placeholder) : ItemsPanel.Children.Count;

            // if the source is still present (was not removed earlier) remove it
            int srcIndex = ItemsPanel.Children.IndexOf(source);
            if (srcIndex >= 0) ItemsPanel.Children.RemoveAt(srcIndex);

            // remove placeholder if present
            if (_placeholder != null && ItemsPanel.Children.Contains(_placeholder))
            {
                ItemsPanel.Children.Remove(_placeholder);
            }

            // insert source at final index
            insertIndex = Math.Max(0, Math.Min(ItemsPanel.Children.Count, insertIndex));
            ItemsPanel.Children.Insert(insertIndex, source);

            // animate source into place (reset transforms)
            EnsureTransform(source);
            AnimateTranslateY(_transforms[source], 0);

            // persist final ordering if needed: fire an event or leave it to the owner code (MainWindow has backing list)
            CleanupDrag(cancelled: false);
        }

        private void ItemsPanel_DragLeave(object? sender, DragEventArgs e)
        {
            // if leaving the panel entirely, animate everything back
            // but do not remove placeholder immediately in case user re-enters
            // we'll cancel if release occurs outside
        }

        private void CleanupDrag(bool cancelled)
        {
            // restore opacity and clear placeholder and transforms
            if (_draggedItem is FrameworkElement fe) fe.Opacity = 1.0;

            // if the drag was cancelled and we removed the source from the panel, reinsert it at original position
            if (cancelled && _isSourceRemoved && _draggedItem != null)
            {
                int safeIndex = Math.Max(0, Math.Min(ItemsPanel.Children.Count, _sourceIndex));
                ItemsPanel.Children.Insert(safeIndex, _draggedItem);
            }

            // animate all transforms back to zero
            foreach (var kv in _transforms.ToList())
            {
                AnimateTranslateY(kv.Value, 0);
            }

            // remove placeholder
            if (_placeholder != null && ItemsPanel.Children.Contains(_placeholder))
            {
                ItemsPanel.Children.Remove(_placeholder);
            }

            _draggedItem = null;
            _sourceIndex = -1;
            _currentTargetIndex = -1;
            _placeholder = null;
            _isSourceRemoved = false;
        }

        private bool IsClickInsideButton(DependencyObject source)
        {
            while (source != null)
            {
                if (source is Button)
                    return true;

                source = VisualTreeHelper.GetParent(source);
            }
            return false;
        }
    }
}