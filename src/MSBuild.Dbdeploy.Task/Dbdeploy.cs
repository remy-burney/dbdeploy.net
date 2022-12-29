using Dbdeploy;
using Dbdeploy.Core;
using Dbdeploy.Core.Configuration;
using Dbdeploy.Core.Database;
using Dbdeploy.Core.Exceptions;

namespace MSBuild.Dbdeploy.Task
{
    using System;
    using System.IO;

    using Microsoft.Build.Framework;

    public class Dbdeploy : ITask
    {
        private readonly DbDeployConfig config;
                        
        public Dbdeploy()
        {
            config = new DbDeployConfig();
        }

        [Required]
        public string DbType
        {
            set { config.Dbms = value; }
        }

        [Required]
        public string DbConnection
        {
            set { config.ConnectionString = value; }
        }

        [Required]
        public string Dir
        {
            get { return config.ScriptDirectory.FullName; }
            set { config.ScriptDirectory = new DirectoryInfo(value); }
        }

        public string OutputFile
        {
            get { return config.OutputFile.FullName; }
            set { config.OutputFile = new FileInfo(value); }
        }

        public string Encoding
        {
            get { return config.Encoding.EncodingName; }
            set { config.Encoding = new OutputFileEncoding(value).AsEncoding(); }
        }

        public string UndoOutputFile
        {
            get { return config.UndoOutputFile.FullName; }
            set { config.UndoOutputFile = new FileInfo(value); }
        }

        public string TemplateDir
        {
            get { return config.TemplateDirectory.FullName; }
            set { config.TemplateDirectory = new DirectoryInfo(value); }
        }

        public string LastChangeToApply
        {
            get { return config.LastChangeToApply != null ? config.LastChangeToApply.ToString() : string.Empty; }
            set { config.LastChangeToApply = string.IsNullOrWhiteSpace(value) ? null : new UniqueChange(value); }
        }

        public string TableName
        {
            get { return config.ChangeLogTableName; }
            set { config.ChangeLogTableName = value; }
        }

        public bool AutoCreateChangeLogTable
        {
            get { return config.AutoCreateChangeLogTable; }
            set { config.AutoCreateChangeLogTable = value; }
        }

        public bool UseSqlCmd
        {
            get { return config.UseSqlCmd; }
            set { config.UseSqlCmd = value; }
        }

        public string Delimiter
        {
            get { return config.Delimiter; }
            set { config.Delimiter = value; }
        }

        public string DelimiterType
        {
            get { return config.DelimiterType.GetType().Name; }
            set { config.DelimiterType = DelimiterTypeFactory.Create(value); }
        }

        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }

        public bool Execute()
        {
            try
            {
                var deployer = new DbDeployer();
                deployer.Execute(config, Console.Out);

                return true;
            }
            catch (UsageException ex)
            {
                Console.Error.WriteLine(ex.Message);

                PrintUsage();
            }
            catch (DbDeployException ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to apply changes: " + ex.Message);
                Console.Error.WriteLine("Stack Trace:");
                Console.Error.Write(ex.StackTrace);
            }

            return false;
        }

        public void PrintUsage()
        {
            string message = Environment.NewLine 
                + Environment.NewLine + "Dbdeploy MSBuild Task Usage"
                + Environment.NewLine + "======================="
                + Environment.NewLine 
                + Environment.NewLine + "\t<Dbdeploy"
                + Environment.NewLine + "\t\tDbConnection=\"[DATABASE CONECTIONSTRING]\" *"
                + Environment.NewLine + "\t\tdbms=\"[YOUR DBMS]\" *"
                + Environment.NewLine + "\t\ttemplatedir=\"[DIRECTORY FOR DBMS TEMPLATE SCRIPTS, IF NOT USING BUILT-IN]\""
                + Environment.NewLine + "\t\tdir=\"[YOUR SCRIPT FOLDER]\" *"
                + Environment.NewLine + "\t\toutputfile=\"[OUTPUT SCRIPT PATH + NAME]\""
                + Environment.NewLine + "\t\tlastChangeToApply=\"[NUMBER OF THE LAST SCRIPT TO APPLY]\""
                + Environment.NewLine + "\t\tundoOutputfile=\"[UNDO SCRIPT PATH + NAME]\""
                + Environment.NewLine + "\t\tchangeLogTableName=\"[CHANGE LOG TABLE NAME - default ChangeLog]\""
                + Environment.NewLine + "\t\tdelimiter=\"[STATEMENT DELIMITER - default ;]\""
                + Environment.NewLine + "\t\tdelimitertype=\"[STATEMENT DELIMITER TYPE - row or normal, default normal]\""
                + Environment.NewLine + "\t/>"
                + Environment.NewLine 
                + Environment.NewLine + "* - Indicates mandatory parameter";

            Console.Out.WriteLine(message);
        }
    }
}