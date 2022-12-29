using System.Collections.Generic;
using System.Data;
using System.Text;
using Dbdeploy.Core.Database;
using Dbdeploy.Core.Exceptions;
using Dbdeploy.Core.Scripts;
using Moq;
using NUnit.Framework;
using Test.Dbdeploy.Scripts;

namespace Test.Dbdeploy.Appliers
{
    class DirectToDbApplierTest
    {
        private Mock<QueryExecuter> queryExecuter;
        private Mock<DatabaseSchemaVersionManager> schemaVersionManager;
        private Mock<QueryStatementSplitter> splitter;

        private DirectToDbApplierAccessor applier;

        [SetUp]
        public void SetUp()
        {
            IDbmsSyntax syntax = null;
            QueryExecuter nullExecuter = null;

            var factory = new Mock<DbmsFactory>("mssql", string.Empty);
            factory.Setup(f => f.CreateConnection()).Returns(new Mock<IDbConnection>().Object);
            factory.Setup(f => f.CreateDbmsSyntax()).Returns(syntax);

            queryExecuter = new Mock<QueryExecuter>(factory.Object);

            schemaVersionManager = new Mock<DatabaseSchemaVersionManager>(nullExecuter, syntax, "empty");

            splitter = new Mock<QueryStatementSplitter>();

            applier = new DirectToDbApplierAccessor(
                queryExecuter.Object,
                schemaVersionManager.Object,
                splitter.Object,
                syntax, 
                "ChangeLog",
                System.Console.Out);
        }

        [Test]
        public void ShouldApplyChangeScriptBySplittingContentUsingTheSplitter() 
        {
            splitter.Setup(s => s.Split("split; content")).Returns(new List<string> { "split", "content" });

            var output = new StringBuilder();
            applier.ApplyChangeScript(new StubChangeScript(1, "script", "split; content"), output);

            queryExecuter.Verify(e => e.Execute("split", It.IsAny<StringBuilder>()));
            queryExecuter.Verify(e => e.Execute("content", It.IsAny<StringBuilder>()));
        }

        [Test]
        public void ShouldRethrowSqlExceptionsWithInformationAboutWhatStringFailed() 
        {
            splitter.Setup(s => s.Split("split; content")).Returns(new List<string> { "split", "content" });
                
            ChangeScript script = new StubChangeScript(1, "script", "split; content");
            
            queryExecuter.Setup(e => e.Execute("split", It.IsAny<StringBuilder>())).Throws(new DummyDbException());

            try 
            {
                var output = new StringBuilder();
                applier.ApplyChangeScript(script, output);
                        
                Assert.Fail("exception expected");
            }
            catch (ChangeScriptFailedException e) 
            {
                Assert.AreEqual("split", e.ExecutedSql);
                Assert.AreEqual(script, e.Script);
            }

            queryExecuter.Verify(e => e.Execute("content"), Times.Never());
        }

        [Test]
        public void ShouldRecordSuccessInSchemaVersionTable() 
        {
            ChangeScript changeScript = new ChangeScript("Scripts", 1, "script.sql");

            applier.RecordScriptStatus(changeScript, ScriptStatus.Success, "Script completed");

            schemaVersionManager.Verify(s => s.RecordScriptStatus(changeScript, ScriptStatus.Success, "Script completed"));
        }

        [Test]
        public void ShouldRecordFailureInSchemaVersionTable()
        {
            ChangeScript changeScript = new ChangeScript("Scripts", 1, "script.sql");

            applier.RecordScriptStatus(changeScript, ScriptStatus.Failure, "Script failed");

            schemaVersionManager.Verify(s => s.RecordScriptStatus(changeScript, ScriptStatus.Failure, "Script failed"));
        }

        [Test]
        public void ShouldCommitTransaction() 
        {
            var scripts = new List<ChangeScript> { new StubChangeScript(1, "description", "content") };

            queryExecuter.Setup(e => e.BeginTransaction()).Callback(() => { return; });
            queryExecuter.Setup(e => e.CommitTransaction()).Callback(() => { return; });

            splitter.Setup(s => s.Split(It.IsAny<string>())).Returns<string>(s => new [] { s });

            applier.Apply(scripts, false);

            queryExecuter.Verify(e => e.BeginTransaction(), Times.Once());
            queryExecuter.Verify(e => e.CommitTransaction(), Times.Once());
        }
    }
}