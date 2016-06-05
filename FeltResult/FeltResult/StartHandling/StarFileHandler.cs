using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FeltAdminCommon;

namespace FeltResult.StartHandling
{
    public static class StarFileHandler
    {
        private static string m_starsDir;

        private static List<int> m_staredShooters100m;

        private static List<int> m_staredShooters200m;

        static StarFileHandler()
        {
            var runningDir = Environment.CurrentDirectory;
            m_starsDir = Path.Combine(runningDir, "Stars");
            if (!Directory.Exists(m_starsDir))
            {
                Directory.CreateDirectory(m_starsDir);
            }

            m_staredShooters100m = GetStoredStarShooter(RangeClass.Range100m);
            m_staredShooters200m = GetStoredStarShooter(RangeClass.Range200m);
        }

        public static List<int> StaredShooters(RangeClass rangeClass)
        {
            if (rangeClass == RangeClass.Range100m)
            {
                return m_staredShooters100m;
            }

            return m_staredShooters200m;
        }

        public static void AddStarShooter(int shooterId, RangeClass rangeClass)
        {
            if (rangeClass == RangeClass.Range100m)
            {
                if (m_staredShooters100m.Contains(shooterId))
                {
                    return;
                }

                m_staredShooters100m.Add(shooterId);
                StoreStaredShooters(m_staredShooters100m, rangeClass);
                return;
            }

            if (m_staredShooters200m.Contains(shooterId))
            {
                return;
            }

            m_staredShooters200m.Add(shooterId);
            StoreStaredShooters(m_staredShooters200m, rangeClass);
        }

        public static void RemoveStaredShooter(int shooterId, RangeClass rangeClass)
        {
            if (rangeClass == RangeClass.Range100m)
            {
                if (!m_staredShooters100m.Contains(shooterId))
                {
                    return;
                }

                m_staredShooters100m.Remove(shooterId);
                StoreStaredShooters(m_staredShooters100m, rangeClass);
                return;
            }

            if (!m_staredShooters200m.Contains(shooterId))
            {
                return;
            }

            m_staredShooters200m.Remove(shooterId);
            StoreStaredShooters(m_staredShooters200m, rangeClass);
        }

        private static void StoreStaredShooters(List<int> staredList, RangeClass rangeClass)
        {
            var filenameforrange = string.Format("StarShooters{0}.txt", rangeClass.ToString());
            var filename = Path.Combine(m_starsDir, filenameforrange);
            var staredShooters = staredList.Select(i => i.ToString()).ToArray();
            File.WriteAllLines(filename, staredShooters);
        }

        private static List<int> GetStoredStarShooter(RangeClass rangeClass)
        {
            var result = new List<int>();
            var filenameforrange = string.Format("StarShooters{0}.txt", rangeClass.ToString());
            var filename = Path.Combine(m_starsDir, filenameforrange);
            if (File.Exists(filename))
            {
                var allRegs = File.ReadAllLines(filename);
                foreach (var reg in allRegs)
                {
                    int shooterId;
                    if (int.TryParse(reg, out shooterId))
                    {
                        result.Add(shooterId);
                    }
                }
            }

            return result;
        }
    }
}
