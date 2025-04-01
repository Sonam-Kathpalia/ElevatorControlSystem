using Serilog;

namespace ElevatorControlSystem.Logging
{
    /// <summary>
    /// Centralized logging configuration using Serilog.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Initializes and configures the Serilog logger.
        /// </summary>
        public static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console() // Logs to the console
                .WriteTo.File("logs/elevator_log.txt", rollingInterval: RollingInterval.Day) // Logs to a file (daily rotation)
                .MinimumLevel.Information() // Set minimum logging level
                .CreateLogger();
        }

        /// <summary>
        /// Closes and flushes the logger to ensure all logs are written.
        /// </summary>
        public static void Shutdown()
        {
            Log.CloseAndFlush();
        }
    }
}
