using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

using FeltAdmin.Database.Engine;
using FeltAdmin.Helpers;
using FeltAdmin.Viewmodels;

using Microsoft.Practices.Prism.Commands;

namespace FeltSpooler.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private Settings m_settings;

        private DelegateCommand m_openDbCommand;

        private DelegateCommand m_copyNextCommand;

        private string m_selectedPath;

        private List<string> m_allFilesList;

        private static readonly byte[] s_updFileContent = new byte[] { 0x4B };

        private const string LeonUpd = "KMI.UPD";
        private const string OrionUpd = "KMO.UPD";
        
        private const string LeonFile = "KMINEW.TXT";
        private const string OrionFile = "KMONEW.TXT";

        public string SelectedPath
        {
            get
            {
                return this.m_selectedPath;
            }
            set
            {
                this.m_selectedPath = value;
                if (DatabaseApi.SelectCompetition(m_selectedPath))
                {
                    this.Init();
                }

                this.OnPropertyChanged("SelectedPath");
            }
        }

        public ICommand OpenDbCommand
        {
            get
            {
                if (m_openDbCommand == null)
                {
                    m_openDbCommand = new DelegateCommand(this.OpenDbExecute);
                }

                return m_openDbCommand;
            }
        }

        public ICommand CopyNextCommand
        {
            get
            {
                if (m_copyNextCommand == null)
                {
                    m_copyNextCommand = new DelegateCommand(this.CopyNextExecute);
                }

                return m_copyNextCommand;
            }
        }

        private void CopyNextExecute()
        {
            if (m_allFilesList != null && m_allFilesList.Any())
            {
                var filename = m_allFilesList.First();
                var fi = new FileInfo(filename);
                var name = fi.Name;
                bool copied = false;
                if (name.Contains("Leon"))
                {
                    var path = m_settings.LeonCommunicationSetting.CommunicationSetup.SelectedPath;
                    copied = CopyFile(path, filename, LeonFile, LeonUpd);
                }
                else if (name.Contains("Minne"))
                {
                    var range = m_settings.OrionSetting.OrionViewModels.SelectMany(o => o.RangeViews).FirstOrDefault(r => r.MinneShooting == true);
                    if (range != null)
                    {
                        var path = range.CommunicationSetup.SelectedPath;
                        copied = CopyFile(path, filename, LeonFile, LeonUpd);
                    }
                    else
                    {
                        copied = true;
                    }
                }
                else if (name.Contains("Orion"))
                {
                    var tokens = filename.Split('_');
                    if (tokens.Length > 3)
                    {
                        int id;
                        if (int.TryParse(tokens[3], out id))
                        {
                            var orion = m_settings.OrionSetting.OrionViewModels.FirstOrDefault(o => o.OrionId == id);
                            if (orion != null)
                            {
                                var path = orion.CommunicationSetup.SelectedPath;
                                copied = CopyFile(path, filename, OrionFile, OrionUpd);
                            }
                        }
                    }
                }

                if (copied)
                {
                    m_allFilesList.RemoveAt(0);
                    this.OnPropertyChanged("AllFilesList");
                }
            }
        }

        private static bool CopyFile(string path, string filename, string outFileName, string outUpdFile)
        {
            var updFile = Path.Combine(path, outUpdFile);
            if (!File.Exists(updFile))
            {
                var kmin = Path.Combine(path, outFileName);
                File.Copy(filename, kmin);
                using (FileStream file = new FileStream(updFile, FileMode.Create, System.IO.FileAccess.Write))
                {
                    file.Write(s_updFileContent, 0, s_updFileContent.Length);
                    file.Flush(true);
                    file.Close();
                }

                return true;
            }

            return false;
        }

        public ObservableCollection<string> AllFilesList
        {
            get
            {
                if (m_allFilesList != null && m_allFilesList.Any())
                {
                    return new ObservableCollection<string>(m_allFilesList);
                }

                return null;
            }
        }

        private void OpenDbExecute()
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

        private void Init()
        {
            m_settings = SettingsHelper.GetSettings();
            if (m_settings != null)
            {
                var backupDir = Path.Combine(m_selectedPath, "Backup");
                if (Directory.Exists(backupDir))
                {
                    var files = Directory.EnumerateFiles(backupDir);
                    m_allFilesList = files.OrderBy(f => f).ToList();
                    this.OnPropertyChanged("AllFilesList");
                }
            }
        }
    }
}
