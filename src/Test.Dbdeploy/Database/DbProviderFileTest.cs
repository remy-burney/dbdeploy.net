using System.IO;
using Dbdeploy.Core.Database;
using NUnit.Framework;

namespace Test.Dbdeploy.Database
{
    [TestFixture]
    public class DbProviderFileTest
    {
        [Test]
        public void ShouldLoadFromAssemblyResourceIfProviderPathIsNull()
        {
            DbProviderFile providerFile = new DbProviderFile();
            Assert.IsNull(providerFile.Path);
            Assert.IsNotNull(providerFile.LoadProviders());
        }
        
        [Test]
        public void ShouldLoadFromAssemblyResourceIfProviderPathIsUntouched()
        {
            DbProviderFile providerFile = new DbProviderFile();
            Assert.IsNotNull(providerFile.LoadProviders());
        }

        [Test, ExpectedException(typeof (FileNotFoundException))]
        public void ShouldNotInitializeInvalidPath()
        {
            DbProviderFile providerFile = new DbProviderFile();
            providerFile.Path = "bogus path";
            providerFile.LoadProviders();
        }
    }
}