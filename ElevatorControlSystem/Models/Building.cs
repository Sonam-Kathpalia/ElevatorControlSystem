using System.Collections.Generic;

namespace ElevatorControlSystem.Models
{
    /// <summary>
    /// Represents the building containing multiple elevators.
    /// </summary>
    public class Building
    {
        /// <summary>
        /// Gets the total number of floors in the building.
        /// </summary> 
        public int NumberOfFloors { get; } // Total Floors

        /// <summary>
        /// Gets the list of elevators operating in the building.
        /// </summary> 
        public List<Elevator> Elevators { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Building"/> class.
        /// </summary>
        /// <param name="numberOfFloors">Total number of floors in the building.</param>
        /// <param name="numberOfElevators">Total number of elevators in the building.</param>
        public Building(int numberOfFloors, int numberOfElevators)
        {
            NumberOfFloors = numberOfFloors;
            Elevators = new List<Elevator>();

            for (int i = 1; i <= numberOfElevators; i++)
                Elevators.Add(new Elevator(i));
        }
    }
}
