using Microsoft.Practices.Prism.Commands;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;

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
                return Parent.SetupMode;
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
        }
    }
}
