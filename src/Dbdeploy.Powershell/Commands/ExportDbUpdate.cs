using System.IO;
using System.Management.Automation;
using Net.Sf.Dbdeploy;

namespace Dbdeploy.Powershell.Commands
{
    using Net.Sf.Dbdeploy.Configuration;

    [Cmdlet(VerbsData.Export, "DbUpdate")]
    public class ExportDbUpdate : DbUpdateBase
    {
        [Parameter(Mandatory = true)]
        public string OutputFile { get; set; }

        [Parameter]
        public string UndoOutputFile { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (string.IsNullOrEmpty(OutputFile))
            {
                WriteError(new ErrorRecord(new PSInvalidOperationException(
                    "Missing a file for output"),
                    "NoOutputFile",
                    ErrorCategory.MetadataError,
                    null));

                return;
            }

            var config = new DbDeployConfig
                             {
                                 Dbms = DatabaseType,
                                 ConnectionString = ConnectionString,
                                 ChangeLogTableName = TableName,
                                 ScriptDirectory = new DirectoryInfo(deltasDirectory),
                                 AutoCreateChangeLogTable = AutoCreateChangeLogTable,
                                 ForceUpdate = ForceUpdate,
                                 UseSqlCmd = UseSqlCmd,
                                 OutputFile = new FileInfo(ToAbsolutePath(OutputFile))
                             };

            if (!string.IsNullOrEmpty(UndoOutputFile))
            {
                config.OutputFile = new FileInfo(ToAbsolutePath(UndoOutputFile));
            }

            var deployer = new DbDeployer();
            deployer.Execute(config, new LambdaTextWriter(WriteVerbose));
        }
    }
}