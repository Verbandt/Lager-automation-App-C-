using Lager_automation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lager_automation.Views
{
    /// <summary>
    /// Interaction logic for AddFloorWIndow.xaml
    /// </summary>
    public partial class AddFloorWIndow : Window
    {
        public string FloorName { get; set; } = string.Empty;
        public List<string> SelectedProperties { get; private set; } = new();

        public AddFloorWIndow()
        {
            InitializeComponent();
            PreLoadProperties();
        }

        public void PreLoadProperties()
        {
            FloorSelectorPanel.Children.Clear();

            // HEIGHT — numeric input
            AddNumericField("Maxhöjd (mm):", 1, 20000);

            // TONNAGE — numeric input
            AddNumericFieldWithUnlock("Tonnage per m²:", 0, 10);

            // 4. Criteria (default Factory)
            AddComboBox("Kriterier:", new List<string> { "Fabrik", "Kund", "EmbTyp" });
        }

        private void AddComboBox(string labelText, List<string> options)
        {
            var label = new TextBlock
            {
                Text = labelText,
                Foreground = Brushes.White,
                FontSize = 14,
                Margin = new Thickness(0, 10, 0, 5)
            };

            var combo = new ComboBox
            {
                Width = 400,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            foreach (var o in options)
                combo.Items.Add(o);

            combo.SelectedIndex = 0;

            FloorSelectorPanel.Children.Add(label);
            FloorSelectorPanel.Children.Add(combo);
        }

        private void AddNumericField(string labelText, int min, int max)
        {
            var label = new TextBlock
            {
                Text = labelText,
                Foreground = Brushes.White,
                FontSize = 14,
                Margin = new Thickness(0, 10, 0, 5)
            };

            var box = new TextBox
            {
                Width = 400,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5),
                Tag = (min, max) // store allowed range in Tag
            };

            // Restrict to digits only (no letters, no decimals)
            box.PreviewTextInput += NumericTextBox_PreviewTextInput;

            FloorSelectorPanel.Children.Add(label);
            FloorSelectorPanel.Children.Add(box);
        }

        private void AddNumericFieldWithUnlock(string labelText, int min, int max)
        {
            // Label
            var label = new TextBlock
            {
                Text = labelText,
                Foreground = Brushes.White,
                FontSize = 14,
                Margin = new Thickness(0, 10, 0, 5)
            };

            // Container
            var panel = new DockPanel { Margin = new Thickness(0, 0, 0, 5) };

            // Numeric field (locked by default)
            var box = new TextBox
            {
                Width = 350,
                FontSize = 14,
                IsReadOnly = true,                          // locked initially
                Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),  // gray background
                Foreground = Brushes.LightGray,             // gray text color
                Tag = (min, max),
                Text = ""                                   // no default number!
            };

            // Allow digits only when unlocked
            box.PreviewTextInput += (s, e) =>
            {
                if (!box.IsReadOnly)
                    e.Handled = !e.Text.All(char.IsDigit);
                else
                    e.Handled = true; // never allow typing when locked
            };

            // Unlock button
            var unlockButton = new Button
            {
                Content = "🔒",        // locked state
                Width = 35,
                Height = 24,
                Margin = new Thickness(5, 0, 0, 0)
            };

            unlockButton.Click += (s, e) =>
            {
                box.IsReadOnly = !box.IsReadOnly;

                if (box.IsReadOnly)
                {
                    // 🔒 Locked state
                    unlockButton.Content = "🔒";
                    box.Background = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                    box.Foreground = Brushes.LightGray;

                    // Clear any user-entered value
                    box.Text = "";
                }
                else
                {
                    // 🔓 Unlocked state
                    unlockButton.Content = "🔓";
                    box.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                    box.Foreground = Brushes.White;

                    // Focus field for typing
                    box.Focus();
                }
            };

            DockPanel.SetDock(unlockButton, Dock.Right);

            panel.Children.Add(unlockButton);
            panel.Children.Add(box);

            FloorSelectorPanel.Children.Add(label);
            FloorSelectorPanel.Children.Add(panel);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SelectedProperties.Clear();

            int heightLimit = 20000;
            int tonnageLimit = 10;

            // --- Get HEIGHT (the first TextBox in the panel) ---
            var heightBox = FloorSelectorPanel.Children
                .OfType<TextBox>()
                .First();

            string heightText = heightBox.Text;

            // Validate height
            if (string.IsNullOrWhiteSpace(heightText))
            {
                MessageBox.Show("Maxhöjd får inte vara tomt.", "Fel",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(heightText, @"^[1-9][0-9]*$") ||
                !int.TryParse(heightText, out int height) ||
                height < 1 || height > heightLimit)
            {
                MessageBox.Show($"Maxhöjd måste vara ett heltal mellan 1 och {heightLimit}",
                    "Fel", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            // --- Get TONNAGE (the TextBox inside the DockPanel) ---
            var tonnageBox = FloorSelectorPanel.Children
                .OfType<DockPanel>()
                .SelectMany(dp => dp.Children.OfType<TextBox>())
                .First();

            string tonnageText;

            if (tonnageBox.IsReadOnly)
            {
                // Locked → always "0"
                tonnageText = "0";
            }
            else
            {
                tonnageText = tonnageBox.Text;

                if (!Regex.IsMatch(tonnageText, @"^([1-9]|10)$") ||
                    !int.TryParse(tonnageText, out int tonnage) ||
                    tonnage < 0 || tonnage > tonnageLimit)
                {
                    MessageBox.Show("Ton måste vara ett heltal mellan 1 och 10.",
                        "Fel", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }


            // --- Get CRITERIA (the only ComboBox used for criteria) ---
            var criteriaCombo = FloorSelectorPanel.Children
                .OfType<ComboBox>()
                .Last(); // the second ComboBox is criteria

            string criteriaValue = criteriaCombo.SelectedItem as string ?? "";


            // --- Store in correct order ---
            SelectedProperties.Add(heightText);   // 0
            SelectedProperties.Add(tonnageText);  // 1
            SelectedProperties.Add(criteriaValue); // 2

            // Name is based on height
            FloorName = $"{heightText} mm";

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow digits 0–9
            e.Handled = !e.Text.All(char.IsDigit);
        }

        public void LoadExistingValues(FloorTemplate floor)
        {
            // --------------------------
            // 1. HEIGHT (TextBox #1)
            // --------------------------
            var heightBox = FloorSelectorPanel.Children
                .OfType<TextBox>()
                .First();

            heightBox.Text = floor.HeightLimit.ToString();


            // --------------------------
            // 2. TONNAGE (DockPanel → TextBox)
            // --------------------------
            var tonnagePanel = FloorSelectorPanel.Children
                .OfType<DockPanel>()
                .First();

            var tonnageBox = tonnagePanel.Children
                .OfType<TextBox>()
                .First();

            var unlockButton = tonnagePanel.Children
                .OfType<Button>()
                .First();

            if (floor.WeightLimitTonageM2 == 0)
            {
                // Locked state
                tonnageBox.IsReadOnly = true;
                tonnageBox.Text = "";
                unlockButton.Content = "🔓";
                tonnageBox.Background = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                tonnageBox.Foreground = Brushes.LightGray;
            }
            else
            {
                // Unlocked state
                tonnageBox.IsReadOnly = false;
                tonnageBox.Text = floor.WeightLimitTonageM2.ToString();
                unlockButton.Content = "🔒";
                tonnageBox.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                tonnageBox.Foreground = Brushes.White;
            }


            // --------------------------
            // 3. CRITERIA (last ComboBox)
            // --------------------------
            var criteriaCombo = FloorSelectorPanel.Children
                .OfType<ComboBox>()
                .Last();

            criteriaCombo.SelectedItem = floor.Criteria;
        }

    }
}
