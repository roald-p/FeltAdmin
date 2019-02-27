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
            var lisens = LisensChecker.GenerateLisens("Bodø-Østre Skytterlag", new DateTime(2030, 06, 30));

            Assert.IsTrue(LisensChecker.Validate("Bodø-Østre Skytterlag", DateTime.Now, lisens));
        }
    }
}
