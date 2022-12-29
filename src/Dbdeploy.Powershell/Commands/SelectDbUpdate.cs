using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Dbdeploy.Core.Database;
using Dbdeploy.Core.Scripts;

namespace Dbdeploy.Powershell.Commands
{
    [Cmdlet(VerbsCommon.Select, "DbUpdate")]
    public class SelectDbUpdate : DbUpdateBase
    {
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var infoTextWriter = new LambdaTextWriter(WriteVerbose);

            List<ChangeScript> allChangeScripts = new DirectoryScanner(infoTextWriter, Encoding.UTF8)
                .GetChangeScriptsForDirectory(new DirectoryInfo(deltasDirectory));
            
            var repository = new ChangeScriptRepository(allChangeScripts);
            var changeScripts = repository.GetAvailableChangeScripts();

            DbmsFactory factory = new DbmsFactory(DatabaseType, ConnectionString);
            var queryExecuter = new QueryExecuter(factory);

            var schemaManager = new DatabaseSchemaVersionManager(queryExecuter, factory.CreateDbmsSyntax(), TableName);

            var appliedChanges = schemaManager.GetAppliedChanges();
            var notAppliedChangeScripts = changeScripts.Where(c => appliedChanges.All(a => a.ScriptNumber != c.ScriptNumber));

            var descriptionPrettyPrinter = new DescriptionPrettyPrinter();

            var objects = notAppliedChangeScripts
                .Select(script => new
                    {
                        Id = script.ScriptNumber,
                        Description = descriptionPrettyPrinter.Format(script.ScriptName),
                        File = script.FileInfo
                    });

            WriteObject(objects, true);
        }
    }
}