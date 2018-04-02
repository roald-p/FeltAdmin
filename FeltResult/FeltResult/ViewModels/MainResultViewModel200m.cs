using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

using FeltAdmin.Viewmodels;

using FeltAdminServer.Data;

using Microsoft.Practices.Prism.Commands;

namespace FeltResult.ViewModels
{
    public class MainResultViewModel200m : ViewModelBase
    {
        private FestResultServiceClient m_serviceClient;

        private List<string> m_competitionsList;

        private string m_selectedCompetition;

        private DelegateCommand m_refreshCommand;

        private ObservableCollection<ResultViewModel> m_results;

        private List<string> m_classes;

        private string m_selectedClass;

        private List<ResultViewModel> m_allResults; 

        public MainResultViewModel200m()
        {
            m_serviceClient = new FestResultServiceClient();
            CompetitionsList = m_serviceClient.GetCompetitions200m().ToList();
        }

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

        public void RefreshExecute()
        {
            if (string.IsNullOrWhiteSpace(this.m_selectedCompetition))
            {
                MessageBox.Show("Må velge konkurranse før refresh kan utføres");
                return;
            }

            var results = this.m_serviceClient.GetResults200m(this.m_selectedCompetition).ToList();

            var classes = results.Select(r => r.Class).Distinct().OrderBy(r => r).ToList();
            if (classes.Any(c => c == "3" || c == "4" || c == "5"))
            {
                classes.Add("3-5");
            }

            ////if (classes.Any(c => c == "2" || c == "3" || c == "4" || c == "5" || c == "V55"))
            ////{
            ////    classes.Add("2-5,V55");
            ////}

            if (!classes.Any(string.IsNullOrWhiteSpace))
            {
                classes.Insert(0, string.Empty);
            }

            this.Classes = classes.ToList();

            var sorted = results.OrderByDescending(r => r.TotalSum).ThenByDescending(r => r.TotalInnerHits);

            m_allResults = new List<ResultViewModel>();

            foreach (var item in sorted)
            {
                m_allResults.Add(new ResultViewModel { Result = item });
            }

            this.BuildList();
        }

        private void BuildList()
        {
            List<string> classes = null;
            if (!string.IsNullOrWhiteSpace(m_selectedClass))
            {
                if (m_selectedClass == "2-5,V55")
                {
                    classes = new List<string> { "2", "3", "4", "5", "V55" };
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
                resultvms = m_allResults;
            }

            for (int i = 0; i < resultvms.Count; i++)
            {
                if (i == 0)
                {
                    resultvms[i].Rangering = i + 1;
                }
                else
                {
                    if (resultvms[i].Result.TotalSum == resultvms[i - 1].Result.TotalSum
                        && resultvms[i].Result.TotalInnerHits == resultvms[i - 1].Result.TotalInnerHits)
                    {
                        resultvms[i].Rangering = resultvms[i - 1].Rangering;
                    }
                    else
                    {
                        resultvms[i].Rangering = i + 1;
                    }
                }
            }

            this.Results = new ObservableCollection<ResultViewModel>(resultvms);
        }
    }
}
