using System.Data.Common;

namespace Test.Dbdeploy
{
    public class DummyDbException : DbException
    {
        public DummyDbException()
            : base("dummy exception")
        {
        }
    }
}
