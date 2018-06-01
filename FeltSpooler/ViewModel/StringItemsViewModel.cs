using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeltSpooler.ViewModel
{
    using FeltAdmin.Viewmodels;

    public class StringItemsViewModel : ViewModelBase
    {
        private List<string> m_registrations;

        public StringItemsViewModel(string filepath, string fileName)
        {
            var myfilename = Path.Combine(filepath, fileName);
            if (File.Exists(myfilename))
            {
                var allLines = File.ReadAllLines(myfilename, Encoding.GetEncoding("iso-8859-1"));
                if (allLines.Any())
                {
                    Registrations = new List<string>(allLines);
                }
                else
                {
                    Registrations = new List<string> {"File is empty"};
                }
            }
            else
            {
                Registrations = new List<string> { string.Format("File not present {0}",fileName)};
            }
        }

        public List<string> Registrations
        {
            get { return m_registrations; }
            set
            {
                m_registrations = value;
                OnPropertyChanged("Registrations");
            }
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
