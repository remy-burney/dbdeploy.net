﻿using System.Collections.Generic;
using Dbdeploy.Core.Database;
using Dbdeploy.Core.Exceptions;

namespace Dbdeploy.Core
{
    /// <summary>
    /// Interface for provider to retrieve changes that have been applied to the database.
    /// </summary>
    public interface IAppliedChangesProvider
    {
        /// <summary>
        /// Gets the applied changes to the database.
        /// </summary>
        /// <returns>List of changes applied.</returns>
        IList<ChangeEntry> GetAppliedChanges();

        /// <summary>
        /// Verifies the change log table exists.
        /// </summary>
        /// <exception cref="ChangelogTableDoesNotExistException">Thrown when the change log table is not found.</exception>
        bool ChangeLogTableExists();
    }
}
