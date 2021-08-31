using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FeltAdminTest.Lisens
{
    using FeltAdminCommon.Lisens;

    [TestClass]
    public class Lisenstests
    {
        [TestMethod]
        public void Overhalla()
        {
            var lisens = LisensChecker.GenerateLisens("Overhalla Skytterlag", new DateTime(2022, 04, 25));
            Assert.IsTrue(LisensChecker.Validate("Overhalla Skytterlag", DateTime.Now, lisens));
        }

        [TestMethod]
        public void Hammerfest()
        {
            var lisens = LisensChecker.GenerateLisens("Hammerfest Skytterlag", new DateTime(2022, 12, 31));
            Assert.IsTrue(LisensChecker.Validate("Hammerfest Skytterlag", DateTime.Now, lisens));
        }

        [TestMethod]
        public void Bø()
        {
            var lisens = LisensChecker.GenerateLisens("Bø Skytterlag", new DateTime(2022, 12, 31));
            Assert.IsTrue(LisensChecker.Validate("Bø Skytterlag", DateTime.Now, lisens));
        }

        [TestMethod]
        public void CheckLisens()
        {
            //var lisens = LisensChecker.GenerateLisens("Overhalla Skytterlag", new DateTime(2021, 08, 23));
            var result = LisensChecker.Validate("Bodø-Østre Skytterlag", DateTime.Now, "Qk9Ew5jDmFNUUkUgU0tZVFRFUkxBRzsyMDMwLTA2LTMwVDAwOjAwOjAw");
            Assert.IsTrue(result);
        }
    }
}
