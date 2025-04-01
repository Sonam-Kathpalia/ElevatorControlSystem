using ElevatorControlSystem.Enum;

namespace ElevatorControlSystem.Models
{
    /// <summary>
    /// Represents a request made by a passenger for an elevator.
    /// </summary>
    public class ElevatorRequest
    {
        /// <summary>
        /// Gets the floor number where the elevator is requested.
        /// </summary>
        public int Floor { get; }

        /// <summary>
        /// Gets the direction in which the passenger wants to travel (Up or Down).
        /// </summary>
        public Direction Direction { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElevatorRequest"/> class.
        /// </summary>
        /// <param name="floor">The floor where the request is made.</param>
        /// <param name="direction">The direction of travel requested (Up or Down).</param>
        public ElevatorRequest(int floor, Direction direction)
        {
            Floor = floor; // Set requested floor
            Direction = direction; // Set requested travel direction
        }
    }
}
