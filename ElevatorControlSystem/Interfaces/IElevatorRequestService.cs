namespace ElevatorControlSystem.Interfaces
{
    /// <summary>
    /// Interface defining the contract for generating elevator requests.
    /// </summary>
    public interface IElevatorRequestService
    {
        /// <summary>
        /// Generates a new elevator request with a random floor and direction.
        /// </summary>
        void GenerateRequest();
    }
}
