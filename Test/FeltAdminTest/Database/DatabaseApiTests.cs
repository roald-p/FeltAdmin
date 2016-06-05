using System;
using System.IO;
using System.Linq;
using System.Threading;

using FeltAdmin.Database.API;
using FeltAdmin.Database.Engine;
using FeltAdmin.Diagnostics;
using FeltAdmin.Leon;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FeltAdminTest.Database
{
    [TestClass]
    public class DatabaseApiTests
    {
        [TestMethod]
        public void CreateNewCompetition_NewDbCreated()
        {
            Directory.Delete(@"C:\FeltAdmin\TEST\Database");
            var expectedPath = Path.Combine(@"C:\FeltAdmin\TEST\Database", "TestStevne_2016.02.26");
            var expectedPath1 = Path.Combine(@"C:\FeltAdmin\TEST\Database", "TestStevne_2016.02.26-1");
            var expectedPath2 = Path.Combine(@"C:\FeltAdmin\TEST\Database", "TestStevne_2016.02.26-2");

            DatabaseApi.CreateNewCompetition("TestStevne", new DateTime(2016, 2, 26));
            Thread.Sleep(100);
            DatabaseApi.CreateNewCompetition("TestStevne", new DateTime(2016, 2, 26));
            Thread.Sleep(100);
            DatabaseApi.CreateNewCompetition("TestStevne", new DateTime(2016, 2, 26));

            var directories = DatabaseApi.GetAllCompetitions();

            Assert.IsTrue(Directory.Exists(expectedPath));
            Assert.IsTrue(Directory.Exists(expectedPath1));
            Assert.IsTrue(Directory.Exists(expectedPath2));
            Assert.AreEqual(expectedPath2, directories[0]);
            Assert.AreEqual(expectedPath1, directories[1]);
            Assert.AreEqual(expectedPath, directories[2]);
        }

        [TestMethod]
        public void Save_RowAdded()
        {
            this.InitTestDatabase("TestStevne", new DateTime(2016, 2, 26));
            var leonPerson = new LeonPerson
            {
                Class = "4",
                ClubName = "Bodø Østre",
                Name = "No Name",
                Range = 2,
                ShooterId = 123456,
                SumIn = 30,
                Target = 1,
                Team = 3
            };
            var result = DatabaseApi.Save(leonPerson);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void LoadFromCache_LeonRegistrations()
        {
            this.InitTestDatabase("TestStevne", new DateTime(2016, 2, 26));
            this.SaveLeonPersons(10);

            var result = DatabaseApi.LoadCompetitionFromTable(TableName.LeonRegistration);
            Assert.IsTrue(result.Count == 10);
        }

        private void InitTestDatabase(string competitionName, DateTime date)
        {
            try
            {
                Directory.Delete(@"C:\FeltAdmin\TEST\Database", true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Test");
            }

            DatabaseApi.CreateNewCompetition(competitionName, date);
        }

        private void SaveLeonPersons(int number)
        {
            for (int i = 0; i < number; i++)
            {
                var leonPerson = new LeonPerson
                                 {
                                     Class = "4",
                                     ClubName = "Bodø Østre",
                                     Name = "No Name" + i,
                                     Range = 2,
                                     ShooterId = 123456 + i,
                                     SumIn = 30,
                                     Target = i + 1,
                                     Team = 3
                                 };
                DatabaseApi.Save(leonPerson);
            }
        }
    }
}
