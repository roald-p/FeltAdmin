using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

using FeltAdmin.Viewmodels;

using FeltAdminCommon;

using FeltAdminServer.Data;

using FeltResult.Comparer;
using Prism.Commands;

namespace FeltResult.ViewModels
{
    public class MainResultViewModelForRange : ViewModelBase
    {
        private const string All = "Alle";

        private const string StaredMarked = "Stjernemerket";

        private FestResultServiceClient m_serviceClient;

        private List<string> m_competitionsList;

        private string m_selectedCompetition;

        private DelegateCommand m_refreshCommand;

        private DelegateCommand m_refreshRegistrationCommand;

        private DelegateCommand m_prevTeamCommand;

        private DelegateCommand m_nextTeamCommand;

        private ObservableCollection<ResultViewModel> m_results;

        private List<string> m_classes;

        private string m_selectedClass;

        private List<ResultViewModel> m_allResults;

        private List<RegistrationViewModel> m_allRegistrations;

        private int m_selectedTeamIndex;

        private ObservableCollection<RegistrationViewModel> m_selectedTeam;

        private List<string> m_starredValues;

        private string m_selectedStarredValue;

        private int m_selectedResultsAfterRange;

        public MainResultViewModelForRange(RangeClass rangeClass)
        {
            RangeClass = rangeClass;
            m_serviceClient = new FestResultServiceClient();
            CompetitionsList = m_serviceClient.GetCompetitions(RangeClass).ToList();
            var selectedStarValues = StarredValues;
            SelectedStarredValue = selectedStarValues.First();
            SelectedResultsAfterRange = 5;
        }

        public RangeClass RangeClass { get; set; }

        [XmlIgnore]
        public ICommand RefreshCommand
        {
            get
            {
                if (m_refreshCommand == null)
                {
                    m_refreshCommand = new DelegateCommand(this.RefreshExecute);
                }

                return m_refreshCommand;
            }
        }

        [XmlIgnore]
        public ICommand RefreshRegistrationCommand
        {
            get
            {
                if (m_refreshRegistrationCommand == null)
                {
                    m_refreshRegistrationCommand = new DelegateCommand(this.RefreshRegistrationExecute);
                }

                return m_refreshRegistrationCommand;
            }
        }

        [XmlIgnore]
        public ICommand PrevTeamCommand
        {
            get
            {
                if (m_prevTeamCommand == null)
                {
                    m_prevTeamCommand = new DelegateCommand(this.PrevTeamExecute);
                }

                return m_prevTeamCommand;
            }
        }

        [XmlIgnore]
        public ICommand NextTeamCommand
        {
            get
            {
                if (m_nextTeamCommand == null)
                {
                    m_nextTeamCommand = new DelegateCommand(this.NextTeamExecute);
                }

                return m_nextTeamCommand;
            }
        }
        
        private void PrevTeamExecute()
        {
            SelectedTeamIndex--;
            BuildRegistrationList();
        }

        private void NextTeamExecute()
        {
            SelectedTeamIndex++;
            BuildRegistrationList();
        }

        public List<string> Classes
        {
            get
            {
                return this.m_classes;
            }
            set
            {
                this.m_classes = value;
                this.OnPropertyChanged("Classes");
            }
        }

        public string SelectedClass
        {
            get
            {
                return this.m_selectedClass;
            }
            set
            {
                this.m_selectedClass = value;
                this.OnPropertyChanged("SelectedClass");
                this.BuildList();
            }
        }

        public string SelectedStarredValue
        {
            get
            {
                return this.m_selectedStarredValue;
            }
            set
            {
                this.m_selectedStarredValue = value;
                this.OnPropertyChanged("SelectedStarredValue");
                this.BuildList();
            }
        }

        public List<string> StarredValues
        {
            get
            {
                return new List<string> { All, StaredMarked };
            }
        }

        public int SelectedResultsAfterRange
        {
            get
            {
                return this.m_selectedResultsAfterRange;
            }
            set
            {
                this.m_selectedResultsAfterRange = value;
                this.OnPropertyChanged("SelectedResultsAfterRange");
                this.BuildList();
            }
        }

        public List<int> ResultsAfterRange
        {
            get
            {
                return new List<int> { 1, 2, 3, 4, 5 };
            }
        }

        public ObservableCollection<ResultViewModel> Results
        {
            get
            {
                return this.m_results;
            }
            set
            {
                this.m_results = value;
                this.OnPropertyChanged("Results");
                this.OnPropertyChanged("Classes");
            }
        }

        public string SelectedCompetition
        {
            get
            {
                return this.m_selectedCompetition;
            }

            set
            {
                this.m_selectedCompetition = value;
                this.OnPropertyChanged("SelectedCompetition");
            }
        }

        public List<string> CompetitionsList
        {
            get
            {
                return this.m_competitionsList;
            }

            set
            {
                this.m_competitionsList = value;
                this.OnPropertyChanged("CompetitionsList");
            }
        }

        public int SelectedTeamIndex    
        {
            get
            {
                return this.m_selectedTeamIndex;
            }
            set
            {
                this.m_selectedTeamIndex = value;
                this.OnPropertyChanged("SelectedTeamIndex");
                this.BuildRegistrationList();
            }
        }

