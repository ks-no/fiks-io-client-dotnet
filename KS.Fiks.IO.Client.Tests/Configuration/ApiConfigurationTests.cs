using KS.Fiks.IO.Client.Configuration;
using Xunit;

namespace KS.Fiks.IO.Client.Tests.Configuration
{
    public class ApiConfigurationTests
    {
        [Fact]
        public void WorksWithEmptyConstructor()
        {
            var result = new ApiConfiguration();
        }
    }
}