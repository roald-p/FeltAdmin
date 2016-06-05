using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FeltResult.ViewModels;

namespace FeltResult.Comparer
{
    public class ResultComparer : IComparer<ResultViewModel>
    {
        public int Compare(ResultViewModel x, ResultViewModel y)
        {
            return x.CompareTo(y);
        }
    }
}
