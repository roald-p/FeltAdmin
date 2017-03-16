using System;

using FeltAdmin.Database.Engine;
using FeltAdmin.Diagnostics;
using FeltAdmin.FileHandlers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FeltAdmin.Leon
{
    public class MinneReader
    {
        private string m_filepath;

        private IFileHandler m_fileHandler;

        public MinneReader(string filepath, IFileHandler fileHandler)
        {
            m_filepath = filepath;
            m_fileHandler = fileHandler;
        }

        public MinneReader(string filepath)
            : this(filepath, new FileHandler())
        {
        }

        public bool CheckForNewFile()
        {
            var filename = GetFilename();
            var updFile = this.GetUPDFilename();
            if (m_fileHandler.Exists(updFile))
            {
                return m_fileHandler.Exists(filename);
            }

            return false;
        }

        public List<MinneRegistration> GetMinneRegistrations()
        {
            var filename = GetFilename();
            var updFile = this.GetUPDFilename();
            var allLines = m_fileHandler.ReadAllLines(filename);

            var result = new Dictionary<string, MinneRegistration>();
            foreach (var line in allLines)
            {
                var lp = this.GetMinneRegistrationFromLine(line);

                if (result.ContainsKey(lp.Key))
                {
                    result.Remove(lp.Key);
                }

                result.Add(lp.Key, lp);
            }

            var dbroot = DatabaseApi.GetActiveCompetition();
            var backupPath = Path.Combine(dbroot, "Backup");
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            var bkupfilename = Path.Combine(backupPath, string.Format("{0}_Minne_kminew.txt", DateTime.Now.ToString("yyyyMMdd-hhmmss")));
            try
            {
                File.Move(filename, bkupfilename);
                File.Delete(updFile);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unable to move to backup: " + filename);
            }

            return result.Values.ToList();
        }

        private string GetFilename()
        {
            return Path.Combine(m_filepath, "KMINEW.TXT");
        }

        private string GetUPDFilename()
        {
            return Path.Combine(m_filepath, "KMI.UPD");
        }

        private MinneRegistration GetMinneRegistrationFromLine(string line)
        {
            var touples = line.Split(';');
            var minneReg = new MinneRegistration();
            minneReg.Range = int.Parse(touples[0]);
            minneReg.Team = int.Parse(touples[1]);
            minneReg.Target = int.Parse(touples[2]);
            minneReg.Name = touples[3];
            minneReg.ClubName = touples[4];
            minneReg.Class = touples[5];
            if (!string.IsNullOrEmpty(touples[6]))
            {
                int sum = 0;
                if (int.TryParse(touples[6], out sum))
                {
                    minneReg.SumIn = sum;
                }
            }

            if (!string.IsNullOrEmpty(touples[7]))
            {
                int id = 0;
                if (int.TryParse(touples[7], out id))
                {
                    minneReg.ShooterId = id;
                }
            }

            return minneReg;
        }
    }
}
