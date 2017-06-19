using System;

using FeltAdmin.Database.Engine;
using FeltAdmin.Diagnostics;
using FeltAdmin.Leon;
using FeltAdmin.Viewmodels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FeltAdmin.Orion
{
    public class OrionGenerate
    {
        private const string ToOrionUPD = "KMI.UPD";
        private const string ToOrionData = "KMINEW.TXT";
        private const string ToOrionDataTmp = "KMINEW.TXT.tmp";
        private static readonly byte[] s_updFileContent = new byte[] { 0x4B };

        public static List<OrionRegistration> GenerateOrionForShooter(LeonPerson person, OrionSetupViewModel orionViewModel)
        {
            var result = new List<OrionRegistration>();
            var allRanges = orionViewModel.OrionViewModels.SelectMany(o => o.RangeViews).OrderBy(r => r.RangeId);
            int currentTeam = person.Team;
            foreach (var range in allRanges)
            {
                if (range.RangeType == RangeType.Pause)
                {
                    currentTeam++;
                    continue;
                }

                var orionRegistration = new OrionRegistration
                                        {
                                            OrionId = range.Parent.OrionId,
                                            Team = currentTeam,
                                            Target = range.FirstTarget + person.Target - 1,
                                            Name = person.Name,
                                            ClubName = person.ClubName,
                                            Class = person.Class,
                                            SumIn = person.SumIn,
                                            ShooterId = person.ShooterId,
                                            RangeId = range.RangeId,
                                            RangeName = range.Name
                                        };

                result.Add(orionRegistration);
                currentTeam++;
            }

            return result;
        }

        public static List<OrionRegistration> GenerateOrionForShooter_old(LeonPerson person, OrionSetupViewModel orionViewModel)
        {
            var result = new List<OrionRegistration>();
            foreach (var orion in orionViewModel.OrionViewModels)
            {
                int currentTeam = person.Team;
                foreach (var range in orion.RangeViews)
                {
                    if (range.RangeType == RangeType.Pause)
                    {
                        currentTeam++;
                        continue;
                    }

                    var orionRegistration = new OrionRegistration
                    {
                        OrionId = orion.OrionId,
                        Team = currentTeam,
                        Target = range.FirstTarget + person.Target - 1,
                        Name = person.Name,
                        ClubName = person.ClubName,
                        Class = person.Class,
                        SumIn = person.SumIn,
                        ShooterId = person.ShooterId,
                        RangeId = range.RangeId,
                        RangeName = range.Name
                    };

                    result.Add(orionRegistration);
                    currentTeam++;
                }
            }

            return result;
        }

        public static void TransferAllRegistrationsToAllOrions(List<OrionRegistration> allShooters, OrionSetupViewModel orionViewModel)
        {
            foreach (var orion in orionViewModel.OrionViewModels)
            {
                var allRegistrationsForThisOrion = allShooters.Where(o => o.OrionId == orion.OrionId).OrderBy(o => o.Team).ThenBy(o => o.Target);
                if (allRegistrationsForThisOrion.Any())
                {
                    WriteAllRegistrationsToOrion(orion.CommunicationSetup.SelectedPath, allRegistrationsForThisOrion, orion.OrionId);
                }
            }
        }

        private static void WriteAllRegistrationsToOrion(string selectedPath, IEnumerable<OrionRegistration> allRegistrationsForThisOrion, int orionId)
        {
            var tmpBasePath = DatabaseApi.GetActiveCompetition();
            var tmpPath = Path.Combine(tmpBasePath, string.Format("Orion_{0}", orionId));
            if (!Directory.Exists(tmpPath))
            {
                Directory.CreateDirectory(tmpPath);
            }

            var filenameTmp = Path.Combine(tmpPath, ToOrionDataTmp);
            var allLinesForThisOrion = new List<string>();
            foreach (var orionRegistration in allRegistrationsForThisOrion)
            {
                var line = orionRegistration.OrionInput;
                allLinesForThisOrion.Add(line);
            }


            // Feilhåndtering her, når maskin ikke er tilgjengelig....
            File.AppendAllLines(filenameTmp, allLinesForThisOrion, Encoding.GetEncoding("ISO-8859-1"));
            
            ////var updFile = Path.Combine(selectedPath, ToOrionUPD);
            ////if (!File.Exists(updFile))
            ////{
            ////    var filename = Path.Combine(selectedPath, ToOrionData);
            ////    File.Move(filenameTmp, filename);
            ////    using (FileStream file = new FileStream(updFile, FileMode.Create, System.IO.FileAccess.Write))
            ////    {
            ////        file.Write(s_updFileContent, 0, s_updFileContent.Length);
            ////        file.Flush(true);
            ////        file.Close();
            ////    }
            ////}
        }

        internal static void CheckTmpFile(OrionSetupViewModel orionViewModel)
        {
            foreach (var orion in orionViewModel.OrionViewModels)
            {
                var path = orion.CommunicationSetup.SelectedPath;
                if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                {
                    return;
                }

                var tmpBasePath = DatabaseApi.GetActiveCompetition();
                var tmpPath = Path.Combine(tmpBasePath, string.Format("Orion_{0}", orion.OrionId));

                var filenameTmp = Path.Combine(tmpPath, ToOrionDataTmp);
                if (File.Exists(filenameTmp))
                {
                    var updFile = Path.Combine(path, ToOrionUPD);
                    if (!File.Exists(updFile))
                    {
                        var filename = Path.Combine(path, ToOrionData);
                        var fileMoveError = false;

                        try
                        {
                            File.Move(filenameTmp, filename);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Unable to move file " + filenameTmp + " to " + filename);
                            fileMoveError = true;
                        }

                        if (!fileMoveError)
                        {
                            try
                            {
                                using (FileStream file = new FileStream(updFile, FileMode.Create, System.IO.FileAccess.Write))
                                {
                                    file.Write(s_updFileContent, 0, s_updFileContent.Length);
                                    file.Flush(true);
                                    file.Close();
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Unable to create file " + updFile);
                                try
                                {
                                    File.Move(filename, filenameTmp);
                                }
                                catch (Exception ex1)
                                {
                                    Log.Error(ex1, "Unable to move back file after UPD file create error " + filename + ", " + filenameTmp);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
