using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Dbdeploy.Core.Database;
using Dbdeploy.Core.Database.SqlCmd;
using Dbdeploy.Core.Exceptions;
using Dbdeploy.Core.Scripts;

namespace Dbdeploy.Core.Appliers
{
    /// <summary>
    /// Applier for running scripts using SQLCMD mode against MSSQL.
    /// </summary>
    public class SqlCmdApplier : IChangeScriptApplier
    {
        /// <summary>
        /// The database connection string.
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// The info text writer to display output information.
        /// </summary>
        private readonly TextWriter infoTextWriter;

        /// <summary>
        /// The schema version manager for tracking change scripts.
        /// </summary>
        private readonly DatabaseSchemaVersionManager schemaVersionManager;

        /// <summary>
        /// The current dbms syntax that will be used for creating the change log table
        /// </summary>
        private readonly IDbmsSyntax dbmsSyntax;

        /// <summary>
        /// The current change log table name
        /// </summary>
        private readonly string changeLogTableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCmdApplier" /> class.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="schemaVersionManager">The schema version manager.</param>
        /// <param name="dbmsSyntax">Database syntax</param>
        /// <param name="changeLogTableName">ChangeLog table name</param>
        /// <param name="infoTextWriter">The info text writer.</param>
        public SqlCmdApplier(
            string connectionString,
            DatabaseSchemaVersionManager schemaVersionManager,
            IDbmsSyntax dbmsSyntax,
            string changeLogTableName,
            TextWriter infoTextWriter)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (schemaVersionManager == null)
            {
                throw new ArgumentNullException("schemaVersionManager");
            }

            if (dbmsSyntax == null)
            {
                throw new ArgumentNullException("dbmsSyntax");
            }

            if (changeLogTableName == null)
            {
                throw new ArgumentNullException("changeLogTableName");
            }

            if (infoTextWriter == null)
            {
                throw new ArgumentNullException("infoTextWriter");
            }

            this.schemaVersionManager = schemaVersionManager;
            this.dbmsSyntax = dbmsSyntax;
            this.changeLogTableName = changeLogTableName;
            this.infoTextWriter = infoTextWriter;
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Applies the specified change scripts.
        /// </summary>
        /// <param name="changeScripts">The change scripts.</param>
        /// <param name="createChangeLogTable">Whether the change log table script should also be generated at the top</param>
        public void Apply(IEnumerable<ChangeScript> changeScripts, bool createChangeLogTable)
        {
            using (var sqlCmdExecutor = new SqlCmdExecutor(connectionString))
            {
                if (createChangeLogTable)
                {
                    CreateChangeLogTable(sqlCmdExecutor);
                }
    
                var enumerable = changeScripts.ToList();
                infoTextWriter.WriteLine(enumerable.Any() ? "Applying change scripts...\n" : "No changes to apply.\n");

                foreach (var script in enumerable)
                {
                    RunScript(script, sqlCmdExecutor);
                }
            }
        }

        private void CreateChangeLogTable(SqlCmdExecutor sqlCmdExecutor)
        {
            infoTextWriter.WriteLine("Creating change log table");

            var output = new StringBuilder();

            try
            {
                if (!sqlCmdExecutor.ExecuteString(dbmsSyntax.CreateChangeLogTableSqlScript(changeLogTableName), output))
                {
                    throw new DbDeployException(string.Format("Create ChangeLog Table '{0}' failed.", changeLogTableName));
                }
            }
            finally
            {
                infoTextWriter.WriteLine(output);
            }
        }

        private void RunScript(ChangeScript script, SqlCmdExecutor sqlCmdExecutor)
        {
            schemaVersionManager.RecordScriptStatus(script, ScriptStatus.Started);

            infoTextWriter.WriteLine(script);
            infoTextWriter.WriteLine("----------------------------------------------------------");
            var output = new StringBuilder();

            var success = false;
            try
            {
                success = sqlCmdExecutor.ExecuteFile(script.FileInfo, output);
                if (!success)
                {
                    throw new DbDeployException(string.Format(CultureInfo.InvariantCulture, "Script '{0}' failed.", script));
                }
            }
            finally
            {
                infoTextWriter.WriteLine(output);
                schemaVersionManager.RecordScriptStatus(script, success ? ScriptStatus.Success : ScriptStatus.Failure,
                    output.ToString());
            }
        }
    }
}
