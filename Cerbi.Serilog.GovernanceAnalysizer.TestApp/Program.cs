using Serilog;
using Serilog.Debugging;
using Cerbi.Serilog.GovernanceAnalyzer;
using System;
using System.IO;

internal class Program
{
    private static void Main(string[] args)
    {
        // Enable Serilog internal diagnostics to stderr
        SelfLog.Enable(Console.Error);

        // Force working directory to the correct location (optional for debugging)
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

        // Setup logger with governance filter
        Log.Logger = new LoggerConfiguration()
            .Filter.WithCerbiGovernance("config/cerbi_governance.json", "default")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Starting test app");

        // ✅ Valid: has required field
        Log.Information("User signed in | userId={userId}", "abc-123");

        // ❌ Missing required field
        Log.Information("Order processed");

        // ✅ Valid: has required fields using ForContext
        Log.ForContext("userId", "abc")
            .ForContext("orderId", 123)
            .Information("Test A: Has required");

        // ❌ Missing userId
        Log.Information("Test B: Missing userId with additional context | userId={userId}", null);

        // ❌ Forbidden field present
        Log.ForContext("ssn", "123-45-6789")
            .Information("Test C: Contains forbidden");

        Log.CloseAndFlush();
    }
}
