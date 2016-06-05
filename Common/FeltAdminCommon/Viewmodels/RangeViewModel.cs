using System.Collections.Generic;
using System.Xml.Serialization;


namespace FeltAdmin.Viewmodels
{
    public class RangeViewModel : ViewModelBase
    {
        private int m_rangeId;

        private string m_name;

        private int m_firstTarget;

        private int m_lastTarget;

        private RangeType m_rangeType;

        private ResultType m_resultType;

        private CommunicationSetup m_communicationSetup;

        private bool m_minneShooting;

        private bool m_doubleRange;

        public RangeViewModel()
        {
            this.CommunicationSetup = new CommunicationSetup();
        }

        [XmlIgnore]
        public bool RangeDetailsVisible
        {
            get
            {
                if (m_rangeType == RangeType.Shooting)
                {
                    return true;
                }

                return false;
            }
        }

        public bool DoubleRange
        {
            get
            {
                return this.m_doubleRange;
            }
            set
            {
                this.m_doubleRange = value;
                this.OnPropertyChanged("DoubleRange");
            }
        }

        public bool MinneShooting
        {
            get
            {
                return this.m_minneShooting;
            }
            set
            {
                this.m_minneShooting = value;
                this.OnPropertyChanged("CommunicationSetupVisible");
                this.OnPropertyChanged("MinneShooting");
            }
        }

        [XmlIgnore]
        public bool CommunicationSetupVisible
        {
            get
            {
                return this.RangeDetailsVisible && this.MinneShooting;
            }
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

        public RangeType RangeType
        {
            get
            {
                return this.m_rangeType;
            }

            set
            {
                this.m_rangeType = value;
                this.OnPropertyChanged("RangeType");
            }
        }

        public int RangeId
        {
            get
            {
                return this.m_rangeId;
            }
            set
            {
                this.m_rangeId = value;
                this.OnPropertyChanged("RangeId");
            }
        }

        public string Name
        {
            get
            {
                return this.m_name;
            }
            set
            {
                this.m_name = value;
                this.OnPropertyChanged("Name");
            }
        }

        public int FirstTarget
        {
            get
            {
                return this.m_firstTarget;
            }
            set
            {
                this.m_firstTarget = value;
                this.OnPropertyChanged("FirstTarget");
            }
        }

        public int LastTarget
        {
            get
            {
                return this.m_lastTarget;
            }
            set
            {
                this.m_lastTarget = value;
                this.OnPropertyChanged("LastTarget");
            }
        }

        [XmlIgnore]
        public List<ResultType> ResultTypes
        {
            get
            {
                return new List<ResultType>() { ResultType.Bane, ResultType.Felt };
            }
        }

        public ResultType ResultType
        {
            get
            {
                return this.m_resultType;
            }
            set
            {
                this.m_resultType = value;
                this.OnPropertyChanged("ResultType");
            }
        }
    }
}
