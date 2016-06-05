using System;
using System.IO;
using System.Text;

using FeltAdmin.Leon;
using FeltAdmin.Orion;

using FeltAdminTest.TestData;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FeltAdminTest.Orion
{
    [TestClass]
    public class OrionGenerateTests
    {
        [TestMethod]
        public void FirstTarget_FirstTeam()
        {
            var settings = TestDataHelper.GetSettings();

            var leonPerson = new LeonPerson
                             {
                                 Class = "4",
                                 ClubName = "Bodø Østre",
                                 Name = "Roald Parelius",
                                 Range = 2,
                                 ShooterId = 123,
                                 SumIn = 0,
                                 Target = 1,
                                 Team = 1
                             };

            var result = OrionGenerate.GenerateOrionForShooter(leonPerson, settings.OrionSetting);

            Assert.IsTrue(result.Count == 6);
            this.AssertRegistration(result[0], 1, 1, 1, 123);
            this.AssertRegistration(result[1], 1, 3, 5, 123);
            this.AssertRegistration(result[2], 1, 5, 9, 123);
            this.AssertRegistration(result[3], 2, 1, 1, 123);
            this.AssertRegistration(result[4], 3, 1, 1, 123);
            this.AssertRegistration(result[5], 4, 1, 1, 123);
        }

        [TestMethod]
        public void SecondTarget_FirstTeam()
        {
            var settings = TestDataHelper.GetSettings();

            var leonPerson = new LeonPerson
            {
                Class = "4",
                ClubName = "Bodø Østre",
                Name = "Roald Parelius",
                Range = 2,
                ShooterId = 123,
                SumIn = 0,
                Target = 2,
                Team = 1
            };

            var result = OrionGenerate.GenerateOrionForShooter(leonPerson, settings.OrionSetting);

            Assert.IsTrue(result.Count == 6);
            this.AssertRegistration(result[0], 1, 1, 2, 123);
            this.AssertRegistration(result[1], 1, 3, 6, 123);
            this.AssertRegistration(result[2], 1, 5, 10, 123);
            this.AssertRegistration(result[3], 2, 1, 2, 123);
            this.AssertRegistration(result[4], 3, 1, 2, 123);
            this.AssertRegistration(result[5], 4, 1, 2, 123);
        }

        [TestMethod]
        public void LastTarget_FirstTeam()
        {
            var settings = TestDataHelper.GetSettings();

            var leonPerson = new LeonPerson
            {
                Class = "4",
                ClubName = "Bodø Østre",
                Name = "Roald Parelius",
                Range = 2,
                ShooterId = 123,
                SumIn = 0,
                Target = 4,
                Team = 1
            };

            var result = OrionGenerate.GenerateOrionForShooter(leonPerson, settings.OrionSetting);

            Assert.IsTrue(result.Count == 6);
            this.AssertRegistration(result[0], 1, 1, 4, 123);
            this.AssertRegistration(result[1], 1, 3, 8, 123);
            this.AssertRegistration(result[2], 1, 5, 12, 123);
            this.AssertRegistration(result[3], 2, 1, 4, 123);
            this.AssertRegistration(result[4], 3, 1, 4, 123);
            this.AssertRegistration(result[5], 4, 1, 4, 123);
        }

        [TestMethod]
        public void LastTarget_SecondTeam()
        {
            var settings = TestDataHelper.GetSettings();

            var leonPerson = new LeonPerson
            {
                Class = "4",
                ClubName = "Bodø Østre",
                Name = "Roald Parelius",
                Range = 2,
                ShooterId = 123,
                SumIn = 0,
                Target = 4,
                Team = 2
            };

            var result = OrionGenerate.GenerateOrionForShooter(leonPerson, settings.OrionSetting);

            Assert.IsTrue(result.Count == 6);
            this.AssertRegistration(result[0], 1, 2, 4, 123);
            this.AssertRegistration(result[1], 1, 4, 8, 123);
            this.AssertRegistration(result[2], 1, 6, 12, 123);
            this.AssertRegistration(result[3], 2, 2, 4, 123);
            this.AssertRegistration(result[4], 3, 2, 4, 123);
            this.AssertRegistration(result[5], 4, 2, 4, 123);
        }

        [TestMethod]
        public void TestEncoding()
        {
            var filename = @"D:\Test\Grovfelt\FeltAdmin\KMINEW.TXT";
            var ofilename = @"D:\KME\Orion2\Data\KMINEW.TXT.tmp";
            var bytes = File.ReadAllBytes(filename);
            var obytes = File.ReadAllBytes(ofilename);

            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes("Øystein");
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            string msg = iso.GetString(isoBytes);
            
        }

        private void AssertRegistration(OrionRegistration registration, int orionid, int team, int target, int shooterid)
        {
            Assert.AreEqual(registration.OrionId, orionid);
            Assert.AreEqual(registration.Team, team);
            Assert.AreEqual(registration.Target, target);
            Assert.AreEqual(registration.ShooterId, shooterid);
        }
    }
}
