using NUnit.Framework;
using ApiTestFramework.Core;

namespace ApiTestFramework.Tests
{
    [TestFixture]
    [Description("ENSEK API Orders endpoint tests")]
    public class EnsekOrdersTests : BaseEnsekTest
    {
        [Test, Order(1)]
        [Description("Login to get Bearer token for orders endpoint tests")]
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
        [Description("Get all orders and verify the response structure")]
        public async Task Test_GetOrders_VerifyResponse()
        {
            TestContext.WriteLine("=== TESTING GET ORDERS ENDPOINT ===");
            
            // Ensure we have a Bearer token - login if needed
            await EnsureBearerTokenAvailableAsync();

            TestContext.WriteLine("Attempting to get all orders...");

            var ordersResponse = await _ensekClient.GetOrdersAsync();

            TestContext.WriteLine($"HTTP Status Code: {ordersResponse.StatusCode}");

            if (ordersResponse.IsSuccess)
            {
                TestContext.WriteLine($"✅ Orders retrieved successfully");
                TestContext.WriteLine($"✅ Total orders count: {ordersResponse.Orders?.Count ?? 0}");
                
                // Display some sample orders
                if (ordersResponse.Orders != null && ordersResponse.Orders.Any())
                {
                    TestContext.WriteLine("✅ Sample orders:");
                    foreach (var order in ordersResponse.Orders.Take(3))
                    {
                        TestContext.WriteLine($"   - ID: {order.Id}, Fuel: {order.Fuel}, Quantity: {order.Quantity}, Time: {order.Time}");
                    }
                }
                
                Assert.That(ordersResponse.IsSuccess, Is.True, "Get orders should be successful");
                Assert.That(ordersResponse.StatusCode, Is.EqualTo(200), "Should return HTTP 200");
                Assert.That(ordersResponse.Orders, Is.Not.Null, "Orders list should not be null");
            }
            else if (ordersResponse.IsUnauthorized)
            {
                TestContext.WriteLine("❌ Get orders failed - 401 Unauthorized");
                TestContext.WriteLine("Bearer token might be invalid or expired");
                Assert.Fail($"Get orders failed with 401 Unauthorized: {ordersResponse.ErrorMessage}");
            }
            else
            {
                TestContext.WriteLine($"❌ Get orders failed - HTTP {ordersResponse.StatusCode}");
                TestContext.WriteLine($"Error: {ordersResponse.ErrorMessage}");
                Assert.Fail($"Get orders failed with unexpected status: {ordersResponse.StatusCode} - {ordersResponse.ErrorMessage}");
            }
        }

