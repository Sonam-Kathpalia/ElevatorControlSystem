/// <summary>
/// Holds configuration settings for the elevator system.
/// </summary>
public class ElevatorSettings
{
    /// <summary>
    /// Total number of floors in the building.
    /// </summary>
    public int NumberOfFloors { get; set; }

    /// <summary>
    /// Total number of elevators in the building.
    /// </summary>
    public int NumberOfElevators { get; set; }

    /// <summary>
    /// Delay time (in seconds) between generating elevator requests.
    /// </summary>
    public int RequestDelayInSeconds { get; set; }

    /// <summary>
    /// Time taken by elevators to move from one floor to next.
    /// </summary>
    public int TimetakenToMoveBetweenFloors { get; set; }
    /// <summary>
    /// Time taken by passengers to enter/leave and elevator door opening and closing.
    /// </summary>
    public int TimetakenByPassengersToEnterAndLeave { get; set; }
}

