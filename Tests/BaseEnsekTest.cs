using NUnit.Framework;
using ApiTestFramework.Core;
using ApiTestFramework.Utilities;
using Microsoft.Extensions.Logging;

namespace ApiTestFramework.Tests
{
    /// <summary>
    /// Base class for ENSEK API tests providing common login and reset functionality
    /// </summary>
    public abstract class BaseEnsekTest
    {
        protected EnsekApiClient _ensekClient = null!;
        protected ILogger _logger = null!;
        protected ILoggerFactory _loggerFactory = null!;
        protected string _baseUrl = string.Empty;
        protected string _username = string.Empty;
        protected string _password = string.Empty;
        protected string? _bearerToken = null;

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            // Load environment variables from .env file
            EnvironmentLoader.LoadDotEnvFile();

            // Setup basic logging for API client only
            _loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            _logger = _loggerFactory.CreateLogger(GetType());

            TestContext.WriteLine($"{GetType().Name} Tests Started");

            // Get configuration from environment variables
            _baseUrl = EnvironmentLoader.GetEnvironmentVariable("ENSEK_BASE_URL", "https://qacandidatetest.ensek.io");
            _username = EnvironmentLoader.GetEnvironmentVariable("ENSEK_USERNAME", "test");
            _password = EnvironmentLoader.GetEnvironmentVariable("ENSEK_PASSWORD", "testing");

            // Create API client (no default auth token)
            _ensekClient = new EnsekApiClient(_baseUrl, 
                _loggerFactory.CreateLogger<EnsekApiClient>());

            TestContext.WriteLine($"{GetType().Name} test setup complete");
        }

        /// <summary>
        /// Common login method to get Bearer token for authenticated endpoints
        /// </summary>
        /// <returns>True if login successful, false otherwise</returns>
        protected async Task<bool> LoginAndSetBearerTokenAsync()
        {
            TestContext.WriteLine("=== GETTING BEARER TOKEN FROM LOGIN ===");
            TestContext.WriteLine("Attempting to login to get Bearer token...");

            var loginRequest = new LoginRequest(_username, _password);
            var loginApiResponse = await _ensekClient.LoginAsync(loginRequest);

            TestContext.WriteLine($"Login HTTP Status Code: {loginApiResponse.StatusCode}");

            if (loginApiResponse.IsSuccess && loginApiResponse.LoginData?.AccessToken != null)
            {
                _bearerToken = loginApiResponse.LoginData.AccessToken;
                _ensekClient.SetBearerToken(_bearerToken);
                
                TestContext.WriteLine("✅ Login successful - Bearer token captured");
                TestContext.WriteLine($"✅ Token length: {_bearerToken.Length} characters");
                
                return true;
            }
            else if (loginApiResponse.IsUnauthorized)
            {
                TestContext.WriteLine("❌ Login failed - 401 Unauthorized");
                TestContext.WriteLine("Check your .env file credentials");
                TestContext.WriteLine($"Error: {loginApiResponse.ErrorMessage}");
                return false;
            }
            else
            {
                TestContext.WriteLine($"❌ Login failed - HTTP {loginApiResponse.StatusCode}");
                TestContext.WriteLine($"Error: {loginApiResponse.ErrorMessage}");
                return false;
            }
        }

        /// <summary>
        /// Common reset method to reset test data after tests
        /// </summary>
        /// <returns>True if reset successful, false otherwise</returns>
        protected async Task<bool> ResetTestDataAsync()
        {
            TestContext.WriteLine("=== RESETTING TEST DATA ===");
            
            // Ensure we have a Bearer token
            if (string.IsNullOrEmpty(_bearerToken))
            {
                TestContext.WriteLine("⚠️ No Bearer token available for reset. Attempting to login first...");
                var loginSuccess = await LoginAndSetBearerTokenAsync();
                if (!loginSuccess)
                {
                    TestContext.WriteLine("❌ Cannot reset - login failed");
                    return false;
                }
            }

            TestContext.WriteLine("Resetting test data with Bearer token...");

            var resetApiResponse = await _ensekClient.ResetTestDataAsync();

            TestContext.WriteLine($"HTTP Status Code: {resetApiResponse.StatusCode}");

            if (resetApiResponse.IsSuccess)
            {
                TestContext.WriteLine("✅ Reset successful");
                TestContext.WriteLine($"✅ Description: {resetApiResponse.ResetData?.Description ?? "No description provided"}");
                return true;
            }
            else if (resetApiResponse.IsUnauthorized)
            {
                TestContext.WriteLine("❌ Reset failed - 401 Unauthorized");
                TestContext.WriteLine("Bearer token might be invalid or expired");
                TestContext.WriteLine($"Error: {resetApiResponse.ErrorMessage}");
                return false;
            }
            else
            {
                TestContext.WriteLine($"❌ Reset failed - HTTP {resetApiResponse.StatusCode}");
                TestContext.WriteLine($"Error: {resetApiResponse.ErrorMessage}");
                return false;
            }
        }

        /// <summary>
        /// Ensures Bearer token is available for tests that need authentication.
        /// Automatically performs login if no valid token is available.
        /// </summary>
        protected async Task EnsureBearerTokenAvailableAsync()
        {
            if (string.IsNullOrEmpty(_bearerToken))
            {
                TestContext.WriteLine("Bearer token not available - performing login...");
                var loginSuccess = await LoginAndSetBearerTokenAsync();
                if (!loginSuccess)
                {
                    Assert.Fail("Failed to obtain Bearer token - login failed");
                }
                TestContext.WriteLine($"✅ Login successful - Bearer token obtained");
            }
            else
            {
                TestContext.WriteLine("✅ Bearer token already available");
            }
        }

        /// <summary>
        /// Synchronous version of EnsureBearerTokenAvailable for backwards compatibility.
        /// This version will fail if no token is available since it cannot perform async login.
        /// Use EnsureBearerTokenAvailableAsync() instead for automatic login.
        /// </summary>
        [Obsolete("Use EnsureBearerTokenAvailableAsync() instead for automatic login capability")]
        protected void EnsureBearerTokenAvailable()
        {
            if (string.IsNullOrEmpty(_bearerToken))
            {
                Assert.Fail("Bearer token is not available. Login test must run first and succeed, or use EnsureBearerTokenAvailableAsync() for automatic login.");
            }
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            TestContext.WriteLine($"{GetType().Name} Tests Completed");
            _ensekClient?.Dispose();
            _loggerFactory?.Dispose();
        }
    }
}
