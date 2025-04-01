using ElevatorControlSystem.Enum;
using ElevatorControlSystem.Interfaces;
using ElevatorControlSystem.Models;
using Microsoft.Extensions.Options;
using Serilog;

namespace ElevatorControlSystem.Services
{
    /// <summary>
    /// Service responsible for generating and processing random elevator requests.
    /// </summary>
    public class ElevatorRequestService : IElevatorRequestService
    {
        private readonly IElevatorService _elevatorService;
        private readonly int _numberOfFloors;
        private readonly Random _random;

        /// <summary>
        /// Initializes the ElevatorRequestService with necessary dependencies.
        /// </summary>
        /// <param name="elevatorService">The service handling elevator movement.</param>
        /// <param name="numberOfFloors">Total number of floors in the building.</param>
        public ElevatorRequestService(IElevatorService elevatorService, IOptions<ElevatorSettings> options)
        {
            _elevatorService = elevatorService;
            _numberOfFloors = options.Value.NumberOfFloors;
            _random = new Random();
        }

        /// <summary>
        /// Generates a new random elevator request and assigns it to an elevator.
        /// </summary>
        public void GenerateRequest()
        {
            try
            {
                int floor = _random.Next(0, _numberOfFloors); // Randomly selects a floor (0 to max floors)
                Direction direction = floor == 0 ? Direction.Up :
                                      floor == _numberOfFloors - 1 ? Direction.Down :
                                      (_random.Next(2) == 0 ? Direction.Up : Direction.Down);

                ElevatorRequest request = new ElevatorRequest(floor, direction);

                Log.Information($"\x1b[34mNew: {direction} request on floor {floor} received");
                _elevatorService.AssignRequest(request);
            }
            catch (Exception ex)
            {
                // Log the error
                Log.Error(ex, "Error occurred while generating a request.");
            }
        }
    }
}
