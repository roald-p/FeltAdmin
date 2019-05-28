using System;
using System.Linq;

using FeltAdmin.Database.Engine;
using FeltAdmin.Diagnostics;
using FeltAdmin.FileHandlers;
using System.Collections.Generic;
using System.IO;
using FeltAdmin.Viewmodels;
using FeltAdmin.Database.API;
using FeltAdminCommon.Orion;

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

            List<OrionNewTarget> pathTargets = null;
            if (allLines != null && allLines.Count > 0)
            {
                pathTargets = GetPatchTargets();                
            }

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
                    PatchOrion(or, pathTargets);
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

        private void PatchOrion(OrionResult or, List<OrionNewTarget> pathTargets)
        {
            var patch = pathTargets.FirstOrDefault(p => p.NewOrionId == or.OrionId && p.NewTarget == or.Target);
            if (patch != null)
            {
                or.OrionId = patch.OriginalOrionId;
                or.Target = patch.OriginalTarget;
            }
        }

        private List<OrionNewTarget> GetPatchTargets()
        {
            var allTouples = DatabaseApi.LoadCompetitionFromTable(TableName.OrionNewTarget);
            if (allTouples == null || !allTouples.Any())
            {
                return new List<OrionNewTarget>();
            }

            var allTargetPatches = allTouples.OfType<OrionNewTarget>().ToList();
            var result = new List<OrionNewTarget>();
            foreach(var patch in allTargetPatches)
            {
                var toRemove = result.FirstOrDefault(r => r.OriginalOrionId == patch.OriginalOrionId && r.OriginalTarget == patch.OriginalTarget);
                if (toRemove != null)
                {
                    result.Remove(toRemove);
                }

                if (patch.OriginalOrionId == patch.NewOrionId && patch.OriginalTarget == patch.NewTarget)
                {
                    continue;
                }

                result.Add(patch);
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
