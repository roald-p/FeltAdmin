using FeltAdmin.Database.API;
using FeltAdmin.Database.Engine;
using FeltAdmin.Leon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;

namespace FeltAdmin.Viewmodels
{
    public class MinneViewModel : IDisposable
    {
        private List<MinneRegistration> m_minneRegistrations;

        private CommunicationSetup m_communicationSetup;
         
        private DispatcherTimer m_dispatcherTimer;

        public MinneViewModel(CommunicationSetup setup)
        {
            m_communicationSetup = setup;
        }

        public List<MinneRegistration> MinneRegistrations
        {
            get
            {
                return this.m_minneRegistrations;
            }
            set
            {
                this.m_minneRegistrations = value;
            }
        }

        public void LoadFromDB()
        {
            var minneRegistrations = DatabaseApi.LoadCompetitionFromTable(TableName.MinneRegistration);
            this.AddNewRegistrations(minneRegistrations.OfType<MinneRegistration>().ToList(), false);
        }

        public void StartReadNewRegistrations()
        {
            var path = this.m_communicationSetup.SelectedPath;
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
            var outputPath = this.m_communicationSetup.SelectedPath;
            //LeonWriter.RegisterPath(path);
            var registrations = this.GetNewRegistrations(outputPath);
            if (registrations != null && registrations.Any())
            {
                this.AddNewRegistrations(registrations, true);
            }

            LeonWriter.CheckTmpFile(outputPath, "MinneLeonTemp");
        }

        public List<MinneRegistration> GetNewRegistrations(string path)
        {
            var reader = new MinneReader(path);
            if (reader.CheckForNewFile())
            {
                return reader.GetMinneRegistrations();
            }

            return null;
        }

        private void AddNewRegistrations(List<MinneRegistration> newRegistrations, bool save)
        {
            if (m_minneRegistrations == null)
            {
                m_minneRegistrations = new List<MinneRegistration>();
            }

            foreach (var newRegistration in newRegistrations)
            {
                var found = m_minneRegistrations.SingleOrDefault(p => p.ShooterId == newRegistration.ShooterId);
                if (found != null)
                {
                    m_minneRegistrations.Remove(found);
                }

                if (!newRegistration.IsEmpty)
                {
                    m_minneRegistrations.Add(newRegistration);
                }

                if (save)
                {
                    DatabaseApi.Save(newRegistration);
                }
            }

            m_minneRegistrations = m_minneRegistrations.OrderBy(p => p.Team).ThenBy(p => p.Target).ToList();
            ////this.BuildLists();
        }

        public void Dispose()
        {
            m_dispatcherTimer.Tick -= dispatcherTimer_Tick;
            m_dispatcherTimer = null;
        }
    }
}
