using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeltAdminCommon.Orion
{
    public class MoveEventArgs : EventArgs
    {
        public List<FeltAdmin.Orion.OrionRegistration> MoveRegistrations { get; set; }
    }
}
