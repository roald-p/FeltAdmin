using System;
using System.Collections.Generic;

using FeltAdmin.FileHandlers;
using FeltAdmin.Leon;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace FeltAdminTest
{
    [TestClass]
    public class LeonReaderTests
    {
        [TestMethod]
        public void ReadNewFile_ok()
        {
            var leonLines = new List<string>();
            leonLines.Add("2;1;1;;;;0;0");
            leonLines.Add("2;1;2;;;;0;0");
            leonLines.Add("2;1;3;;;;0;0");
            leonLines.Add("2;1;4;;;;0;0");
            leonLines.Add("2;1;5;;;;0;0");
            leonLines.Add("2;1;6;;;;0;0");
            leonLines.Add("2;1;7;;;;0;0");
            leonLines.Add("2;1;8;;;;0;0");
            leonLines.Add("2;1;9;;;;0;0");
            leonLines.Add("2;1;10;;;;0;0");
            leonLines.Add("2;1;11;;;;0;0");
            leonLines.Add("2;1;12;;;;0;0");
            leonLines.Add("2;1;13;;;;0;0");
            leonLines.Add("2;1;14;;;;0;0");
            leonLines.Add("2;1;15;Roald Parelius;;;0;0");
            leonLines.Add("2;1;15;Roald Parelius;Bodø Østre ;;0;0");
            leonLines.Add("2;1;15;Roald Parelius;Bodø Østre ;4;0;0");
            leonLines.Add("2;1;15;Roald Parelius;Bodø Østre ;4;0;211");
            leonLines.Add("2;1;16;;;;0;0");
            leonLines.Add("2;1;17;;;;0;0");
            leonLines.Add("2;1;18;Jan Erik Aasheim;Bodø Østre ;5;0;210");
            leonLines.Add("2;1;19;Arnt Eirik Aune;Beiarn ;5;0;209");
            leonLines.Add("2;1;20;Lars-Håkon Nohr Nystad;Bodø Østre ;5;0;208");
            leonLines.Add("2;1;21;Lars-Andreas Nystad;Bodø Østre ;5;0;207");
            leonLines.Add("2;1;22;;;;0;0");
            leonLines.Add("2;1;23;;;;0;0");
            leonLines.Add("2;1;24;;;;0;0");
            leonLines.Add("2;1;25;;;;0;0");

            var fileHandler = new Mock<IFileHandler>();
            fileHandler.Setup(f => f.Exists(It.IsAny<string>())).Returns(true);
            fileHandler.Setup(f => f.ReadAllLines(It.IsAny<string>())).Returns(leonLines);

            var leonReader = new LeonReader(@"D:\KME\Orion2\Data", fileHandler.Object);
            var newFile = leonReader.CheckForNewFile();
            if (newFile)
            {
                var leonPersons = leonReader.GetLeonPersons();
                Assert.AreEqual(5, leonPersons.Count);
            }

            Assert.IsTrue(newFile);
        }
    }
}
