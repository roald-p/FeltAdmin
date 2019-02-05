using System;

using FeltAdmin.Orion;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FeltAdminTest.Orion
{
    [TestClass]
    public class OrionResultTests
    {
        [TestMethod]
        public void TimeFormat()
        {
            var time = DateTime.Now;
            var str = time.ToUniversalTime();
            var s2 = time.ToString("s");
        }

        [TestMethod]
        public void MinneInput_Ok()
        {
            var minneskyting = "2;3;4;1234;Hugh Cardiff;Bodø Østre;4;55;*XX8X7;XX0XX*";

            var result = OrionResult.ParseFromOrion(minneskyting, null);

            AssertResult(result, 2, 3, 4, 1234, "Hugh Cardiff", "Bodø Østre", "4", 55, "*XX8X7;XX0XX*", "*XX8X7", "XX0XX*");
        }

        [TestMethod]
        public void PegasusInput_Ok()
        {
            //var pegasusLine = "2;3;4;1234;Hugh Cardiff;Bodø Østre;4;55;*XX8X7;XX0XX*";
            var pegasusLine = "2;3;4;1234;Hugh Cardiff ;Bodø Østre ;4;6;#*.0#*.0#*.0#*.0#1.0#1.0";
            var result = OrionResult.ParseFromOrion(pegasusLine, null);

            AssertResult(result, 2, 3, 4, 1234, "Hugh Cardiff", "Bodø Østre", "4", 6, "****XX", "****XX");
        }

        private void AssertResult(
            OrionResult result,
            int orionid,
            int team,
            int target,
            int shooterid,
            string name,
            string clubname,
            string classname,
            int totalsum,
            string allseries,
            params string[] series)
        {
            Assert.AreEqual(orionid, result.OrionId);
            Assert.AreEqual(team, result.Team);
            Assert.AreEqual(target, result.Target);
            Assert.AreEqual(shooterid, result.ShooterId);
            Assert.AreEqual(name, result.Name);
            Assert.AreEqual(clubname, result.ClubName);
            Assert.AreEqual(classname, result.Class);
            Assert.AreEqual(totalsum, result.TotalSum);
            Assert.AreEqual(allseries, result.AllSeries);

            var cnt = series.Length;
            for (int c = 0; c < cnt; c++)
            {
                Assert.AreEqual(series[c], result.Series[c]);
            }
        }
    }
}
