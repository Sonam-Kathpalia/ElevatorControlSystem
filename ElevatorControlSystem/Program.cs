using ElevatorControlSystem.Interfaces;
using ElevatorControlSystem.Logging;
using ElevatorControlSystem.Models;
using ElevatorControlSystem.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

class Program
{
    static async Task Main()
    {
        try
        {
            Logger.Initialize();
            Log.Information("Elevator Control System Started");

            // Create and configure the Host
            using IHost host = CreateHostBuilder().Build();

            // Resolve services within a scoped context
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var requestService = services.GetRequiredService<IElevatorRequestService>();
                var settings = services.GetRequiredService<IOptions<ElevatorSettings>>().Value;

                Log.Information("Simulation Running... Press 'Q' to Quit\n");

                while (true)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                    {
                        Log.Information("\nSimulation Stopped!");
                        break;
                    }
                    try
                    {
                        requestService.GenerateRequest(); // Generate a new elevator request
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error occurred while generating a request.");
                    }

                    await Task.Delay(settings.RequestDelayInSeconds * 1000); // Wait before the next request
                }
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception occurred during application startup.");
        }
        finally
        {
            // Ensure logging is properly flushed and closed when the application shuts down
            Logger.Shutdown();
        }
    }

    /// <summary>
    /// Configures and creates the Host with Dependency Injection and Configuration setup.
    /// </summary>
    static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Bind configuration to ElevatorSettings
                services.Configure<ElevatorSettings>(context.Configuration.GetSection("ElevatorSettings"));

                // Register dependencies using DI
                services.AddSingleton<Building>(sp =>
                {
                    var settings = sp.GetRequiredService<IOptions<ElevatorSettings>>().Value;
                    return new Building(settings.NumberOfFloors, settings.NumberOfElevators);
                });

                services.AddSingleton<IElevatorService, ElevatorService>();
                services.AddSingleton<IElevatorRequestService, ElevatorRequestService>();
            });
}
