using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeltAdmin.FileHandlers
{
    public class FileHandler : IFileHandler
    {
        public bool Exists(string filename)
        {
            return File.Exists(filename);
        }

        public List<string> ReadAllLines(string filename)
        {
            return new List<string>(File.ReadAllLines(filename, Encoding.GetEncoding("iso-8859-1")));
        }
    }
}
