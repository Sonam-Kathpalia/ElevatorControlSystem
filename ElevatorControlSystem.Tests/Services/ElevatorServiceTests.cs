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
        public void AssignRequest_ShouldAssignToNearestElevator()
        {
            // Arrange: Create an elevator request for Floor 5 in Up direction.
            var request = new ElevatorRequest(5, Direction.Up);
            var elevators = new List<Elevator> { new Elevator(1), new Elevator(2), new Elevator(3), new Elevator(4) };

            // Act: Assign the request to an elevator.
            var assignedElevator = _elevatorService.AssignRequest(request);

            // Assert: Verify that the nearest available elevator is assigned to requested floor 5
            Assert.NotNull(assignedElevator);
            Assert.Equal(assignedElevator.Id , elevators[0].Id);
        }

        /// <summary>
        /// Test to verify that no elevator should be assigned if unavailable
        /// </summary>

        [Fact]
        public void AssignRequest_ShouldNotAssignIfNoElevatorAvailable()
        {
            // Arrange
            foreach (var elevator in _building.Elevators)
                elevator.CurrentDirection = Direction.Up; // All elevators are occupied

            var request = new ElevatorRequest(7, Direction.Up);

            // Act
            _elevatorService.AssignRequest(request);

            // Assert
            Assert.All(_building.Elevators, e => Assert.NotEqual(e.CurrentFloor, 5));
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
        /// Test to verify that the elevator moves correctly towards the requested floor when assigned a request
        /// </summary>

        [Fact]
        public void MoveElevator_ShouldMoveCorrectly()
        {
            // Arrange
            var elevator = _building.Elevators.First();
            var initialFloor = elevator.CurrentFloor;

            // Act
            Task.Run(() => _elevatorService.AssignRequest(new ElevatorRequest(5, Direction.Up)));
            Task.Delay(3000).Wait(); // Simulate some time passing

            // Assert
            Assert.True(elevator.CurrentFloor >= initialFloor);
        }    
    }
}
