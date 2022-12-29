using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using Dbdeploy.Core.Database;
using Dbdeploy.Core.Exceptions;
using Dbdeploy.Core.Scripts;

namespace Dbdeploy.Core.Appliers
{
    public class DirectToDbApplier : IChangeScriptApplier
    {
        private readonly QueryExecuter queryExecuter;

        private readonly DatabaseSchemaVersionManager schemaVersionManager;

        private readonly QueryStatementSplitter splitter;

        private readonly IDbmsSyntax dbmsSyntax;

        private readonly string changeLogTableName;

        private readonly TextWriter infoTextWriter;

        public DirectToDbApplier(
            QueryExecuter queryExecuter,
            DatabaseSchemaVersionManager schemaVersionManager,
            QueryStatementSplitter splitter,
            IDbmsSyntax dbmsSyntax,
            string changeLogTableName,
            TextWriter infoTextWriter)
        {
            if (queryExecuter == null)
                throw new ArgumentNullException("queryExecuter");

            if (schemaVersionManager == null)
                throw new ArgumentNullException("schemaVersionManager");

            if (splitter == null)
                throw new ArgumentNullException("splitter");

            if (infoTextWriter == null)
                throw new ArgumentNullException("infoTextWriter");

            this.queryExecuter = queryExecuter;
            this.schemaVersionManager = schemaVersionManager;
            this.splitter = splitter;
            this.dbmsSyntax = dbmsSyntax;
            this.changeLogTableName = changeLogTableName;
            this.infoTextWriter = infoTextWriter;
        }

        public void Apply(IEnumerable<ChangeScript> changeScripts, bool createChangeLogTable)
        {
            if (createChangeLogTable)
            {
                infoTextWriter.WriteLine("Creating change log table");
                queryExecuter.Execute(dbmsSyntax.CreateChangeLogTableSqlScript(changeLogTableName));
            }

            var enumerable = changeScripts.ToList();
            infoTextWriter.WriteLine(enumerable.Any() ? "Applying change scripts...\n" : "No changes to apply.\n");

            foreach (var script in enumerable)
            {
                RecordScriptStatus(script, ScriptStatus.Started);

                // Begin transaction
                queryExecuter.BeginTransaction();

                infoTextWriter.WriteLine(script);
                infoTextWriter.WriteLine("----------------------------------------------------------");

                // Apply changes and update ChangeLog table
                var output = new StringBuilder();
                try
                {
                    ApplyChangeScript(script, output);
                    RecordScriptStatus(script, ScriptStatus.Success, output.ToString());
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        output.AppendLine(ex.InnerException.Message);
                    }

                    RecordScriptStatus(script, ScriptStatus.Failure, output.ToString());
                    throw;
                }

                // Commit transaction
                queryExecuter.CommitTransaction();
            }
        }

        /// <summary>
        /// Applies the change script.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="output">The output from applying the change script.</param>
        protected void ApplyChangeScript(ChangeScript script, StringBuilder output)
        {
            ICollection<string> statements = splitter.Split(script.GetContent());

            int i = 0;

            foreach (var statement in statements)
            {
                try
                {
                    if (statements.Count > 1)
                    {
                        infoTextWriter.WriteLine(" -> statement " + (i + 1) + " of " + statements.Count + "...");
                    }

                    queryExecuter.Execute(statement, output);

                    i++;
                }
                catch (DbException e)
                {
                    throw new ChangeScriptFailedException(e, script, i + 1, statement);
                }
                finally
                {
                    // Write out SQL execution output.
                    if (output.Length > 0)
                    {
                        infoTextWriter.WriteLine(output.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Records details about a change script in the database.
        /// </summary>
        /// <param name="changeScript">The change script.</param>
        /// <param name="status">Status of the script execution.</param>
        /// <param name="output">The output from running the script.</param>
        protected void RecordScriptStatus(ChangeScript changeScript, ScriptStatus status, string output = null) 
        {
            schemaVersionManager.RecordScriptStatus(changeScript, status, output);
        }
    }
}
