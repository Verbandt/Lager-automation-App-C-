using DocumentFormat.OpenXml.Office2019.Presentation;
using Lager_automation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public partial class AddRackWindow : Window
    {
        public string SectionName { get; set; } = string.Empty;
        public List<Part> SelectedParts { get; private set; } = new();
        public string SelectedCriteria { get; private set; } = string.Empty;

        public AddRackWindow()
        {
            InitializeComponent();
            LoadPartSelectors();
        }

        private void LoadPartSelectors()
        {
            PartSelectorPanel.Children.Clear();

            var orderedCategories = new List<string>
            {
                "Gavel",
                "Balk",
                "Genomskjutningsskydd"
            };

            foreach (var category in orderedCategories)
            {
                var label = new TextBlock
                {
                    Text = category,
                    Foreground = (Brush)Application.Current.Resources["TextBrush"],
                    FontSize = 14,
                    Margin = new Thickness(0, 10, 0, 5)
                };

                var combo = new ComboBox
                {
                    Width = 400,
                    FontSize = 14,
                    DisplayMemberPath = "PartName",
                    SelectedValuePath = "CodeName",
                    Margin = new Thickness(0, 0, 0, 5)
                };

                var parts = RacksCost.Parts.Values
                    .Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (category == "genomskjutningsskydd")
                {
                    parts.Insert(0, new Part("NONE", "-- Ingen --", category, 0, 0, category));
                }

                combo.ItemsSource = parts;
                combo.SelectedIndex = parts.Count > 0 ? 0 : -1;

                label.Margin = new Thickness(62, 30, 0, 6);

                PartSelectorPanel.Children.Add(label);
                PartSelectorPanel.Children.Add(combo);
            }

            var criteriaLabel = new TextBlock
            {
                Text = "Kriterie:",
                Foreground = (Brush)Application.Current.Resources["TextBrush"],
                FontSize = 14,
                Margin = new Thickness(62, 30, 0, 6)
            };

            var criteriaCombo = new ComboBox
            {
                Width = 400,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5),
                Name = "CriteriaSelector"
            };

            criteriaCombo.Items.Add("Resten");
            criteriaCombo.Items.Add("Fabrik");
            criteriaCombo.Items.Add("Utlastningsplats");
            criteriaCombo.Items.Add("Kund");
            criteriaCombo.Items.Add("EmbTyp");

            criteriaCombo.SelectedIndex = 0;

            PartSelectorPanel.Children.Add(criteriaLabel);
            PartSelectorPanel.Children.Add(criteriaCombo);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SelectedParts.Clear();
            Part? firstPart = null;

            // All ComboBoxes in the panel (3 parts + 1 criteria)
            var comboBoxes = PartSelectorPanel.Children.OfType<ComboBox>().ToList();
            if (comboBoxes.Count == 0)
                return;

            // The last ComboBox is always the criteria one
            var criteriaCombo = comboBoxes.Last();
            var partCombos = comboBoxes.Take(comboBoxes.Count - 1);

            // Collect selected parts
            foreach (var combo in partCombos)
            {
                if (combo.SelectedItem is Part part && part.CodeName != "NONE")
                {
                    SelectedParts.Add(part);
                    firstPart ??= part;
                }
            }

            if (firstPart == null)
            {
                MessageBox.Show("Välj minst en del innan du sparar.", "Fel",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SectionName = firstPart.PartName;

            // Save criteria for caller
            SelectedCriteria = criteriaCombo.SelectedItem as string ?? "Fabrik";

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        public void PreselectParts(SectionTemplate section)
        {
            var comboBoxes = PartSelectorPanel.Children.OfType<ComboBox>().ToList();
            if (comboBoxes.Count == 0)
                return;

            var criteriaCombo = comboBoxes.Last();
            var partCombos = comboBoxes.Take(comboBoxes.Count - 1);

            // Preselect parts by CodeName
            foreach (var combo in partCombos)
            {
                if (combo.ItemsSource is IEnumerable<Part> items)
                {
                    var matchCode = section.Parts
                        .Select(p => p.CodeName)
                        .FirstOrDefault(code => items.Any(i => i.CodeName == code));

                    if (matchCode != null)
                        combo.SelectedValue = matchCode;   // uses SelectedValuePath = "CodeName"
                }
            }

            // Preselect criteria
            if (!string.IsNullOrWhiteSpace(section.Criteria))
                criteriaCombo.SelectedItem = section.Criteria;
        }


    }
}
