using System;

namespace Dbdeploy.Core.Exceptions
{
    public class DbDeployException : Exception
    {
        public DbDeployException(string message)
            : base(message)
        {
        }

        public DbDeployException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public DbDeployException(Exception inner)
            : base(inner.Message, inner)
        {
        }
    }
}