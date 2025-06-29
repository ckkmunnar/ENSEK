using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ApiTestFramework.Core
{
    public class EnsekApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EnsekApiClient> _logger;

        public EnsekApiClient(string baseUrl, ILogger<EnsekApiClient>? logger = null)
        {
            _logger = logger ?? CreateDefaultLogger();
            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _logger.LogInformation("ENSEK API client initialized with base URL: {BaseUrl}", baseUrl);
        }

        /// <summary>
        /// Sets the Bearer token for authenticated requests
        /// </summary>
        /// <param name="bearerToken">The Bearer token to use for authorization</param>
        public void SetBearerToken(string bearerToken)
        {
            // Remove existing Authorization headers
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            
            // Add the Bearer token
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
            _logger.LogInformation("Bearer token set for authenticated requests");
        }

        /// <summary>
        /// Authenticates with the ENSEK API using credentials
        /// </summary>
        /// <param name="loginRequest">Login credentials (username and password)</param>
        /// <returns>Login response with status information</returns>
        public async Task<LoginApiResponse> LoginAsync(LoginRequest loginRequest)
        {
            _logger.LogInformation("Attempting to login with username: {Username}", loginRequest.Username);
            
            try
            {
                // Create a new HttpRequestMessage to avoid using default headers
                var json = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var request = new HttpRequestMessage(HttpMethod.Post, "/ENSEK/login")
                {
                    Content = content
                };
                
                // Send request without any Authorization header
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Login successful for user: {Username}", loginRequest.Username);
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return new LoginApiResponse
                    {
                        IsSuccess = true,
                        StatusCode = (int)response.StatusCode,
                        LoginData = loginResponse,
                        ErrorMessage = null,
                        RawResponse = responseContent
                    };
                }
                else
                {
                    _logger.LogWarning("Login failed for user: {Username}. Status: {StatusCode}, Response: {Response}", 
                        loginRequest.Username, response.StatusCode, responseContent);
                    
                    // Try to parse error response
                    ErrorResponse? errorResponse = null;
                    try
                    {
                        errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                    catch
                    {
                        // If we can't parse as ErrorResponse, that's okay
                    }
                    
                    return new LoginApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = (int)response.StatusCode,
                        LoginData = null,
                        ErrorMessage = errorResponse?.Message ?? responseContent,
                        ErrorData = errorResponse,
                        RawResponse = responseContent
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during login for user: {Username}", loginRequest.Username);
                return new LoginApiResponse
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    LoginData = null,
                    ErrorMessage = ex.Message,
                    RawResponse = null
                };
            }
        }

        /// <summary>
        /// Resets the test data back to its initial state
        /// </summary>
        /// <returns>Reset response with status information</returns>
        public async Task<ResetApiResponse> ResetTestDataAsync()
        {
            _logger.LogInformation("Resetting test data");
            
            try
            {
                var response = await _httpClient.PostAsync("/ENSEK/reset", null);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Test data reset successful. Status: {StatusCode}", response.StatusCode);
                    var resetResponse = JsonSerializer.Deserialize<ResetResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return new ResetApiResponse
                    {
                        IsSuccess = true,
                        StatusCode = (int)response.StatusCode,
                        ResetData = resetResponse,
                        ErrorMessage = null,
                        RawResponse = responseContent
                    };
                }
                else
                {
                    _logger.LogWarning("Test data reset failed. Status: {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseContent);
                    
                    // Try to parse error response
                    ErrorResponse? errorResponse = null;
                    try
                    {
                        errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                    catch
                    {
                        
                    }
                    
                    return new ResetApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = (int)response.StatusCode,
                        ResetData = null,
                        ErrorMessage = errorResponse?.Message ?? responseContent,
                        ErrorData = errorResponse,
                        RawResponse = responseContent
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during test data reset");
                return new ResetApiResponse
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    ResetData = null,
                    ErrorMessage = ex.Message,
                    RawResponse = null
                };
            }
        }

        /// <summary>
        /// Purchases energy units from the ENSEK API
        /// </summary>
        /// <param name="id">Energy type ID</param>
        /// <param name="quantity">Quantity to purchase</param>
        /// <returns>Buy response with status information</returns>
        public async Task<BuyApiResponse> BuyEnergyAsync(int id, int quantity)
        {
            _logger.LogInformation("Purchasing energy - ID: {Id}, Quantity: {Quantity}", id, quantity);
            
            try
            {
                var response = await _httpClient.PutAsync($"/ENSEK/buy/{id}/{quantity}", null);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Energy purchase successful. ID: {Id}, Quantity: {Quantity}, Status: {StatusCode}", 
                        id, quantity, response.StatusCode);
                    var buyResponse = JsonSerializer.Deserialize<BuyResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return new BuyApiResponse
                    {
                        IsSuccess = true,
                        StatusCode = (int)response.StatusCode,
                        BuyData = buyResponse,
                        ErrorMessage = null,
                        RawResponse = responseContent
                    };
                }
                else
                {
                    _logger.LogWarning("Energy purchase failed. ID: {Id}, Quantity: {Quantity}, Status: {StatusCode}, Response: {Response}", 
                        id, quantity, response.StatusCode, responseContent);
                    
                    // Try to parse error response
                    ErrorResponse? errorResponse = null;
                    try
                    {
                        errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                    catch
                    {
                        // If we can't parse as ErrorResponse, that's okay
                    }
                    
                    return new BuyApiResponse
                    {
                        IsSuccess = false,
                        StatusCode = (int)response.StatusCode,
                        BuyData = null,
                        ErrorMessage = errorResponse?.Message ?? responseContent,
                        ErrorData = errorResponse,
                        RawResponse = responseContent
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during energy purchase. ID: {Id}, Quantity: {Quantity}", id, quantity);
                return new BuyApiResponse
                {
                    IsSuccess = false,
                    StatusCode = 0,
                    BuyData = null,
                    ErrorMessage = ex.Message,
                    RawResponse = null
                };
            }
        }

        /// <summary>
        /// Gets all orders from the ENSEK API
        /// </summary>
        /// <returns>Orders response with status information and order list</returns>
        public async Task<OrdersApiResponse> GetOrdersAsync()
        {
            _logger.LogInformation("Attempting to get orders");
            
            try
            {
                var response = await _httpClient.GetAsync("/ENSEK/orders");
                var content = await response.Content.ReadAsStringAsync();
                
                var ordersResponse = new OrdersApiResponse
                {
                    StatusCode = (int)response.StatusCode,
                    RawResponse = content
                };
                
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var orders = JsonSerializer.Deserialize<List<Order>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        ordersResponse.IsSuccess = true;
                        ordersResponse.Orders = orders;
                        
                        _logger.LogInformation("Orders retrieved successfully. Count: {OrderCount}", orders?.Count ?? 0);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize orders response");
                        ordersResponse.ErrorMessage = $"Failed to parse orders response: {ex.Message}";
                    }
                }
                else
                {
                    _logger.LogWarning("Get orders failed with status: {StatusCode}", response.StatusCode);
                    
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        
                        ordersResponse.ErrorData = errorResponse;
                        ordersResponse.ErrorMessage = errorResponse?.Message ?? $"HTTP {response.StatusCode} error";
                    }
                    catch (JsonException)
                    {
                        ordersResponse.ErrorMessage = $"HTTP {response.StatusCode}: {content}";
                    }
                }
                
                return ordersResponse;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed during get orders");
                return new OrdersApiResponse
                {
                    StatusCode = 0,
                    ErrorMessage = $"HTTP request failed: {ex.Message}",
                    RawResponse = ex.ToString()
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Get orders request timed out");
                return new OrdersApiResponse
                {
                    StatusCode = 0,
                    ErrorMessage = $"Request timed out: {ex.Message}",
                    RawResponse = ex.ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during get orders");
                return new OrdersApiResponse
                {
                    StatusCode = 0,
                    ErrorMessage = $"Unexpected error: {ex.Message}",
                    RawResponse = ex.ToString()
                };
            }
        }

        private static ILogger<EnsekApiClient> CreateDefaultLogger()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            return loggerFactory.CreateLogger<EnsekApiClient>();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
