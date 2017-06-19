using System;

using FeltAdmin.Database.Engine;
using FeltAdmin.Diagnostics;
using FeltAdmin.Orion;
using FeltAdmin.Viewmodels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using FeltAdminCommon.Helpers;
using FeltAdminCommon.Leon;

namespace FeltAdmin.Leon
{
    public class LeonWriter
    {
        private const string ToLeonUPD = "KMO.UPD";
        private const string ToLeonData = "KMONEW.TXT";
        private const string ToLeonDataTmp = "KMONEW.TXT.tmp";
        private static readonly byte[] s_updFileContent = new byte[] { 0x4B };

        public static void WriteLeonResults(List<int> finishedShooters, List<OrionResult> orionResults, OrionSetupViewModel orionSetup, List<LeonPerson> leonPersons, List<MinneRegistration> minnePersons)
        {
            var tmpBasePath = DatabaseApi.GetActiveCompetition();

            var tmpPath = Path.Combine(tmpBasePath, "LeonTmp");
            var tmpPathMinne = Path.Combine(tmpBasePath, "MinneLeonTemp");
            if (!Directory.Exists(tmpPath))
            {
                Directory.CreateDirectory(tmpPath);
            }

            if (!Directory.Exists(tmpPathMinne))
            {
                Directory.CreateDirectory(tmpPathMinne);
            }

            var leonResultsFelt = new List<string>();
            var leonResultsBane = new List<string>();
            MinneRegistration minnePerson = null;
            LeonPerson leonPerson = null;
            foreach (var finishedShooter in finishedShooters)
            {
                var allResultsForShooter = orionResults.Where(o => o.ShooterId == finishedShooter);
                leonPerson = leonPersons.SingleOrDefault(l => l.ShooterId == finishedShooter);

                if (minnePersons != null)
                {
                    minnePerson = minnePersons.SingleOrDefault(l => l.ShooterId == finishedShooter);
                }

                int sumFelt = 0;
                int sumBane = 0;
                List<string> allFeltSeries = new List<string>();
                List<string> allMinneSeries = new List<string>();

                var allRanges = orionSetup.OrionViewModels.SelectMany(o => o.RangeViews).OrderBy(r => r.RangeId);
                ////foreach (var orion in orionSetup.OrionViewModels)
                ////{
                foreach (var rangeViewModel in allRanges)
                    {
                        if (rangeViewModel.RangeType == RangeType.Shooting)
                        {
                            var resultForThisRange = CalculateOrionAndRange.GetResultForThisRange(allResultsForShooter, rangeViewModel.Parent, rangeViewModel);
                            if (resultForThisRange == null)
                            {
                                if (rangeViewModel.ResultType == ResultType.Felt)
                                {
                                    allFeltSeries.Add(string.Empty);
                                }
                            }
                            else
                            {
                                if (resultForThisRange.ResultType == ResultType.Felt)
                                {
                                    sumFelt += resultForThisRange.GetSum();
                                    allFeltSeries.AddRange(resultForThisRange.Series);
                                }
                                else if (resultForThisRange.ResultType == ResultType.Bane)
                                {
                                    sumBane += resultForThisRange.GetSum();
                                    allMinneSeries.AddRange(resultForThisRange.Series);
                                }
                            }
                        }
                    }
                ////}

                ////foreach (var orionResult in allResultsForShooter)
                ////{
                ////    if (orionResult.ResultType == ResultType.Felt)
                ////    {
                ////        sumFelt += orionResult.GetSum();
                ////        allFeltSeries.AddRange(orionResult.Series);
                ////    }
                ////    else if (orionResult.ResultType == ResultType.Bane)
                ////    {
                ////        sumBane += orionResult.GetSum();
                ////        allMinneSeries.AddRange(orionResult.Series);
                ////    }
                ////}

                if (allFeltSeries.Any() && leonPerson != null)
                {
                    var allShots = string.Join(";", allFeltSeries).ToUpper();

                    var leonLine = string.Format(
                        "{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                        leonPerson.Range,
                        leonPerson.Team,
                        leonPerson.Target,
                        leonPerson.ShooterId,
                        leonPerson.Name,
                        leonPerson.ClubName,
                        leonPerson.Class,
                        sumFelt,
                        allShots);

                    leonResultsFelt.Add(leonLine);
                }

                if (allMinneSeries.Any() && minnePerson != null)
                {
                    var allShots = string.Join(";", allMinneSeries).ToUpper();

                    var leonLine = string.Format(
                        "{0};{1};{2};{3};{4};{5};{6};{7};{8}",
                        minnePerson.Range,
                        minnePerson.Team,
                        minnePerson.Target,
                        minnePerson.ShooterId,
                        minnePerson.Name,
                        minnePerson.ClubName,
                        minnePerson.Class,
                        sumBane,
                        allShots);

                    leonResultsBane.Add(leonLine);
                }

                var finishedPerson = new FinishedPerson
                                     {
                                         Name = leonPerson.Name,
                                         ShooterId = leonPerson.ShooterId,
                                         Target = leonPerson.Target,
                                         Team = leonPerson.Team
                                     };
                DatabaseApi.Save(finishedPerson);
            }

            if (leonResultsFelt.Any())
            {
                var filenameTmp = Path.Combine(tmpPath, ToLeonDataTmp);

                File.AppendAllLines(filenameTmp, leonResultsFelt, Encoding.GetEncoding("ISO-8859-1"));
            }

            if (leonResultsBane.Any())
            {
                var filenameTmp = Path.Combine(tmpPathMinne, ToLeonDataTmp);

                File.AppendAllLines(filenameTmp, leonResultsBane, Encoding.GetEncoding("ISO-8859-1"));
            }
        }

        internal static void CheckTmpFile(string path, bool minne = false)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                return;
            }

            var tmpBasePath = DatabaseApi.GetActiveCompetition();
            string tmpDir;
            if (minne == false)
            {
                tmpDir = "LeonTmp";
            }
            else
            {
                tmpDir = "MinneLeonTemp";
            }

            var tmpPath = Path.Combine(tmpBasePath, tmpDir);

            var filenameTmp = Path.Combine(tmpPath, ToLeonDataTmp);
            if (File.Exists(filenameTmp))
            {
                var updFile = Path.Combine(path, ToLeonUPD);
                if (!File.Exists(updFile))
                {
                    var filename = Path.Combine(path, ToLeonData);
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
