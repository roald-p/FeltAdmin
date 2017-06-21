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
    public class OrionGeneratePausedTests
    {
        [TestMethod]
        public void FirstTarget_FirstTeam()
        {
            var settings = TestDataHelper.GetSettings("FeltAdminTest.TestData.FeltAdmin2OrionPauses.xml");

            var leonPerson = new LeonPerson
                             {
                                 Class = "4",
                                 ClubName = "Bodø Østre",
                                 Name = "Roald Parelius",
                                 Range = 1,
                                 ShooterId = 123,
                                 SumIn = 0,
                                 Target = 1,
                                 Team = 1
                             };

            foreach (var orionView in settings.OrionSetting.OrionViewModels)
            {
                foreach (var range in orionView.RangeViews)
                {
                    range.Parent = orionView;
                }
            }

            //AssertRegistration(OrionRegistration registration, int orionid, int team, int target, int shooterid)

            var result = OrionGenerate.GenerateOrionForShooter(leonPerson, settings.OrionSetting);

            Assert.IsTrue(result.Count == 5);
            this.AssertRegistration(result[0], 2, 1, 1, 123);
            this.AssertRegistration(result[1], 2, 3, 5, 123);
            this.AssertRegistration(result[2], 3, 5, 1, 123);
            this.AssertRegistration(result[3], 3, 7, 4, 123);
            this.AssertRegistration(result[4], 2, 9, 14, 123);
            
        }

        [TestMethod]
        public void SecondTarget_FirstTeam()
        {
            var settings = TestDataHelper.GetSettings("FeltAdminTest.TestData.FeltAdmin2OrionPauses.xml");

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
            var settings = TestDataHelper.GetSettings("FeltAdminTest.TestData.FeltAdmin2OrionPauses.xml");

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
            var settings = TestDataHelper.GetSettings("FeltAdminTest.TestData.FeltAdmin2OrionPauses.xml");

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


        private void AssertRegistration(OrionRegistration registration, int orionid, int team, int target, int shooterid)
        {
            Assert.AreEqual(registration.OrionId, orionid);
            Assert.AreEqual(registration.Team, team);
            Assert.AreEqual(registration.Target, target);
            Assert.AreEqual(registration.ShooterId, shooterid);
        }
    }
}
