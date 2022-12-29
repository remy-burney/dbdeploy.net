using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using Dbdeploy.Core;
using Dbdeploy.Core.Configuration;
using Dbdeploy.Core.Exceptions;

namespace Dbdeploy.Console
{
    /// <summary>
    /// Manages all options for the command line.
    /// </summary>
    public static class OptionsManager
    {
        /// <summary>
        /// Prints the options usage.
        /// </summary>
        public static void PrintUsage()
        {
            var rootCommand = Initialize(null, new ConfigFileInfo());
            rootCommand.InvokeAsync("");
        }

        /// <summary>
        /// Parses the specified args into a deployments configuration.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>
        /// Configuration set.
        /// </returns>
        /// <exception cref="UsageException">Throws when unknown or invalid parameters are found.</exception>
        public static DbDeploymentsConfig ParseOptions(string[] args)
        {
            // Initialize configuration with a single deployment.
            var deploymentsConfig = new DbDeploymentsConfig();
            try
            {
                var configFile = new ConfigFileInfo();
                var config = new DbDeployConfig();
                var rootCommand = Initialize(config, configFile);
                deploymentsConfig.Deployments.Add(config);

                var result = rootCommand.Parse(args);
                
                if (result.UnmatchedTokens.Count > 0 && result.UnmatchedTokens.Any(s => !string.IsNullOrEmpty(s.Trim())))
                {
                    throw new UsageException("Unknown parameter(s): " + string.Join(", ", result.UnmatchedTokens));
                }

                // If a configuration file was specified in the command, use that instead of options.
                if (configFile.FileInfo != null)
                {
                    var configurationManager = new DbDeployConfigurationManager();
                    deploymentsConfig = configurationManager.ReadConfiguration(configFile.FileInfo.FullName);
                }
            }
            catch (Exception e)
            {
                throw new UsageException(e.Message, e);
            }

            return deploymentsConfig;
        }


        /// <summary>
        /// Initializes the specified config.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <param name="configFile">The config file to read if found.</param>
        /// <returns>
        /// Option set for the config.
        /// </returns>
        private static RootCommand Initialize(DbDeployConfig config, ConfigFileInfo configFile)
        {
            var optionD = new Option<string>(aliases: new[] { "--dbms", "-d" },
                description: "DBMS type ('mssql', 'mysql' or 'ora')");
            var optionC = new Option<string>(aliases: new[] { "--connectionstring", "-c" },
                description: "connection string for database')");
            var optionS = new Option<string>(aliases: new[] { "--scriptdirectory", "-s" },
                description: "directory containing change scripts (default: .)')");
            var optionT = new Option<string>(aliases: new[] { "--changelogtablename", "-t" },
                description: "name of change log table to use (default: ChangeLog)");
            var optionO = new Option<string>(aliases: new[] { "--outputfile", "-o" },
                description: "output file");
            var optionA = new Option<string>(aliases: new[] { "--autocreatechangelogtable", "-a" },
                description: "automatically creates the change log table if it does not exist (true or false).  Defaults to true.");
            var optionF = new Option<string>(aliases: new[] { "--forceupdate", "-f" },
                description: "forces previously failed scripts to be run again (true or false).  Defaults to false.");
            var optionU = new Option<string>(aliases: new[] { "--usesqlcmd", "-u" },
                description: "runs scripts in SQLCMD mode (true or false).  Defaults to false.");
            
            var optionL = new Option<string>(aliases: new[] { "--lastchangetoapply", "-l" }, description: "sets the last change to apply in the form of folder/scriptnumber (v1.0.0/4).");
            var optionE = new Option<string>(aliases: new[] { "--encoding", "-e" }, description: "encoding for input and output files (default: UTF-8)");
            var optionTemplate = new Option<string>(aliases: new[] { "--templatedirectory" }, description: "template directory");
            var optionDelimiter = new Option<string>(aliases: new[] { "--delimiter" }, description: "delimiter to separate sql statements");
            var optionDelimiterType = new Option<string>(aliases: new[] { "--delimitertype" }, description: "delimiter type to separate sql statements (row or normal)");
            var optionLineEnding = new Option<string>(aliases: new[] { "--lineending" }, description: "line ending to use when applying scripts direct to db (platform, cr, crlf, lf)");
            var optionConfig = new Option<string>(aliases: new[] { "--config" }, description: "configuration file to use for all settings.");

            RootCommand rootCommand = new RootCommand("Converts an image file from one format to another.")
            {
                optionD,
                optionC,
                optionS,
                optionO,
                optionT,
                optionA,
                optionF,
                optionU,
                optionL,
                optionE,
                optionTemplate,
                optionDelimiter,
                optionDelimiterType,
                optionLineEnding,
                optionConfig
            };
            rootCommand.SetHandler(s => config.Dbms = s, optionD);
            rootCommand.SetHandler(s => config.ConnectionString = StripQuotes(s), optionD);
            rootCommand.SetHandler(s => config.ScriptDirectory = new DirectoryInfo(StripQuotes(s)), optionS);
            rootCommand.SetHandler(s => config.ChangeLogTableName = StripQuotes(s),optionT);
            rootCommand.SetHandler(s => config.OutputFile = new FileInfo(StripQuotes(s)),optionO);
            rootCommand.SetHandler(s => config.AutoCreateChangeLogTable = s.ToLowerInvariant() != "false" ,optionA);
            rootCommand.SetHandler(s => config.ForceUpdate = s.ToLowerInvariant() == "true", optionF);
            rootCommand.SetHandler(s => config.UseSqlCmd = s.ToLowerInvariant() == "true", optionU);
                
            
            rootCommand.SetHandler(s => config.LastChangeToApply = !string.IsNullOrWhiteSpace(s) ? new UniqueChange(s) : null, optionL);
            rootCommand.SetHandler(s => config.Encoding = new OutputFileEncoding(StripQuotes(s)).AsEncoding(), optionE);
            rootCommand.SetHandler(s => config.TemplateDirectory = new DirectoryInfo(StripQuotes(s)), optionTemplate);
            rootCommand.SetHandler(s => config.Delimiter = s, optionDelimiter);
            rootCommand.SetHandler(s => config.DelimiterType = Parser.ParseDelimiterType(s), optionDelimiterType);
            rootCommand.SetHandler(s => config.LineEnding = Parser.ParseLineEnding(s), optionLineEnding);
            rootCommand.SetHandler(s => configFile.FileInfo = !string.IsNullOrWhiteSpace(s) ? new FileInfo(StripQuotes(s)) : null, optionConfig);
                
            return rootCommand;
        }

        /// <summary>
        /// Strips the quotes from around the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Value with quotes removed.</returns>
        private static string StripQuotes(string value)
        {
            if (value != null)
            {
                value = value.Trim();

                if ((value.StartsWith("\"", StringComparison.OrdinalIgnoreCase) &&
                     value.EndsWith("\"", StringComparison.OrdinalIgnoreCase))
                    || (value.StartsWith("'", StringComparison.OrdinalIgnoreCase) &&
                        value.EndsWith("'", StringComparison.OrdinalIgnoreCase)))
                {
                    return value.Substring(1, value.Length - 2);
                }
            }

            return value;
        }
    }
}