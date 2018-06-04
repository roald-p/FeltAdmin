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
    using System.Threading;

    public class LeonWriter
    {
        private const string ToLeonUPD = "KMO.UPD";

        private const string ToLeonData = "KMONEW.TXT";

        private const string ToLeonDataTmp = "KMONEW.TXT.tmp";

        private static readonly byte[] s_updFileContent = new byte[] { 0x4B };

        private static object syncObject = new object();

        private static string m_minnePath;

        public static void WriteLeonResults(
            List<int> finishedShooters,
            List<OrionResult> orionResults,
            OrionSetupViewModel orionSetup,
            List<LeonPerson> leonPersons,
            List<MinneRegistration> minnePersons)
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
                        var resultForThisRange = CalculateOrionAndRange.GetResultForThisRange(
                            allResultsForShooter,
                            rangeViewModel.Parent,
                            rangeViewModel);
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
                                resultForThisRange.ValidSeries = resultForThisRange.CalculateValidSeriesForRange(rangeViewModel);

                                allFeltSeries.AddRange(resultForThisRange.ValidSeries);
                            }
                            else if (resultForThisRange.ResultType == ResultType.Bane)
                            {
                                sumBane += resultForThisRange.GetSum();
                                foreach (var rawSerie in resultForThisRange.Series)
                                {
                                    int len = rangeViewModel.CountingShoots;

                                    if (rawSerie.Length < rangeViewModel.CountingShoots)
                                    {
                                        len = rawSerie.Length;
                                    }
                                    string serieSingle = rawSerie.Substring(0, len);
                                    int slen = serieSingle.Length;
                                    while (slen < rangeViewModel.CountingShoots)
                                    {
                                        serieSingle = serieSingle + "0";
                                        slen++;
                                    }
                                    allMinneSeries.Add(serieSingle);
                                }
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
                        "{0};{1};{2};{3};{4};{5};{6};{7};{8};",
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
                        "{0};{1};{2};{3};{4};{5};{6};{7};{8};",
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

            lock (syncObject)
            {
                if (leonResultsFelt.Any())
                {
                    //var writefilenameTmp = Path.Combine(tmpPath, "WRITE"+ToLeonDataTmp);
                    var filenameTmp = Path.Combine(tmpPath, ToLeonDataTmp);
                    File.AppendAllLines(filenameTmp, leonResultsFelt, Encoding.GetEncoding("ISO-8859-1"));
                    //if (File.Exists(filenameTmp))
                    //{
                    //    Log.Error("File alredy exsist deliting {0}", filenameTmp);
                    //    File.Delete(filenameTmp);
                    //}

                    //File.Move(writefilenameTmp, filenameTmp);
                }

                if (leonResultsBane.Any())
                {
                    //var writefilenameTmp = Path.Combine(tmpPathMinne, "WRITE"+ToLeonDataTmp);
                    var filenameTmp = Path.Combine(tmpPathMinne, ToLeonDataTmp);
                    File.AppendAllLines(filenameTmp, leonResultsBane, Encoding.GetEncoding("ISO-8859-1"));
                    //if (File.Exists(filenameTmp))
                    //{
                    //    Log.Error("File alredy exsist deliting {0}", filenameTmp);
                    //    File.Delete(filenameTmp);
                    //}
                    //File.Move(writefilenameTmp, filenameTmp);
                }
            }
        }

        private static bool HasUPD(string path)
        {
            var updFile = Path.Combine(path, ToLeonUPD);
            return File.Exists(updFile);
        }

        internal static void CheckAnyTempFiles(string feltPath, string feltTempDir, string minneTempDir)
        {
            if (HasUPD(feltPath))
            {
                return;
            }

            if (HasUPD(m_minnePath))
            {
                return;
            }

            if (CheckTmpFile(feltPath, feltTempDir) == false)
            {
                CheckTmpFile(m_minnePath, minneTempDir);
            }
        }

        internal static bool CheckTmpFile(string outputPath, string tmpDir)
        {
            if (string.IsNullOrWhiteSpace(outputPath) || !Directory.Exists(outputPath))
            {
                return false;
            }

            lock (syncObject)
            {

                var tmpBasePath = DatabaseApi.GetActiveCompetition();
                var tmpPath = Path.Combine(tmpBasePath, tmpDir);

                var filenameTmp = Path.Combine(tmpPath, ToLeonDataTmp);
                if (!File.Exists(filenameTmp))
                {
                    return false;
                }

                var updFile = Path.Combine(outputPath, ToLeonUPD);
                if (!File.Exists(updFile))
                {
                    var filename = Path.Combine(outputPath, ToLeonData);

                    var backupDirName = "ToLeonBackup";
                    var backupDir = Path.Combine(tmpBasePath, backupDirName);
                    var backupFilename = Path.Combine(backupDir, $"{DateTime.Now.ToString("yyyyMMdd-HHmmss")}_Leon_kmonew.txt");

                    try
                    {
                        if (!Directory.Exists(backupDir))
                        {
                            Directory.CreateDirectory(backupDir);
                        }

                        File.Copy(filenameTmp, backupFilename);
                    }
                    catch (Exception exCopy)
                    {
                        Log.Error(exCopy, "Unable to copy file " + filenameTmp + " to " + filename);
                    }

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

                            return true;
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

            return false;
        }

        internal static void RegisterMinnePath(string outputPath)
        {
            m_minnePath = outputPath;
        }

        //internal static void CheckTmpFile(string path)
        //{
        //    lock (syncObject)
        //    {
                
        //        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        //        {
        //            return;
        //        }

        //        var tmpBasePath = DatabaseApi.GetActiveCompetition();


        //        string tmpDir;
        //        string tmpDirMinne;
        //        tmpDir = "LeonTmp";
        //        tmpDirMinne = "MinneLeonTemp";
           


        //        var tmpPath = Path.Combine(tmpBasePath, tmpDir);
        //        var tmpPathMinne = Path.Combine(tmpBasePath, tmpDirMinne);
        //        var bkupPath = Path.Combine(tmpBasePath, "BackupOutput");

        //        if (!Directory.Exists(bkupPath))
        //        {
        //            Directory.CreateDirectory(bkupPath);
        //        }

        //        var filenameTmp = Path.Combine(tmpPath, ToLeonDataTmp);
        //        var filenameMinneTmp = Path.Combine(tmpPathMinne, ToLeonDataTmp);
        //        //20170625 - 084813_Leon_kminew
        //        string filenamebkupTmp = string.Format("{0}_Leon_{1}.txt", DateTime.Now.ToString("yyyyMMdd-HHmmss"), tmpDir);
        //        var filenameBkupTmp = Path.Combine(bkupPath, filenamebkupTmp);

        //        string filenameMinnebkupTmp = string.Format("{0}_Leon_{1}.txt", DateTime.Now.ToString("yyyyMMdd-HHmmss"), tmpDirMinne);
        //        var filenameMinneBkupTmp = Path.Combine(bkupPath, filenameMinnebkupTmp);

        //        if (File.Exists(filenameTmp))
        //        {
        //            Log.Info("starting to export {0} to Path {1}", filenameTmp, path);
        //            var updFile = Path.Combine(path, ToLeonUPD);
        //            var updMinneFile = string.Empty;
        //            if (!string.IsNullOrEmpty(m_minnePath))
        //            {
        //                updMinneFile = Path.Combine(m_minnePath, ToLeonUPD);
        //            }

        //            if (!ExsistTempFiles(path, m_minnePath))
        //            {
        //                var filename = Path.Combine(path, ToLeonData);

        //                var fileMoveError = false;
        //                try
        //                {
        //                    File.Copy(filenameTmp, filenameBkupTmp);
        //                    File.Move(filenameTmp, filename);
        //                }
        //                catch (Exception ex)
        //                {
        //                    Log.Error(ex, "Unable to move file " + filenameTmp + " to " + filename);
        //                    fileMoveError = true;
        //                }

        //                if (!fileMoveError)
        //                {
        //                    try
        //                    {
        //                        using (FileStream file = new FileStream(updFile, FileMode.Create, System.IO.FileAccess.Write))
        //                        {
        //                            file.Write(s_updFileContent, 0, s_updFileContent.Length);
        //                            file.Flush(true);
        //                            file.Close();
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Log.Error(ex, "Unable to create file " + updFile);
        //                        try
        //                        {
        //                            File.Move(filename, filenameTmp);
        //                        }
        //                        catch (Exception ex1)
        //                        {
        //                            Log.Error(ex1, "Unable to move back file after UPD file create error " + filename + ", " + filenameTmp);
        //                        }
        //                    }

        //                    Thread.Sleep(500);
                           
        //                }
        //            }
        //            else
        //            {
        //                Log.Info("Cant export to export UPD files exsist {0} or {1}", updFile, updMinneFile);
        //                Thread.Sleep(500);
        //            }
        //        }
        //        else if (File.Exists(filenameMinneTmp) && !string.IsNullOrEmpty(m_minnePath))
        //        {
        //            Log.Info("starting to export {0} to Path {1}", filenameMinneTmp, m_minnePath);
                   
        //            var updMinneFile = Path.Combine(m_minnePath, ToLeonUPD);

        //            if (!ExsistTempFiles(path, m_minnePath))
        //            {
        //                var filename = Path.Combine(m_minnePath, ToLeonData);

        //                var fileMoveError = false;
        //                try
        //                {
        //                    File.Copy(filenameMinneTmp, filenameMinneBkupTmp);
        //                    File.Move(filenameMinneTmp, filename);
        //                }
        //                catch (Exception ex)
        //                {
        //                    Log.Error(ex, "Unable to move file " + filenameTmp + " to " + filename);
        //                    fileMoveError = true;
        //                }

        //                if (!fileMoveError)
        //                {
        //                    try
        //                    {
        //                        using (FileStream file = new FileStream(updMinneFile, FileMode.Create, System.IO.FileAccess.Write))
        //                        {
        //                            file.Write(s_updFileContent, 0, s_updFileContent.Length);
        //                            file.Flush(true);
        //                            file.Close();
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        Log.Error(ex, "Unable to create file " + updMinneFile);
        //                        try
        //                        {
        //                            File.Move(filename, filenameTmp);
        //                        }
        //                        catch (Exception ex1)
        //                        {
        //                            Log.Error(ex1, "Unable to move back file after UPD file create error " + filename + ", " + filenameTmp);
        //                        }
        //                    }

        //                    Thread.Sleep(500);

        //                }
        //            }
        //            else
        //            {
        //                Log.Info("Cant export to export UPD files exsist ");
        //                Thread.Sleep(500);
        //            }
        //        }

        //    }
        //}

        //private static bool ExsistTempFiles(string path, string minnePath)
        //{
        //    if (!string.IsNullOrEmpty(path))
        //    {
        //        var updFile = Path.Combine(path, ToLeonUPD);
        //        if (File.Exists(updFile))
        //        {
        //            return true;
        //        }
        //    }

        //    if (string.IsNullOrEmpty(minnePath))
        //    {
        //        return false;
        //    }

        //    var updMinneFile = Path.Combine(minnePath, ToLeonUPD);

        //    if (File.Exists(updMinneFile))
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        //public static void RegisterPath(string path)
        //{
        //    if (string.IsNullOrEmpty(m_minnePath))
        //    {
        //        m_minnePath = path;
        //    }
        //}

    }
}
