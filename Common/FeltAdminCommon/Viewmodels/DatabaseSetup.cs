using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Input;

using FeltAdmin.Database.Engine;
using FeltAdmin.Diagnostics;

using FeltAdminCommon.Viewmodels;

using MessageBox = System.Windows.Forms.MessageBox;

namespace FeltAdmin.Viewmodels
{
    using FeltAdmin.Helpers;
    using Prism.Commands;

    public class DatabaseSetup : ViewModelBase
    {
        private ObservableCollection<DataBaseViewModel> m_dataBaseNames;

        private DataBaseViewModel m_selectedDatabase;

        private DelegateCommand m_createDatabaseCommand;

        private DelegateCommand m_createTemplateCommand;

        private DelegateCommand m_changeTemplateCommand;

        private string m_newDatabaseName;

        private string m_newTemplateName;
        
        private DateTime m_newDatabaseStartTime;

        private bool m_initDatabaseMode;
        private bool m_initTemplateMode;
        
        public delegate void DbSelectedEventHandler(object sender);

        public event DbSelectedEventHandler DbSelected;

        public delegate void TemplateSelectedEventHandler(object sender);

        public event TemplateSelectedEventHandler TemplateSelected;

        public event TemplateSelectedEventHandler ChangeTemplate;

        private ObservableCollection<string> m_AvailableTemplates;

        private string m_SelectedAvailableTemplate;

        public DatabaseSetup()
        {
            InitExistingDatabase();
            NewDatabaseStartTime = DateTime.Now.Date;
            var templateNames=SettingsHelper.GetTemplatesNames();
            TemplatesfileNames = new ObservableCollection<string>(templateNames);
            InitAvaliableTemplates();
        }

