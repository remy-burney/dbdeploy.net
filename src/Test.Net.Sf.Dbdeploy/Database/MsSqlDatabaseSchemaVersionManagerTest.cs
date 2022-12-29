

namespace Net.Sf.Dbdeploy.Database
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Microsoft.Data.SqlClient;
    using Appliers;
    using Configuration;
    using SqlCmd;
    using Scripts;
    using NUnit.Framework;

    [Category("MSSQL"), Category("DbIntegration")]
    public class MsSqlDatabaseSchemaVersionManagerTest : AbstractDatabaseSchemaVersionManagerTest
    {
        private static string _connectionString;
        private const string FOLDER = "Scripts";

        private readonly string[] CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGES = new [] 
        {
            "No table found with name 'ChangeLog'.",
        };

		private const string DBMS = "mssql";

        protected override string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    _connectionString = ConfigurationManager.AppSettings["ConnString-" + Environment.MachineName]
                                        ?? ConfigurationManager.AppSettings["ConnString"];
                }
                return _connectionString;
            }
        }

        protected override string Folder
        {
            get { return FOLDER; }
        }

        protected override string[] ChangelogTableDoesNotExistMessages
        {
            get { return CHANGELOG_TABLE_DOES_NOT_EXIST_MESSAGES; }
        }

    	protected override string Dbms
    	{
			get { return DBMS; }
    	}

        [Test]
    	public void ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted()
    	{
			EnsureTableDoesNotExist();
			CreateTable();
    		InsertRowIntoTable(3);
			var changeNumbers = new List<ChangeEntry>(databaseSchemaVersion.GetAppliedChanges());

			Assert.AreEqual(1, changeNumbers.Count);
			Assert.AreEqual("Scripts/3", changeNumbers[0].UniqueKey);
		}


        [Test]
        public void TestDoesNotRunSecondScriptIfFirstScriptFails()
        {
            EnsureTableDoesNotExist("TableWeWillUse");
            EnsureTableDoesNotExist(TableName);

            var factory = new DbmsFactory(Dbms, ConnectionString);
            var dbmsSyntax = factory.CreateDbmsSyntax();

            var output = new StringBuilder();
            
            var applier = new TemplateBasedApplier(
                new StringWriter(output),
                dbmsSyntax,
                TableName,
                ";",
                DbDeployDefaults.DelimiterType,
                DbDeployDefaults.TemplateDirectory);

            applier.Apply(new ChangeScript[]
            {
                new StubChangeScript(1, "1.test.sql", "INSERT INTO TableWeWillUse VALUES (1);"), 
                new StubChangeScript(2, "2.test.sql", "CREATE TABLE dbo.TableWeWillUse (Id int NULL);"), 
            }, createChangeLogTable: true);

            using (var sqlExecuter = new SqlCmdExecutor(ConnectionString))
            {
                var cmdOutput = new StringBuilder();
                sqlExecuter.ExecuteString(output.ToString(), cmdOutput);
            }
            AssertTableDoesNotExist("TableWeWillUse");
        }


        [Test]
        public override void TestCanRetrieveSchemaVersionFromDatabase()
        {
            base.TestCanRetrieveSchemaVersionFromDatabase();
        }

        [Test]
        public override void TestReturnsNoAppliedChangesWhenDatabaseTableDoesNotExist()
        {
            base.TestReturnsNoAppliedChangesWhenDatabaseTableDoesNotExist();
        }

        [Test]
        public override void TestShouldReturnEmptySetWhenTableHasNoRows()
        {
            base.TestShouldReturnEmptySetWhenTableHasNoRows();
        }

        [Test]
        public override void TestShouldCreateChangeLogTableWhenToldToDoSo()
        {
            base.TestShouldCreateChangeLogTableWhenToldToDoSo();
        }

        /// <summary>
        /// Tests that <see cref="DatabaseSchemaVersionManager" /> can create a change log table under a specified schema.
        /// </summary>
        [Test]
        public void TestShouldHandleCreatingChangeLogTableWithSchema()
        {
            EnsureTableDoesNotExist("log.Installs");

            var factory = new DbmsFactory(Dbms, ConnectionString);
            var executer = new QueryExecuter(factory);
            var databaseSchemaManager = new DatabaseSchemaVersionManager(executer, factory.CreateDbmsSyntax(), "log.Installs");

            var applier = new DirectToDbApplier(executer, databaseSchemaManager, new QueryStatementSplitter(),
                factory.CreateDbmsSyntax(), "log.Installs", new NullWriter());
            
            applier.Apply(new ChangeScript[] {}, createChangeLogTable: true);

            AssertTableExists("log.Installs");
        }

        /// <summary>
        /// Ensures the table does not exist.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        protected override void EnsureTableDoesNotExist(string tableName)
        {
            var syntax = new MsSqlDbmsSyntax();
            var tableInfo = syntax.GetTableInfo(tableName);
            ExecuteSql(string.Format(
                CultureInfo.InvariantCulture,
@"IF (EXISTS (SELECT * 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_SCHEMA = '{0}' 
    AND  TABLE_NAME = '{1}'))
BEGIN
    DROP Table {0}.{1}
END", 
                tableInfo.Schema, tableInfo.TableName));
        }

        protected override IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        protected override void InsertRowIntoTable(int i)
        {
            ExecuteSql("INSERT INTO " + TableName
                       + " (Folder, ScriptNumber, StartDate, CompleteDate, AppliedBy, ScriptName, ScriptStatus, ScriptOutput) VALUES ( "
                       + "'" + FOLDER + "', " + i
                       + ", getdate(), getdate(), user_name(), 'Unit test', 1, '')");
        }
    }
}