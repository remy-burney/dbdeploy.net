using System.IO;
using Dbdeploy.Core.Database;

namespace Dbdeploy.Core.Appliers
{
    public class UndoTemplateBasedApplier : TemplateBasedApplier
    {
        public UndoTemplateBasedApplier(
            TextWriter writer,
            IDbmsSyntax syntax,
            string changeLogTableName,
            string delimiter,
            IDelimiterType delimiterType,
            DirectoryInfo templateDirectory)
            : base(writer, syntax, changeLogTableName, delimiter, delimiterType, templateDirectory)
        {
        }

        protected override string GetTemplateQualifier()
        {
            return "undo";
        }
    }
}
