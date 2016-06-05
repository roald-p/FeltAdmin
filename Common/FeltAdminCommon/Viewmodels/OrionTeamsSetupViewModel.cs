
using FeltAdmin.Database.API;
using FeltAdmin.Database.Engine;
using FeltAdmin.Leon;
using FeltAdmin.Orion;

using FeltAdminCommon.Orion;

using Microsoft.Practices.Prism.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace FeltAdmin.Viewmodels
{
    public class OrionTeamsSetupViewModel : ViewModelBase
    {
        private DelegateCommand m_generateOrionTeamsCommand;

        private DelegateCommand m_transferMovedOrionCommand;

        private LeonViewModel m_leonViewModel;

        private OrionSetupViewModel m_orionSetupViewModel;

        private List<OrionRegistration> m_orionRegistrations;

        private int m_selectedOrion;

        private int m_selectedTeam;

        private ObservableCollection<int> m_teams;

        private ObservableCollection<int> m_orions;

        private List<OrionRegistration> m_newRegistrations;
        private List<OrionRegistration> m_newRegistrationsThatMustSelectFirstRange;

        public delegate void MoveRegistrationEventHandler(object sender, FeltAdminCommon.Orion.MoveEventArgs e);
        public event MoveRegistrationEventHandler MoveRegistrations;

        ////public class NewRegistrationEventArgs : EventArgs
        ////{
        ////    public List<LeonPerson> NewRegistrations { get; set; }
        ////}

        ////public delegate void NewLeonRegistrationsEventHandler(object sender, NewRegistrationEventArgs e);

        ////public event NewLeonRegistrationsEventHandler NewLeonRegistrations;

        public OrionTeamsSetupViewModel(LeonViewModel leonViewModel, OrionSetupViewModel orionSetupViewModel)
        {
            m_leonViewModel = leonViewModel;
            m_orionSetupViewModel = orionSetupViewModel;
        }

        public ICommand GenerateOrionTeamsCommand
        {
            get
            {
                if (m_generateOrionTeamsCommand == null)
                {
                    m_generateOrionTeamsCommand = new DelegateCommand(this.GenerateOrionTeamsExecute);
                }

                return m_generateOrionTeamsCommand;
            }
        }

        public ICommand TransferMovedOrionCommand
        {
            get
            {
                if (m_transferMovedOrionCommand == null)
                {
                    m_transferMovedOrionCommand = new DelegateCommand(this.TransferMovedOrionExecute);
                }

                return m_transferMovedOrionCommand;
            }
        }

        public List<OrionRegistration> NewRegistrationsThatMustSelectFirstRange
        {
            get
            {
                return this.m_newRegistrationsThatMustSelectFirstRange;
            }
            set
            {
                this.m_newRegistrationsThatMustSelectFirstRange = value;
            }
        }

        public List<OrionRegistration> NewRegistrations
        {
            get
            {
                return this.m_newRegistrations;
            }
            set
            {
                this.m_newRegistrations = value;
            }
        }

        public void TransferMovedOrionExecute()
        {
            if (MoveRegistrations != null && m_newRegistrationsThatMustSelectFirstRange != null && m_newRegistrationsThatMustSelectFirstRange.Any())
            {
                var toBeTransfered = m_newRegistrationsThatMustSelectFirstRange.Where(r => r.RemoveReg == false);
                if (toBeTransfered.Any())
                {
                    m_orionRegistrations.AddRange(toBeTransfered);
                    m_newRegistrations.AddRange(toBeTransfered);
                }

                m_newRegistrationsThatMustSelectFirstRange.Clear();

                MoveRegistrations(this, new MoveEventArgs {MoveRegistrations = toBeTransfered.ToList() });
                this.OnPropertyChanged("NewRegistrationsThatMustSelectFirstRange");
            }
        }

        public void GenerateOrionTeamsExecute()
        {
            var allOrionRegistrations = new List<OrionRegistration>();
            var leonpersons = m_leonViewModel.LeonPersons;
            foreach (var leonperson in leonpersons)
            {
                var orionRegsForPerson = OrionGenerate.GenerateOrionForShooter(leonperson, m_orionSetupViewModel);
                allOrionRegistrations.AddRange(orionRegsForPerson);
            }

            m_orionRegistrations = allOrionRegistrations;
            this.OnPropertyChanged("OrionRegistrations");
            this.BuildLists();
        }

        public List<OrionRegistration> OrionRegistrations
        {
            get
            {
                return m_orionRegistrations;
            }
        }

        public int SelectedOrion
        {
            get
            {
                return this.m_selectedOrion;
            }
            set
            {
                this.m_selectedOrion = value;
                this.OnPropertyChanged("SelectedOrion");

                ////var selectedTeam = m_selectedTeam;
                var teams = m_orionRegistrations.Where(o => o.OrionId == value).Select(o => o.Team).Distinct().OrderBy(o => o);
                m_teams = new ObservableCollection<int>(teams);
                this.OnPropertyChanged("Teams");

                ////if (m_teams.Contains(selectedTeam))
                ////{
                ////    SelectedTeam = selectedTeam;
                ////}
                ////else if (selectedTeam == 0 && m_teams.Any())
                ////{
                ////    SelectedTeam = m_teams.First();
                ////}

                SelectedTeam = m_teams.First();
            }
        }

        public int SelectedTeam
        {
            get
            {
                return this.m_selectedTeam;
            }
            set
            {
                this.m_selectedTeam = value;
                this.OnPropertyChanged("SelectedTeam");
                this.OnPropertyChanged("SelectedOrionRegistrations");
            }
        }

        public ObservableCollection<int> Orions
        {
            get
            {
                return this.m_orions;
            }
            set
            {
                this.m_orions = value;
                this.OnPropertyChanged("Orions");
            }
        }

        public ObservableCollection<int> Teams
        {
            get
            {
                return this.m_teams;
            }
            set
            {
                this.m_teams = value;
                this.OnPropertyChanged("Teams");
            }
        }

        public ObservableCollection<OrionRegistration> SelectedOrionRegistrations
        {
            get
            {
                if (m_selectedOrion > 0 && m_selectedTeam > 0 && m_orionRegistrations.Any())
                {
                    var registrations = m_orionRegistrations.Where(r => r.OrionId == m_selectedOrion && r.Team == m_selectedTeam).OrderBy(o => o.Team).ThenBy(o => o.Target);
                    return new ObservableCollection<OrionRegistration>(registrations);
                }

                return null;
            }
        }

        public List<OrionRegistration> AddNewRegistrations(List<LeonPerson> newRegistrations)
        {
            if (m_newRegistrations == null)
            {
                m_newRegistrations = new List<OrionRegistration>();
            }

            if (m_newRegistrationsThatMustSelectFirstRange == null)
            {
                m_newRegistrationsThatMustSelectFirstRange = new List<OrionRegistration>();
            }

            var results = DatabaseApi.LoadCompetitionFromTable(TableName.OrionResult).OfType<OrionResult>();

            foreach (var newRegistration in newRegistrations)
            {
                if (newRegistration.IsEmpty)
                {
                    var firstOrionId = m_orionSetupViewModel.OrionViewModels.First().OrionId;
                    var firstRegs =
                        m_orionRegistrations.Where(
                            o => o.OrionId == firstOrionId && o.Team == newRegistration.Team && o.Target == newRegistration.Target);

                    if (firstRegs != null && firstRegs.Count() > 1)
                    {
                        
                    }

                    var firstReg = firstRegs.FirstOrDefault();

                    if (firstReg != null && m_orionRegistrations.Any(o => o.ShooterId == firstReg.ShooterId))
                    {
                        var deleteRegistrations = m_orionRegistrations.Where(o => o.ShooterId == firstReg.ShooterId).ToList();
                        foreach (var deleteRegistration in deleteRegistrations)
                        {
                            if (results.Any(r => r.OrionId == deleteRegistration.OrionId && r.Team == deleteRegistration.Team))
                            {
                                continue;
                            }
                            else
                            {
                                deleteRegistration.Deleted = true;
                                m_newRegistrations.Add(deleteRegistration);
                                m_orionRegistrations.Remove(deleteRegistration);
                            }
                        }
                    }
                }
                else
                {
                    if (m_orionRegistrations.Any(o => o.ShooterId == newRegistration.ShooterId))
                    {
                        var deleteRegistrations = m_orionRegistrations.Where(o => o.ShooterId == newRegistration.ShooterId).ToList();
                        foreach (var deleteRegistration in deleteRegistrations)
                        {
                            if (results.Any(r => r.OrionId == deleteRegistration.OrionId && r.Team == deleteRegistration.Team))
                            {
                                continue;
                            }
                            else
                            {
                                deleteRegistration.Deleted = true;
                                m_newRegistrations.Add(deleteRegistration);
                                m_orionRegistrations.Remove(deleteRegistration);
                            }
                        }
                    }

                    var orionRegsForPerson = OrionGenerate.GenerateOrionForShooter(newRegistration, m_orionSetupViewModel);
                    foreach (var orionRegistration in orionRegsForPerson)
                    {
                        if (results.Any(r => r.OrionId == orionRegistration.OrionId && r.Team == orionRegistration.Team))
                        {
                            continue;
                        }
                        else
                        {
                            if (results.Any(r => r.ShooterId == orionRegistration.ShooterId))
                            {
                                m_newRegistrationsThatMustSelectFirstRange.Add(orionRegistration);
                            }
                            else
                            {
                                m_orionRegistrations.Add(orionRegistration);
                                m_newRegistrations.Add(orionRegistration);
                            }
                        }
                    }
                }
            }

            if (m_newRegistrationsThatMustSelectFirstRange.Any())
            {
                this.OnPropertyChanged("MovedOrionRegistrations");
            }

            this.OnPropertyChanged("OrionRegistrations");
            this.BuildLists();

            return m_newRegistrations;
        }

        public ObservableCollection<OrionRegistration> MovedOrionRegistrations
        {
            get
            {
                if (m_newRegistrationsThatMustSelectFirstRange != null && m_newRegistrationsThatMustSelectFirstRange.Any())
                {
                    return new ObservableCollection<OrionRegistration>(m_newRegistrationsThatMustSelectFirstRange);
                }

                return null;
            }
        }


        private void BuildLists()
        {
            var selectedOrion = m_selectedOrion;
            var orions = m_orionRegistrations.Select(o => o.OrionId).Distinct();
            m_orions = new ObservableCollection<int>(orions);
            this.OnPropertyChanged("Orions");

            if (m_orions.Contains(selectedOrion))
            {
                SelectedOrion = selectedOrion;
            }
            else if (m_selectedOrion == 0 && m_orions.Any())
            {
                SelectedOrion = m_orions.First();
            }
        }
    }
}
