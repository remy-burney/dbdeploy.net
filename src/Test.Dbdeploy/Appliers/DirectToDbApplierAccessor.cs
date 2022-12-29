﻿using System.IO;
using System.Text;
using Dbdeploy.Core.Appliers;
using Dbdeploy.Core.Database;
using Dbdeploy.Core.Scripts;

namespace Test.Dbdeploy.Appliers
{
    internal class DirectToDbApplierAccessor : DirectToDbApplier
    {
        public DirectToDbApplierAccessor(
            QueryExecuter queryExecuter, 
            DatabaseSchemaVersionManager schemaVersionManager, 
            QueryStatementSplitter splitter, 
            IDbmsSyntax dbmsSyntax,
            string changeLogTableName,
            TextWriter infoTextWriter)
            : base(queryExecuter, schemaVersionManager, splitter, dbmsSyntax, changeLogTableName, infoTextWriter)
        {
        }
        
        public new void ApplyChangeScript(ChangeScript script, StringBuilder output)
        {
            base.ApplyChangeScript(script, output);
        }

        public new void RecordScriptStatus(ChangeScript changeScript, ScriptStatus status, string output)
        {
            base.RecordScriptStatus(changeScript, status, output);
        }
    }
}