        public ObservableCollection<RegistrationViewModel> SelectedTeam
        {
            get
            {
                return this.m_selectedTeam;
            }
            set
            {
                this.m_selectedTeam = value;
                this.OnPropertyChanged("SelectedTeam");
            }
        }

        public void RefreshExecute()
        {
            if (string.IsNullOrWhiteSpace(this.m_selectedCompetition))
            {
                MessageBox.Show("Må velge konkurranse før refresh kan utføres");
                return;
            }

            var results = this.m_serviceClient.GetResults(RangeClass, this.m_selectedCompetition).ToList();

            var classes = results.Select(r => r.Class).Distinct().OrderBy(r => r).ToList();
            //if (classes.Any(c => c == "2" || c == "3" || c == "4" || c == "5" || c == "V55"))
            //{
            //    classes.Add("2-5,V55");
            //}

            if (classes.Any(c => c == "3" || c == "4" || c == "5" ))
            {
                classes.Add("3-5");
            }

            ////if (!classes.Any(string.IsNullOrWhiteSpace))
            ////{
            ////    classes.Insert(0, string.Empty);
            ////}

            this.Classes = classes.ToList();

            var sorted = results.OrderByDescending(r => r.TotalSum).ThenByDescending(r => r.TotalInnerHits);

            m_allResults = new List<ResultViewModel>();

            foreach (var item in sorted)
            {
                m_allResults.Add(new ResultViewModel { RangeClass = RangeClass, Result = item });
            }

            this.BuildList();
        }

        public void UpdateStared(int shooterid, bool isStared)
        {
            if (m_allResults == null || !m_allResults.Any())
            {
                return;
            }

            var rvm = m_allResults.SingleOrDefault(r => r.Result.ShooterId == shooterid);
            if (rvm != null)
            {
                rvm.IsStared = isStared;
                if (SelectedStarredValue == StaredMarked)
                {
                    this.BuildList();
                }
            }
        }

        public void RefreshRegistrationExecute()
        {
            if (string.IsNullOrWhiteSpace(this.m_selectedCompetition))
            {
                MessageBox.Show("Må velge konkurranse før refresh kan utføres");
                return;
            }

            var results = this.m_serviceClient.GetRegistrations(RangeClass, this.m_selectedCompetition).ToList();
            m_allRegistrations = new List<RegistrationViewModel>();
            foreach (var item in results)
            {
                m_allRegistrations.Add(new RegistrationViewModel {Parent = this, RangeClass = RangeClass, Registration = item });
            }

            this.BuildRegistrationList();
        }

        private void BuildRegistrationList()
        {
            if (SelectedTeamIndex == 0)
            {
                SelectedTeamIndex = 1;
            }

            if (m_allRegistrations == null || !m_allRegistrations.Any())
            {
                return;
            }

            var maxTeamIndex = m_allRegistrations.Max(r => r.Team);
            if (SelectedTeamIndex > maxTeamIndex)
            {
                SelectedTeamIndex = maxTeamIndex;
            }

            var items = m_allRegistrations.Where(r => r.Team == SelectedTeamIndex).OrderBy(r => r.Team).ThenBy(r => r.Target).ToList();
            SelectedTeam = new ObservableCollection<RegistrationViewModel>(items);
        }

        private void BuildList()
        {
            if (m_allResults == null || !m_allResults.Any())
            {
                this.Results = new ObservableCollection<ResultViewModel>();
                return;
            }

            foreach (var resultViewModel in m_allResults)
            {
                resultViewModel.ResultAfterRange = m_selectedResultsAfterRange;
            }

            List<string> classes = null;
            if (!string.IsNullOrWhiteSpace(m_selectedClass))
            {
                if (m_selectedClass == "3-5")
                {
                    classes = new List<string> {  "3", "4", "5"};
                }
                else
                {
                    classes = new List<string> { m_selectedClass };
                }
            }

            List<ResultViewModel> resultvms;
            if (classes != null)
            {
                resultvms = m_allResults.Where(a => classes.Contains(a.Class)).ToList();
            }
            else
            {
                resultvms = m_allResults.Where(r => r.Class != "1").ToList();
            }

            resultvms.Sort(new ResultComparer());

            for (int i = 0; i < resultvms.Count; i++)
            {
                if (i == 0)
                {
                    resultvms[i].Rangering = i + 1;
                }
                else
                {
                    if (resultvms[i].CompareTo(resultvms[i - 1]) == 0)
                    {
                        resultvms[i].Rangering = resultvms[i - 1].Rangering;
                    }
                    else
                    {
                        resultvms[i].Rangering = i + 1;
                    }
                }
            }

            if (m_selectedStarredValue == StaredMarked)
            {
                var starFiltered = resultvms.Where(r => r.IsStared).ToList();
                this.Results = new ObservableCollection<ResultViewModel>(starFiltered);
            }
            else
            {
                this.Results = new ObservableCollection<ResultViewModel>(resultvms);
            }
        }
    }
}
