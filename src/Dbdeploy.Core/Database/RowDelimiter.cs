﻿using System;

namespace Dbdeploy.Core.Database
{
    /// <summary>
    /// Delimiter must be on a line all by itself
    /// </summary>
    public class RowDelimiter : IDelimiterType
    {
        public bool Matches(string line, string delimiter)
        {
            return line != null && line.Equals(delimiter, StringComparison.OrdinalIgnoreCase);
        }
    }
}
