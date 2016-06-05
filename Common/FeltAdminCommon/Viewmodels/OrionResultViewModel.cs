using System;
using System.Linq;

using FeltAdmin.Database.API;
using FeltAdmin.Database.Engine;
using FeltAdmin.Diagnostics;
using FeltAdmin.Orion;
using System.Collections.Generic;
using System.Windows.Threading;

namespace FeltAdmin.Viewmodels
{
    public class OrionResultViewModel : ViewModelBase, IDisposable
    {
        private DispatcherTimer m_dispatcherTimer;

        private OrionSetupViewModel m_orionSetupViewModel;

        private List<OrionResult> m_orionResults;

        public delegate void OrionResultsEventHandler(object sender, OrionResultsEventArgs e);

        public event OrionResultsEventHandler NewOrionResults;
        
        public OrionResultViewModel(OrionSetupViewModel orionSetupViewModel)
        {
            this.m_orionSetupViewModel = orionSetupViewModel;
        }

        public List<OrionResult> OrionResults
        {
            get
            {
                return this.m_orionResults;
            }
            set
            {
                this.m_orionResults = value;
            }
        }

        public void StartOrionCommunication()
        {
            if (m_dispatcherTimer == null)
            {
                m_dispatcherTimer = new DispatcherTimer();
                m_dispatcherTimer.Tick += m_dispatcherTimer_Tick;
                m_dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
                m_dispatcherTimer.Start();
            }
        }

        private void m_dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (NewOrionResults != null)
            {
                var newResults = new List<OrionResult>();
                foreach (var orionViewModel in m_orionSetupViewModel.OrionViewModels)
                {
                    var path = orionViewModel.CommunicationSetup.SelectedPath;
                    var reader = new OrionReader(path);
                    var newResultsForThisOrion = reader.GetOrionResult();
                    if (newResultsForThisOrion != null && newResultsForThisOrion.Any())
                    {
                        newResults.AddRange(newResultsForThisOrion);
                    }
                }

                if (newResults.Any())
                {
                    var args = new OrionResultsEventArgs() { NewResults = newResults };
                    NewOrionResults(this, args);
                }
            }
        }

        public void Dispose()
        {
            m_dispatcherTimer.Tick -= m_dispatcherTimer_Tick;
        }

        internal void LoadFromDb()
        {
            var orionResults = DatabaseApi.LoadCompetitionFromTable(TableName.OrionResult);
            this.AddNewRegistrations(orionResults.OfType<OrionResult>().ToList());
        }

        private RangeViewModel GetRange(OrionResult orionResult, out int orionId)
        {
            orionId = 0;
            foreach (var orionViewModel in m_orionSetupViewModel.OrionViewModels)
            {
                if (orionResult.OrionId == orionViewModel.OrionId)
                {
                    foreach (var rangeViewModel in orionViewModel.RangeViews)
                    {
                        if (rangeViewModel.RangeType == RangeType.Shooting && orionResult.Target >= rangeViewModel.FirstTarget && orionResult.Target <= rangeViewModel.LastTarget)
                        {
                            orionId = orionViewModel.OrionId;
                            return rangeViewModel;
                        }
                    }
                }
            }

            Log.Error(
                string.Format(
                    "Could not find rangeid for result: {0} Orionid={1} Lag={2} Skive={3} Serie={4}",
                    orionResult.Name,
                    orionResult.OrionId,
                    orionResult.Team,
                    orionResult.Target,
                    orionResult.AllSeries));

            return null;
        }

        public void AddNewRegistrations(List<OrionResult> orionResults)
        {
            if (m_orionResults == null)
            {
                m_orionResults = new List<OrionResult>();
            }

            foreach (var newRegistration in orionResults)
            {
                int orionId;
                var rangeForRegistration = this.GetRange(newRegistration, out orionId);

                var found = m_orionResults.SingleOrDefault(p => p.ShooterId == newRegistration.ShooterId && p.OrionId == newRegistration.OrionId && this.GetRange(p, out orionId).RangeId == rangeForRegistration.RangeId);
                if (found != null)
                {
                    m_orionResults.Remove(found);
                    m_orionResults.Add(newRegistration);
                }
                else if (found == null)
                {
                    m_orionResults.Add(newRegistration);
                }
            }
        }
    }
}
