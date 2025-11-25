using Serilog;
using Serilog.Debugging;
using Cerbi.Serilog.Governance;
using System;
using System.IO;

internal class Program
{
    private static void Main(string[] args)
    {
        // Enable Serilog internal diagnostics to stderr
        SelfLog.Enable(Console.Error);

        // Force working directory to the correct location
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        Console.WriteLine("=== Cerbi Serilog Governance - Production Readiness Test Suite ===\n");

        // Setup logger with governance filter
        Log.Logger = new LoggerConfiguration()
            .CerbiGovernance(o =>
            {
                o.Profile = "default";
                o.ConfigPath = "cerbi_governance.json";  // File is copied to output root
            })
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Starting comprehensive governance test suite");

        // ============================================================
        // TEST CATEGORY 1: Required Fields Validation
        // ============================================================
        Console.WriteLine("\n--- Test Category 1: Required Fields ---");
        
        // TEST 1.1: ✅ Valid - Both required fields present
        Console.WriteLine("TEST 1.1: Both required fields (userId, orderId) present");
        Log.ForContext("userId", "user-001")
            .ForContext("orderId", 12345)
            .Information("Order created successfully");

        // TEST 1.2: ❌ Missing userId (required)
        Console.WriteLine("TEST 1.2: Missing userId (SHOULD BE BLOCKED)");
        Log.ForContext("orderId", 12346)
            .Information("Order without userId");

        // TEST 1.3: ❌ Missing orderId (required)
        Console.WriteLine("TEST 1.3: Missing orderId (SHOULD BE BLOCKED)");
        Log.ForContext("userId", "user-002")
            .Information("Log without orderId");

        // TEST 1.4: ❌ Missing both required fields
        Console.WriteLine("TEST 1.4: Missing both required fields (SHOULD BE BLOCKED)");
        Log.Information("Log with no required fields");

        // TEST 1.5: ✅ Valid - Required fields with null check
        Console.WriteLine("TEST 1.5: Required fields with additional context");
        Log.ForContext("userId", "user-003")
            .ForContext("orderId", 12347)
            .ForContext("timestamp", DateTime.UtcNow)
            .Information("Order with extra context");

        // ============================================================
        // TEST CATEGORY 2: Forbidden Fields Validation
        // ============================================================
        Console.WriteLine("\n--- Test Category 2: Forbidden Fields ---");

        // TEST 2.1: ❌ Forbidden field 'ssn' present
        Console.WriteLine("TEST 2.1: Forbidden field 'ssn' (SHOULD BE BLOCKED)");
        Log.ForContext("userId", "user-004")
            .ForContext("orderId", 12348)
            .ForContext("ssn", "123-45-6789")
            .Information("Attempt to log SSN");

        // TEST 2.2: ✅ Valid - No forbidden fields
        Console.WriteLine("TEST 2.2: No forbidden fields present");
        Log.ForContext("userId", "user-005")
            .ForContext("orderId", 12349)
            .ForContext("email", "user@example.com")
            .Information("Valid log with email");

        // ============================================================
        // TEST CATEGORY 3: Edge Cases
        // ============================================================
        Console.WriteLine("\n--- Test Category 3: Edge Cases ---");

        // TEST 3.1: ✅ Null values in non-required fields
        Console.WriteLine("TEST 3.1: Null values in optional fields");
        Log.ForContext("userId", "user-006")
            .ForContext("orderId", 12350)
            .ForContext("notes", null)
            .Information("Order with null optional field");

        // TEST 3.2: ❌ Null value in required field
        Console.WriteLine("TEST 3.2: Null value in required field (SHOULD BE BLOCKED)");
        Log.ForContext("userId", null)
            .ForContext("orderId", 12351)
            .Information("Order with null userId");

        // TEST 3.3: ✅ Empty string in required field (if present, may be valid depending on config)
        Console.WriteLine("TEST 3.3: Empty string in required field");
        Log.ForContext("userId", "")
            .ForContext("orderId", 12352)
            .Information("Order with empty userId");

        // TEST 3.4: ✅ Very long strings
        Console.WriteLine("TEST 3.4: Very long string values");
        Log.ForContext("userId", new string('x', 1000))
            .ForContext("orderId", 12353)
            .Information("Order with extremely long userId");

        // ============================================================
        // TEST CATEGORY 4: Exception Handling
        // ============================================================
        Console.WriteLine("\n--- Test Category 4: Exception Handling ---");

        // TEST 4.1: ✅ Logging with exception
        Console.WriteLine("TEST 4.1: Logging with exception object");
        try
        {
            throw new InvalidOperationException("Test exception");
        }
        catch (Exception ex)
        {
            Log.ForContext("userId", "user-007")
                .ForContext("orderId", 12354)
                .Error(ex, "Order processing failed");
        }

        // TEST 4.2: ❌ Exception without required fields (SHOULD BE BLOCKED)
        Console.WriteLine("TEST 4.2: Exception without required fields (SHOULD BE BLOCKED)");
        try
        {
            throw new ArgumentException("Test argument exception");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error without required fields");
        }

        // ============================================================
        // TEST CATEGORY 5: Different Log Levels
        // ============================================================
        Console.WriteLine("\n--- Test Category 5: Log Levels ---");

        // TEST 5.1: ✅ Debug level
        Console.WriteLine("TEST 5.1: Debug level with required fields");
        Log.ForContext("userId", "user-008")
            .ForContext("orderId", 12355)
            .Debug("Debug message");

        // TEST 5.2: ✅ Warning level
        Console.WriteLine("TEST 5.2: Warning level with required fields");
        Log.ForContext("userId", "user-009")
            .ForContext("orderId", 12356)
            .Warning("Warning message");

        // TEST 5.3: ✅ Error level
        Console.WriteLine("TEST 5.3: Error level with required fields");
        Log.ForContext("userId", "user-010")
            .ForContext("orderId", 12357)
            .Error("Error message");

        // TEST 5.4: ✅ Fatal level with required fields
        Console.WriteLine("TEST 5.4: Fatal level with required fields");
        Log.ForContext("userId", "user-011")
            .ForContext("orderId", 12358)
            .Fatal("Fatal message");

        // ============================================================
        // TEST CATEGORY 6: Complex Scenarios
        // ============================================================
        Console.WriteLine("\n--- Test Category 6: Complex Scenarios ---");

        // TEST 6.1: ✅ Multiple contexts chained
        Console.WriteLine("TEST 6.1: Multiple chained ForContext calls");
        Log.ForContext("userId", "user-012")
            .ForContext("orderId", 12359)
            .ForContext("productId", "prod-001")
            .ForContext("quantity", 5)
            .ForContext("price", 99.99m)
            .Information("Complex order scenario");

        // TEST 6.2: ✅ Structured object
        Console.WriteLine("TEST 6.2: Logging structured objects");
        var order = new { OrderId = 12360, UserId = "user-013", Items = new[] { "item1", "item2" } };
        Log.ForContext("userId", order.UserId)
            .ForContext("orderId", order.OrderId)
            .Information("Order placed: {@Order}", order);

        // TEST 6.3: ❌ Mixed valid and invalid fields
        Console.WriteLine("TEST 6.3: Mix of valid fields with forbidden field (SHOULD BE BLOCKED)");
        Log.ForContext("userId", "user-014")
            .ForContext("orderId", 12361)
            .ForContext("ssn", "987-65-4321")
            .ForContext("email", "test@example.com")
            .Information("Order with mixed fields");

        // ============================================================
        // TEST CATEGORY 7: High Volume Simulation
        // ============================================================
        Console.WriteLine("\n--- Test Category 7: High Volume Simulation ---");

        Console.WriteLine("TEST 7.1: Rapid valid logs (100 events)");
        for (int i = 0; i < 100; i++)
        {
            Log.ForContext("userId", $"user-{i + 100}")
                .ForContext("orderId", 13000 + i)
                .Information("High volume test event {EventNumber}", i);
        }

        Console.WriteLine("TEST 7.2: Rapid mixed valid/invalid logs (50 events)");
        for (int i = 0; i < 50; i++)
        {
            if (i % 2 == 0)
            {
                // Valid
                Log.ForContext("userId", $"user-{i + 200}")
                    .ForContext("orderId", 14000 + i)
                    .Information("Valid event {EventNumber}", i);
            }
            else
            {
                // Invalid - missing required fields
                Log.Information("Invalid event {EventNumber}", i);
            }
        }

        Console.WriteLine("\n=== Test Suite Complete ===");
        Log.ForContext("userId", "system")
            .ForContext("orderId", 99999)
            .Information("All governance tests executed - Test suite completed successfully");

        Log.CloseAndFlush();
    }
}
