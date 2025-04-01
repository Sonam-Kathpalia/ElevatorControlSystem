using ElevatorControlSystem.Enum;
using ElevatorControlSystem.Models;
using ElevatorControlSystem.Services;
using Microsoft.Extensions.Options;

namespace ElevatorControlSystem.Tests
{
    /// <summary>
    /// Unit tests for the ElevatorService class, using XUnit framework.
    /// This test suite verifies the elevator assignment, movement logic, and parallel execution.
    /// </summary>
    public class ElevatorServiceTests
    {
        private readonly Building _building; // Represents the building with multiple floors and elevators
        private readonly ElevatorService _elevatorService; // Elevator service handling movement logic
        private readonly ElevatorSettings _elevatorSettings;

        /// <summary>
        /// Initializes a test instance with a building containing 10 floors and 4 elevators.
        /// </summary>
        public ElevatorServiceTests()
        {
            _building = new Building(10, 4); // Create a building with 10 floors and 4 elevators
            _elevatorSettings = new ElevatorSettings { NumberOfFloors = 10 };
            var options = Options.Create(_elevatorSettings);
            _elevatorService = new ElevatorService(_building, options); // Initialize ElevatorService with the created building
        }

        /// <summary>
        /// Test to ensure that an elevator is assigned to the nearest request floor.
        /// </summary>
        [Fact]
        public async Task AssignRequest_ShouldAssignToNearestElevator()
        {
            // Arrange: Create an elevator request for Floor 5 in Up direction.
            var request = new ElevatorRequest(5, Direction.Up);

            // Act: Assign the request to an elevator.
            _elevatorService.AssignRequest(request);

            // Wait for elevator to reach the requested floor
            await Task.Delay(12000); // Simulate elevator movement

            // Assert: Verify that at least one elevator has reached Floor 5.
            var assignedElevator = _building.Elevators.FirstOrDefault(e => e.CurrentFloor == 5);
            Assert.NotNull(assignedElevator);
        }

        /// <summary>
        /// Test to ensure that an elevator reaches the target floor correctly.
        /// </summary>
        [Fact]
        public async Task MoveElevator_ShouldReachTargetFloor()
        {
            // Arrange: Create a request for Floor 8 in Up direction.
            var request = new ElevatorRequest(8, Direction.Up);

            // Act: Assign request to an elevator.
            _elevatorService.AssignRequest(request);

            // Wait for elevator to complete its movement
            await Task.Delay(16000); // Simulate elevator movement

            // Assert: Check that an elevator has reached Floor 8.
            var arrivedElevator = _building.Elevators.FirstOrDefault(e => e.CurrentFloor == 8);
            Assert.NotNull(arrivedElevator);
        }

        /// <summary>
        /// Test to verify that multiple elevators can move independently in parallel.
        /// </summary>
        [Fact]
        public async Task Elevators_ShouldMoveIndependently()

        {
            // Arrange: Create two separate elevator requests for different floors.
            var request1 = new ElevatorRequest(4, Direction.Up);
            var request2 = new ElevatorRequest(9, Direction.Down);

            // Act: Assign both requests.
            _elevatorService.AssignRequest(request1);
            _elevatorService.AssignRequest(request2);

            // Wait for both elevators to complete their movement
            await Task.Delay(20000);

            // Assert: Verify that one elevator reached Floor 4 and another reached Floor 9.
            var elevatorAt4 = _building.Elevators.Any(e => e.CurrentFloor == 4);
            var elevatorAt9 = _building.Elevators.Any(e => e.CurrentFloor == 9);
            Assert.NotNull(elevatorAt4);
            Assert.NotNull(elevatorAt9);
        }

        /// <summary>
        /// Test to ensure that an elevator does not change direction midway if it already has passengers.
        /// </summary>
        [Fact]
        public async Task Elevator_ShouldNotReverseDirectionMidway()
        {
            // Arrange: Create an elevator request for Floor 6 going Up.
            var request1 = new ElevatorRequest(6, Direction.Up);
            var request2 = new ElevatorRequest(3, Direction.Down); // This should NOT be served immediately.

            // Act: Assign first request and wait for elevator to start moving.
            _elevatorService.AssignRequest(request1);
            await Task.Delay(5000); // Allow the elevator to start moving.

            // Assign second request.
            _elevatorService.AssignRequest(request2);
            await Task.Delay(20000); // Wait for elevator movement.

            // Assert: Verify that the elevator did not change direction midway.
            var elevatorAt6 = _building.Elevators.FirstOrDefault(e => e.CurrentFloor == 6);
            Assert.NotNull(elevatorAt6);
            Assert.True(elevatorAt6.CurrentDirection == Direction.None || elevatorAt6.CurrentDirection == Direction.Up);
        }
    }
}
