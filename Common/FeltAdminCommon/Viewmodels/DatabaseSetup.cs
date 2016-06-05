using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Annotations;
using System.Windows.Forms;
using System.Windows.Input;

using FeltAdmin.Database.Engine;
using FeltAdmin.Diagnostics;

using Microsoft.Practices.Prism.Commands;

namespace FeltAdmin.Viewmodels
{
    public class DatabaseSetup : ViewModelBase
    {
        private ObservableCollection<string> m_dataBaseNames;

        private string m_selectedDatabase;

        private DelegateCommand m_createDatabaseCommand;

        private string m_newDatabaseName;

        private DateTime m_newDatabaseStartTime;

        private bool m_initDatabaseMode;

        public delegate void DbSelectedEventHandler(object sender);

        public event DbSelectedEventHandler DbSelected;

        public DatabaseSetup()
        {
            InitExistingDatabase();
            NewDatabaseStartTime = DateTime.Now.Date;
        }

        private void InitExistingDatabase()
        {
            var existing = DatabaseApi.GetAllCompetitions();
            if (existing != null && existing.Any())
            {
                DataBaseNames = new ObservableCollection<string>(existing);
                SelectedDatabase = DataBaseNames.First();
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

            this.InitDatabaseMode = false;

            if (DbSelected != null)
            {
                DbSelected(this);
            }
        }

        public ICommand CreateDatabaseCommand
        {
            get
            {
                if (m_createDatabaseCommand == null)
                {
                    m_createDatabaseCommand = new DelegateCommand(this.CreateDatabaseExecute);
                }

                return m_createDatabaseCommand;
            }
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

            DatabaseApi.CreateNewCompetition(NewDatabaseName, NewDatabaseStartTime);
            this.InitExistingDatabase();
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

        public ObservableCollection<string> DataBaseNames
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

        public string SelectedDatabase
        {
            get
            {
                return this.m_selectedDatabase;
            }
            set
            {
                this.m_selectedDatabase = value;
                this.OnPropertyChanged("SelectedDatabase");
                if (!DatabaseApi.SelectCompetition(this.m_selectedDatabase))
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
    }
}
