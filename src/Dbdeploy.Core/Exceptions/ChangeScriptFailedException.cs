using System;
using System.Data.Common;
using Dbdeploy.Core.Scripts;

namespace Dbdeploy.Core.Exceptions
{
    public class ChangeScriptFailedException : DbDeployException
    {
        private readonly ChangeScript script;

        private readonly int statement;

        private readonly string executedSql;

        public ChangeScriptFailedException(DbException cause, ChangeScript script, int statement, string executedSql)
            : base(cause)
	    {
            this.script = script;
            this.statement = statement;
            this.executedSql = executedSql;
	    }

        public ChangeScript Script
        {
            get { return script;}
        }

        public string ExecutedSql
        {
            get { return executedSql;}
        }

        public int Statement
        {
            get { return statement;}
        }

        public override string  Message
        {
	        get
            {
                return "Change script " + script + " failed while executing statement " + statement + ":" + Environment.NewLine
                  + executedSql + Environment.NewLine 
                  + " -> " + InnerException.Message;
	        }
        }
    }
}