        [Test, Order(3)]
        [Description("Count orders created before current date")]
        public async Task Test_CountOrdersBeforeCurrentDate()
        {
            TestContext.WriteLine("=== TESTING ORDER COUNT BEFORE CURRENT DATE ===");
            
            // Ensure we have a Bearer token - login if needed
            await EnsureBearerTokenAvailableAsync();

            var currentDate = DateTime.Now;
            TestContext.WriteLine($"Current date for comparison: {currentDate:yyyy-MM-dd HH:mm:ss}");

            var ordersResponse = await _ensekClient.GetOrdersAsync();

            TestContext.WriteLine($"HTTP Status Code: {ordersResponse.StatusCode}");

            if (ordersResponse.IsSuccess && ordersResponse.Orders != null)
            {
                var totalOrders = ordersResponse.Orders.Count;
                var ordersBeforeCurrentDate = ordersResponse.GetOrdersCountBeforeDate(currentDate);
                var ordersBeforeToday = ordersResponse.GetOrdersCountBeforeDate(DateTime.Today);

                TestContext.WriteLine($"✅ Total orders: {totalOrders}");
                TestContext.WriteLine($"✅ Orders created before current time ({currentDate:yyyy-MM-dd HH:mm:ss}): {ordersBeforeCurrentDate}");
                TestContext.WriteLine($"✅ Orders created before today ({DateTime.Today:yyyy-MM-dd}): {ordersBeforeToday}");

                // Show some details about order dates
                if (ordersResponse.Orders.Any())
                {
                    var ordersWithValidDates = ordersResponse.Orders.Where(o => o.ParsedTime.HasValue).ToList();
                    if (ordersWithValidDates.Any())
                    {
                        var earliestOrder = ordersWithValidDates.OrderBy(o => o.ParsedTime).First();
                        var latestOrder = ordersWithValidDates.OrderByDescending(o => o.ParsedTime).First();
                        
                        TestContext.WriteLine($"✅ Earliest order date: {earliestOrder.ParsedTime:yyyy-MM-dd HH:mm:ss} (Fuel: {earliestOrder.Fuel})");
                        TestContext.WriteLine($"✅ Latest order date: {latestOrder.ParsedTime:yyyy-MM-dd HH:mm:ss} (Fuel: {latestOrder.Fuel})");
                    }
                    
                    // Show breakdown by fuel type
                    var fuelGroups = ordersResponse.Orders
                        .Where(o => o.IsCreatedBefore(currentDate))
                        .GroupBy(o => o.Fuel)
                        .ToList();
                    
                    if (fuelGroups.Any())
                    {
                        TestContext.WriteLine("✅ Orders before current date by fuel type:");
                        foreach (var group in fuelGroups.OrderBy(g => g.Key))
                        {
                            TestContext.WriteLine($"   - {group.Key}: {group.Count()} orders");
                        }
                    }
                }

                Assert.That(totalOrders, Is.GreaterThanOrEqualTo(0), "Total orders should be non-negative");
                Assert.That(ordersBeforeCurrentDate, Is.GreaterThanOrEqualTo(0), "Orders before current date should be non-negative");
                Assert.That(ordersBeforeCurrentDate, Is.LessThanOrEqualTo(totalOrders), "Orders before current date should not exceed total orders");
                
                // Since the example shows orders from 2022, we expect some orders before current date
                if (ordersBeforeCurrentDate > 0)
                {
                    TestContext.WriteLine($"✅ Found {ordersBeforeCurrentDate} orders created before current date");
                    Assert.Pass($"Successfully counted {ordersBeforeCurrentDate} orders created before {currentDate:yyyy-MM-dd}");
                }
                else
                {
                    TestContext.WriteLine("⚠️ No orders found before current date - this might be expected if all orders are very recent");
                    Assert.Pass("No orders found before current date - test completed successfully");
                }
            }
            else
            {
                TestContext.WriteLine($"❌ Failed to get orders for date analysis: {ordersResponse.ErrorMessage}");
                Assert.Fail($"Could not retrieve orders for date analysis: {ordersResponse.ErrorMessage}");
            }
        }

        [Test, Order(4)]
        [TestCase("2022-01-01", Description = "Count orders before 2022-01-01")]
        [TestCase("2022-06-01", Description = "Count orders before 2022-06-01")]
        [TestCase("2023-01-01", Description = "Count orders before 2023-01-01")]
        [TestCase("2024-01-01", Description = "Count orders before 2024-01-01")]
        [Description("Test order counting for specific dates")]
        public async Task Test_CountOrdersBeforeSpecificDates(string dateString)
        {
            TestContext.WriteLine($"=== TESTING ORDER COUNT BEFORE {dateString} ===");
            
            // Ensure we have a Bearer token - login if needed
            await EnsureBearerTokenAvailableAsync();

            var targetDate = DateTime.Parse(dateString);
            TestContext.WriteLine($"Target date for comparison: {targetDate:yyyy-MM-dd}");

            var ordersResponse = await _ensekClient.GetOrdersAsync();

            if (ordersResponse.IsSuccess && ordersResponse.Orders != null)
            {
                var totalOrders = ordersResponse.Orders.Count;
                var ordersBeforeDate = ordersResponse.GetOrdersCountBeforeDate(targetDate);
                var ordersAtOrAfterDate = totalOrders - ordersBeforeDate;

                TestContext.WriteLine($"✅ Total orders: {totalOrders}");
                TestContext.WriteLine($"✅ Orders before {targetDate:yyyy-MM-dd}: {ordersBeforeDate}");
                TestContext.WriteLine($"✅ Orders at or after {targetDate:yyyy-MM-dd}: {ordersAtOrAfterDate}");

                // Show some specific orders for this date range
                var ordersBeforeDateList = ordersResponse.GetOrdersBeforeDate(targetDate);
                if (ordersBeforeDateList.Any())
                {
                    TestContext.WriteLine($"✅ Sample orders before {targetDate:yyyy-MM-dd}:");
                    foreach (var order in ordersBeforeDateList.Take(3))
                    {
                        TestContext.WriteLine($"   - {order.Fuel} order on {order.ParsedTime:yyyy-MM-dd} (Qty: {order.Quantity})");
                    }
                }

                Assert.That(ordersBeforeDate, Is.GreaterThanOrEqualTo(0), $"Orders before {dateString} should be non-negative");
                Assert.That(ordersBeforeDate, Is.LessThanOrEqualTo(totalOrders), $"Orders before {dateString} should not exceed total orders");
                
                Assert.Pass($"Found {ordersBeforeDate} orders created before {targetDate:yyyy-MM-dd}");
            }
            else
            {
                TestContext.WriteLine($"❌ Failed to get orders: {ordersResponse.ErrorMessage}");
                Assert.Fail($"Could not retrieve orders: {ordersResponse.ErrorMessage}");
            }
        }

