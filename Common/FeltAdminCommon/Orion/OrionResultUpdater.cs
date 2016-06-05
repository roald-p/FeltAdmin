using FeltAdmin.Diagnostics;
using FeltAdmin.Viewmodels;
using System.Collections.Generic;
using System.Linq;

namespace FeltAdmin.Orion
{
    public class OrionResultUpdater
    {
        private OrionSetupViewModel m_orionSetupViewModel;

        public OrionResultUpdater(OrionSetupViewModel orionSetupViewModel)
        {
            m_orionSetupViewModel = orionSetupViewModel;
        }

        public List<OrionRegistration> GetUpdatedRegistrationsAfterResultRegistration(
            List<OrionResult> newOrionResults,
            List<OrionRegistration> allRegistrations,
            List<OrionResult> allResults,
            out List<int> finishedShouterIds)
        {
            var result = new List<OrionRegistration>();
            finishedShouterIds = new List<int>();
            foreach (var orionResult in newOrionResults)
            {
                var currentOrionId = 0;
                var currentRange = this.GetRange(orionResult, out currentOrionId);
                orionResult.ResultType = currentRange.ResultType;
                int nextRangeId;
                var nextOrionId = this.GetNextOrionIdAndRangeId(orionResult, out nextRangeId);
                if (nextOrionId == 0)
                {
                    finishedShouterIds.Add(orionResult.ShooterId);
                    continue;
                }

                var registrationsForShooter =
                    allRegistrations.Where(r => r.ShooterId == orionResult.ShooterId).OrderBy(r => r.OrionId).ThenBy(r => r.RangeId);

                var sum = 0;

                if (allResults != null)
                {
                    var allResultsForShooter = allResults.Where(o => o.ShooterId == orionResult.ShooterId).OrderBy(r => r.OrionId).ThenBy(r => r.Team);
                    foreach (var prevResult in allResultsForShooter)
                    {
                        int prevOrionId;
                        var prevRange = this.GetRange(prevResult, out prevOrionId);
                        if (prevOrionId < currentOrionId)
                        {
                            if (prevRange.ResultType == currentRange.ResultType)
                            {
                                sum += prevResult.GetSum(prevRange.ResultType);
                            }
                        }
                        else if (prevOrionId == currentOrionId && prevRange.RangeId < currentRange.RangeId)
                        {
                            if (prevRange.ResultType == currentRange.ResultType)
                            {
                                sum += prevResult.GetSum(prevRange.ResultType);
                            }
                        }
                    }
                }

                sum += orionResult.GetSum(currentRange.ResultType);

                foreach (var registration in registrationsForShooter)
                {
                    if (registration.OrionId < nextOrionId)
                    {
                        continue;
                    }
                    if (registration.OrionId == orionResult.OrionId && registration.RangeId <= currentRange.RangeId)
                    {
                        continue;
                    }

                    var registrationRange = this.GetRange(registration);
                    if (currentRange.ResultType == registrationRange.ResultType)
                    {
                        registration.SumIn = sum;
                        result.Add(registration);
                    }
                }
            }

            return result;
        }

        private RangeViewModel GetRange(OrionRegistration registration)
        {
            foreach (var orionViewModel in m_orionSetupViewModel.OrionViewModels)
            {
                if (registration.OrionId == orionViewModel.OrionId)
                {
                    foreach (var rangeViewModel in orionViewModel.RangeViews)
                    {
                        if (rangeViewModel.RangeType == RangeType.Shooting && registration.Target >= rangeViewModel.FirstTarget && registration.Target <= rangeViewModel.LastTarget)
                        {
                            return rangeViewModel;
                        }
                    }
                }

            }

            return null;
        }

        private RangeViewModel GetRange(OrionResult orionResult, out int orionId)
        {
            orionId = 0;
            foreach (var orionViewModel in m_orionSetupViewModel.OrionViewModels)
            {
                if (orionResult.OrionId == orionViewModel.OrionId)
                {
                    foreach (var rangeViewModel in orionViewModel.RangeViews)
                    {
                        if (rangeViewModel.RangeType == RangeType.Shooting && orionResult.Target >= rangeViewModel.FirstTarget && orionResult.Target <= rangeViewModel.LastTarget)
                        {
                            orionId = orionViewModel.OrionId;
                            return rangeViewModel;
                        }
                    }
                }
            }

            Log.Error(
                string.Format(
                    "Could not find rangeid for result: {0} Orionid={1} Lag={2} Skive={3} Serie={4}",
                    orionResult.Name,
                    orionResult.OrionId,
                    orionResult.Team,
                    orionResult.Target,
                    orionResult.AllSeries));

            return null;
        }

        private int GetNextOrionIdAndRangeId(OrionResult orionResult, out int rangeId)
        {
            rangeId = 0;
            foreach (var orionViewModel in m_orionSetupViewModel.OrionViewModels)
            {
                if (orionResult.OrionId == orionViewModel.OrionId)
                {
                    foreach (var rangeViewModel in orionViewModel.RangeViews)
                    {
                        if (rangeViewModel.RangeType == RangeType.Shooting && orionResult.Target < rangeViewModel.FirstTarget)
                        {
                            rangeId = rangeViewModel.RangeId;
                            return orionViewModel.OrionId;
                        }
                    }
                }
                else if (orionResult.OrionId < orionViewModel.OrionId)
                {
                    foreach (var rangeViewModel in orionViewModel.RangeViews)
                    {
                        if (rangeViewModel.RangeType == RangeType.Shooting)
                        {
                            rangeId = rangeViewModel.RangeId;
                            return orionViewModel.OrionId;
                        }
                    }
                }
            }

            return 0;
        }

        public void SetResultTypeOnOrionResult(List<OrionResult> orionResult)
        {
            foreach (var result in orionResult)
            {
                int orionId;
                var range = this.GetRange(result, out orionId);
                result.ResultType = range.ResultType;
            }
        }

        internal void AddSums(List<OrionRegistration> newRegistrations, List<OrionResult> allResults)
        {
            if (newRegistrations == null || !newRegistrations.Any())
            {
                return;
            }

            foreach (var orionRegistration in newRegistrations)
            {
                if (orionRegistration.Deleted)
                {
                    continue;
                }

                var resultsForShooter = allResults.Where(r => r.ShooterId == orionRegistration.ShooterId);
                if (!resultsForShooter.Any())
                {
                    return;
                }

                var rangeForThisRegistration = GetRange(orionRegistration);
                orionRegistration.SumIn = 0;
                foreach (var orionResult in resultsForShooter)
                {
                    int orionId;
                    var rangeForThisResult = this.GetRange(orionResult, out orionId);
                    if (rangeForThisRegistration.ResultType != rangeForThisResult.ResultType)
                    {
                        continue;
                    }

                    if (orionRegistration.OrionId > orionResult.OrionId)
                    {
                        orionRegistration.SumIn += orionResult.GetSum(rangeForThisResult.ResultType);
                    }
                    else if (orionRegistration.OrionId == orionResult.OrionId && rangeForThisRegistration.RangeId > rangeForThisResult.RangeId)
                    {
                        orionRegistration.SumIn += orionResult.GetSum(rangeForThisResult.ResultType);
                    }
                }
            }
        }
    }
}
