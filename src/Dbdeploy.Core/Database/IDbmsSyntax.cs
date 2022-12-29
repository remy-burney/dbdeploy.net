namespace Dbdeploy.Core.Database
{
    /// <summary>
    /// Interface for Database Management System syntax.
    /// </summary>
    public interface IDbmsSyntax
    {
        /// <summary>
        /// Gets the syntax to get the current timestamp.
        /// </summary>
        /// <value>
        /// The current timestamp syntax.
        /// </value>
        string CurrentTimestamp { get; }

        /// <summary>
        /// Gets the syntax to get the current user.
        /// </summary>
        /// <value>
        /// The current user syntax.
        /// </value>
        string CurrentUser { get; }

        /// <summary>
        /// Gets the default schema for a table.
        /// </summary>
        /// <value>Default schema.</value>
        string DefaultSchema { get; }

        /// <summary>
        /// Gets the Change Log Table create script.
        /// </summary>
        /// <param name="tableName">Name of the change log table.</param>
        /// <returns>
        /// Script to create a Change Log table for the current DBMS.
        /// </returns>
        string CreateChangeLogTableSqlScript(string tableName);

        /// <summary>
        /// Gets the table name and schema.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>Table name and schema.</returns>
        TableInfo GetTableInfo(string tableName);

        /// <summary>
        /// Gets the syntax for checking if a table exists.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>SQL for checking if a table exists.</returns>
        string TableExists(string tableName);

        /// <summary>
        /// Returns the template file name, used by a template applier, for the current syntax
        /// </summary>
        /// <param name="templateQualifier">Template qualifier, like apply or undo</param>
        /// <returns>Template filename, including extention</returns>
        string GetTemplateFileNameFor(string templateQualifier);
    }
}