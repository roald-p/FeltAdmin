using FeltAdmin.Leon;
using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace FeltAdmin.Viewmodels
{
    public class LeonCommunication : ViewModelBase, IDisposable
    {
        private CommunicationSetup m_communicationSetup;
        private DelegateCommand m_startLeonCommand;
        private DispatcherTimer m_dispatcherTimer;

        public LeonCommunication()
        {
            CommunicationSetup = new CommunicationSetup();
        }

        public delegate void LeonRegistrationsEventHandler(object sender, LeonEventArgs e);

        public event LeonRegistrationsEventHandler NewLeonRegistrations;
        
        [XmlIgnore]
        public ICommand StartLeonCommand
        {
            get
            {
                if (m_startLeonCommand == null)
                {
                    m_startLeonCommand = new DelegateCommand(this.StartLeonExecute);
                }

                return m_startLeonCommand;
            }
        }

        public CommunicationSetup CommunicationSetup
        {
            get
            {
                return this.m_communicationSetup;
            }

            set
            {
                this.m_communicationSetup = value;
                this.OnPropertyChanged("CommunicationSetup");
            }
        }

        public void StartLeonExecute()
        {
            var path = CommunicationSetup.SelectedPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Katalog ikke angitt");
                return;
            }

            if (!Directory.Exists(path))
            {
                MessageBox.Show("Katalog finnes ikke: " + path);
                return;
            }

            if (m_dispatcherTimer == null)
            {
                m_dispatcherTimer = new DispatcherTimer();
                m_dispatcherTimer.Tick += dispatcherTimer_Tick;
                m_dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
                m_dispatcherTimer.Start();
            }
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            this.InitGet();
        }

        private void InitGet()
        {
            var path = CommunicationSetup.SelectedPath;
            if (NewLeonRegistrations != null)
            {
                var registrations = this.GetNewRegistrations(path);
                if (registrations != null && registrations.Any())
                {
                    var args = new LeonEventArgs { NewRegistrations = registrations };
                    NewLeonRegistrations(this, args);
                }
            }

            LeonWriter.CheckTmpFile(path);
        }

        public List<LeonPerson> GetNewRegistrations(string path)
        {
            var reader = new LeonReader(path);
            if (reader.CheckForNewFile())
            {
                return reader.GetLeonPersons();
            }

            return null;
        }

        public class LeonEventArgs : EventArgs
        {
            public List<LeonPerson> NewRegistrations { get; set; }
        }

        public void Dispose()
        {
            m_dispatcherTimer.Tick -= dispatcherTimer_Tick;
            m_dispatcherTimer = null;
        }

        internal List<string> Validate()
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(this.CommunicationSetup.SelectedPath))
            {
                errors.Add("Kommunikasjonskatalog ikke satt opp for Leon");
            }
            else if (!Directory.Exists(this.CommunicationSetup.SelectedPath))
            {
                errors.Add("Kommunikasjonskatalog for Leon finnes ikke: " + this.CommunicationSetup.SelectedPath);
            }
            return errors;
        }
    }
}
