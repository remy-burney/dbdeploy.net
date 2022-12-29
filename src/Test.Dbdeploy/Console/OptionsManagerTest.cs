using System;
using System.IO;
using System.Linq;
using Dbdeploy.Console;
using Dbdeploy.Core.Configuration;
using Dbdeploy.Core.Database;
using NUnit.Framework;

namespace Test.Dbdeploy.Console
{
    [TestFixture]
    public class OptionsManagerTest
    {
        [Test]
        public void CanParseConnectionStringFromCommandLine() 
        {
            var config = Enumerable.First(OptionsManager.ParseOptions("-c \"DataSource:.\\SQLEXPRESS;...;\"".Split(' ')).Deployments);
            Assert.AreEqual("DataSource:.\\SQLEXPRESS;...;", config.ConnectionString);
        }

        [Test]
        public void ThisIsntReallyATestBecuaseThereIsNoAssertButItsVeryUsefulToLookAtTheResult() 
        {
            OptionsManager.PrintUsage();
        }

        [Test]
        public void CheckAllOfTheOtherFieldsParseOkHere() 
        {
            var config = Enumerable.First(OptionsManager.ParseOptions(
                ("-c \"DataSource:.\\SQLEXPRESS;...;\" " +
                 "--scriptdirectory . -o output.sql " +
                 "--changelogtablename my-change-log " +
                 "--dbms ora " +
                 "--templatedirectory /tmp/mytemplates " +
                 "--delimiter \\ --delimitertype row").Split(' ')).Deployments);

            Assert.AreEqual("DataSource:.\\SQLEXPRESS;...;", config.ConnectionString);
            Assert.AreEqual(Environment.CurrentDirectory, config.ScriptDirectory.FullName);
            Assert.AreEqual("output.sql", config.OutputFile.Name);
            Assert.AreEqual("ora", config.Dbms);
            Assert.AreEqual("my-change-log", config.ChangeLogTableName);
            Assert.AreEqual("\\", config.Delimiter);
            Assert.IsInstanceOfType(typeof(RowDelimiter), config.DelimiterType);
            Assert.IsTrue(config.TemplateDirectory.FullName.EndsWith(Path.DirectorySeparatorChar + "tmp" + Path.DirectorySeparatorChar + "mytemplates"));
        }

        [Test]
        public void DelimiterTypeWorksOk() 
        {
            var config = Enumerable.First(OptionsManager.ParseOptions("--delimitertype normal".Split(' ')).Deployments);
            Assert.IsInstanceOfType(typeof(NormalDelimiter), config.DelimiterType);

            config = Enumerable.First(OptionsManager.ParseOptions("--delimitertype row".Split(' ')).Deployments);
            Assert.IsInstanceOfType(typeof(RowDelimiter), config.DelimiterType);
        }

        [Test]
        public void LineEndingWorksOk()
        {
            var config = new DbDeployConfig();
            Assert.AreEqual(DbDeployDefaults.LineEnding, config.LineEnding);

            config = Enumerable.First(OptionsManager.ParseOptions("--lineending cr".Split(' ')).Deployments);
            Assert.AreEqual(LineEnding.Cr, config.LineEnding);

            config = Enumerable.First(OptionsManager.ParseOptions("--lineending crlf".Split(' ')).Deployments);
            Assert.AreEqual(LineEnding.CrLf, config.LineEnding);

            config = Enumerable.First(OptionsManager.ParseOptions("--lineending lf".Split(' ')).Deployments);
            Assert.AreEqual(LineEnding.Lf, config.LineEnding);

            config = Enumerable.First(OptionsManager.ParseOptions("--lineending platform".Split(' ')).Deployments);
            Assert.AreEqual(LineEnding.Platform, config.LineEnding);

        }
    }
}
