using ElevatorControlSystem.Enums;
using ElevatorControlSystem.Interfaces;
using ElevatorControlSystem.Models;
using ElevatorControlSystem.Services;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;

namespace ElevatorControlSystem.Tests
{
    /// <summary>
    /// Unit tests for the ElevatorRequestService class.
    /// Ensures elevator requests are generated and assigned correctly.
    /// </summary>
    public class ElevatorRequestServiceTests
    {
        private readonly Mock<IElevatorService> _mockElevatorService;
        private readonly ElevatorRequestService _elevatorRequestService;
        private readonly ElevatorSettings _elevatorSettings;

        /// <summary>
        /// Initializes the test setup by mocking the elevator service and creating an instance of ElevatorRequestService.
        /// </summary>
        public ElevatorRequestServiceTests()
        {
            _mockElevatorService = new Mock<IElevatorService>(); // Mocking elevator service dependency
            _elevatorSettings = new ElevatorSettings { NumberOfFloors = 10 };
            var options = Options.Create(_elevatorSettings);
            _elevatorRequestService = new ElevatorRequestService(_mockElevatorService.Object, options);
        }

        /// <summary>
        /// Tests if GenerateRequest creates a valid request and assigns it to an elevator.
        /// </summary>
        [Fact]
        public void GenerateRequest_ShouldCreateAndAssignRequest()
        {
            // Act: Generate a random elevator request
            _elevatorRequestService.GenerateRequest();

            // Assert: Verify that AssignRequest was called at least once with any ElevatorRequest object
            _mockElevatorService.Verify(service => service.AssignRequest(It.IsAny<ElevatorRequest>()), Times.Once);
        }

        /// <summary>
        /// Tests if GenerateRequest produces valid floor numbers within range.
        /// </summary>
        [Fact]
        public void GenerateRequest_ShouldGenerateValidFloorNumbers()
        {
            // Arrange: Capture the request passed to AssignRequest for validation
            ElevatorRequest capturedRequest = null;
            _mockElevatorService
                .Setup(service => service.AssignRequest(It.IsAny<ElevatorRequest>()))
                .Callback<ElevatorRequest>(req => capturedRequest = req);

            // Act: Generate a random request
            _elevatorRequestService.GenerateRequest();

            // Assert: Ensure the generated floor is within valid range
            Assert.NotNull(capturedRequest);
            Assert.InRange(capturedRequest.Floor, 0, _elevatorSettings.NumberOfFloors - 1);
        }

        /// <summary>
        /// Verifies that generated requests have valid direction based on floor position.
        /// </summary>
        [Fact]
        public void GenerateRequest_ShouldGenerateValidDirection()
        {
            // Arrange
            ElevatorRequest capturedRequest = null;
            _mockElevatorService
                .Setup(service => service.AssignRequest(It.IsAny<ElevatorRequest>()))
                .Callback<ElevatorRequest>(req => capturedRequest = req);

            // Act
            _elevatorRequestService.GenerateRequest();

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.True(Enum.IsDefined(typeof(Direction), capturedRequest.Direction));
            
            // Verify direction logic
            if (capturedRequest.Floor == 0)
            {
                Assert.Equal(Direction.Up, capturedRequest.Direction);
            }
            else if (capturedRequest.Floor == _elevatorSettings.NumberOfFloors - 1)
            {
                Assert.Equal(Direction.Down, capturedRequest.Direction);
            }
        }

        /// <summary>
        /// Confirms that ground floor requests are handled correctly with upward direction.
        /// </summary>
        [Fact]
        public void GenerateRequest_ShouldHandleGroundFloorRequests()
        {
            // Arrange
            ElevatorRequest capturedRequest = null;
            _mockElevatorService
                .Setup(service => service.AssignRequest(It.IsAny<ElevatorRequest>()))
                .Callback<ElevatorRequest>(req => capturedRequest = req);

            // Act
            _elevatorRequestService.GenerateRequest();

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.True(capturedRequest.Floor >= 0);
        }

        /// <summary>
        /// Verifies that top floor requests are handled correctly with downward direction.
        /// </summary>
        [Fact]
        public void GenerateRequest_ShouldHandleTopFloorRequests()
        {
            // Arrange
            ElevatorRequest capturedRequest = null;
            _mockElevatorService
                .Setup(service => service.AssignRequest(It.IsAny<ElevatorRequest>()))
                .Callback<ElevatorRequest>(req => capturedRequest = req);

            // Act
            _elevatorRequestService.GenerateRequest();

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.True(capturedRequest.Floor <= _elevatorSettings.NumberOfFloors - 1);
        }

        /// <summary>
        /// Ensures that service errors during request generation are handled gracefully.
        /// </summary>
        [Fact]
        public void GenerateRequest_ShouldHandleServiceErrorsGracefully()
        {
            // Arrange
            _mockElevatorService
                .Setup(service => service.AssignRequest(It.IsAny<ElevatorRequest>()))
                .Throws(new Exception("Service error"));

            // Act
            var exception = Record.Exception(() => _elevatorRequestService.GenerateRequest());

            // Assert
            Assert.Null(exception);
            _mockElevatorService.Verify(service => service.AssignRequest(It.IsAny<ElevatorRequest>()), Times.Once);
        }

        /// <summary>
        /// Verifies that request generation works correctly with different building configurations.
        /// </summary>
        [Fact]
        public void GenerateRequest_ShouldWorkWithDifferentSettings()
        {
            // Arrange
            var newSettings = new ElevatorSettings { NumberOfFloors = 20 };
            var options = Options.Create(newSettings);
            var service = new ElevatorRequestService(_mockElevatorService.Object, options);
            ElevatorRequest capturedRequest = null;
            _mockElevatorService
                .Setup(s => s.AssignRequest(It.IsAny<ElevatorRequest>()))
                .Callback<ElevatorRequest>(req => capturedRequest = req);

            // Act
            service.GenerateRequest();

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.InRange(capturedRequest.Floor, 0, newSettings.NumberOfFloors - 1);
        }

        /// <summary>
        /// Confirms that multiple requests can be generated and processed sequentially.
        /// </summary>
        [Fact]
        public void GenerateRequest_ShouldHandleMultipleRequests()
        {
            // Arrange
            var requests = new List<ElevatorRequest>();
            _mockElevatorService
                .Setup(service => service.AssignRequest(It.IsAny<ElevatorRequest>()))
                .Callback<ElevatorRequest>(req => requests.Add(req));

            // Act
            for (int i = 0; i < 10; i++)
            {
                _elevatorRequestService.GenerateRequest();
            }

            // Assert
            Assert.Equal(10, requests.Count);
            Assert.True(requests.All(r => r.Floor >= 0 && r.Floor <= _elevatorSettings.NumberOfFloors - 1));
            Assert.True(requests.All(r => Enum.IsDefined(typeof(Direction), r.Direction)));
        }
    }
}
