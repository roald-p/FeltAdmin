using FeltAdmin.Database.API;
using FeltAdmin.Database.Engine;
using FeltAdmin.Helpers;
using FeltAdmin.Leon;
using FeltAdmin.Orion;
using FeltAdmin.Viewmodels;
using FeltAdminCommon;
using FeltAdminCommon.Helpers;

using FeltAdminServer.Data;
using System.Collections.Generic;
using System.Linq;

namespace FeltAdminServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class FestResultService : IFestResultService
    {
        public List<string> GetCompetitions(RangeClass rangeClass)
        {
            if (rangeClass == RangeClass.Range100m)
            {
                return DatabaseApi.GetAllCompetitions100m();
                
            }
            else
            {
                return DatabaseApi.GetAllCompetitions200m();
            }
        }

        public List<Result> GetResults(RangeClass rangeClass, string competition)
        {
            if (rangeClass == RangeClass.Range100m)
            {
                return GetResults100m(competition);
            }
            else
            {
                return GetResults200m(competition);
            }
        }

        public List<Registration> GetRegistrations(RangeClass rangeClass, string competition)
        {
            if (rangeClass == RangeClass.Range100m)
            {
                return GetRegistrations100m(competition);
            }
            else
            {
                return GetRegistrations200m(competition);
            }
        }

        private List<LeonPerson> WashRegistrations(List<LeonPerson> rawlist)
        {
            var result = new List<LeonPerson>();
            foreach (var leonPerson in rawlist)
            {
                var found = result.SingleOrDefault(r => r.ShooterId == leonPerson.ShooterId);
                if (found != null)
                {
                    result.Remove(found);
                }

                result.Add(leonPerson);
            }

            return result;
        }

        private List<Result> GetResults200m(string competition)
        {
            DatabaseApi.SelectCompetition200m(competition);
            var settings = SettingsHelper.GetSettings();
            var rawResults = DatabaseApi.LoadCompetitionFromTable(TableName.OrionResult);
            var registrations = DatabaseApi.LoadCompetitionFromTable(TableName.LeonRegistration);
            var actualResults = this.AddNewResults(rawResults.OfType<OrionResult>().ToList(), settings);
            var results = BuildResultObjects(actualResults);
            foreach (var item in results)
            {
                if (registrations.Any())
                {
                    var registration = registrations.OfType<LeonPerson>().Last(r => r.ShooterId == item.ShooterId);
                    if (registration != null)
                    {
                        item.Name = registration.Name;
                        item.Class = registration.Class;
                        item.ClubName = registration.ClubName;
                    }
                }
            }

            return results;
        }

        private List<Result> GetResults100m(string competition)
        {
            DatabaseApi.SelectCompetition100m(competition);
            var settings = SettingsHelper.GetSettings();
            var rawResults = DatabaseApi.LoadCompetitionFromTable(TableName.OrionResult);
            var actualResults = this.AddNewResults(rawResults.OfType<OrionResult>().ToList(), settings);
            return BuildResultObjects(actualResults);
        }

        private List<Registration> GetRegistrations100m(string competition)
        {
            DatabaseApi.SelectCompetition100m(competition);
            var rawResults = DatabaseApi.LoadCompetitionFromTable(TableName.LeonRegistration);
            var actualRegs = this.AddNewRegistrations(rawResults.OfType<LeonPerson>().ToList());
            var results = this.GetResults100m(competition);

            foreach (var result in results)
            {
                var reg = actualRegs.SingleOrDefault(r => r.ShooterId == result.ShooterId);
                if (reg != null)
                {
                    reg.Result = result;
                }
            }

            return actualRegs;
        }

        private List<Registration> GetRegistrations200m(string competition)
        {
            DatabaseApi.SelectCompetition200m(competition);
            var rawResults = DatabaseApi.LoadCompetitionFromTable(TableName.LeonRegistration);
            var actualRegs = this.AddNewRegistrations(rawResults.OfType<LeonPerson>().ToList());
            var results = this.GetResults200m(competition);

            foreach (var result in results)
            {
                var reg = actualRegs.SingleOrDefault(r => r.ShooterId == result.ShooterId);
                if (reg != null)
                {
                    reg.Result = result;
                }
            }

            return actualRegs;
        }

        private List<Result> BuildResultObjects(List<OrionResult> actualResults)
        {
            var settings = SettingsHelper.GetSettings();
            var result = new List<Result>();
            var resultsForShooter = actualResults.GroupBy(r => r.ShooterId);
            foreach (var shooterRes in resultsForShooter)
            {
                var results = shooterRes.OrderBy(r => r.Team);
                var resultForShooter = new Result();
                resultForShooter.ShooterId = results.First().ShooterId;
                resultForShooter.Class = results.First().Class;
                resultForShooter.ClubName = results.First().ClubName;
                resultForShooter.Name = results.First().Name;
                resultForShooter.FeltHolds = new List<FeltHold>();
                var series = new List<string>();

                var allRanges = settings.OrionSetting.OrionViewModels.SelectMany(o => o.RangeViews).OrderBy(r => r.RangeId);
                ////foreach (var orion in settings.OrionSetting.OrionViewModels)
                ////{
                foreach (var rangeViewModel in allRanges)
                    {
                        if (rangeViewModel.RangeType == RangeType.Shooting)
                        {
                            var orionResult = CalculateOrionAndRange.GetResultForThisRange(results, rangeViewModel.Parent, rangeViewModel);
                            if (orionResult == null)
                            {
                                if (rangeViewModel.ResultType == ResultType.Felt)
                                {
                                    var fh = new FeltHold { Hits = 0, InnerHits = 0 };
                                    resultForShooter.FeltHolds.Add(fh);
                                    series.Add("x/x");
                                }
                                else
                                {
                                    resultForShooter.Minne = 0;
                                    resultForShooter.MinneInner = 0;
                                }
                            }
                            else
                            {
                                if (orionResult.ResultType == ResultType.Felt)
                                {
                                    if (orionResult.DoubleRange)
                                    {
                                        var fh1 = new FeltHold { Hits = orionResult.GetSum(ResultType.Felt, rangeViewModel.CountingShoots, 1), InnerHits = orionResult.GetInnerHits(1, rangeViewModel.CountingShoots) };
                                        var fh2 = new FeltHold { Hits = orionResult.GetSum(ResultType.Felt, rangeViewModel.CountingShoots, 2), InnerHits = orionResult.GetInnerHits(2, rangeViewModel.CountingShoots) };
                                        resultForShooter.FeltHolds.Add(fh1);
                                        resultForShooter.TotalSum += fh1.Hits;
                                        resultForShooter.TotalInnerHits += fh1.InnerHits;
                                        series.Add(string.Format("{0}/{1}", fh1.Hits, fh1.InnerHits));

                                        resultForShooter.FeltHolds.Add(fh2);
                                        resultForShooter.TotalSum += fh2.Hits;
                                        resultForShooter.TotalInnerHits += fh2.InnerHits;
                                        series.Add(string.Format("{0}/{1}", fh2.Hits, fh2.InnerHits));
                                    }
                                    else
                                    {
                                        var fh = new FeltHold { Hits = orionResult.GetSum(ResultType.Felt, rangeViewModel.CountingShoots), InnerHits = orionResult.GetInnerHits(0, rangeViewModel.CountingShoots) };
                                        resultForShooter.FeltHolds.Add(fh);
                                        resultForShooter.TotalSum += fh.Hits;
                                        resultForShooter.TotalInnerHits += fh.InnerHits;
                                        series.AddRange(orionResult.GetPrintableSeries(rangeViewModel.CountingShoots));
                                    }
                                }
                                else
                                {
                                    resultForShooter.Minne = orionResult.GetSum(ResultType.Bane, rangeViewModel.CountingShoots);
                                    resultForShooter.MinneInner = orionResult.GetInnerHits(rangeViewModel.CountingShoots);
                                }
                                
                            }
                        }
                    }
                ////}

                //////foreach (var orionResult in results)
                //////{
                //////    if (orionResult.ResultType == ResultType.Felt)
                //////    {
                //////        if (orionResult.DoubleRange)
                //////        {
                //////            var fh1 = new FeltHold { Hits = orionResult.GetSum(ResultType.Felt, 1), InnerHits = orionResult.GetInnerHits(1) };
                //////            var fh2 = new FeltHold { Hits = orionResult.GetSum(ResultType.Felt, 2), InnerHits = orionResult.GetInnerHits(2) };
                //////            resultForShooter.FeltHolds.Add(fh1);
                //////            resultForShooter.TotalSum += fh1.Hits;
                //////            resultForShooter.TotalInnerHits += fh1.InnerHits;
                //////            series.Add(string.Format("{0}/{1}", fh1.Hits, fh1.InnerHits));

                //////            resultForShooter.FeltHolds.Add(fh2);
                //////            resultForShooter.TotalSum += fh2.Hits;
                //////            resultForShooter.TotalInnerHits += fh2.InnerHits;
                //////            series.Add(string.Format("{0}/{1}", fh2.Hits, fh2.InnerHits));
                //////        }
                //////        else
                //////        {
                //////            var fh = new FeltHold { Hits = orionResult.GetSum(), InnerHits = orionResult.GetInnerHits() };
                //////            resultForShooter.FeltHolds.Add(fh);
                //////            resultForShooter.TotalSum += fh.Hits;
                //////            resultForShooter.TotalInnerHits += fh.InnerHits;
                //////            series.AddRange(orionResult.GetPrintableSeries());
                //////        }
                //////    }
                //////    else
                //////    {
                //////        resultForShooter.Minne = orionResult.GetSum();
                //////        resultForShooter.MinneInner = orionResult.GetInnerHits();
                //////    }
                //////}

                resultForShooter.TotalResult = string.Join(" ", series);
                result.Add(resultForShooter);
            }

            return result;
        }

        private List<OrionResult> AddNewResults(List<OrionResult> orionResults, Settings settings)
        {
            var results = new List<OrionResult>();

            foreach (var newRegistration in orionResults)
            {
                int orionId;
                var rangeForRegistration = this.GetRange(newRegistration, settings, out orionId);
                newRegistration.ResultType = rangeForRegistration.ResultType;
                newRegistration.DoubleRange = rangeForRegistration.DoubleRange;
                var found = results.SingleOrDefault(p => p.ShooterId == newRegistration.ShooterId && p.OrionId == newRegistration.OrionId && this.GetRange(p, settings, out orionId).RangeId == rangeForRegistration.RangeId);
                if (found != null)
                {
                    results.Remove(found);
                    results.Add(newRegistration);
                }
                else if (found == null)
                {
                    results.Add(newRegistration);
                }
            }

            return results;
        }

        private RangeViewModel GetRange(OrionResult orionResult, Settings settings, out int orionId)
        {
            orionId = 0;
            foreach (var orionViewModel in settings.OrionSetting.OrionViewModels)
            {
                if (orionResult.OrionId == orionViewModel.OrionId)
                {
                    foreach (var rangeViewModel in orionViewModel.RangeViews)
                    {
                        if (rangeViewModel.RangeType == FeltAdmin.Viewmodels.RangeType.Shooting && orionResult.Target >= rangeViewModel.FirstTarget && orionResult.Target <= rangeViewModel.LastTarget)
                        {
                            orionId = orionViewModel.OrionId;
                            return rangeViewModel;
                        }
                    }
                }
            }

            ////Log.Error(
            ////    string.Format(
            ////        "Could not find rangeid for result: {0} Orionid={1} Lag={2} Skive={3} Serie={4}",
            ////        orionResult.Name,
            ////        orionResult.OrionId,
            ////        orionResult.Team,
            ////        orionResult.Target,
            ////        orionResult.AllSeries));

            return null;
        }

        private List<Registration> AddNewRegistrations(List<LeonPerson> registrations)
        {
            var results = new List<Registration>();

            foreach (var newRegistration in registrations)
            {
                if (newRegistration.ShooterId == 0)
                {
                    continue;
                }

                var registration = Registration.GetRegistration(newRegistration);

                var found = results.SingleOrDefault(p => p.ShooterId == newRegistration.ShooterId);
                if (found != null)
                {
                    results.Remove(found);
                    results.Add(registration);
                }
                else if (found == null)
                {
                    results.Add(registration);
                }
            }

            return results;
        }
    }
}
