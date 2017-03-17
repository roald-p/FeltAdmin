using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

using FeltAdmin.Database.API;
using FeltAdmin.Database.Engine;
using FeltAdmin.Helpers;
using FeltAdmin.Leon;
using FeltAdmin.Orion;

using FeltAdminCommon.Leon;

using Microsoft.Practices.Prism.Commands;
using System.Windows.Input;


namespace FeltAdmin.Viewmodels
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Forms;

    public class MainViewModel : ViewModelBase
    {
        private LeonCommunication m_leonCommunication;

        private MinneViewModel m_minneViewModel;

        public OrionResultViewModel OrionResultViewModel { get; set; }

        public string LeonTeamToRegister
        {
            get
            {
                return this.m_leonTeamToRegister;
            }
            set
            {
                this.m_leonTeamToRegister = value;
                this.OnPropertyChanged("LeonTeamToRegister");
            }
        }

        public LeonViewModel Leon
        {
            get
            {
                return this.m_leon;
            }
            set
            {
                this.m_leon = value;
                this.OnPropertyChanged("Leon");
            }
        }

        public OrionTeamsSetupViewModel OrionTeamsSetupViewModel
        {
            get
            {
                return this.m_orionTeamsSetupViewModel;
            }
            set
            {
                this.m_orionTeamsSetupViewModel = value;
                this.OnPropertyChanged("OrionTeamsSetupViewModel");
            }
        }

        public OrionCommunicationViewModel OrionCommunicationViewModel
        {
            get
            {
                return this.m_orionCommunicationViewModel;
            }
            set
            {
                this.m_orionCommunicationViewModel = value;
                this.OnPropertyChanged("OrionCommunicationViewModel");
            }
        }

        private DelegateCommand m_useSelectedTemplateCommand;
        private DelegateCommand m_saveSettingsCommand;

        private DelegateCommand m_saveSettingsAsTemplateCommand;
        
        private DelegateCommand m_startProductionCommand;

        private DelegateCommand m_etterRegistrerCommand;

        private OrionSetupViewModel m_mainOrionViewModel;

        private DatabaseSetup m_databaseSetup;

        private bool m_initDatabaseMode;

        private LeonViewModel m_leon;

        private OrionTeamsSetupViewModel m_orionTeamsSetupViewModel;

        private OrionCommunicationViewModel m_orionCommunicationViewModel;

        private OrionResultUpdater m_orionResultUpdater;

        private bool m_productionMode;

        private string m_leonTeamToRegister;

        public delegate void DisableDbSelect(object sender);

        public event DisableDbSelect DisableDb;

        public ICommand EtterRegistrerCommand
        {
            get
            {
                if (m_etterRegistrerCommand == null)
                {
                    m_etterRegistrerCommand = new DelegateCommand(this.EtterRegistrerExecute);
                }

                return m_etterRegistrerCommand;
            }
        }

        public void EtterRegistrerExecute()
        {
            if (string.IsNullOrWhiteSpace(this.LeonTeamToRegister))
            {
                return;
            }

            int maxTeam = 0;
            if (int.TryParse(this.LeonTeamToRegister, out maxTeam))
            {
                this.RegisterResults(maxTeam);
            }
        }
        

        public ICommand UseSelectedTemplateCommand
        {
            get
            {
                if (m_useSelectedTemplateCommand == null)
                {
                    m_useSelectedTemplateCommand = new DelegateCommand(this.UseSelectedTemplateExecute);
                }

                return m_useSelectedTemplateCommand;
            }
        }

        private void UseSelectedTemplateExecute()
        {
            var templateName = SelectedTemplatesfileName;

            var settings = SettingsHelper.GetTemplateSettings(templateName);
            if (settings.OrionSetting != null)
            {
                MainOrionViewModel = settings.OrionSetting;
            }
            else
            {
                if (SettingsHelper.IsTemplateAvaliable())
                {
                    var names = SettingsHelper.GetTemplatesNames();
                    foreach (var tempName in names)
                    {
                        CheckTemplate(tempName);
                    }
                }

                MainOrionViewModel = new OrionSetupViewModel();
            }

            MainOrionViewModel.Parent = this;

            if (settings.LeonCommunicationSetting != null)
            {
                LeonCommunication = settings.LeonCommunicationSetting;
            }
            else
            {
                LeonCommunication = new LeonCommunication();
            }
        }

        public ICommand SaveSettingsAsTemplateCommand
        {
            get
            {
                if (m_saveSettingsAsTemplateCommand == null)
                {
                    m_saveSettingsAsTemplateCommand = new DelegateCommand(this.SaveSettingsTemplateExecute);
                }

                return m_saveSettingsAsTemplateCommand;
            }
        }

        public ICommand SaveSettingsCommand
        {
            get
            {
                if (m_saveSettingsCommand == null)
                {
                    m_saveSettingsCommand = new DelegateCommand(this.SaveSettingsExecute);
                }

                return m_saveSettingsCommand;
            }
        }

        public ICommand StartProductionCommand
        {
            get
            {
                if (m_startProductionCommand == null)
                {
                    m_startProductionCommand = new DelegateCommand(this.StartProductionExecute);
                }

                return m_startProductionCommand;
            }
        }

        public bool ProductionMode
        {
            get
            {
                return this.m_productionMode;
            }
            set
            {
                this.m_productionMode = value;
                this.OnPropertyChanged("ProductionMode");
                this.OnPropertyChanged("SetupMode");
            }
        }

        public bool SetupMode
        {
            get
            {
                return !ProductionMode;
            }
        }

        public void StartProductionExecute()
        {
            SaveSettingsExecute();
            var settings = SettingsHelper.GetSettings();

            LeonCommunication.NewLeonRegistrations += LeonCommunication_NewLeonRegistrations;

            Leon = new LeonViewModel();
            Leon.LoadFromDb();
            LeonCommunication.StartLeonExecute();
            OrionTeamsSetupViewModel = new OrionTeamsSetupViewModel(Leon, MainOrionViewModel);
            this.OrionTeamsSetupViewModel.GenerateOrionTeamsExecute();

            OrionCommunicationViewModel = new OrionCommunicationViewModel(OrionTeamsSetupViewModel, MainOrionViewModel);
            this.OrionCommunicationViewModel.StartOrionCommunication();

            this.OrionResultViewModel = new OrionResultViewModel(settings.OrionSetting);
            this.OrionResultViewModel.LoadFromDb();
            this.OrionTeamsSetupViewModel.MoveRegistrations += OrionTeamsSetupViewModel_MoveRegistrations;
            this.OrionResultViewModel.NewOrionResults += OrionResultViewModel_NewOrionResults;
            m_orionResultUpdater = new OrionResultUpdater(settings.OrionSetting);
            m_orionResultUpdater.SetResultTypeOnOrionResult(OrionResultViewModel.OrionResults);
            this.OrionResultViewModel.StartOrionCommunication();

            var minneRange = settings.OrionSetting.OrionViewModels.SelectMany(o => o.RangeViews).SingleOrDefault(r => r.MinneShooting == true);
            if (minneRange != null)
            {
                this.m_minneViewModel = new MinneViewModel(minneRange.CommunicationSetup);
                this.m_minneViewModel.LoadFromDB();
                this.m_minneViewModel.StartReadNewRegistrations();
            }

            ProductionMode = true;
            MainOrionViewModel.SendPropChanged();
        }


        public void SaveSettingsTemplateExecute()
        {
            var settings = SettingsHelper.GetSettings();
            settings.OrionSetting = MainOrionViewModel;
            settings.LeonCommunicationSetting = LeonCommunication;

            SaveFileDialog dlg = new SaveFileDialog();
            var templateDir=SettingsHelper.GetTemplateDir();
            dlg.InitialDirectory = templateDir;
            dlg.Filter = "Template files (*.template)|*.template|All Files (*.*)|*.*";
            dlg.RestoreDirectory = true;
            dlg.CheckFileExists = false;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedFileName = dlg.FileName;
                var ext = Path.GetFileName(selectedFileName);
                CheckTemplate(ext);
                SettingsHelper.SaveSettingsAsTemplate(selectedFileName,settings);
            }
        }

      

        public void SaveSettingsExecute()
        {
            var settings = SettingsHelper.GetSettings();
            settings.OrionSetting = MainOrionViewModel;
            settings.LeonCommunicationSetting = LeonCommunication;
            SettingsHelper.SaveSettings(settings);
        }

        public MainViewModel()
        {
            this.InitDatabaseMode = true;
            m_templatesfileNames = new ObservableCollection<string>();
            this.DatabaseSetup = new DatabaseSetup();
            this.DatabaseSetup.DbSelected += DatabaseSetup_DbSelected;
        }

        void DatabaseSetup_DbSelected(object sender)
        {
            this.InitDatabaseMode = false;
            if (DisableDb != null)
            {
                DisableDb(this);
            }

            var settings = SettingsHelper.GetSettings();
            if (settings.OrionSetting != null)
            {
                MainOrionViewModel = settings.OrionSetting;
            }
            else
            {
                if (SettingsHelper.IsTemplateAvaliable())
                {
                    var names=SettingsHelper.GetTemplatesNames();
                    foreach (var tempName in names)
                    {
                        CheckTemplate(tempName);
                    }
                }

                MainOrionViewModel = new OrionSetupViewModel();
            }

            MainOrionViewModel.Parent = this;

            if (settings.LeonCommunicationSetting != null)
            {
                LeonCommunication = settings.LeonCommunicationSetting;
            }
            else
            {
                LeonCommunication = new LeonCommunication();
            }
        }

        private void RegisterResults(int uptoTeamNumber)
        {
            if (this.Leon == null || this.OrionResultViewModel == null || this.MainOrionViewModel == null)
            {
                return;
            }

            var toBeRegistered = Leon.LeonPersons.Where(l => l.Team <= uptoTeamNumber).Select(l => l.ShooterId).ToList();
            if (toBeRegistered != null && toBeRegistered.Any())
            {
                if (m_minneViewModel != null && m_minneViewModel.MinneRegistrations != null)
                {
                    LeonWriter.WriteLeonResults(
                        toBeRegistered,
                        this.OrionResultViewModel.OrionResults,
                        this.MainOrionViewModel,
                        this.Leon.LeonPersons,
                        this.m_minneViewModel.MinneRegistrations);
                }
                else
                {
                    LeonWriter.WriteLeonResults(
                        toBeRegistered,
                        this.OrionResultViewModel.OrionResults,
                        this.MainOrionViewModel,
                        this.Leon.LeonPersons,
                        null);
                }
            }
        }

        void OrionResultViewModel_NewOrionResults(object sender, Orion.OrionResultsEventArgs e)
        {
            var newResults = e.NewResults;
            List<int> finishedShooters;
            var updatedRegistrations = m_orionResultUpdater.GetUpdatedRegistrationsAfterResultRegistration(
                newResults,
                this.OrionTeamsSetupViewModel.OrionRegistrations,
                this.OrionResultViewModel.OrionResults,
                out finishedShooters);
            if (finishedShooters.Any())
            {
                var finishedPersons = Leon.LeonPersons.Where(l => finishedShooters.Contains(l.ShooterId));
                var maxTeamNumber = finishedPersons.Max(f => f.Team);
                var shouldBeFinished = Leon.LeonPersons.Where(l => l.Team <= maxTeamNumber).Select(l => l.ShooterId);
                var finishedRegistrations =
                    DatabaseApi.LoadCompetitionFromTable(TableName.FinishedShooter).OfType<FinishedPerson>().Select(f => f.ShooterId);
                var missingPersons = shouldBeFinished.Except(finishedRegistrations).Except(finishedShooters);
                finishedShooters.AddRange(missingPersons);
            }

            this.OrionCommunicationViewModel.UpdateChangesToOrion(updatedRegistrations);
            this.OrionResultViewModel.AddNewRegistrations(newResults);

            if (finishedShooters != null && finishedShooters.Any())
            {
                if (m_minneViewModel != null && m_minneViewModel.MinneRegistrations != null)
                {
                    LeonWriter.WriteLeonResults(
                        finishedShooters,
                        this.OrionResultViewModel.OrionResults,
                        this.MainOrionViewModel,
                        this.Leon.LeonPersons,
                        this.m_minneViewModel.MinneRegistrations);
                }
                else
                {
                    LeonWriter.WriteLeonResults(
                        finishedShooters,
                        this.OrionResultViewModel.OrionResults,
                        this.MainOrionViewModel,
                        this.Leon.LeonPersons,
                        null);
                }
            }
        }

        void LeonCommunication_NewLeonRegistrations(object sender, LeonCommunication.LeonEventArgs e)
        {
            //var finale = e.NewRegistrations.Where(r => r.Team > 100);
            //var innledende = e.NewRegistrations.Where(r => r.Team <= 100);

            //if (innledende.Any())
            //{
                //var innledendelist = innledende.ToList();
                Leon.AddNewRegistrations(e.NewRegistrations);
                var newRegistrations = this.OrionTeamsSetupViewModel.AddNewRegistrations(e.NewRegistrations);
                m_orionResultUpdater.AddSums(newRegistrations, this.OrionResultViewModel.OrionResults);
                this.OrionCommunicationViewModel.UpdateChangesToOrion();
            //}

            //if (finale.Any())
            //{

            //}
        }

        void OrionTeamsSetupViewModel_MoveRegistrations(object sender, FeltAdminCommon.Orion.MoveEventArgs e)
        {
            var newRegs = e.MoveRegistrations;
            m_orionResultUpdater.AddSums(newRegs, this.OrionResultViewModel.OrionResults);
            this.OrionCommunicationViewModel.UpdateChangesToOrion();
        }

        public LeonCommunication LeonCommunication
        {
            get
            {
                return this.m_leonCommunication;
            }
            set
            {
                this.m_leonCommunication = value;
                this.OnPropertyChanged("LeonCommunication");
            }
        }

        public OrionSetupViewModel MainOrionViewModel
        {
            get
            {
                return this.m_mainOrionViewModel;
            }
            set
            {
                this.m_mainOrionViewModel = value;
                this.OnPropertyChanged("MainOrionViewModel");
            }
        }

        public DatabaseSetup DatabaseSetup
        {
            get
            {
                return this.m_databaseSetup;
            }
            set
            {
                this.m_databaseSetup = value;
                this.OnPropertyChanged("DatabaseSetup");
            }
        }

        public bool InitDatabaseMode
        {
            get
            {
                return this.m_initDatabaseMode;
            }
            set
            {
                this.m_initDatabaseMode = value;
                this.OnPropertyChanged("InitDatabaseMode");
            }
        }

        private void CheckTemplate(string ext)
        {
            if (!m_templatesfileNames.Contains(ext))
            {
                var name=Path.GetFileNameWithoutExtension(ext);
                m_templatesfileNames.Add(name);
            }
            
            m_SelectedTemplatesfileName = m_templatesfileNames[0];
            this.OnPropertyChanged("TemplatesfileNames");
            this.OnPropertyChanged("SelectedTemplatesfileName");
        }

        private ObservableCollection<string> m_templatesfileNames;
        public ObservableCollection<string> TemplatesfileNames
        {
            get
            {
                return this.m_templatesfileNames;
            }
            set
            {
                this.m_templatesfileNames = value;
                this.OnPropertyChanged("TemplatesfileNames");
            }

        }

        private string m_SelectedTemplatesfileName;
        public string SelectedTemplatesfileName
        {
            get
            {
                return this.m_SelectedTemplatesfileName;
            }
            set
            {
                this.m_SelectedTemplatesfileName = value;
                this.OnPropertyChanged("SelectedTemplatesfileName");
                this.OnPropertyChanged("SetupModeTemplateAvailable");
            }
        }

        public Visibility TemplateVisibility
        {
            get
            {
                if (SetupMode)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Hidden;
                }
               
            } 
        }

        public bool SetupModeTemplateAvailable
        {
            get
            {
                if (SetupMode && !string.IsNullOrEmpty(m_SelectedTemplatesfileName))
                {
                    return true;
                }

                return false;
            }
        }

    }
}
