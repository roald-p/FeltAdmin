using FeltAdminCommon.Helpers;
using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;

namespace FeltAdmin.Viewmodels
{
    public class OrionViewModel : ViewModelBase
    {
        private CommunicationSetup m_communicationSetup;

        private int m_orionId;

        private ObservableCollection<RangeViewModel> m_rangeView;

        private DelegateCommand m_addRangeCommand;

        private DelegateCommand m_addPauseCommand;

        private DelegateCommand m_removeRangeCommand;

        private RangeViewModel m_selectedRange;
        private bool m_finalRange;

        [XmlIgnore]
        public ICommand RemoveRange
        {
            get
            {
                if (m_removeRangeCommand == null)
                {
                    m_removeRangeCommand = new DelegateCommand(this.RemoveRangeExecute);
                }

                return m_removeRangeCommand;
            }
        }

        [XmlIgnore]
        public ICommand AddRangeCommand
        {
            get
            {
                if (m_addRangeCommand == null)
                {
                    m_addRangeCommand = new DelegateCommand(this.AddRangeExecute);
                }

                return m_addRangeCommand;
            }
        }

        [XmlIgnore]
        public ICommand AddPauseCommand
        {
            get
            {
                if (m_addPauseCommand == null)
                {   
                    m_addPauseCommand = new DelegateCommand(this.AddPauseExecute);
                }

                return m_addPauseCommand;
            }
        }

        [XmlIgnore]
        public bool RangeVisible
        {
            get
            {
                if (m_rangeView != null && m_rangeView.Any())
                {
                    return true;
                }

                return false;
            }
        }

        public void RemoveRangeExecute()
        {
            if (SelectedRange == null)
            {
                return;
            }

            m_rangeView.Remove(SelectedRange);
            this.OnPropertyChanged("RangeViews");
            this.OnPropertyChanged("RangeVisible");
            this.OnPropertyChanged("SortedRanges");
    
        }

        public void AddRangeExecute()
        {
            var newRange = new RangeViewModel(this);
            newRange.RangeType = RangeType.Shooting;
            if (m_rangeView == null)
            {
                m_rangeView = new ObservableCollection<RangeViewModel>();
                //newRange.RangeId = 1;
            }
            ////else
            ////{
            ////    newRange.RangeId = m_rangeView.Max(r => r.RangeId) + 1;
            ////}

            newRange.RangeId = OrionRangeIdHelper.GetNextRangeId();

            m_rangeView.Add(newRange);
            this.OnPropertyChanged("RangeViews");
            this.OnPropertyChanged("RangeVisible");
            this.OnPropertyChanged("SortedRanges");
        }

        public void AddPauseExecute()
        {
            var newRange = new RangeViewModel(this);
            newRange.RangeType = RangeType.Pause;
            if (m_rangeView == null)
            {
                m_rangeView = new ObservableCollection<RangeViewModel>();
                //newRange.RangeId = 1;
            }
            ////else
            ////{
            ////    if (m_rangeView.Count > 0)
            ////    {
            ////        newRange.RangeId = m_rangeView.Max(r => r.RangeId) + 1;
            ////    }
            ////    else
            ////    {
            ////        newRange.RangeId = 1;
            ////    }
            ////}

            newRange.RangeId = OrionRangeIdHelper.GetNextRangeId();

            m_rangeView.Add(newRange);
            this.OnPropertyChanged("RangeViews");
            this.OnPropertyChanged("RangeVisible");
            this.OnPropertyChanged("SortedRanges");

        }

        [XmlIgnore]
        public RangeViewModel SelectedRange
        {
            get
            {
                return this.m_selectedRange;
            }
            set
            {
                this.m_selectedRange = value;
                this.OnPropertyChanged("SelectedRange");
            }
        }

        public OrionViewModel()
        {
            CommunicationSetup = new CommunicationSetup();
            this.OnPropertyChanged("RangeVisible");
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

        public int OrionId
        {
            get
            {
                return this.m_orionId;
            }

            set
            {
                this.m_orionId = value;
                this.OnPropertyChanged("OrionId");
            }
        }

        public bool FinalRange
        {
            get {return m_finalRange; } 
            set
            {
                m_finalRange = value;
                OnPropertyChanged(nameof(FinalRange));
            }
        }

        public ObservableCollection<RangeViewModel> RangeViews
        {
            get
            {
                return this.m_rangeView;
            }
            set
            {
                this.m_rangeView = value;
                this.OnPropertyChanged("RangeViews");
                this.OnPropertyChanged("RangeVisible");
                this.OnPropertyChanged("SortedRanges");
            }
        }
    }
}
