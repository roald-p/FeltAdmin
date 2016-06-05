using System;
using System.Collections.Generic;

namespace FeltAdmin.Orion
{
    public class OrionResultsEventArgs : EventArgs
    {
        public List<OrionResult> NewResults { get; set; }
    }
}
