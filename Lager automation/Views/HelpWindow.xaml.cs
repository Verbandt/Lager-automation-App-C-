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
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        private void RackHelpButton_Click(object sender, RoutedEventArgs e)
        {
            LoadHelpImages(
                "Images/Instructions/racks/racks_step1.png",
                "Images/Instructions/racks/racks_step2.png"
            );
        }

        private void FloorHelpButton_Click(object sender, RoutedEventArgs e)
        {
            LoadHelpImages(
                "Images/Help/rack_step1.png",
                "Images/Help/rack_step2.png",
                "Images/Help/rack_step3.png"
            );
        }

        private void ExcelHelpButton_Click(object sender, RoutedEventArgs e)
        {
            LoadHelpImages(
                "Images/Help/rack_step1.png",
                "Images/Help/rack_step2.png",
                "Images/Help/rack_step3.png"
            );
        }

        private void LoadHelpImages(params string[] imagePaths)
        {
            // Clear old content
            HelpContentPanel.Children.Clear();

            foreach (var path in imagePaths)
            {
                var img = new Image
                {
                    Source = new BitmapImage(new Uri($"pack://application:,,,/{path}", UriKind.Absolute)),
                    Margin = new Thickness(0, 0, 0, 20),
                    Stretch = Stretch.Uniform,
                    MaxWidth = 800   // keep readable
                };

                HelpContentPanel.Children.Add(img);
            }
        }
    }
}
