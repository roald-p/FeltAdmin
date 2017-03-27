using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeltSpooler.ViewModel
{
    public class FileNameInfo
    {
        public DateTime CreatedTime { get; set; }

        public string FileName { get; set; }

        public override string ToString()
        {
            return FileName;
        }
    }
}
