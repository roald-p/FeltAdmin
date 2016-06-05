
using FeltAdmin.Viewmodels;

using FeltAdminCommon;

namespace FeltResult.ViewModels
{
    public class MainResultViewModel : ViewModelBase
    {
        private MainResultViewModelForRange m_mainResultViewModel100;

        private MainResultViewModelForRange m_mainResultViewModel200;

        public MainResultViewModel()
        {
            this.MainResultViewModel100 = new MainResultViewModelForRange(RangeClass.Range100m);
            this.MainResultViewModel200 = new MainResultViewModelForRange(RangeClass.Range200m);
        }

        public MainResultViewModelForRange MainResultViewModel100
        {
            get
            {
                return this.m_mainResultViewModel100;
            }
            set
            {
                this.m_mainResultViewModel100 = value;
                this.OnPropertyChanged("MainResultViewModel100");
            }
        }

        public MainResultViewModelForRange MainResultViewModel200
        {
            get
            {
                return this.m_mainResultViewModel200;
            }
            set
            {
                this.m_mainResultViewModel200 = value;
                this.OnPropertyChanged("MainResultViewModel200");
            }
        }
    }
}
