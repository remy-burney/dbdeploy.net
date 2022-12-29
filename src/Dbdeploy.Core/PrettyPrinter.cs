using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dbdeploy.Core
{
    public class PrettyPrinter
    {
        public string Format(IEnumerable<UniqueChange> changes)
        {
            var changeList = changes.ToList();
            if (!changeList.Any())
            {
                return "  (none)";
            }

            StringBuilder builder = new StringBuilder();

            int? lastRangeStart = null;
            int? lastNumber;

            var changesByFolder = changeList.GroupBy(c => c.Folder).ToList();
            bool isFirst;
            foreach (var group in changesByFolder)
            {
                AppendFolder(builder, group.Key);

                isFirst = true;
                lastNumber = null;
                foreach (var thisNumber in group.Select(c => c.ScriptNumber))
                {
                    if (!lastNumber.HasValue)
                    {
                        // first in loop
                        lastNumber = thisNumber;
                        lastRangeStart = thisNumber;
                    }
                    else if (thisNumber == lastNumber + 1)
                    {
                        // continuation of current range
                        lastNumber = thisNumber;
                    }
                    else
                    {
                        // doesn't fit into last range - so output the old range and
                        // start a new one
                        AppendRange(builder, lastRangeStart.Value, lastNumber.Value, isFirst);
                        isFirst = false;
                        lastNumber = thisNumber;
                        lastRangeStart = thisNumber;
                    }
                }

                AppendRange(builder, lastRangeStart.Value, lastNumber.Value, isFirst);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Appends the specified scripts folder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="folder">The last folder.</param>
        private static void AppendFolder(StringBuilder builder, string folder)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.AppendFormat("  {0}\t", folder);
        }

        private void AppendRange(StringBuilder builder, int lastRangeStart, int lastNumber, bool isFirst)
        {
            if (lastRangeStart == lastNumber)
            {
                AppendWithPossibleComma(builder, lastNumber, isFirst);
            }
            else if (lastRangeStart + 1 == lastNumber)
            {
                AppendWithPossibleComma(builder, lastRangeStart, isFirst);
                AppendWithPossibleComma(builder, lastNumber, false);
            }
            else
            {
                AppendWithPossibleComma(builder, lastRangeStart + ".." + lastNumber, isFirst);
            }
        }

        private void AppendWithPossibleComma(StringBuilder builder, Object o, bool isFirst)
        {
            if (!isFirst)
            {
                builder.Append(", ");
            }

            builder.Append(o);
        }
    }
}