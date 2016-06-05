using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FeltAdmin.Viewmodels;

namespace FeltAdmin.Helpers
{
    public class Settings
    {
        public string LeonPath { get; set; }
        public OrionSetupViewModel OrionSetting { get; set; }
        public LeonCommunication LeonCommunicationSetting { get; set; }
    }
}
