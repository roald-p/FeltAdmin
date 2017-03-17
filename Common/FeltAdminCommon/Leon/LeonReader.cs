using System;

using FeltAdmin.Database.Engine;
using FeltAdmin.Diagnostics;
using FeltAdmin.FileHandlers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FeltAdmin.Leon
{
    public class LeonReader
    {
        private string m_filepath;

        private IFileHandler m_fileHandler;

        public LeonReader(string filepath, IFileHandler fileHandler)
        {
            m_filepath = filepath;
            m_fileHandler = fileHandler;
        }

        public LeonReader(string filepath) : this(filepath, new FileHandler())
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

        public List<LeonPerson> GetLeonPersons()
        {
            var filename = GetFilename();
            var updFile = this.GetUPDFilename();
            var allLines = m_fileHandler.ReadAllLines(filename);

            var result = new Dictionary<string, LeonPerson>();
            foreach (var line in allLines)
            {
                var lp = this.GetLeonPersonFromLine(line);

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

            var bkupfilename = Path.Combine(backupPath, string.Format("{0}_Leon_kminew.txt", DateTime.Now.ToString("yyyyMMdd-hhmmss")));
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

        private LeonPerson GetLeonPersonFromLine(string line)
        {
            var touples = line.Split(';');
            var leonPerson = new LeonPerson();
            leonPerson.Range = int.Parse(touples[0]);
            leonPerson.Team = int.Parse(touples[1]);
            leonPerson.Target = int.Parse(touples[2]);
            leonPerson.Name = touples[3];
            leonPerson.ClubName = touples[4];
            leonPerson.Class = touples[5];
            if (!string.IsNullOrEmpty(touples[6]))
            {
                int sum = 0;
                if (int.TryParse(touples[6], out sum))
                {
                    leonPerson.SumIn = sum;
                }
            }

            if (!string.IsNullOrEmpty(touples[7]))
            {
                int id = 0;
                if (int.TryParse(touples[7], out id))
                {
                    leonPerson.ShooterId = id;
                }
            }

            return leonPerson;
        }
    }
}
