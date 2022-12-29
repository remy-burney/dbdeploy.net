using System;
using System.IO;
using Dbdeploy.Core;
using Dbdeploy.Core.Configuration;
using Dbdeploy.Core.Database;
using Dbdeploy.Core.Exceptions;
using NAnt.Core;
using NAnt.Core.Attributes;

namespace Dbdeploy.NAnt
{
    [TaskName("dbdeploy")]
    public class NAntTask : Task
    {
        private readonly DbDeployConfig config;

        public NAntTask()
        {
            config = new DbDeployConfig();
        }
        
        [TaskAttribute("dbType", Required = true)]
        public string DbType
        {
            set { config.Dbms = value; }
        }
        
        [TaskAttribute("dbConnection", Required = true)]
        public string DbConnection
        {
            set { config.ConnectionString = value; }
        }
        
        [TaskAttribute("dir", Required = true)]
        public DirectoryInfo Dir
        {
            get { return config.ScriptDirectory; }
            set { config.ScriptDirectory = value; }
        }
        
        [TaskAttribute("outputFile")]
        public FileInfo OutputFile
        {
            get { return config.OutputFile; }
            set { config.OutputFile = value; }
        }
        
        [TaskAttribute("encoding")]
        public string OutputEncoding
        {
            get { return config.Encoding.EncodingName; }
            set { config.Encoding = new OutputFileEncoding(value).AsEncoding(); }
        }
        
        [TaskAttribute("undoOutputFile")]
        public FileInfo UndoOutputFile
        {
            get { return config.UndoOutputFile; }
            set { config.UndoOutputFile = value; }
        }
        
        [TaskAttribute("templateDir")]
        public DirectoryInfo TemplateDir
        {
            get { return config.TemplateDirectory; }
            set { config.TemplateDirectory = value; }
        }
        
        [TaskAttribute("lastChangeToApply")]
        public string LastChangeToApply
        {
            get { return config.LastChangeToApply != null ? config.LastChangeToApply.UniqueKey : string.Empty; }
            set { config.LastChangeToApply = string.IsNullOrWhiteSpace(value) ? null : new UniqueChange(value); }
        }
        
        [TaskAttribute("changeLogTable")]
        public string ChangeLogTable
        {
            get { return config.ChangeLogTableName; }
            set { config.ChangeLogTableName = value; }
        }

        /// <summary>
        /// Gets or sets if the change log table should be automatically created if it does not exist.
        /// </summary>
        /// <value>
        /// The auto create change table.
        /// </value>
        [TaskAttribute("autoCreateChangeLogTable")]
        public bool AutoCreateChangeLogTable
        {
            get { return config.AutoCreateChangeLogTable; }
            set { config.AutoCreateChangeLogTable = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to retry and previously failed scripts.
        /// </summary>
        /// <value>
        ///   <c>true</c> if force update; otherwise, <c>false</c>.
        /// </value>
        [TaskAttribute("forceUpdate")]
        public bool ForceUpdate
        {
            get { return config.ForceUpdate; }
            set { config.ForceUpdate = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use SQLCMD mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if use SQL CMD; otherwise, <c>false</c>.
        /// </value>
        [TaskAttribute("useSqlCmd")]
        public bool UseSqlCmd
        {
            get { return config.UseSqlCmd; }
            set { config.UseSqlCmd = value; }
        }
        
        [TaskAttribute("delimiter")]
        public string Delimiter
        {
            get { return config.Delimiter; }
            set { config.Delimiter = value; }
        }
        
        [TaskAttribute("delimiterType")]
        public string DelimiterType
        {
            get { return config.DelimiterType.GetType().Name; }
            set { config.DelimiterType = DelimiterTypeFactory.Create(value); }
        }

        protected override void ExecuteTask()
        {
            try
            {
                var deployer = new DbDeployer();
                deployer.Execute(config, Console.Out);
            }
            catch (UsageException ex)
            {
                Console.Error.WriteLine(ex.Message);

                PrintUsage();
            }
            catch (DbDeployException ex)
            {
                Console.Error.WriteLine(ex.Message);

                throw new BuildException(ex.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to apply changes: " + ex);
                Console.Error.WriteLine("Stack Trace:");
                Console.Error.Write(ex.StackTrace);

                throw new BuildException(ex.Message);
            }
        }

        public void PrintUsage()
        {
            string message = "\n\nDbdeploy Ant Task Usage"
                + "\n======================="
                + "\n\n\t<dbdeploy"
                + "\n\t\tdbType=\"[DATABASE TYPE - mssql/mysql/ora]\" *"
                + "\n\t\tdbConnection=\"[DATABASE CONNECTION STRING]\" *"
                + "\n\t\ttemplatedir=\"[DIRECTORY FOR DBMS TEMPLATE SCRIPTS, IF NOT USING BUILT-IN]\""
                + "\n\t\tdir=\"[YOUR SCRIPT FOLDER]\" *"
                + "\n\t\tencoding=\"[CHARSET OF IN- AND OUTPUT SQL SCRIPTS - default UTF-8]\""
                + "\n\t\toutputfile=\"[OUTPUT SCRIPT PATH + NAME]\""
                + "\n\t\tlastChangeToApply=\"[NUMBER OF THE LAST SCRIPT TO APPLY]\""
                + "\n\t\tundoOutputfile=\"[UNDO SCRIPT PATH + NAME]\""
                + "\n\t\tchangeLogTableName=\"[CHANGE LOG TABLE NAME]\""
                + "\n\t\tdelimiter=\"[STATEMENT DELIMITER - default ;]\""
                + "\n\t\tdelimitertype=\"[STATEMENT DELIMITER TYPE - row or normal, default normal]\""
                + "\n\t/>"
                + "\n\n* - Indicates mandatory parameter";

            Console.Out.WriteLine(message);
        }
    }
}