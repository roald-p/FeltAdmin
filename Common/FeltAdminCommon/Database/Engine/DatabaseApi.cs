using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Media;

using FeltAdmin.Configuration;
using FeltAdmin.Database.API;
using System;
using System.Collections.Generic;

using FeltAdmin.Diagnostics;
using FeltAdmin.Leon;
using FeltAdmin.Orion;

using FeltAdminCommon.Leon;

namespace FeltAdmin.Database.Engine
{
    public class DatabaseApi
    {
        private static string m_databaseBasePath = ConfigurationLoader.GetAppSettingsValue("DatabasePath");
        private static string m_databaseBasePath100m = ConfigurationLoader.GetAppSettingsValue("DatabasePath100m");
        private static string m_databaseBasePath200m = ConfigurationLoader.GetAppSettingsValue("DatabasePath200m");

        private static string m_databasePath;

        public static bool DbSelected
        {
            get
            {
                if (string.IsNullOrWhiteSpace(m_databasePath))
                {
                    return false;
                }

                return true;
            }
        }

        public static void CreateNewCompetition(string nameOfEvent, DateTime startOfEvent)
        {
            if (string.IsNullOrEmpty(m_databaseBasePath))
            {
                throw new ConfigurationErrorsException("DatabasePath missing in configuration");
            }

            if (!Directory.Exists(m_databaseBasePath))
            {
                Directory.CreateDirectory(m_databaseBasePath);
            }

            var nameOfEventWithDate = string.Format("{0}_{1}", nameOfEvent, startOfEvent.ToString("yyyy.MM.dd"));
            m_databasePath = Path.Combine(m_databaseBasePath, nameOfEventWithDate);
            if (!Directory.Exists(m_databasePath))
            {
                Directory.CreateDirectory(m_databasePath);
            }
            else
            {
                int C = 1;
                var nameOfEventWithDateAndSequenceNumber = string.Format("{0}_{1}-{2}", nameOfEvent, startOfEvent.ToString("yyyy.MM.dd"), C);
                m_databasePath = Path.Combine(m_databaseBasePath, nameOfEventWithDateAndSequenceNumber);
                while (Directory.Exists(m_databasePath))
                {
                    C++;
                    nameOfEventWithDateAndSequenceNumber = string.Format("{0}_{1}-{2}", nameOfEvent, startOfEvent.ToString("yyyy.MM.dd"), C);
                    m_databasePath = Path.Combine(m_databaseBasePath, nameOfEventWithDateAndSequenceNumber);
                }

                Directory.CreateDirectory(m_databasePath);
            }
        }

        public static List<string> GetAllCompetitions100m()
        {
            return GetAllCompetitions(m_databaseBasePath100m);
        }

        public static List<string> GetAllCompetitions200m()
        {
            return GetAllCompetitions(m_databaseBasePath200m);
        }

        public static List<string> GetAllCompetitions()
        {
            return GetAllCompetitions(m_databaseBasePath);
        }

        public static List<string> GetAllCompetitions(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ConfigurationErrorsException("DatabasePath missing in configuration");
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var directories = Directory.EnumerateDirectories(path);
            if (!directories.Any())
            {
                return new List<string>();
            }

            var sortedDirs = directories.OrderByDescending(d => Directory.GetCreationTime(d));
            var tryGetPath = Path.Combine(path, sortedDirs.First());
            if (Directory.Exists(tryGetPath))
            {
                m_databasePath = tryGetPath;
            }

            return sortedDirs.ToList();
        }

        public static bool SelectCompetition100m(string competition)
        {
            return SelectCompetition(competition, m_databaseBasePath100m);
        }

        public static bool SelectCompetition200m(string competition)
        {
            return SelectCompetition(competition, m_databaseBasePath200m);
        }
        
        public static bool SelectCompetition(string competition)
        {
            return SelectCompetition(competition, m_databaseBasePath);
        }

        public static bool SelectCompetition(string competition, string basepath)
        {
            var path = Path.Combine(basepath, competition);
            if (Directory.Exists(path))
            {
                m_databasePath = path;
                return true;
            }

            Log.Error("Database path not existing:" + path);
            return false;
        }

        public static List<IDatabaseObject> LoadCompetitionFromTable(TableName tableName)
        {
            if (string.IsNullOrWhiteSpace(m_databasePath))
            {
                Log.Error("Select or create database must be called before saving is possible");
                return null;
            }

            var filename = Path.Combine(m_databasePath, string.Format("{0}.bos", tableName.ToString()));
            if (!File.Exists(filename))
            {
                return new List<IDatabaseObject>();
            }

            var result = new List<IDatabaseObject>();

            var lines = File.ReadAllLines(filename);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                switch (tableName)
                {
                    case TableName.LeonRegistration:
                        result.Add(LeonPerson.Parse(line));
                        break;
                    case TableName.OrionResult:
                        result.Add(OrionResult.ParseFromDatabase(line));
                        break;
                    case TableName.MinneRegistration:
                        result.Add(MinneRegistration.Parse(line));
                        break;
                    case TableName.FinishedShooter:
                        result.Add(FinishedPerson.Parse(line));
                        break;
                }
            }

            return result;
        }

        public static bool Save(IDatabaseObject databaseObject)
        {
            if (string.IsNullOrWhiteSpace(m_databasePath))
            {
                Log.Error("Select or create database must be called before saving is possible");
                return false;
            }

            var filename = Path.Combine(m_databasePath, string.Format("{0}.bos", databaseObject.TableName.ToString()));
            var row = databaseObject.RowInfo.OrderBy(r => r.Key).Select(r => r.Value);
            var colums = new List<string>();
            foreach (var columnInfo in row)
            {
                colums.Add(string.Format("{0}={1}", columnInfo.ColumnName, columnInfo.ColumnValue.ToString()));
            }

            var rowstring = string.Join(";", colums);

            using (StreamWriter sw = File.AppendText(filename)) 
            {
                sw.WriteLine(rowstring);
            }

            return true;
        }

        public static string GetActiveCompetition()
        {
            return m_databasePath;
        }
    }
}
