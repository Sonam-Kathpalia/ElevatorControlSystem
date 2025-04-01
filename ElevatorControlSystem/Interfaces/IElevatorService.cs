using ElevatorControlSystem.Models;

namespace ElevatorControlSystem.Interfaces
{
    /// <summary>
    /// Interface defining the contract for elevator service operations.
    /// </summary>
    public interface IElevatorService
    {
        /// <summary>
        /// Assigns an elevator request to the most suitable elevator.
        /// </summary>
        /// <param name="request">The elevator request containing the requested floor and direction.</param>
        void AssignRequest(ElevatorRequest request);
    }
}
