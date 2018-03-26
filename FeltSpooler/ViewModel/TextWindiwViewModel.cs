using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeltSpooler.ViewModel
{
    using FeltAdmin.Viewmodels;

    public class TextWindiwViewModel : ViewModelBase
    {

        public TextWindiwViewModel(string filename)
        {
            
        }

        private string m_text;

        public string Text
        {
            get
            {
                return this.m_text;
            }
            set
            {
                this.m_text = value;

                this.OnPropertyChanged("Text");
            }
        }
    }
}
