using System.Windows.Threading;

using FeltAdmin.Database.API;
using FeltAdmin.Database.Engine;
using FeltAdmin.Leon;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace FeltAdmin.Viewmodels
{
    public class LeonViewModel : ViewModelBase
    {
        private List<LeonPerson> m_leonPersons;

        private ObservableCollection<int> m_teams;

        private int m_selectedTeam;

        public void LoadFromDb()
        {
            var leonPersons = DatabaseApi.LoadCompetitionFromTable(TableName.LeonRegistration);
            this.AddNewRegistrations(leonPersons.OfType<LeonPerson>().ToList(), false);
        }

        public List<LeonPerson> LeonPersons
        {
            get
            {
                return m_leonPersons;
            }

            //set
            //{
            //    m_leonPersons = value;
            //    this.BuildLists();
            //}
        }
            
        public ObservableCollection<int> Teams
        {
            get
            {
                return m_teams;
            }

            set
            {
                m_teams = value;
                this.OnPropertyChanged("Teams");
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
                var leonForTeam = m_leonPersons.Where(p => p.Team == value);
                m_selectedLeonPersons = new ObservableCollection<LeonPerson>(leonForTeam);
                this.m_selectedTeam = value;
                this.OnPropertyChanged("SelectedTeam");
                this.OnPropertyChanged("SelectedLeonPersons");
            }
        }

        private ObservableCollection<LeonPerson> m_selectedLeonPersons;

        public ObservableCollection<LeonPerson> SelectedLeonPersons
        {
            get
            {
                return m_selectedLeonPersons;
            }
        }

        public void AddNewRegistrations(List<LeonPerson> newRegistrations, bool save = true)
        {
            if (m_leonPersons == null)
            {
                m_leonPersons = new List<LeonPerson>();
            }

            foreach (var newRegistration in newRegistrations)
            {
                var found = m_leonPersons.SingleOrDefault(p => p.Team == newRegistration.Team && p.Target == newRegistration.Target);
                if (found != null)
                {
                    m_leonPersons.Remove(found);
                }

                if (!newRegistration.IsEmpty)
                {
                    m_leonPersons.Add(newRegistration);
                }

                if (save)
                {
                    DatabaseApi.Save(newRegistration);
                }
            }

            m_leonPersons = m_leonPersons.OrderBy(p => p.Team).ThenBy(p => p.Target).ToList();
            this.BuildLists();
        }

        private void BuildLists()
        {
            var selectedTeam = m_selectedTeam;
            var teams = m_leonPersons.Select(p => p.Team).Distinct();
            m_teams = new ObservableCollection<int>(teams);
            this.OnPropertyChanged("Teams");

            if (m_teams.Contains(selectedTeam))
            {
                SelectedTeam = selectedTeam;
            }
            else if (m_selectedTeam == 0 && m_teams.Any())
            {
                SelectedTeam = m_teams.First();
            }
        }
    }
}
