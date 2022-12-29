using System.IO;
using System.Management.Automation;
using Dbdeploy.Core;
using Dbdeploy.Core.Configuration;

namespace Dbdeploy.Powershell.Commands
{
    [Cmdlet(VerbsCommon.Push, "DbUpdate")]
    public class PushDbUpdate: DbUpdateBase
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var config = new DbDeployConfig
            {
                Dbms = DatabaseType,
                ConnectionString = ConnectionString,
                ChangeLogTableName = TableName,
                ScriptDirectory = new DirectoryInfo(deltasDirectory),
            };

            var deployer = new DbDeployer();
            deployer.Execute(config, new LambdaTextWriter(WriteVerbose));
        }
    }
}