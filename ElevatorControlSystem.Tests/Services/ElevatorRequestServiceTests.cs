using ElevatorControlSystem.Enum;
using ElevatorControlSystem.Interfaces;
using ElevatorControlSystem.Models;
using ElevatorControlSystem.Services;
using Microsoft.Extensions.Options;
using Moq;

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
            Assert.InRange(capturedRequest.Floor, 0, _elevatorSettings.NumberOfFloors);
        }
        
    }
}
