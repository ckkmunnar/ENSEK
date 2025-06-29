using NUnit.Framework;
using ApiTestFramework.Core;

namespace ApiTestFramework.Tests
{
    [TestFixture]
    [Description("ENSEK API Reset endpoint and workflow tests")]
    public class EnsekResetTests : BaseEnsekTest
    {
        [Test, Order(1)]
        [Description("Login to get Bearer token for reset endpoint tests")]
        public async Task Test_Login_GetBearerToken()
        {
            var success = await LoginAndSetBearerTokenAsync();
            
            if (!success)
            {
                Assert.Fail("Login failed - check credentials and API availability");
            }
            
            Assert.That(_bearerToken, Is.Not.Null.And.Not.Empty, "Bearer token should not be null or empty");
        }
        [Test, Order(2)]
        [Description("Test reset endpoint with Bearer token authentication")]
        public async Task Test_ResetEndpoint()
        {
            TestContext.WriteLine("=== TESTING RESET ENDPOINT WITH BEARER TOKEN ===");
            
            // Ensure we have a Bearer token - login if needed
            await EnsureBearerTokenAvailableAsync();

            var success = await ResetTestDataAsync();
            
            if (success)
            {
                Assert.Pass("Reset operation completed successfully");
            }
            else
            {
                Assert.Fail("Reset operation failed");
            }
        }
    }
}
