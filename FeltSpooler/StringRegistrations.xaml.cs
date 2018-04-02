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

namespace FeltSpooler
{
    using FeltSpooler.ViewModel;

    /// <summary>
    /// Interaction logic for TextWindow.xaml
    /// </summary>
    public partial class StringRegistrations : Window
    {
        public StringRegistrations(StringItemsViewModel model)
        {
            DataContext = model;
            InitializeComponent();
            
        }
    }
}
