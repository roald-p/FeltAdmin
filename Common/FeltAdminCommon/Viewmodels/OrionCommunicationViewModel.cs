using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Threading;

using FeltAdmin.Orion;
using Prism.Commands;
using System.Windows.Input;

namespace FeltAdmin.Viewmodels
{
    public class OrionCommunicationViewModel : ViewModelBase, IDisposable
    {
        private OrionTeamsSetupViewModel m_orionTeamsSetup;

        private OrionSetupViewModel m_orionSetup;

        private DelegateCommand m_transferToOrionCommand;

        private DispatcherTimer m_dispatcherTimer;

        public ICommand TransferToOrionCommand
        {
            get
            {
                if (m_transferToOrionCommand == null)
                {
                    m_transferToOrionCommand = new DelegateCommand(this.TransferToOrionExecute);
                }

                return m_transferToOrionCommand;
            }
        }

        public OrionCommunicationViewModel(OrionTeamsSetupViewModel orionTeamsSetupViewModel, OrionSetupViewModel orionSetupViewModel)
        {
            m_orionSetup = orionSetupViewModel;
            m_orionTeamsSetup = orionTeamsSetupViewModel;
        }

        public void TransferToOrionExecute()
        {
            OrionGenerate.TransferAllRegistrationsToAllOrions(m_orionTeamsSetup.OrionRegistrations, m_orionSetup);
        }

        public void UpdateChangesToOrion()
        {
            if (m_orionTeamsSetup.NewRegistrations.Any())
            {
                OrionGenerate.TransferAllRegistrationsToAllOrions(m_orionTeamsSetup.NewRegistrations, m_orionSetup);
                m_orionTeamsSetup.NewRegistrations.Clear();
            }
        }

        public void UpdateChangesToOrion(List<OrionRegistration> updatedRegistrations)
        {
            if (updatedRegistrations != null && updatedRegistrations.Any())
            {
                OrionGenerate.TransferAllRegistrationsToAllOrions(updatedRegistrations, m_orionSetup);
            }
        }

        public void StartOrionCommunication()
        {
            if (m_dispatcherTimer == null)
            {
                m_dispatcherTimer = new DispatcherTimer();
                m_dispatcherTimer.Tick += m_dispatcherTimer_Tick;
                m_dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                m_dispatcherTimer.Start();
            }
        }

        void m_dispatcherTimer_Tick(object sender, EventArgs e)
        {
            OrionGenerate.CheckTmpFile(m_orionSetup);
        }

        public void Dispose()
        {
            m_dispatcherTimer.Tick -= m_dispatcherTimer_Tick;
        }
    }
}
