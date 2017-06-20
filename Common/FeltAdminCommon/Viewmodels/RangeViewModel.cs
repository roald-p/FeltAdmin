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

        private OrionViewModel m_parent;

        public RangeViewModel(OrionViewModel parent)
        {
            this.CommunicationSetup = new CommunicationSetup();
            m_parent = parent;
        }

        public RangeViewModel()
        {
        }

        [XmlIgnore]
        public OrionViewModel Parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                m_parent = value;
            }
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

        [XmlIgnore]
        public string DoubleRangeStr
        {
            get
            {
                if (m_doubleRange)
                {
                    return "Dobbelthold";
                }

                return "Enkelthold";
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

        [XmlIgnore]
        public string NameOrType
        {
            get
            {
                if (!string.IsNullOrEmpty(m_name))
                {
                    return m_name;
                }

                if (m_rangeType == RangeType.Shooting)
                {
                    if (m_minneShooting)
                    {
                        return "Minneskyting";
                    }

                    return "Skyting";
                }

                return "Pause";
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

        [XmlIgnore]
        public bool IsShooting
        {
            get
            {
                return m_rangeType == RangeType.Shooting;
            }
        }

        [XmlIgnore]
        public string ResultTypeStr
        {
            get
            {
                if (m_resultType == ResultType.Bane)
                {
                    return "Bane";
                }
                
                if (m_resultType == ResultType.Felt)
                {
                    return "Felt";
                }

                return null;
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

        [XmlIgnore]
        public string OrionIdStr
        {
            get
            {
                if (m_parent != null)
                {
                    return m_parent.OrionId.ToString();
                }

                return "OrionId ikke satt";
            }
        }
    }
}
