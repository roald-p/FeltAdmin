using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FeltAdminTest.Service
{
    using FeltAdminCommon;

    using FeltAdminServer;

    [TestClass]
    public class FestResultServiceUnitTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            FestResultService serv = new FestResultService();
            var lists=serv.GetResults(RangeClass.Range200m, "TEST3_2017.06.25");
        }
    }
}
