﻿using System.IO;
using Net.Sf.Dbdeploy.Database;

namespace Net.Sf.Dbdeploy.Appliers
{
    public class UndoTemplateBasedApplier : TemplateBasedApplier
    {
        public UndoTemplateBasedApplier(
            TextWriter writer,
            string syntax,
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