using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Prism.Commands;

using FeltAdmin.Helpers;

namespace FeltAdmin.Viewmodels
{
    public class CommunicationSetup : ViewModelBase
    {
        private string m_selectedPath;

        private DelegateCommand m_openFileDialogCommand;

        public ICommand OpenFileDialogCommand
        {
            get
            {
                if (m_openFileDialogCommand == null)
                {
                    m_openFileDialogCommand = new DelegateCommand(this.OpenFileDialogExecute);
                }

                return m_openFileDialogCommand;
            }
        }

        public string SelectedPath
        {
            get
            {
                return this.m_selectedPath;
            }
            set
            {
                this.m_selectedPath = value;
                this.OnPropertyChanged("SelectedPath");
            }
        }

        public void OpenFileDialogExecute()
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (!string.IsNullOrWhiteSpace(SelectedPath) && Directory.Exists(SelectedPath))
                {
                    dlg.SelectedPath = SelectedPath;
                }

                var result = dlg.ShowDialog();
                if (result == DialogResult.OK)
                {
                    SelectedPath = dlg.SelectedPath;
                }
            }
        }
    }
}
