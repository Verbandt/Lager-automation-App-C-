using System.Data;
using System.Windows;

namespace Lager_automation.Views
{
    /// <summary>
    /// Interaction logic for InputDataTemplateWindow.xaml
    /// </summary>
    public partial class InputDataTemplateWindow : Window
    {
        public InputDataTemplateWindow(DataTable table)
        {
            InitializeComponent();
            TemplateDataGrid.ItemsSource = table.DefaultView;
        }
    }
}
