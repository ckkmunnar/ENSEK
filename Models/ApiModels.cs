using System.Text.Json.Serialization;

namespace ApiTestFramework.Core
{
    // Response model for the reset endpoint
    public class ResetResponse
    {
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        
        [JsonPropertyName("timestamp")]
        public DateTime? Timestamp { get; set; }
        
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        
        public override string ToString()
        {
            return $"Reset Response - Description: {Description}, Status: {Status}, Message: {Message}";
        }
    }

    // Login request model
    public class LoginRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Creates a login request with the specified credentials
        /// </summary>
        /// <param name="username">Username for login</param>
        /// <param name="password">Password for login</param>
        public LoginRequest(string username = "", string password = "")
        {
            Username = username;
            Password = password;
        }
    }

    // Login response model  
    public class LoginResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
        
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        
        public override string ToString()
        {
            return $"Login Response - Message: {Message}, AccessToken: {(!string.IsNullOrEmpty(AccessToken) ? "***PRESENT***" : "MISSING")}";
        }
    }

    // Error response model (for both endpoints)
    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
        
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        public override string ToString()
        {
            return $"Error: {Error} - {Message}";
        }
    }

    // Wrapper for Login API response that includes HTTP status and error details
    public class LoginApiResponse
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public LoginResponse? LoginData { get; set; }
        public string? ErrorMessage { get; set; }
        public ErrorResponse? ErrorData { get; set; }
        public string? RawResponse { get; set; }
        
        /// <summary>
        /// Indicates if this was a 401 Unauthorized response
        /// </summary>
        [JsonIgnore]
        public bool IsUnauthorized => StatusCode == 401;
        
        public override string ToString()
        {
            if (IsSuccess)
                return $"Login Success (HTTP {StatusCode}): {LoginData?.ToString() ?? "No data"}";
            else
                return $"Login Failed (HTTP {StatusCode}): {ErrorMessage ?? "No error message"}";
        }
    }

    // Wrapper for Reset API response that includes HTTP status and error details
    public class ResetApiResponse
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public ResetResponse? ResetData { get; set; }
        public string? ErrorMessage { get; set; }
        public ErrorResponse? ErrorData { get; set; }
        public string? RawResponse { get; set; }
        
        /// <summary>
        /// Indicates if this was a 401 Unauthorized response
        /// </summary>
        [JsonIgnore]
        public bool IsUnauthorized => StatusCode == 401;
        
        public override string ToString()
        {
            if (IsSuccess)
                return $"Reset Success (HTTP {StatusCode}): {ResetData?.ToString() ?? "No data"}";
            else
                return $"Reset Failed (HTTP {StatusCode}): {ErrorMessage ?? "No error message"}";
        }
    }

    // Response model for the buy endpoint
    public class BuyResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        
        /// <summary>
        /// Extracts the purchased quantity from the message
        /// Example: "You have purchased 2970 m³ at a cost of 3.4 there are 10 units remaining..."
        /// Returns: 2970
        /// </summary>
        [JsonIgnore]
        public int? PurchasedQuantity
        {
            get
            {
                if (string.IsNullOrEmpty(Message))
                    return null;
                
                var pattern = @"You have purchased (\d+)";
                var match = System.Text.RegularExpressions.Regex.Match(Message, pattern);
                
                if (match.Success && int.TryParse(match.Groups[1].Value, out var quantity))
                    return quantity;
                
                return null;
            }
        }
        
        /// <summary>
        /// Extracts the cost from the message
        /// Example: "You have purchased 2970 m³ at a cost of 3.4000000000000004 there are 10 units remaining..."
        /// Returns: 3.4
        /// </summary>
        [JsonIgnore]
        public decimal? Cost
        {
            get
            {
                if (string.IsNullOrEmpty(Message))
                    return null;
                
                var pattern = @"at a cost of ([\d.]+)";
                var match = System.Text.RegularExpressions.Regex.Match(Message, pattern);
                
                if (match.Success && decimal.TryParse(match.Groups[1].Value, out var cost))
                    return Math.Round(cost, 2); // Round to 2 decimal places
                
                return null;
            }
        }
        
        /// <summary>
        /// Extracts the remaining units from the message
        /// Example: "You have purchased 2970 m³ at a cost of 3.4 there are 10 units remaining..."
        /// Returns: 10
        /// </summary>
        [JsonIgnore]
        public int? RemainingUnits
        {
            get
            {
                if (string.IsNullOrEmpty(Message))
                    return null;
                
                var pattern = @"there are (-?\d+) units remaining";
                var match = System.Text.RegularExpressions.Regex.Match(Message, pattern);
                
                if (match.Success && int.TryParse(match.Groups[1].Value, out var remaining))
                    return remaining;
                
                return null;
            }
        }
        
        /// <summary>
        /// Extracts the order ID from the message
        /// Example: "...Your order id is 69d09d33-9944-4d27-9f89-c5ffddf8a4e8"
        /// Also handles: "...Your orderid is 69d09d33-9944-4d27-9f89-c5ffddf8a4e8"
        /// Returns: "69d09d33-9944-4d27-9f89-c5ffddf8a4e8"
        /// </summary>
        [JsonIgnore]
        public string? OrderId
        {
            get
            {
                if (string.IsNullOrEmpty(Message))
                    return null;
                
                var pattern = @"Your order\s?id is ([a-fA-F0-9-]+)";
                var match = System.Text.RegularExpressions.Regex.Match(Message, pattern);
                
                if (match.Success)
                    return match.Groups[1].Value;
                
                return null;
            }
        }
        
        /// <summary>
        /// Extracts the unit type from the message (e.g., "m³", "kWh", "Litres")
        /// Example: "You have purchased 2970 m³ at a cost of..."
        /// Returns: "m³"
        /// </summary>
        [JsonIgnore]
        public string? UnitType
        {
            get
            {
                if (string.IsNullOrEmpty(Message))
                    return null;
                
                var pattern = @"You have purchased \d+ ([^\s]+)";
                var match = System.Text.RegularExpressions.Regex.Match(Message, pattern);
                
                if (match.Success)
                    return match.Groups[1].Value;
                
                return null;
            }
        }
        
        /// <summary>
        /// Indicates if the purchase was successful (contains specific purchase details)
        /// </summary>
        [JsonIgnore]
        public bool IsSuccessfulPurchase => PurchasedQuantity.HasValue && Cost.HasValue && !string.IsNullOrEmpty(OrderId);
        
        /// <summary>
        /// Indicates if there was no fuel available to purchase
        /// </summary>
        [JsonIgnore]
        public bool IsNoFuelAvailable => Message?.Contains("There is no") == true || Message?.Contains("fuel to purchase") == true;
        
        public override string ToString()
        {
            if (IsSuccessfulPurchase)
            {
                return $"Buy Response - Purchased: {PurchasedQuantity} {UnitType}, Cost: {Cost:F2}, Remaining: {RemainingUnits}, OrderID: {OrderId}";
            }
            else
            {
                return $"Buy Response - Message: {Message}";
            }
        }
    }

    // Wrapper for Buy API response that includes HTTP status and error details
    public class BuyApiResponse
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public BuyResponse? BuyData { get; set; }
        public string? ErrorMessage { get; set; }
        public ErrorResponse? ErrorData { get; set; }
        public string? RawResponse { get; set; }
        
        /// <summary>
        /// Indicates if this was a 401 Unauthorized response
        /// </summary>
        [JsonIgnore]
        public bool IsUnauthorized => StatusCode == 401;
        
        /// <summary>
        /// Indicates if this was a 400 Bad Request response
        /// </summary>
        [JsonIgnore]
        public bool IsBadRequest => StatusCode == 400;
        
        public override string ToString()
        {
            if (IsSuccess)
                return $"Buy Success (HTTP {StatusCode}): {BuyData?.ToString() ?? "No data"}";
            else
                return $"Buy Failed (HTTP {StatusCode}): {ErrorMessage ?? "No error message"}";
        }
    }

    // Order model for individual order items
    public class Order
    {
        [JsonPropertyName("fuel")]
        public string? Fuel { get; set; }
        
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        
        [JsonPropertyName("time")]
        public string? Time { get; set; }
        
        /// <summary>
        /// Parses the time string to DateTime if possible
        /// </summary>
        [JsonIgnore]
        public DateTime? ParsedTime
        {
            get
            {
                if (string.IsNullOrEmpty(Time))
                    return null;
                
                if (DateTime.TryParse(Time, out var parsedDate))
                    return parsedDate;
                
                return null;
            }
        }
        
        /// <summary>
        /// Checks if this order was created before the specified date
        /// </summary>
        /// <param name="date">The date to compare against</param>
        /// <returns>True if order was created before the date, false otherwise</returns>
        public bool IsCreatedBefore(DateTime date)
        {
            return ParsedTime.HasValue && ParsedTime.Value < date;
        }
        
        public override string ToString()
        {
            return $"Order - ID: {Id}, Fuel: {Fuel}, Quantity: {Quantity}, Time: {Time}";
        }
    }

    // Wrapper for Orders API response that includes HTTP status and error details
    public class OrdersApiResponse
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public List<Order>? Orders { get; set; }
        public string? ErrorMessage { get; set; }
        public ErrorResponse? ErrorData { get; set; }
        public string? RawResponse { get; set; }
        
        /// <summary>
        /// Indicates if this was a 401 Unauthorized response
        /// </summary>
        [JsonIgnore]
        public bool IsUnauthorized => StatusCode == 401;
        
        /// <summary>
        /// Indicates if this was a 400 Bad Request response
        /// </summary>
        [JsonIgnore]
        public bool IsBadRequest => StatusCode == 400;
        
        /// <summary>
        /// Gets the count of orders created before the specified date
        /// </summary>
        /// <param name="date">The date to compare against</param>
        /// <returns>Number of orders created before the date</returns>
        public int GetOrdersCountBeforeDate(DateTime date)
        {
            if (Orders == null) return 0;
            return Orders.Count(order => order.IsCreatedBefore(date));
        }
        
        /// <summary>
        /// Gets orders created before the specified date
        /// </summary>
        /// <param name="date">The date to compare against</param>
        /// <returns>List of orders created before the date</returns>
        public List<Order> GetOrdersBeforeDate(DateTime date)
        {
            if (Orders == null) return new List<Order>();
            return Orders.Where(order => order.IsCreatedBefore(date)).ToList();
        }
        
        public override string ToString()
        {
            if (IsSuccess)
                return $"Orders Success (HTTP {StatusCode}): {Orders?.Count ?? 0} orders retrieved";
            else
                return $"Orders Failed (HTTP {StatusCode}): {ErrorMessage ?? "No error message"}";
        }
    }
}
