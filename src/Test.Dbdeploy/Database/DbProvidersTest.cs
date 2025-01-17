using Dbdeploy.Core.Database;
using NUnit.Framework;

namespace Test.Dbdeploy.Database
{
    [TestFixture]
    public class DbProvidersTest
    {
        private DbProviders providers;

        [SetUp]
        public void Setup()
        {
            providers = new DbProviderFile().LoadProviders();
        }
     
        [Test]
        public void TestCanLoadMsSqlProvider()
        {
            Assert.IsNotNull(providers.GetProvider("mssql"));
        }
        [Test]
        public void TestCanLoadOracleProvider()
        {
            Assert.IsNotNull(providers.GetProvider("ora"));
        }

        [Test]
        public void TestCanLoadMySQLProvider()
        {
            Assert.IsNotNull(providers.GetProvider("mysql"));
        }
    }
}