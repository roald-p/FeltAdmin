using System;
using System.Linq;

using FeltAdmin.Database.Engine;
using FeltAdmin.Diagnostics;
using FeltAdmin.FileHandlers;
using System.Collections.Generic;
using System.IO;
using FeltAdmin.Viewmodels;

namespace FeltAdmin.Orion
{
    public class OrionReader
    {
        private string m_filepath;

        private IFileHandler m_fileHandler;

        public OrionReader(string filepath, IFileHandler fileHandler)
        {
            m_filepath = filepath;
            m_fileHandler = fileHandler;
        }

        public OrionReader(string filepath)
            : this(filepath, new FileHandler())
        {
        }

        private bool CheckForNewFile()
        {
            var filename = GetFilename();
            var updFile = this.GetUPDFilename();
            if (m_fileHandler.Exists(updFile))
            {
                return m_fileHandler.Exists(filename);
            }

            return false;
        }

        public List<OrionResult> GetOrionResult(OrionViewModel orionViewModel)
        {
            if (this.CheckForNewFile() == false)
            {
                return null;
            }

            var filename = GetFilename();
            var updFile = this.GetUPDFilename();
            var allLines = m_fileHandler.ReadAllLines(filename);
            var id = "NoId";
            var result = new List<OrionResult>();
            foreach (var line in allLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var or = OrionResult.ParseFromOrion(line, orionViewModel);
                id = or.OrionId.ToString();
                if (or.ShooterId > 0)
                {
                    DatabaseApi.Save(or);
                    result.Add(or);
                }
            }

            var dbroot = DatabaseApi.GetActiveCompetition();
            var backupPath = Path.Combine(dbroot, "Backup");
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            var bkupfilename = Path.Combine(backupPath, string.Format("{0}_Orion_{1}_kmonew.txt", DateTime.Now.ToString("yyyyMMdd-HHmmss"), id));
            try
            {
                File.Move(filename, bkupfilename);
                File.Delete(updFile);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unable to move to backup: " + filename);
            }

            return result;
        }

        private string GetFilename()
        {
            if (string.IsNullOrWhiteSpace(m_filepath))
            {
                return null;
            }

            return Path.Combine(m_filepath, "KMONEW.TXT");
        }

        private string GetUPDFilename()
        {
            return Path.Combine(m_filepath, "KMO.UPD");
        }
    }
}
