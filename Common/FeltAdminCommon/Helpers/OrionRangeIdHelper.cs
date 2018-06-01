using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeltAdminCommon.Helpers
{
    public static class OrionRangeIdHelper
    {
        private static int s_rangeId=0;

        public static int GetNextRangeId()
        {
            s_rangeId++;
            return s_rangeId;
        }
    }
}
