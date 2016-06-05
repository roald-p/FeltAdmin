using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using FeltAdmin.Viewmodels;

namespace FeltAdmin.TemplateSelectors
{
    public class RangeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RangeTemplate { get; set; }

        public DataTemplate PauseTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return RangeTemplate;
        }
    }
}
