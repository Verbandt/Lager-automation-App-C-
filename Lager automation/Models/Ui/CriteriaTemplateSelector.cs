using Lager_automation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Lager_automation.Models
{
    public class CriteriaTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? FactoryTemplate { get; set; }
        public DataTemplate? CustomerTemplate { get; set; }
        public DataTemplate? StackingHeightTemplate { get; set; }
        public DataTemplate? TheRestTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is FilterViewModel vm)
            {
                return vm.SelectedCriteria switch
                {
                    FilterCriteria.Factory => FactoryTemplate,
                    FilterCriteria.Customer => CustomerTemplate,
                    FilterCriteria.StackingHeight => StackingHeightTemplate,
                    FilterCriteria.TheRest => TheRestTemplate,
                    _ => null
                };
            }
            return null;
        }
    }
}
