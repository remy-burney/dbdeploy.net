﻿using System;
using System.Collections.Generic;
using System.IO;
using Dbdeploy.Core;
using Dbdeploy.Core.Appliers;
using Dbdeploy.Core.Database;
using Dbdeploy.Core.Scripts;
using NUnit.Framework;
using Test.Dbdeploy.Scripts;

namespace Test.Dbdeploy.Database
{
    [TestFixture]
    public class ScriptGenerationTest
    {
        [Test]
        public void GenerateConsolidatedChangesScriptForAllDatabasesAndCompareAgainstTemplate()
        {
            foreach (string syntax in new[] { "mssql", "mysql", "ora" }) 
            {
                try 
                {
                    System.Console.WriteLine("Testing syntax {0}\n", syntax);

                    RunIntegratedTestAndConfirmOutputResults(syntax, new DirectoryInfo(@".\Resources"));
                }
                catch (Exception e) 
                {
                    throw new ApplicationException("Failed while testing syntax " + syntax, e);
                }
            }
        }

        [Test]
        public void GenerateConsolidatedChangesScriptForAllDatabasesLoadingTemplatesFromResourcesAndCompareAgainstTemplate()
        {
            foreach (string syntax in new[] { "mssql", "mysql", "ora" }) 
            {
                try 
                {
                    System.Console.WriteLine("Testing syntax {0}\n", syntax);

                    RunIntegratedTestAndConfirmOutputResults(syntax, null);
                }
                catch (Exception e) 
                {
                    throw new ApplicationException("Failed while testing syntax " + syntax, e);
                }
            }
        }

        private void RunIntegratedTestAndConfirmOutputResults(string syntaxName, DirectoryInfo templateDirectory) 
        {
            StringWriter writer = new StringWriter();

            ChangeScript changeOne = new StubChangeScript(1, "001_change.sql", "-- contents of change script 1");
            ChangeScript changeTwo = new StubChangeScript(2, "002_change.sql", "-- contents of change script 2");

            List<ChangeScript> changeScripts = new List<ChangeScript> { changeOne, changeTwo };
            ChangeScriptRepository changeScriptRepository = new ChangeScriptRepository(changeScripts);

            var factory = new DbmsFactory(syntaxName, string.Empty);
            var dbmsSyntax = factory.CreateDbmsSyntax();

            var createChangeLogTable = false;
            StubSchemaManager schemaManager = new StubSchemaManager(dbmsSyntax, createChangeLogTable);

            IChangeScriptApplier applier = new TemplateBasedApplier(writer, dbmsSyntax, "ChangeLog", ";", new NormalDelimiter(), templateDirectory);
            Controller controller = new Controller(changeScriptRepository, schemaManager, applier, null, createChangeLogTable, System.Console.Out);

            controller.ProcessChangeScripts(null);

            var actual = writer.ToString();

            try
            {
                Assert.AreEqual(ReadExpectedFileContents(GetExpectedFilename(syntaxName)), actual);
            }
            catch (Exception)
            {
                // Output actual template on failure.
                System.Console.WriteLine("\n\nActual Template ({0}):", syntaxName);
                System.Console.WriteLine(actual);
                throw;
            }
        }

        private String GetExpectedFilename(string dbSyntaxName) 
        {
            return @"Resources\Database\" + dbSyntaxName + "_expected.sql";
        }

        private string ReadExpectedFileContents(string expectedFilename)
        {
            Stream stream = File.OpenRead(expectedFilename);

            StreamReader reader = new StreamReader(stream);

            try 
            {
                return ReadEntireStreamIntoAStringWithConversionToSystemDependantLineTerminators(reader);
            }
            finally 
            {
                reader.Dispose();
            }
        }

        private string ReadEntireStreamIntoAStringWithConversionToSystemDependantLineTerminators(TextReader reader)
        {
            StringWriter writer = new StringWriter();
            try 
            {
                string line;

                while ((line = reader.ReadLine()) != null) 
                {
                    writer.WriteLine(line);
                }

                writer.Flush();

                return writer.ToString();
            }
            finally 
            {
                writer.Dispose();
            }
        }

        private class StubSchemaManager : DatabaseSchemaVersionManager 
        {
            private readonly bool changeLogTableExists;

            public StubSchemaManager(IDbmsSyntax syntax, bool changeLogTableExists)
                : base(null, syntax, "ChangeLog")
            {
                this.changeLogTableExists = changeLogTableExists;
            }

            public override bool ChangeLogTableExists()
            {
                return !changeLogTableExists;
            }

            public override IList<ChangeEntry> GetAppliedChanges()
            {
                return new List<ChangeEntry>();
            }
        }
    }
}