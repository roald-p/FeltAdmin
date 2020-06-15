using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FeltAdminTest.Lisens
{
    using FeltAdminCommon.Lisens;

    [TestClass]
    public class Lisenstests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var lisens = LisensChecker.GenerateLisens("Hammerfest Skytterlag", new DateTime(2021, 06, 10));
            //var lisens = LisensChecker.GenerateLisens("Overhalla Skytterlag", new DateTime(2020, 08, 23));
            Assert.IsTrue(LisensChecker.Validate("Hammerfest Skytterlag", DateTime.Now, lisens));
        }
    }
}
