using System.IO;

namespace FeltAdminCommon.Viewmodels
{
    public class DataBaseViewModel
    {
        public string DatabasePath { get; set; }

        public string DatabaseName
        {
            get
            {
                return Path.GetFileName(DatabasePath);
            }
        }
    }
}
