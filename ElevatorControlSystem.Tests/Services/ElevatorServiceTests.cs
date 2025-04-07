using ElevatorControlSystem.Enums;
using ElevatorControlSystem.Models;
using ElevatorControlSystem.Services;
using Microsoft.Extensions.Options;

namespace ElevatorControlSystem.Tests
{
    public class ElevatorServiceTests
    {
        private readonly Building _building;
        private readonly ElevatorService _elevatorService;
        private readonly ElevatorSettings _elevatorSettings;

        public ElevatorServiceTests()
        {
            _building = new Building(10, 4);
            _elevatorSettings = new ElevatorSettings 
            { 
                NumberOfFloors = 10,
                TimetakenToMoveBetweenFloors = 10,
                TimetakenByPassengersToEnterAndLeave = 10
            };
            var options = Options.Create(_elevatorSettings);
            _elevatorService = new ElevatorService(_building, options);
        }

        /// <summary>
        /// Verifies that a request is assigned to the closest available elevator.
        /// </summary>
        [Fact]
        public void AssignRequest_ShouldAssignToNearestElevator()
        {
            // Arrange
            var request = new ElevatorRequest(5, Direction.Up);
            var elevator = _building.Elevators.First();
            elevator.CurrentFloor = 3;
            elevator.CurrentDirection = Direction.None;

            // Act
            var assignedElevator = _elevatorService.AssignRequest(request);

            // Assert
            Assert.NotNull(assignedElevator);
            Assert.Equal(elevator.Id, assignedElevator.Id);
        }

        /// <summary>
        /// Confirms that no elevator is assigned when all elevators are busy in opposite direction.
        /// </summary>
        [Fact]
        public void AssignRequest_ShouldNotAssignIfNoElevatorAvailable()
        {
            // Arrange
            foreach (var elevator in _building.Elevators)
            {
                elevator.CurrentDirection = Direction.Up;
                elevator.CurrentFloor = 7;
            }

            var request = new ElevatorRequest(5, Direction.Down);

            // Act
            var assignedElevator = _elevatorService.AssignRequest(request);

            // Assert
            Assert.Null(assignedElevator);
        }

        /// <summary>
        /// Ensures that an elevator already at the requested floor is assigned without movement.
        /// </summary>
        [Fact]
        public void AssignRequest_ShouldHandleSameFloorRequest()
        {
            // Arrange
            var elevator = _building.Elevators.First();
            elevator.CurrentFloor = 5;
            elevator.CurrentDirection = Direction.None;
            var request = new ElevatorRequest(5, Direction.Up);

            // Act
            var assignedElevator = _elevatorService.AssignRequest(request);

            // Assert
            Assert.NotNull(assignedElevator);
            Assert.Equal(elevator.Id, assignedElevator.Id);
            Assert.Equal(5, assignedElevator.CurrentFloor);
        }

        /// <summary>
        /// Validates that an idle elevator is preferred over a moving one for new requests.
        /// </summary>
        [Fact]
        public void AssignRequest_ShouldPreferIdleElevator()
        {
            // Arrange
            var elevator1 = _building.Elevators[0];
            var elevator2 = _building.Elevators[1];
            
            elevator1.CurrentFloor = 3;
            elevator1.CurrentDirection = Direction.Up;
            
            elevator2.CurrentFloor = 4;
            elevator2.CurrentDirection = Direction.None;

            var request = new ElevatorRequest(5, Direction.Up);

            // Act
            var assignedElevator = _elevatorService.AssignRequest(request);

            // Assert
            Assert.NotNull(assignedElevator);
            Assert.Equal(elevator2.Id, assignedElevator.Id);
        }

        /// <summary>
        /// Checks that an elevator moving in the same direction as the request is preferred.
        /// </summary>
        [Fact]
        public void AssignRequest_ShouldPreferSameDirectionElevator()
        {
            // Arrange
            var elevator1 = _building.Elevators[0];
            var elevator2 = _building.Elevators[1];
            
            elevator1.CurrentFloor = 3;
            elevator1.CurrentDirection = Direction.Up;
            
            elevator2.CurrentFloor = 3;
            elevator2.CurrentDirection = Direction.Down;

            var request = new ElevatorRequest(5, Direction.Up);

            // Act
            var assignedElevator = _elevatorService.AssignRequest(request);

            // Assert
            Assert.NotNull(assignedElevator);
            Assert.Equal(elevator1.Id, assignedElevator.Id);
        }

        /// <summary>
        /// Confirms that the nearest elevator is selected when multiple elevators are available.
        /// </summary>
        [Fact]
        public void AssignRequest_ShouldPreferCloserElevator()
        {
            // Arrange
            var elevator1 = _building.Elevators[0];
            var elevator2 = _building.Elevators[1];
            
            elevator1.CurrentFloor = 2;
            elevator1.CurrentDirection = Direction.None;
            
            elevator2.CurrentFloor = 8;
            elevator2.CurrentDirection = Direction.None;

            var request = new ElevatorRequest(3, Direction.Up);

            // Act
            var assignedElevator = _elevatorService.AssignRequest(request);

            // Assert
            Assert.NotNull(assignedElevator);
            Assert.Equal(elevator1.Id, assignedElevator.Id);
        }

        /// <summary>
        /// Verifies proper handling of requests at the top and bottom floors of the building.
        /// </summary>
        [Fact]
        public void AssignRequest_ShouldHandleEdgeFloors()
        {
            // Arrange
            var elevator = _building.Elevators.First();
            elevator.CurrentDirection = Direction.None;

            // Test bottom floor
            elevator.CurrentFloor = 0;
            var upRequest = new ElevatorRequest(0, Direction.Up);
            
            // Act & Assert for bottom floor
            var assignedElevator1 = _elevatorService.AssignRequest(upRequest);
            Assert.NotNull(assignedElevator1);
            Assert.Equal(elevator.Id, assignedElevator1.Id);

            // Test top floor
            elevator.CurrentFloor = 9;
            var downRequest = new ElevatorRequest(9, Direction.Down);
            
            // Act & Assert for top floor
            var assignedElevator2 = _elevatorService.AssignRequest(downRequest);
            Assert.NotNull(assignedElevator2);
            Assert.Equal(elevator.Id, assignedElevator2.Id);
        }

        /// <summary>
        /// Ensures that multiple requests can be assigned to the same elevator sequentially.
        /// </summary>
        [Fact]
        public void AssignRequest_ShouldHandleMultipleRequests()
        {
            // Arrange
            var elevator = _building.Elevators.First();
            elevator.CurrentFloor = 0;
            elevator.CurrentDirection = Direction.None;

            var requests = new List<ElevatorRequest>
            {
                new ElevatorRequest(2, Direction.Up),
                new ElevatorRequest(4, Direction.Up),
                new ElevatorRequest(6, Direction.Up)
            };

            // Act
            var assignedElevators = requests.Select(r => _elevatorService.AssignRequest(r)).ToList();

            // Assert
            Assert.All(assignedElevators, e => Assert.NotNull(e));
            Assert.All(assignedElevators, e => Assert.Equal(elevator.Id, e.Id));
        }
    }
}
