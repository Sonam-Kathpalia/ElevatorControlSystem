using ElevatorControlSystem.Enums;

namespace ElevatorControlSystem.Models
{
    /// <summary>
    /// Represents an individual elevator in the building.
    /// </summary>
    public class Elevator
    {
        /// <summary>
        /// Gets the unique identifier for the elevator.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets or sets the current floor where the elevator is located.
        /// </summary>
        public int CurrentFloor { get; set; }

        /// <summary>
        /// Gets or sets the current direction in which the elevator is moving.
        /// </summary>
        public Direction CurrentDirection { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Elevator"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the elevator.</param>
        public Elevator(int id)
        {
            Id = id;
            CurrentFloor = 0; // Default starting position at ground floor (floor 0)
            CurrentDirection = Direction.None; // Initially, the elevator is stationary
        }
    }
}
