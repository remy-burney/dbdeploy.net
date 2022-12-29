namespace Net.Sf.Dbdeploy.Database
{
    using System;
    using System.Data;
    using System.Reflection;

    public class DbmsFactory
    {
        private readonly string dbms;

        private readonly string connectionString;

        private readonly DbProviders providers;

        public DbmsFactory(string dbms, string connectionString)
        {
            this.dbms = dbms;
            this.connectionString = connectionString;

            providers = new DbProviderFile().LoadProviders();
        }

        public virtual IDbmsSyntax CreateDbmsSyntax()
        {
            switch (dbms)
            {
                case "ora":
                    return new OracleDbmsSyntax();
                case "mssql":
                    return new MsSqlDbmsSyntax();
                case "mysql":
                    return new MySqlDbmsSyntax();
                default:
                    throw new ArgumentException("Supported dbms: ora, mssql, mysql");
            }
        }

        public virtual IDbConnection CreateConnection()
        {
            DatabaseProvider provider = providers.GetProvider(dbms);

            Assembly assembly = Assembly.Load(provider.AssemblyName);
            Type type = assembly.GetType(provider.ConnectionClass);
            if (type == null)
                throw new NullReferenceException($"Cannot get type {provider.ConnectionClass} for DbConnection");
            return (IDbConnection)Activator.CreateInstance(type, connectionString);
        }
    }
}