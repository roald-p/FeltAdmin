using System.Collections.Generic;
using System.Windows.Documents;

namespace FeltAdmin.FileHandlers
{
    public interface IFileHandler
    {
        bool Exists(string filename);
        
        List<string> ReadAllLines(string filename);
    }
}