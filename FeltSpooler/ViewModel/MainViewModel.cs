using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using FeltAdmin.Configuration;
using FeltAdmin.Database.Engine;
using FeltAdmin.Helpers;
using FeltAdmin.Viewmodels;

using Microsoft.Practices.Prism.Commands;

namespace FeltSpooler.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private DispatcherTimer m_dispatcherTimer;

        private Settings m_settings;

        private DelegateCommand m_openDbCommand;

        private DelegateCommand m_copyNextCommand;

        private DelegateCommand m_viewOrionCommand;

        private DelegateCommand m_deleteOrionCommand;

        private string m_selectedPath;

        private List<FileNameInfo> m_allFilesList;

        private static readonly byte[] s_updFileContent = new byte[] { 0x4B };

        private const string LeonUpd = "KMI.UPD";
        private const string OrionUpd = "KMO.UPD";
        
        private const string LeonFile = "KMINEW.TXT";
        private const string OrionFile = "KMONEW.TXT";

        private ObservableCollection<string> m_orionPaths;

        public ObservableCollection<string>  OrionPaths
        {
            get
            {
                return this.m_orionPaths;
            }
            set
            {
                this.m_orionPaths = value;
               
                this.OnPropertyChanged("OrionPaths");
            }
        }

        private string m_selectedOrionPath;

        public string SelectedOrionPath
        {
            get
            {
                return this.m_selectedOrionPath;
            }
            set
            {
                this.m_selectedOrionPath = value;
               
                this.OnPropertyChanged("SelectedOrionPath");
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

        public ICommand ViewOrionCommand
        {
            get
            {
                if (m_viewOrionCommand == null)
                {
                    m_viewOrionCommand = new DelegateCommand(this.ViewOrionExecute);
                }

                return m_viewOrionCommand;
            }
        }

        public ICommand DeleteOrionCommand
        {
            get
            {
                if (m_deleteOrionCommand == null)
                {
                    m_deleteOrionCommand = new DelegateCommand(this.DeleteOrionExecute);
                }

                return m_deleteOrionCommand;
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

        public bool SimulateOrion { get; set; }
        public bool SimulateLeon { get; set; }
        private void CopyNextExecute()
        {
            if (m_allFilesList != null && m_allFilesList.Any())
            {
                var fileinfo = m_allFilesList.First();
                var fi = new FileInfo(fileinfo.FileName);
                var name = fi.Name;
                bool copied = false;
                if (name.Contains("Leon"))
                {
                    var path = m_settings.LeonCommunicationSetting.CommunicationSetup.SelectedPath;
                    copied = CopyFile(path, fileinfo.FileName, LeonFile, LeonUpd);
                }
                else if (name.Contains("Minne"))
                {
                    var range = m_settings.OrionSetting.OrionViewModels.SelectMany(o => o.RangeViews).FirstOrDefault(r => r.MinneShooting == true);
                    if (range != null)
                    {
                        var path = range.CommunicationSetup.SelectedPath;
                        copied = CopyFile(path, fileinfo.FileName, LeonFile, LeonUpd);
                    }
                    else
                    {
                        copied = true;
                    }
                }
                else if (name.Contains("Orion"))
                {
                    var tokens = fileinfo.FileName.Split('_');
                    if (tokens.Length > 3)
                    {
                        int id;
                        if (int.TryParse(tokens[4], out id))
                        {
                            var orion = m_settings.OrionSetting.OrionViewModels.FirstOrDefault(o => o.OrionId == id);
                            if (orion != null)
                            {
                                string[]  allLines=File.ReadAllLines(fileinfo.FileName);
                                SetTeamNumber(allLines);
                                var path = orion.CommunicationSetup.SelectedPath;
                                copied = CopyFile(path, fileinfo.FileName, OrionFile, OrionUpd);
                            }
                        }
                        else
                        {
                            m_allFilesList.RemoveAt(0);
                            this.OnPropertyChanged("AllFilesList");
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

        private void SetTeamNumber(string[] allLines)
        {
            int minTeam = 20000;
            int maxteam = 0;

            if (allLines != null)
            {
                foreach (var line in allLines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        var elements = line.Split(new[] { ';' });
                        if (elements.Length >= 2)
                        {
                            int team = -1;
                            if (int.TryParse(elements[1], out team))
                            {
                                if (team > maxteam)
                                {
                                    maxteam = team;
                                }
                                if (team < minTeam)
                                {
                                    minTeam = team;
                                }
                            }
                        }
                    }
                }
            }

            this.TeamNumber = string.Format("{0}-{1}", minTeam, maxteam);
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

        public ObservableCollection<FileNameInfo> AllFilesList
        {
            get
            {
                if (m_allFilesList != null && m_allFilesList.Any())
                {
                    return new ObservableCollection<FileNameInfo>(m_allFilesList);
                }

                return null;
            }
        }

        private string m_TeamNumber;

        public string TeamNumber
        {
            get
            {
                return m_TeamNumber;
            }

            set
            {
                  m_TeamNumber=value;
                this.OnPropertyChanged("TeamNumber");
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
                else
                {
                    var databasePath = ConfigurationLoader.GetAppSettingsValue("DatabasePath");
                    if (!string.IsNullOrWhiteSpace(databasePath) && Directory.Exists(databasePath))
                    {
                        dlg.SelectedPath = databasePath;
                    }
                }

                var result = dlg.ShowDialog();
                if (result == DialogResult.OK)
                {
                    SelectedPath = dlg.SelectedPath;
                    if (m_dispatcherTimer == null)
                    {
                        m_dispatcherTimer = new DispatcherTimer();
                        m_dispatcherTimer.Tick += dispatcherTimer_Tick;
                        m_dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
                        m_dispatcherTimer.Start();
                    }
                }
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // delete orion input files
            if (SimulateOrion)
            {
                foreach (var orionPath in m_orionPaths)
                {
                    if (Directory.Exists(orionPath))
                    {
                        var updFile = Path.Combine(orionPath, "KMI.UPD");
                        if (File.Exists(updFile))
                        {
                            DeleteFile(orionPath, "KMINEW.TXT");
                            DeleteFile(orionPath, "KMI.UPD");
                        }
                    }
                }
            }

            if (SimulateLeon)
            {
                if (m_settings != null && m_settings.LeonCommunicationSetting != null &&
                    m_settings.LeonCommunicationSetting.CommunicationSetup != null &&
                    !string.IsNullOrWhiteSpace(m_settings.LeonCommunicationSetting.CommunicationSetup.SelectedPath))
                {
                    if (Directory.Exists(m_settings.LeonCommunicationSetting.CommunicationSetup.SelectedPath))
                    {
                        var updFile = Path.Combine(m_settings.LeonCommunicationSetting.CommunicationSetup.SelectedPath, "KMO.UPD");
                        if (File.Exists(updFile))
                        {
                            DeleteFile(m_settings.LeonCommunicationSetting.CommunicationSetup.SelectedPath, "KMONEW.TXT");
                            DeleteFile(m_settings.LeonCommunicationSetting.CommunicationSetup.SelectedPath, "KMO.UPD");
                        }
                    }
                }

                var minne = m_settings.OrionSetting.OrionViewModels.SelectMany(r => r.RangeViews).SingleOrDefault(r => r.MinneShooting);
                if (minne != null)
                {
                    if (Directory.Exists(minne.CommunicationSetup.SelectedPath))
                    {
                        var updFile = Path.Combine(minne.CommunicationSetup.SelectedPath, "KMO.UPD");
                        if (File.Exists(updFile))
                        {
                            DeleteFile(minne.CommunicationSetup.SelectedPath, "KMONEW.TXT");
                            DeleteFile(minne.CommunicationSetup.SelectedPath, "KMO.UPD");
                        }
                    }
                }
            }
        }

        private void ViewOrionExecute()
        {
            if (!string.IsNullOrEmpty(this.m_selectedOrionPath))
            {

                if (Directory.Exists(this.m_selectedOrionPath))
                {
                    var viewModel = new StringItemsViewModel(m_selectedOrionPath, "KMINEW.TXT");
                    var tet = new StringRegistrations(viewModel);
                    tet.ShowDialog();
                }
            }
        }

        private void DeleteOrionExecute()
        {
            if (!string.IsNullOrEmpty(this.m_selectedOrionPath))
            {

                if (Directory.Exists(this.m_selectedOrionPath))
                {
                    // Delete registrations first
                    DeleteFile(m_selectedOrionPath, "KMINEW.TXT");
                    DeleteFile(m_selectedOrionPath, "KMI.UPD");

                    //System.IO.DirectoryInfo di = new DirectoryInfo(m_selectedOrionPath);

                    //foreach (FileInfo file in di.GetFiles())
                    //{
                    //    file.Delete();
                    //}
                }
            }
        }

        private static void DeleteFile(string path, string filename)
        {
            var fileToDelete = Path.Combine(path, filename);
            if (File.Exists(fileToDelete))
            {
                File.Delete(fileToDelete);
            }
        }

        private void Init()
        {
            m_settings = SettingsHelper.GetSettings();
            OrionPaths = new ObservableCollection<string>();
            if (m_settings != null)
            {
                var backupDir = Path.Combine(m_selectedPath, "Backup_Orig");
                if (Directory.Exists(backupDir))
                {
                    var allFiles = new List<FileNameInfo>();
                    var files = Directory.EnumerateFiles(backupDir);
                    foreach (var file in files)
                    {
                        var time = File.GetLastWriteTime(file);
                        allFiles.Add(new FileNameInfo { FileName = file, CreatedTime = time });
                    }
                    m_allFilesList = allFiles.OrderBy(f => f.CreatedTime).ToList();
                    this.OnPropertyChanged("AllFilesList");
                }

                if (m_settings.OrionSetting != null)
                {
                    foreach (var orion in m_settings.OrionSetting.OrionViewModels)
                    {
                        if (orion.CommunicationSetup != null)
                        {
                            if (!string.IsNullOrEmpty(orion.CommunicationSetup.SelectedPath))
                            {
                                OrionPaths.Add(orion.CommunicationSetup.SelectedPath);
                            }
                        }
                    }
                }
            }
        }
    }
}
