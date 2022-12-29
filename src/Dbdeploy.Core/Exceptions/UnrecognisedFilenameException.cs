using System;

namespace Dbdeploy.Core.Exceptions
{
    public class UnrecognisedFilenameException : DbDeployException
    {
        public UnrecognisedFilenameException(string message, Exception inner) 
            : base(message, inner)
        {
        }

        public UnrecognisedFilenameException(string message) 
            : base(message)
        {
        }

        public UnrecognisedFilenameException(Exception inner)
            : base(inner)
        {
        }
    }
}