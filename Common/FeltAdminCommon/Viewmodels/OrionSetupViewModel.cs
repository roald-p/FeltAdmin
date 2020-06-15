using System.Collections.Generic;
using System.IO;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;
using Prism.Commands;

namespace FeltAdmin.Viewmodels
{
    public class OrionSetupViewModel : ViewModelBase
    {
        private ObservableCollection<OrionViewModel> m_orionViewModels;

        private DelegateCommand m_newOrionCommand;

        private DelegateCommand m_removeCommand;

        private OrionViewModel m_selectedOrion;

        private MainViewModel m_parent;

        public OrionSetupViewModel()
        {
            m_orionViewModels = new ObservableCollection<OrionViewModel>();
            this.OnPropertyChanged("OrionVisible");
        }

        [XmlIgnore]
        public MainViewModel Parent
        {
            get
            {
                return this.m_parent;
            }
            set
            {
                this.m_parent = value;
                this.OnPropertyChanged("SetupMode");
            }
        }

        public void SendPropChanged()
        {
            this.OnPropertyChanged("SetupMode");
        }

        [XmlIgnore]
        public bool SetupMode
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.SetupMode;
                }

                return true;
            }
        }

        [XmlIgnore]
        public ICommand NewOrionCommand
        {
            get
            {
                if (m_newOrionCommand == null)
                {
                    m_newOrionCommand = new DelegateCommand(this.NewOrionExecute);
                }

                return m_newOrionCommand;
            }
        }

        [XmlIgnore]
        public ICommand RemoveOrion
        {
            get
            {
                if (m_removeCommand == null)
                {
                    m_removeCommand = new DelegateCommand(this.RemoveOrionExecute);
                }

                return m_removeCommand;
            }
        }
        
        public ObservableCollection<OrionViewModel> OrionViewModels
        {
            get
            {
                return this.m_orionViewModels;
            }
            set
            {
                this.m_orionViewModels = value;
                this.OnPropertyChanged("OrionViewModels");
                this.OnPropertyChanged("SortedRanges");
            }
        }

        [XmlIgnore]
        public ObservableCollection<RangeViewModel> SortedRanges {
            get
            {
                foreach (var orionViewModel in m_orionViewModels)
                {
                    foreach (var range in orionViewModel.RangeViews)
                    {
                        range.Parent = orionViewModel;
                    }
                }

                var ranges = m_orionViewModels.SelectMany(o => o.RangeViews).OrderBy(r => r.RangeId);
                if (ranges.Any())
                {
                    return new ObservableCollection<RangeViewModel>(ranges);
                }

                return null;
            } 
        }

        [XmlIgnore]
        public OrionViewModel SelectedOrion
        {
            get
            {
                return this.m_selectedOrion;
            }

            set
            {
                this.m_selectedOrion = value;
                this.OnPropertyChanged("SelectedOrion");
            }
        }

        [XmlIgnore]
        public bool OrionVisible
        {
            get
            {
                return m_orionViewModels != null && m_orionViewModels.Any();
            }
        }

        public void RemoveOrionExecute()
        {
            if (SelectedOrion == null)
            {
                return;
            }

            m_orionViewModels.Remove(SelectedOrion);
            this.OnPropertyChanged("OrionViewModels");
            this.OnPropertyChanged("OrionVisible");
            this.OnPropertyChanged("SortedRanges");
        }

        public void NewOrionExecute()
        {
            if (m_orionViewModels == null)
            {
                m_orionViewModels = new ObservableCollection<OrionViewModel>();
            }

            var newOrion = new OrionViewModel();
            if (m_orionViewModels.Any())
            {
                newOrion.OrionId = m_orionViewModels.Max(o => o.OrionId) + 1;
            }
            else
            {
                newOrion.OrionId = 1;
            }

            m_orionViewModels.Add(newOrion);
            this.OnPropertyChanged("OrionViewModels");
            this.OnPropertyChanged("OrionVisible");
            this.OnPropertyChanged("SortedRanges");
        }

        internal List<string> Validate(LeonCommunication lc)
        {
            var errors = lc.Validate();
            if (SortedRanges == null)
            {
                errors.Add("Det finnes ingen holdid generert ");
                return errors;
            }

            var ids = SortedRanges.GroupBy(r => r.RangeId);
            var duplicateIds = ids.Where(i => i.Count() > 1);
            if (duplicateIds.Any())
            {
                errors.Add("Duplikate holdid'er: " + string.Join(",", duplicateIds.Select(d => d.Key)));
            }

            // FirstTarget > LastTarget
            foreach (var rangeViewModel in SortedRanges)
            {
                if (rangeViewModel.RangeType == RangeType.Shooting && rangeViewModel.FirstTarget > rangeViewModel.LastTarget)
                {
                    errors.Add("Første skivenr er høyere enn siste for holdid " + rangeViewModel.RangeId);
                }
            }

            // Targets overlap
            foreach (var orionViewModel in m_orionViewModels)
            {
                var sortedRanges = orionViewModel.RangeViews.Where(h => h.RangeType == RangeType.Shooting).OrderBy(r => r.FirstTarget).ThenBy(r => r.LastTarget).ToList();
                for (int i = 0; i < sortedRanges.Count() - 1; i++)
                {
                    if (sortedRanges[i].FirstTarget >= sortedRanges[i + 1].FirstTarget)
                    {
                        errors.Add("Overlappende skivenummer for holid'er: " + sortedRanges[i].RangeId + ", " + sortedRanges[i+1].RangeId);
                    }
                    else if (sortedRanges[i].LastTarget >= sortedRanges[i + 1].FirstTarget)
                    {
                        errors.Add("Overlappende skivenummer for holid'er: " + sortedRanges[i].RangeId + ", " + sortedRanges[i + 1].RangeId);
                    }
                }
            }

            // Number of targets per range not equal
            var groupByNumOfTargets = SortedRanges.Where(r => r.RangeType == RangeType.Shooting).GroupBy(r => r.LastTarget - r.FirstTarget);
            if (groupByNumOfTargets.Count() > 1)
            {
                errors.Add("Forskjellig antall skiver på hold");
            }

            // Shooting ranges with zero counting shoots
            foreach (var rangeViewModel in SortedRanges)
            {
                if (rangeViewModel.RangeType == RangeType.Shooting && rangeViewModel.CountingShoots < 1)
                {
                    errors.Add(string.Format("Tellende skudd = {0} for HoldId= {1}", rangeViewModel.CountingShoots, rangeViewModel.RangeId));
                }
            }

            
            // Missing and duplicate communication catalogs
            var catalogs = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(lc.CommunicationSetup.SelectedPath))
            {
                catalogs.Add(lc.CommunicationSetup.SelectedPath, "Leon kommunikasjon");
            }

            foreach (var orionViewModel in m_orionViewModels)
            {
                if (string.IsNullOrWhiteSpace(orionViewModel.CommunicationSetup.SelectedPath))
                {
                    errors.Add("Kommunikasjonskatalog ikke satt opp for orion " + orionViewModel.OrionId);
                }
                else if (!Directory.Exists(orionViewModel.CommunicationSetup.SelectedPath))
                {
                    errors.Add("Kommunikasjonskatalog for orion finnes ikke: " + orionViewModel.CommunicationSetup.SelectedPath);
                }
                else if (catalogs.ContainsKey(orionViewModel.CommunicationSetup.SelectedPath))
                {
                    errors.Add(string.Format("Kommunikasjonskatalog \"{0}\" for OrionId = {1} er allerede i bruk for {2}", orionViewModel.CommunicationSetup.SelectedPath, orionViewModel.OrionId, catalogs[orionViewModel.CommunicationSetup.SelectedPath]));
                }
                else
                {
                    catalogs.Add(orionViewModel.CommunicationSetup.SelectedPath, string.Format("OrionId = {0}", orionViewModel.OrionId));
                }
            }

            // Minne
            var minnerange = SortedRanges.Where(r => r.MinneShooting == true);
            {
                foreach (var rangeViewModel in minnerange)
                {
                    if (string.IsNullOrWhiteSpace(rangeViewModel.CommunicationSetup.SelectedPath))
                    {
                        errors.Add("Kommunikasjonskatalog ikke satt opp for minneskyting");
                    }
                    else if (!Directory.Exists(rangeViewModel.CommunicationSetup.SelectedPath))
                    {
                        errors.Add("Kommunikasjonskatalog for minneskyting finnes ikke: " + rangeViewModel.CommunicationSetup.SelectedPath);
                    }

                    if (catalogs.ContainsKey(rangeViewModel.CommunicationSetup.SelectedPath))
                    {
                        errors.Add(string.Format("Kommunikasjonskatalog \"{0}\" for minne på HoldId = {1} er allerede i bruk for {2}", rangeViewModel.CommunicationSetup.SelectedPath, rangeViewModel.RangeId, catalogs[rangeViewModel.CommunicationSetup.SelectedPath]));
                    }
                    else
                    {
                        catalogs.Add(rangeViewModel.CommunicationSetup.SelectedPath, string.Format("OrionId = {0}", rangeViewModel.RangeId));
                    }

                }
            }

            // Final shooting
            var finalOrions = m_orionViewModels.Where(o => o.FinalRange == true);
            if (finalOrions != null && finalOrions.Count() > 1)
            {
                var orionIds = finalOrions.Select(o => o.OrionId);
                var errorMessage = string.Format("Finale bare stsøttet for en orion og må være dobbelthold på en kommando, orion-id'er med finale: {0}", string.Join(",", orionIds));
                errors.Add(errorMessage);
            }

            return errors;
        }
    }
}
