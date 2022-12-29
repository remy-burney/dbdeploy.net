namespace Net.Sf.Dbdeploy.Database
{
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;

    using Moq;

    using Scripts;

    using NUnit.Framework;

    /// <summary>
    /// Unit tests for <see cref="DatabaseSchemaVersionManager" /> class.
    /// </summary>
    [TestFixture]
    public class DatabaseSchemaVersionManagerTest
    {
        private readonly ChangeScript script = new ChangeScript("Alpha", 99, "Some Description");

        private DatabaseSchemaVersionManager schemaVersionManager;

        private Mock<IDataReader> expectedResultSet;
        private Mock<QueryExecuter> queryExecuter;
        private Mock<IDbmsSyntax> syntax;

        private string changeLogTableName;

        private List<string> executedQueries;

        [SetUp]
        public void SetUp() 
        {
            changeLogTableName = "ChangeLog";

            expectedResultSet = new Mock<IDataReader>();

            var connection = new Mock<IDbConnection>();

            var factory = new Mock<DbmsFactory>("mssql", string.Empty);
            factory.Setup(f => f.CreateConnection()).Returns(connection.Object);
        
            queryExecuter = new Mock<QueryExecuter>(factory.Object);

            syntax = new Mock<IDbmsSyntax>();
            syntax.Setup(s => s.TableExists(It.IsAny<string>()))
                .Returns<string>(t => string.Format(CultureInfo.InvariantCulture,
@"SELECT table_schema 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = '{0}'", t));

            executedQueries = new List<string>();

            var checkForChangeLogDataReader = new Mock<IDataReader>();
            checkForChangeLogDataReader
                .Setup(r => r.Read())
                .Returns(true);

            queryExecuter
                .Setup(e => e.ExecuteQuery(It.Is<string>(v => v.Contains("INFORMATION_SCHEMA"))))
                .Returns(() => checkForChangeLogDataReader.Object);

            queryExecuter
                .Setup(e => e.ExecuteQuery(It.Is<string>(v => !v.Contains("INFORMATION_SCHEMA")), It.IsAny<object[]>()))
                .Returns(expectedResultSet.Object)
                .Callback<string, object[]>((q, a) => executedQueries.Add(q));

            schemaVersionManager = new DatabaseSchemaVersionManager(queryExecuter.Object, syntax.Object, changeLogTableName);
        }

        [Test]
        public void ShouldUseQueryExecuterToReadInformationFromTheChangelogTable() 
        {
            var readResults = new List<bool> { true, true, true, false };
            var readEnumerator = readResults.GetEnumerator();
            var getResults = new List<ChangeEntry>
                                 {
                                     new ChangeEntry("Alpha", 5)
                                         {
                                             ChangeId = 1,
                                             ScriptName = "05.test.sql",
                                             Status = ScriptStatus.Success,
                                             Output = "Complete"
                                         }, 
                                     new ChangeEntry("Alpha", 9)
                                         {
                                             ChangeId = 2,
                                             ScriptName = "09.test.sql",
                                             Status = ScriptStatus.ProblemResolved,
                                             Output = "Fixed"
                                         }, 
                                     new ChangeEntry("Alpha", 12)
                                         {
                                             ChangeId = 3,
                                             ScriptName = "12.test.sql",
                                             Status = ScriptStatus.Failure,
                                             Output = "Failed"
                                         }, 
                                 };
            var getEnumerator = getResults.GetEnumerator();

            expectedResultSet.Setup(rs => rs.Read()).Returns(() =>
            {
                readEnumerator.MoveNext();
                getEnumerator.MoveNext();
                return readEnumerator.Current;
            });
            expectedResultSet.Setup(rs => rs["Folder"]).Returns(() => getEnumerator.Current.Folder);
            expectedResultSet.Setup(rs => rs["ScriptNumber"]).Returns(() => (short)getEnumerator.Current.ScriptNumber);
            expectedResultSet.Setup(rs => rs["ChangeId"]).Returns(() => getEnumerator.Current.ChangeId);
            expectedResultSet.Setup(rs => rs["ScriptName"]).Returns(() => getEnumerator.Current.ScriptName);
            expectedResultSet.Setup(rs => rs["ScriptStatus"]).Returns(() => (byte)getEnumerator.Current.Status);
            expectedResultSet.Setup(rs => rs["ScriptOutput"]).Returns(() => getEnumerator.Current.Output);

            var changes = schemaVersionManager.GetAppliedChanges().ToList();
        
            Assert.AreEqual(3, changes.Count, "Incorrect number of changes found.");
            for (int i = 0; i < getResults.Count; i++)
            {
                AssertChangeProperties(getResults[i], changes[i]);
            }
        }

        [Test]
        public void ShouldUpdateChangelogTable() 
        {
            syntax.Setup(s => s.CurrentUser).Returns("DBUSER");
            syntax.Setup(s => s.CurrentTimestamp).Returns("TIMESTAMP");

            schemaVersionManager.RecordScriptStatus(script, ScriptStatus.Success, "Script output");
            string expected = @"INSERT INTO ChangeLog (Folder, ScriptNumber, ScriptName, StartDate, CompleteDate, AppliedBy, ScriptStatus, ScriptOutput) VALUES (@1, @2, @3, TIMESTAMP, TIMESTAMP, DBUSER, @4, @5) 
SELECT ChangeId FROM ChangeLog WHERE Folder = @1 and ScriptNumber = @2";

            Assert.AreEqual(expected, executedQueries.FirstOrDefault(), "The query executed was incorrect.");

            queryExecuter.Verify(e => e.ExecuteQuery(expected, script.Folder, script.ScriptNumber, script.ScriptName, (int)ScriptStatus.Success, "Script output"), Times.Once());
        }

        [Test]
        public void ShouldGenerateSqlStringToDeleteChangelogTableAfterUndoScriptApplication() 
        {
            string sql = schemaVersionManager.GetChangelogDeleteSql(script);
            string expected = "DELETE FROM ChangeLog WHERE Folder = 'Alpha' AND ScriptNumber = 99";

            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void ShouldGetAppliedChangesFromSpecifiedChangelogTableName()
        {
            changeLogTableName = "user_specified_changelog";

            var schemaVersionManagerWithDifferentTableName =
                new DatabaseSchemaVersionManager(queryExecuter.Object, syntax.Object, changeLogTableName);

            schemaVersionManagerWithDifferentTableName.GetAppliedChanges();

            queryExecuter.Verify(e => e.ExecuteQuery(It.Is<string>(s => s.StartsWith("SELECT ChangeId, Folder, ScriptNumber, ScriptName, ScriptStatus, ScriptOutput FROM user_specified_changelog"))));
        }

        [Test]
        public void ShouldGenerateSqlStringContainingSpecifiedChangelogTableNameOnDelete() 
        {
            var schemaVersionManagerWithDifferentTableName =
                new DatabaseSchemaVersionManager(queryExecuter.Object, syntax.Object, "user_specified_changelog");

            string updateSql = schemaVersionManagerWithDifferentTableName.GetChangelogDeleteSql(script);

            Assert.IsTrue(updateSql.StartsWith("DELETE FROM user_specified_changelog "));
        }

        /// <summary>
        /// Asserts the change properties.
        /// </summary>
        /// <param name="expected">The expected.</param>
        /// <param name="retrieved">The retrieved.</param>
        private static void AssertChangeProperties(ChangeEntry expected, ChangeEntry retrieved)
        {
            Assert.AreEqual(expected.Folder, retrieved.Folder, "Folder does not match.");
            Assert.AreEqual(expected.ScriptNumber, retrieved.ScriptNumber, "ScriptNumber does not match.");
            Assert.AreEqual(expected.ChangeId, retrieved.ChangeId, "ChangeId does not match.");
            Assert.AreEqual(expected.ScriptName, retrieved.ScriptName, "ScriptName does not match.");
            Assert.AreEqual(expected.Status, retrieved.Status, "ScriptStatus does not match.");
            Assert.AreEqual(expected.Output, retrieved.Output, "ScriptOutput does not match.");
        }
    }
}