using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Media;

using FeltAdmin.Viewmodels;

using FeltAdminCommon;

using FeltAdminServer.Data;
using System;
using System.Linq;

using FeltResult.StartHandling;

namespace FeltResult.ViewModels
{
    public class ResultViewModel : ViewModelBase, IComparable
    {
        private Result m_result;

        private int m_rangering;

        private bool m_isStared;

        private int m_resultAfterRange = 5;

        private string m_seriesAfterRange;

        private int m_totalAfterRange;

        private int m_innerAfterRange;

        public int ResultAfterRange
        {
            get
            {
                return this.m_resultAfterRange;
            }
            set
            {
                this.m_resultAfterRange = value;
                this.CalculateTotalsAfterRange();
            }
        }

        public void CalculateTotalsAfterRange()
        {
            if (m_result != null && m_result.FeltHolds != null && m_result.FeltHolds.Count() > 0)
            {
                var allSeries = new List<string>();
                int innerTotal = 0;
                int total = 0;
                for (int i = 0; i < m_resultAfterRange && i < m_result.FeltHolds.Count(); i++)
                {
                    innerTotal += m_result.FeltHolds[i].InnerHits;
                    total += m_result.FeltHolds[i].Hits;
                    allSeries.Add(string.Format("{0}/{1}", m_result.FeltHolds[i].Hits, m_result.FeltHolds[i].InnerHits));
                }

                SeriesAfterRange = string.Join(" ", allSeries);
                TotalAfterRange = total;
                InnerAfterRange = innerTotal;
                this.OnPropertyChanged("TotalAfterRangeFormatted");
            }
        }

        public int InnerAfterRange
        {
            get
            {
                return this.m_innerAfterRange;
            }
            set
            {
                this.m_innerAfterRange = value;
                this.OnPropertyChanged("InnerAfterRange");
            }
        }

        public int TotalAfterRange
        {
            get
            {
                return this.m_totalAfterRange;
            }
            set
            {
                this.m_totalAfterRange = value;
                this.OnPropertyChanged("TotalAfterRange");
            }
        }

        public string SeriesAfterRange
        {
            get
            {
                return this.m_seriesAfterRange;
            }
            set
            {
                this.m_seriesAfterRange = value;
                this.OnPropertyChanged("SeriesAfterRange");
            }
        }

        public string TotalAfterRangeFormatted
        {
            get
            {
                return string.Format("{0}/{1}", TotalAfterRange, InnerAfterRange);
            }
        }

        public RangeClass RangeClass { get; set; }

        public Result Result
        {
            get
            {
                return this.m_result;
            }
            set
            {
                this.m_result = value;
                this.CalculateTotalsAfterRange();
                this.OnPropertyChanged("Result");

                var staredShooters = StarFileHandler.StaredShooters(RangeClass);
                if (staredShooters.Contains(m_result.ShooterId))
                {
                    IsStared = true;
                }

            }
        }

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
            }
        }

        public string Name
        {
            get
            {
                return this.m_result.Name;
            }
        }

        public string Class
        {
            get
            {
                return this.m_result.Class;
            }
        }

        public string ClubName
        {
            get
            {
                return this.m_result.ClubName;
            }
        }

        public string Total
        {
            get
            {
                return string.Format("{0}/{1}", m_result.TotalSum, m_result.TotalInnerHits);
            }
        }

        public string Series
        {
            get
            {
                return this.m_result.TotalResult;
            }
        }

        public int Minne
        {
            get
            {
                return this.m_result.Minne;
            }
        }

        public int MinneInner
        {
            get
            {
                return this.m_result.MinneInner;
            }
        }

        public string FormattedMinne
        {
            get
            {
                return string.Format("{0}({1})", Minne, MinneInner);
            }
        }

        public int Rangering
        {
            get
            {
                return this.m_rangering;
            }
            set
            {
                this.m_rangering = value;
                this.OnPropertyChanged("Rangering");
            }
        }

        public int CompareTo(object obj)
        {
            var y = obj as ResultViewModel;
            if (y == null)
            {
                return -1;
            }

            if (this.TotalAfterRange < y.TotalAfterRange)
            {
                return 1;
            }

            if (this.TotalAfterRange > y.TotalAfterRange)
            {
                return -1;
            }

            if (this.InnerAfterRange < y.InnerAfterRange)
            {
                return 1;
            }

            if (this.InnerAfterRange > y.InnerAfterRange)
            {
                return -1;
            }

            if (this.Result.FeltHolds.Count() >= m_resultAfterRange && y.Result.FeltHolds.Count() >= m_resultAfterRange)
            {
                for (int i = m_resultAfterRange - 1; i >= 0; i--)
                {
                    if (this.Result.FeltHolds[i].Hits > y.Result.FeltHolds[i].Hits)
                    {
                        return -1;
                    }

                    if (this.Result.FeltHolds[i].Hits < y.Result.FeltHolds[i].Hits)
                    {
                        return 1;
                    }

                    if (this.Result.FeltHolds[i].InnerHits > y.Result.FeltHolds[i].InnerHits)
                    {
                        return -1;
                    }

                    if (this.Result.FeltHolds[i].InnerHits < y.Result.FeltHolds[i].InnerHits)
                    {
                        return 1;
                    }
                }
            }

            if (this.Result.FeltHolds.Count() < y.Result.FeltHolds.Count())
            {
                return -1;
            }

            if (this.Result.FeltHolds.Count() > y.Result.FeltHolds.Count())
            {
                return 1;
            }

            return 0;



            ////if (this.Result.TotalSum < y.Result.TotalSum)
            ////{
            ////    return 1;
            ////}

            ////if (this.Result.TotalSum > y.Result.TotalSum)
            ////{
            ////    return -1;
            ////}

            ////if (this.Result.TotalInnerHits < y.Result.TotalInnerHits)
            ////{
            ////    return 1;
            ////}

            ////if (this.Result.TotalInnerHits > y.Result.TotalInnerHits)
            ////{
            ////    return -1;
            ////}

            ////if (this.Result.FeltHolds.Count() < y.Result.FeltHolds.Count())
            ////{
            ////    return -1;
            ////}

            ////if (this.Result.FeltHolds.Count() > y.Result.FeltHolds.Count())
            ////{
            ////    return 1;
            ////}

            ////for (int i = this.Result.FeltHolds.Count() - 1; i >= 0; i--)
            ////{
            ////    if (this.Result.FeltHolds[i].Hits > y.Result.FeltHolds[i].Hits)
            ////    {
            ////        return -1;
            ////    }

            ////    if (this.Result.FeltHolds[i].Hits < y.Result.FeltHolds[i].Hits)
            ////    {
            ////        return 1;
            ////    }

            ////    if (this.Result.FeltHolds[i].InnerHits > y.Result.FeltHolds[i].InnerHits)
            ////    {
            ////        return -1;
            ////    }

            ////    if (this.Result.FeltHolds[i].InnerHits < y.Result.FeltHolds[i].InnerHits)
            ////    {
            ////        return 1;
            ////    }
            ////}

            ////return 0;

        }
    }
}
