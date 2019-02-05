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
            var lisens = LisensChecker.GenerateLisens("Overhalla Skytterlag", new DateTime(2019, 08, 23));

            Assert.IsTrue(LisensChecker.Validate("Overhalla Skytterlag", DateTime.Now, lisens));
        }
    }
}
