using FeltAdmin.Orion;
using FeltAdmin.Viewmodels;
using System.Linq;

namespace FeltAdminCommon.Helpers
{
    public class CalculateOrionAndRange
    {
        public static RangeViewModel GetRangeFromRegistration(OrionSetupViewModel orionSetup, OrionRegistration registration)
        {
            var orion = orionSetup.OrionViewModels.Single(o => o.OrionId == registration.OrionId);
            foreach (var range in orion.RangeViews)
            {
                if (range.RangeType == RangeType.Shooting && registration.Target >= range.FirstTarget && registration.Target <= range.LastTarget)
                {
                    return range;
                }
            }

            return null;
        }

        public static int GetRangeIdFromResult(OrionSetupViewModel orionSetup, OrionResult orionResult)
        {
            var orion = orionSetup.OrionViewModels.Single(o => o.OrionId == orionResult.OrionId);
            foreach (var range in orion.RangeViews)
            {
                if (range.RangeType == RangeType.Shooting && orionResult.Target >= range.FirstTarget && orionResult.Target <= range.LastTarget)
                {
                    return range.RangeId;
                }
            }

            return 0;
        }

        public static int GetNextOrionAndRange(OrionSetupViewModel orionSetup, OrionResult orionResult, out int nextRangeId)
        {
            nextRangeId = 0;
            var currentRangeId = GetRangeIdFromResult(orionSetup, orionResult);

            foreach (var orionViewModel in orionSetup.OrionViewModels)
            {
                if (orionViewModel.OrionId < orionResult.OrionId)
                {
                    continue;
                }

                if (orionViewModel.OrionId == orionResult.OrionId)
                {
                    foreach (var rangeViewModel in orionViewModel.RangeViews)
                    {
                        if (rangeViewModel.RangeId > currentRangeId && rangeViewModel.RangeType == RangeType.Shooting)
                        {
                            return rangeViewModel.RangeId;
                        }
                    }
                }

                if (orionViewModel.OrionId > orionResult.OrionId)
                {
                    foreach (var rangeViewModel in orionViewModel.RangeViews)
                    {
                        if (rangeViewModel.RangeType == RangeType.Shooting)
                        {
                            nextRangeId = rangeViewModel.RangeId;
                            return orionViewModel.OrionId;
                        }
                    }
                }
            }

            return 0;
        }

        public static OrionResult GetResultForThisRange(System.Collections.Generic.IEnumerable<OrionResult> allResultsForShooter, OrionViewModel orion, RangeViewModel rangeViewModel)
        {
            var resultForThisOrion = allResultsForShooter.Where(r => r.OrionId == orion.OrionId);
            foreach (var orionResult in resultForThisOrion)
            {
                if (orionResult.Target >= rangeViewModel.FirstTarget && orionResult.Target <= rangeViewModel.LastTarget)
                {
                    return orionResult;
                }
            }

            return null;
        }
    }
}
