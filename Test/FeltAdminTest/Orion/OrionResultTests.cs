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

            var result = OrionResult.ParseFromOrion(minneskyting);

            AssertResult(result, 2, 3, 4, 1234, "Hugh Cardiff", "Bodø Østre", "4", 55, "*XX8X7;XX0XX*", "*XX8X7", "XX0XX*");
        }

        [TestMethod]
        public void GetFinalSeriesFromDoubleRange_12Shots()
        {
            var fromOion = "2;101;4;1234;Hugh Cardiff;Bodø Østre;4;55;*XX0X0XX0XX*";
            var or = OrionResult.ParseFromOrion(fromOion);

            var result = or.GetFinalSeriesFromDoubleRange();

            Assert.IsTrue(result.Count==2);
            Assert.AreEqual("*XX0X0", result[0]);
            Assert.AreEqual("XX0XX*", result[1]);
        }

        [TestMethod]
        public void GetFinalSeriesFromDoubleRange_11Shots()
        {
            var fromOion = "2;101;4;1234;Hugh Cardiff;Bodø Østre;4;55;*XX0X0XX0XX";
            var or = OrionResult.ParseFromOrion(fromOion);

            var result = or.GetFinalSeriesFromDoubleRange();

            Assert.IsTrue(result.Count == 2);
            Assert.AreEqual("*XX0X0", result[0]);
            Assert.AreEqual("XX0XX", result[1]);
        }

        [TestMethod]
        public void GetFinalSeriesFromDoubleRange_7Shots()
        {
            var fromOion = "2;101;4;1234;Hugh Cardiff;Bodø Østre;4;55;*XX0X0X";
            var or = OrionResult.ParseFromOrion(fromOion);

            var result = or.GetFinalSeriesFromDoubleRange();

            Assert.IsTrue(result.Count == 2);
            Assert.AreEqual("*XX0X0", result[0]);
            Assert.AreEqual("X", result[1]);
        }

        [TestMethod]
        public void GetFinalSeriesFromDoubleRange_6Shots()
        {
            var fromOion = "2;101;4;1234;Hugh Cardiff;Bodø Østre;4;55;*XX0X0";
            var or = OrionResult.ParseFromOrion(fromOion);

            var result = or.GetFinalSeriesFromDoubleRange();

            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual("*XX0X0", result[0]);
        }

        [TestMethod]
        public void GetFinalSeriesFromDoubleRange_5Shots()
        {
            var fromOion = "2;101;4;1234;Hugh Cardiff;Bodø Østre;4;55;*XX0X";
            var or = OrionResult.ParseFromOrion(fromOion);

            var result = or.GetFinalSeriesFromDoubleRange();

            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual("*XX0X", result[0]);
        }

        [TestMethod]
        public void GetFinalSeriesFromDoubleRange_1Shots()
        {
            var fromOion = "2;101;4;1234;Hugh Cardiff;Bodø Østre;4;55;*";
            var or = OrionResult.ParseFromOrion(fromOion);

            var result = or.GetFinalSeriesFromDoubleRange();

            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual("*", result[0]);
        }

        [TestMethod]
        public void GetFinalSeriesFromDoubleRange_0Shots()
        {
            var fromOion = "2;101;4;1234;Hugh Cardiff;Bodø Østre;4;55;";
            var or = OrionResult.ParseFromOrion(fromOion);

            var result = or.GetFinalSeriesFromDoubleRange();

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void GetFinalSeriesFromDoubleRange_13Shots()
        {
            var fromOion = "2;101;4;1234;Hugh Cardiff;Bodø Østre;4;55;X*X*0*X*X*0*X";
            var or = OrionResult.ParseFromOrion(fromOion);

            var result = or.GetFinalSeriesFromDoubleRange();

            Assert.IsTrue(result.Count == 2);
            Assert.AreEqual("X*X*0*", result[0]);
            Assert.AreEqual("X*X*0*", result[1]);
        }
        [TestMethod]
        public void GetFinalSeriesFromDoubleRange_14Shots()
        {
            var fromOion = "2;101;4;1234;Hugh Cardiff;Bodø Østre;4;55;X*X*0*X*X*0*XX";
            var or = OrionResult.ParseFromOrion(fromOion);

            var result = or.GetFinalSeriesFromDoubleRange();

            Assert.IsTrue(result.Count == 2);
            Assert.AreEqual("X*X*0*", result[0]);
            Assert.AreEqual("X*X*0*", result[1]);
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
            Assert.AreEqual(series[0], result.Series[0]);
            Assert.AreEqual(series[1], result.Series[1]);
        }
    }
}
