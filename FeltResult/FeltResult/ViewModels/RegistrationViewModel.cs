using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

using FeltAdmin.Viewmodels;

using FeltAdminCommon;

using FeltAdminServer.Data;

using FeltResult.StartHandling;

using Prism.Commands;

namespace FeltResult.ViewModels
{
    public class RegistrationViewModel : ViewModelBase
    {
        private Registration m_registration;

        private DelegateCommand m_toggleStarCommand;

        private bool m_isStared;

        [XmlIgnore]
        public ICommand ToggleStarCommand
        {
            get
            {
                if (m_toggleStarCommand == null)
                {
                    m_toggleStarCommand = new DelegateCommand(this.ToggleStarExecute);
                }

                return m_toggleStarCommand;
            }
        }

        private void ToggleStarExecute()
        {
            IsStared = !IsStared;
            Parent.UpdateStared(this.Registration.ShooterId, IsStared);
        }

        public MainResultViewModelForRange Parent { get; set; }

        public RangeClass RangeClass { get; set; }  

        public bool IsStared    
        {
            get
            {
                return this.m_isStared;
            }
            set
            {
                this.m_isStared = value;
                this.OnPropertyChanged("IsStared");
                this.OnPropertyChanged("StarColour");
                if (m_isStared == false)
                {
                    StarFileHandler.RemoveStaredShooter(m_registration.ShooterId, RangeClass);
                }
                else
                {
                    StarFileHandler.AddStarShooter(m_registration.ShooterId, RangeClass);
                }
            }
        }

        public Brush StarColour
        {
            get
            {
                if (IsStared)
                {
                    return Brushes.Orange;
                }

                return Brushes.Gray;
            }
        }

        public Registration Registration
        {
            get
            {
                return this.m_registration;
            }
            set
            {
                this.m_registration = value;
                this.OnPropertyChanged("Registration");
                this.OnPropertyChanged("Result");

                var staredShooters = StarFileHandler.StaredShooters(RangeClass);
                if (staredShooters.Contains(m_registration.ShooterId))
                {
                    IsStared = true;
                }
            }
        }

        public int Team
        {
            get
            {
                return this.m_registration.Team;
            }
        }

        public int Target
        {
            get
            {
                return this.m_registration.Target;
            }
        }

        public Result Result
        {
            get
            {
                return m_registration.Result;
            }
        }

        public string Name
        {
            get
            {
                return this.m_registration.Name;
            }
        }

        public string Class
        {
            get
            {
                return this.m_registration.Class;
            }
        }

        public string ClubName
        {
            get
            {
                return this.m_registration.ClubName;
            }
        }

        public bool ResultVisible
        {
            get
            {
                return m_registration.Result != null;
            }
        }

        public string Total
        {
            get
            {
                if (m_registration.Result == null)
                {
                    return string.Empty;
                }

                return string.Format("{0}/{1}", m_registration.Result.TotalSum, m_registration.Result.TotalInnerHits);
            }
        }

        public string Series
        {
            get
            {
                if (m_registration.Result == null)
                {
                    return string.Empty;
                }

                return this.m_registration.Result.TotalResult;
            }
        }

        public int Minne
        {
            get
            {
                if (m_registration.Result == null)
                {
                    return 0;
                }

                return this.m_registration.Result.Minne;
            }
        }

        public int MinneInner
        {
            get
            {
                if (m_registration.Result == null)
                {
                    return 0;
                }

                return this.m_registration.Result.MinneInner;
            }
        }

        public string FormattedMinne
        {
            get
            {
                if (m_registration.Result == null)
                {
                    return string.Empty;
                }

                return string.Format("{0}({1})", Minne, MinneInner);
            }
        }
    }
}
