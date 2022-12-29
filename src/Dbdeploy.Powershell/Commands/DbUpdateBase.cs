using System;
using System.IO;
using System.Management.Automation;

namespace Dbdeploy.Powershell.Commands
{
    using System.Linq;

    using Net.Sf.Dbdeploy.Configuration;

    public class DbUpdateBase : PSCmdlet
    {
        private DbDeployConfig config;

        protected string deltasDirectory;
        private string tableName = DbDeployDefaults.ChangeLogTableName;

        private string databaseType = DbDeployDefaults.Dbms;

        [Parameter(Mandatory = false)]
        public string ConfigurationFile { get; set; }

        [Parameter(Mandatory = true, Position = 0)]
        public string DeltasDirectory { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Defaults to mssql")]
        public string DatabaseType
        {
            get { return databaseType; }
            set { databaseType = value; }
        }

        [Parameter(Mandatory = false)]
        public string ConnectionString { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Changelog table name. Defaults to ChangeLog")]
        public string TableName
        {
            get { return tableName; }
            set { tableName = value; }
        }

        [Parameter(Mandatory = false, HelpMessage = "Sets if the Changelog table should be automatically created. Defaults to true")]
        public bool AutoCreateChangeLogTable { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Sets if previously failed scripts should be retried. Defaults to false")]
        public bool ForceUpdate { get; set; }

        [Parameter(Mandatory = false, HelpMessage = "Sets if SQLCMD mode should be used. Defaults to false")]
        public bool UseSqlCmd { get; set; }

        protected override void ProcessRecord()
        {
            var configurationFile = ToAbsolutePath(ConfigurationFile);
            deltasDirectory = ToAbsolutePath(DeltasDirectory);

            if (!string.IsNullOrEmpty(configurationFile) && File.Exists(configurationFile))
            {
                var configurationManager = new DbDeployConfigurationManager();
                config = configurationManager.ReadConfiguration(configurationFile).Deployments.FirstOrDefault();
                if (config == null) throw new NullReferenceException("Configuration file not found or empty");
                if (string.IsNullOrEmpty(DatabaseType) || DatabaseType == DbDeployDefaults.Dbms)
                    DatabaseType = config.Dbms;

                if (string.IsNullOrEmpty(ConnectionString))
                    ConnectionString = config.ConnectionString;

                if (string.IsNullOrEmpty(TableName) || TableName == DbDeployDefaults.ChangeLogTableName)
                    TableName = config.ChangeLogTableName;
            }

            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new InvalidDataException(
                    "Missing connection string. It must either be in the config file or passed as a parameter");
            }
        }

        protected string ToAbsolutePath(string deltasDirectory)
        {
            if (string.IsNullOrEmpty(deltasDirectory))
                return null;

            if (!Path.IsPathRooted(deltasDirectory))
            {
                deltasDirectory = Path.Combine(SessionState.Path.CurrentFileSystemLocation.Path, deltasDirectory);
            }

            return deltasDirectory;
        }
    }
}