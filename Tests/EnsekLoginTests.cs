using NUnit.Framework;
using ApiTestFramework.Core;

namespace ApiTestFramework.Tests
{
    [TestFixture]
    [Description("ENSEK API Login endpoint tests")]
    public class EnsekLoginTests : BaseEnsekTest
    {

        [Test, Order(1)]
        [Description("Test ENSEK login endpoint with valid credentials")]
        public async Task Test_LoginEndpoint_ValidCredentials()
        {
            TestContext.WriteLine("=== TESTING LOGIN ENDPOINT - VALID CREDENTIALS ===");
            TestContext.WriteLine("Attempting to login...");
            
            var loginRequest = new LoginRequest
            {
                Username = _username,
                Password = _password
            };

            var loginApiResponse = await _ensekClient.LoginAsync(loginRequest);
            
            TestContext.WriteLine($"HTTP Status Code: {loginApiResponse.StatusCode}");
            
            if (loginApiResponse.IsSuccess && loginApiResponse.LoginData != null)
            {
                TestContext.WriteLine("✅ Login successful");
                
                // Check if we received an access token
                if (!string.IsNullOrEmpty(loginApiResponse.LoginData.AccessToken))
                {
                    TestContext.WriteLine($"✅ Access token received: {loginApiResponse.LoginData.AccessToken.Length} characters");
                }
                
                // Check if we received a message
                if (!string.IsNullOrEmpty(loginApiResponse.LoginData.Message))
                {
                    TestContext.WriteLine($"✅ Message: {loginApiResponse.LoginData.Message}");
                }
                
                Assert.Pass($"Login endpoint is working - HTTP {loginApiResponse.StatusCode} with access_token: {(!string.IsNullOrEmpty(loginApiResponse.LoginData.AccessToken) ? "YES" : "NO")}, message: {loginApiResponse.LoginData.Message ?? "NONE"}");
            }
            else
            {
                TestContext.WriteLine("❌ Login failed");
                TestContext.WriteLine($"HTTP Status Code: {loginApiResponse.StatusCode}");
                TestContext.WriteLine($"Error Message: {loginApiResponse.ErrorMessage ?? "No error message"}");
                
                if (!string.IsNullOrEmpty(loginApiResponse.RawResponse))
                {
                    TestContext.WriteLine($"Raw Response: {loginApiResponse.RawResponse}");
                }
                
                // Check specifically for 401 Unauthorized
                if (loginApiResponse.IsUnauthorized)
                {
                    Assert.Fail($"Valid credentials test failed - received 401 Unauthorized. Error: {loginApiResponse.ErrorMessage}. Check if credentials in .env are correct.");
                }
                else
                {
                    Assert.Fail($"Login failed with unexpected status code {loginApiResponse.StatusCode}. Error: {loginApiResponse.ErrorMessage}");
                }
            }
        }

        [Test, Order(2)]
        [Description("Test error handling with invalid credentials")]
        public async Task Test_LoginEndpoint_InvalidCredentials()
        {
            TestContext.WriteLine("=== TESTING LOGIN ENDPOINT - INVALID CREDENTIALS ===");
            
            var invalidLoginRequest = new LoginRequest
            {
                Username = "invalid_user",
                Password = "invalid_password"
            };

            var loginApiResponse = await _ensekClient.LoginAsync(invalidLoginRequest);
            
            TestContext.WriteLine($"Error handling test - HTTP Status: {loginApiResponse.StatusCode}");
            TestContext.WriteLine($"Error Message: {loginApiResponse.ErrorMessage ?? "No error message"}");
            
            if (loginApiResponse.IsUnauthorized)
            {
                TestContext.WriteLine("✅ Correctly received 401 Unauthorized for invalid credentials");
                Assert.Pass($"Error handling verified - 401 Unauthorized returned for invalid credentials: {loginApiResponse.ErrorMessage}");
            }
            else if (!loginApiResponse.IsSuccess)
            {
                TestContext.WriteLine($"✅ Login failed as expected with HTTP {loginApiResponse.StatusCode}");
                Assert.Pass($"Error handling verified - HTTP {loginApiResponse.StatusCode} returned for invalid credentials: {loginApiResponse.ErrorMessage}");
            }
            else
            {
                TestContext.WriteLine("⚠️ Unexpected: Invalid credentials resulted in successful login");
                Assert.Fail("Error handling test failed- login success with invalid credentials");
            }
        }
    }
}