        private void InitExistingDatabase()
        {
            var existing = DatabaseApi.GetAllCompetitions();
            if (existing != null && existing.Any())
            {
                var dataBaseViewModels = new List<DataBaseViewModel>();
                foreach (var dbPath in existing)
                {
                    dataBaseViewModels.Add(new DataBaseViewModel { DatabasePath = dbPath });
                }

                SelectedDatabase = dataBaseViewModels.First();
                DataBaseNames = new ObservableCollection<DataBaseViewModel>(dataBaseViewModels);
            }
        }
        public string SelectedAvailableTemplate
        {
            get
            {
                return this.m_SelectedAvailableTemplate;
            }
            set
            {
                this.m_SelectedAvailableTemplate = value;
                this.OnPropertyChanged("SelectedAvailableTemplate");
                if (m_createDatabaseCommand != null)
                {
                    this.m_createDatabaseCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private void InitAvaliableTemplates()
        {
            var names = SettingsHelper.GetTemplatesNames();
            AvailableTemplates = new ObservableCollection<string>(names);
            SelectedAvailableTemplate = null;
        }
        public ObservableCollection<string> AvailableTemplates
        {
            get
            {
                return this.m_AvailableTemplates;
            }
            set
            {
                this.m_AvailableTemplates = value;
                this.OnPropertyChanged("AvailableTemplates");
            }

        }


        private DelegateCommand m_initDbDoneCommand;

        public ICommand InitDbDoneCommand
        {
            get
            {
                if (m_initDbDoneCommand == null)
                {
                    m_initDbDoneCommand = new DelegateCommand(this.InitDbDoneExecute);
                }

                return m_initDbDoneCommand;
            }
        }

        public void InitDbDoneExecute()
        {
            if (!DatabaseApi.DbSelected)
            {
                MessageBox.Show("Database must be selected or created");
                return;
            }

            var settings = SettingsHelper.GetSettings();
            var errors = settings.OrionSetting.Validate(settings.LeonCommunicationSetting);
            if (errors != null && errors.Count > 0)
            {
                var messages = string.Join(Environment.NewLine, errors);
                System.Windows.MessageBox.Show(messages, "Feil i konfigurasjon", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.InitDatabaseMode = false;

            if (DbSelected != null)
            {
                DbSelected(this);
            }
        }
       
        public ICommand ChangeTemplateCommand
        {
            get
            {
                if (m_changeTemplateCommand == null)
                {
                    m_changeTemplateCommand = new DelegateCommand(this.ChangeTemplateExecute, CanChangeTemplateExecute);
                }

                return m_changeTemplateCommand;
            }
        }

        private void ChangeTemplateExecute()
        {
            InitTemplateMode = false;

            if (!string.IsNullOrEmpty(SelectedTemplatesfileName))
            {
                m_choosenTemplateName = SelectedTemplatesfileName;
            }

            if (ChangeTemplate != null)
            {
                ChangeTemplate(this);
            }
        }

        private bool CanChangeTemplateExecute()
        {
            if (this.m_templatesfileNames != null && this.m_templatesfileNames.Count > 0)
            {
                if (string.IsNullOrEmpty(SelectedTemplatesfileName))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public ICommand CreateTemplateCommand
        {
            get
            {
                if (m_createTemplateCommand == null)
                {
                    m_createTemplateCommand = new DelegateCommand(this.CreateTemplateExecute, CanCreateTemplateExecute);
                }

                return m_createTemplateCommand;
            }
        }

        public ICommand CreateDatabaseCommand
        {
            get
            {
                if (m_createDatabaseCommand == null)
                {
                    m_createDatabaseCommand = new DelegateCommand(this.CreateDatabaseExecute, CanCreateDatabaseExecute);
                }

                return m_createDatabaseCommand;
            }
        }

        private bool CanCreateDatabaseExecute()
        {
            if (string.IsNullOrEmpty(this.m_SelectedAvailableTemplate))
            {
                return false;
            }

            if (string.IsNullOrEmpty(m_newDatabaseName))
            {
                return false;
            }

            return true;
        }

        private void CreateTemplateExecute()
        {
            InitTemplateMode = false;

            if (!string.IsNullOrEmpty(NewTemplateName) )
            {
                m_choosenTemplateName = NewTemplateName;
            }
           

            if (TemplateSelected != null)
            {
                TemplateSelected(this);
            }
        }

        private bool CanCreateTemplateExecute()
        {
            if (string.IsNullOrEmpty(NewTemplateName))
            {
                return false;
            }

            return true;
        }

        public void CreateDatabaseExecute()
        {
            if (string.IsNullOrWhiteSpace(NewDatabaseName))
            {
                MessageBox.Show("Du må angi navn på stevnet");
                return;
            }

            if (NewDatabaseStartTime == DateTime.MinValue)
            {
                NewDatabaseStartTime = DateTime.Now;
            }

            DatabaseApi.CreateNewCompetition(SelectedAvailableTemplate,NewDatabaseName, NewDatabaseStartTime);
            this.InitExistingDatabase();
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
                m_changeTemplateCommand.RaiseCanExecuteChanged();
            }
        }


        public string NewTemplateName
        {
            get
            {
                return this.m_newTemplateName;
            }
            set
            {
                this.m_newTemplateName = value;
                this.OnPropertyChanged("NewTemplateName");
                this.m_createTemplateCommand.RaiseCanExecuteChanged();
            }
        }

       private string m_choosenTemplateName;
       public string ChoosenTemplateName
        {
            get
            {
                return this.m_choosenTemplateName;
            }
            set
            {
                this.m_choosenTemplateName = value;
                this.OnPropertyChanged("ChoosenTemplateName");
            }
        }


        public string NewDatabaseName
        {
            get
            {
                return this.m_newDatabaseName;
            }
            set
            {
                this.m_newDatabaseName = value;
                this.OnPropertyChanged("NewDatabaseName");
                this.m_createDatabaseCommand.RaiseCanExecuteChanged();
            }
        }

        public DateTime NewDatabaseStartTime
        {
            get
            {
                return this.m_newDatabaseStartTime;
            }
            set
            {
                this.m_newDatabaseStartTime = value;
                this.OnPropertyChanged("NewDatabaseStartTime");
            }
        }

        public ObservableCollection<DataBaseViewModel> DataBaseNames
        {
            get
            {
                return this.m_dataBaseNames;
            }
            set
            {
                this.m_dataBaseNames = value;
                this.OnPropertyChanged("DataBaseNames");
            }
        }

        public DataBaseViewModel SelectedDatabase
        {
            get
            {
                return this.m_selectedDatabase;
            }
            set
            {
                this.m_selectedDatabase = value;
                this.OnPropertyChanged("SelectedDatabase");
                if (!DatabaseApi.SelectCompetition(this.m_selectedDatabase.DatabasePath))
                {
                    Log.Error("Unable to select database:" + value);
                    throw new ConfigurationErrorsException("Unable to select database:" + value);
                }
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

        public bool InitTemplateMode
        {
            get
            {
                return this.m_initTemplateMode;
            }
            set
            {
                this.m_initTemplateMode = value;
                this.OnPropertyChanged("InitTemplateMode");
            }
        }
    }
}