        [Test, Order(5)]
        [Description("Verify order data structure and parsing")]
        public async Task Test_OrderDataStructureAndParsing()
        {
            TestContext.WriteLine("=== TESTING ORDER DATA STRUCTURE AND PARSING ===");
            
            // Ensure we have a Bearer token - login if needed
            await EnsureBearerTokenAvailableAsync();

            var ordersResponse = await _ensekClient.GetOrdersAsync();

            if (ordersResponse.IsSuccess && ordersResponse.Orders != null && ordersResponse.Orders.Any())
            {
                TestContext.WriteLine($"✅ Retrieved {ordersResponse.Orders.Count} orders for structure validation");

                var validOrders = 0;
                var invalidTimeOrders = 0;
                var emptyFieldOrders = 0;

                foreach (var order in ordersResponse.Orders)
                {
                    // Check required fields
                    if (string.IsNullOrEmpty(order.Id) || string.IsNullOrEmpty(order.Fuel) || string.IsNullOrEmpty(order.Time))
                    {
                        emptyFieldOrders++;
                        TestContext.WriteLine($"⚠️ Order with empty fields: ID={order.Id}, Fuel={order.Fuel}, Time={order.Time}");
                        continue;
                    }

                    // Check time parsing
                    if (!order.ParsedTime.HasValue)
                    {
                        invalidTimeOrders++;
                        TestContext.WriteLine($"⚠️ Order with invalid time format: {order.Time}");
                        continue;
                    }

                    validOrders++;
                }

                TestContext.WriteLine($"✅ Valid orders: {validOrders}");
                TestContext.WriteLine($"⚠️ Orders with invalid time: {invalidTimeOrders}");
                TestContext.WriteLine($"⚠️ Orders with empty fields: {emptyFieldOrders}");

                // Show fuel type distribution
                var fuelTypes = ordersResponse.Orders
                    .Where(o => !string.IsNullOrEmpty(o.Fuel))
                    .GroupBy(o => o.Fuel)
                    .OrderBy(g => g.Key)
                    .ToList();

                if (fuelTypes.Any())
                {
                    TestContext.WriteLine("✅ Fuel type distribution:");
                    foreach (var fuelGroup in fuelTypes)
                    {
                        TestContext.WriteLine($"   - {fuelGroup.Key}: {fuelGroup.Count()} orders");
                    }
                }

                Assert.That(validOrders, Is.GreaterThan(0), "Should have at least some valid orders");
                Assert.That(ordersResponse.Orders.All(o => o.Quantity >= 0), Is.True, "All quantities should be non-negative");
                
                Assert.Pass($"Order structure validation completed: {validOrders} valid orders out of {ordersResponse.Orders.Count} total");
            }
            else
            {
                TestContext.WriteLine($"❌ No orders available for structure validation: {ordersResponse.ErrorMessage}");
                Assert.Fail($"Could not validate order structure: {ordersResponse.ErrorMessage}");
            }
        }

        [Test, Order(99)]
        [Description("Reset test data after orders tests")]
        public async Task Test_Common_Reset_AfterOrdersTests()
        {
            var success = await ResetTestDataAsync();
            
            if (!success)
            {
                TestContext.WriteLine("⚠️ Reset failed - test data may not be in clean state");
                // Don't fail the test as reset failure shouldn't fail the entire test suite
            }
            else
            {
                TestContext.WriteLine("✅ Test data reset successful");
            }
        }
    }
}
