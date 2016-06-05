using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

using FeltAdmin.Helpers;
using FeltAdmin.Leon;
using FeltAdmin.Orion;

using Microsoft.Practices.Prism.Commands;
using System.Windows.Input;


namespace FeltAdmin.Viewmodels
{
    public class MainViewModel : ViewModelBase
    {
        private LeonCommunication m_leonCommunication;

        private MinneViewModel m_minneViewModel;

        public OrionResultViewModel OrionResultViewModel { get; set; }

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

        private DelegateCommand m_saveSettingsCommand;

        private DelegateCommand m_startProductionCommand;

        private OrionSetupViewModel m_mainOrionViewModel;

        private DatabaseSetup m_databaseSetup;

        private bool m_initDatabaseMode;

        private LeonViewModel m_leon;

        private OrionTeamsSetupViewModel m_orionTeamsSetupViewModel;

        private OrionCommunicationViewModel m_orionCommunicationViewModel;

        private OrionResultUpdater m_orionResultUpdater;

        private bool m_productionMode;

        public delegate void DisableDbSelect(object sender);

        public event DisableDbSelect DisableDb;

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

        void OrionResultViewModel_NewOrionResults(object sender, Orion.OrionResultsEventArgs e)
        {
            var newResults = e.NewResults;
            List<int> finishedShooters;
            var updatedRegistrations = m_orionResultUpdater.GetUpdatedRegistrationsAfterResultRegistration(
                newResults,
                this.OrionTeamsSetupViewModel.OrionRegistrations,
                this.OrionResultViewModel.OrionResults,
                out finishedShooters);

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
            var finale = e.NewRegistrations.Where(r => r.Team > 100);
            var innledende = e.NewRegistrations.Where(r => r.Team <= 100);

            if (innledende.Any())
            {
                var innledendelist = innledende.ToList();
                Leon.AddNewRegistrations(innledendelist);
                var newRegistrations = this.OrionTeamsSetupViewModel.AddNewRegistrations(innledendelist);
                m_orionResultUpdater.AddSums(newRegistrations, this.OrionResultViewModel.OrionResults);
                this.OrionCommunicationViewModel.UpdateChangesToOrion();
            }

            if (finale.Any())
            {

            }
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
    }
}
