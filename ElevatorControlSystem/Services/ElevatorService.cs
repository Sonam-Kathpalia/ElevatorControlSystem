using ElevatorControlSystem.Enums;
using ElevatorControlSystem.Interfaces;
using ElevatorControlSystem.Models;
using Microsoft.Extensions.Options;
using Serilog;
using System.Collections.Concurrent;

namespace ElevatorControlSystem.Services
{
    /// <summary>
    /// Manages elevator operations including request assignment and movement.
    /// Supports parallel execution and thread-safe operations.
    /// </summary>
    public class ElevatorService : IElevatorService
    {
        private readonly Building _building; // The building containing elevators
        private readonly int _timetakenToMoveBetweenFloors;
        private readonly int _timetakenByPassengersToEnterAndLeave;

        // Thread-safe dictionaries to store elevator stop requests
        private readonly ConcurrentDictionary<int, SortedSet<int>> _upStops;
        private readonly ConcurrentDictionary<int, SortedSet<int>> _downStops;

        /// <summary>
        /// Initializes the elevator service, assigns building reference, and starts movement tasks for each elevator.
        /// </summary>
        /// <param name="building">The building containing elevators</param>
        public ElevatorService(Building building, IOptions<ElevatorSettings> options)
        {
            _building = building;
            _timetakenToMoveBetweenFloors = options.Value.TimetakenToMoveBetweenFloors;
            _timetakenByPassengersToEnterAndLeave = options.Value.TimetakenByPassengersToEnterAndLeave;
            _upStops = new ConcurrentDictionary<int, SortedSet<int>>();
            _downStops = new ConcurrentDictionary<int, SortedSet<int>>();

            foreach (var elevator in _building.Elevators)
            {
                _upStops[elevator.Id] = new SortedSet<int>();
                _downStops[elevator.Id] = new SortedSet<int>();

                // Start elevator movement in a separate task for parallel execution
                Task.Run(() => MoveElevator(elevator));
            }
        }

        /// <summary>
        /// Assigns an elevator request to the best available elevator.
        /// </summary>
        /// <param name="request">The elevator request with target floor and direction</param>
        public Elevator? AssignRequest(ElevatorRequest request)
        {
            try
            {
                var bestElevator = FindBestElevator(request);
                if (bestElevator == null)
                {
                    Log.Warning($"No elevator available for Floor {request.Floor} ({request.Direction})");
                    return bestElevator;
                }

                // If the best elevator is already at the requested floor, we don't need to add it to the stop list
                if (bestElevator.CurrentFloor == request.Floor)
                {
                    Log.Information($"\x1b[31mElevator {bestElevator.Id} is already at Floor {request.Floor}. No movement needed. Doors opening...");
                    return bestElevator;    
                }


                // Add request to the appropriate stop list in a thread-safe manner
                if (request.Direction == Direction.Up)
                {
                    lock (_upStops[bestElevator.Id])
                    {
                        _upStops[bestElevator.Id].Add(request.Floor);
                    }
                }
                else
                {
                    lock (_downStops[bestElevator.Id])
                    {
                        _downStops[bestElevator.Id].Add(request.Floor);
                    }
                }
               
                Log.Information($"Assigned Elevator {bestElevator.Id} to Floor {request.Floor}");

                return bestElevator;
            }
            catch (Exception ex)
            {               
                Log.Error(ex, "Error occurred while assigning a request.");
                return null;
            }
        }

        /// <summary>
        /// Finds the best elevator to handle a given request based on proximity and availability.
        /// </summary>
        /// <param name="request">The elevator request</param>
        /// <returns>The best available elevator or null if no suitable elevator is found</returns>
        private Elevator? FindBestElevator(ElevatorRequest request)
        {
            try
            {
                return _building.Elevators
                .Where(e => e.CurrentDirection == Direction.None || e.CurrentDirection == request.Direction)
                .OrderBy(e => Math.Abs(e.CurrentFloor - request.Floor))
                .FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while finding the best elevator.");
                return null;
            }
        }

        /// <summary>
        /// Moves the elevator to its assigned floors in parallel execution.
        /// Ensures efficient movement without unnecessary direction changes (no "yo-yo" effect).
        /// </summary>
        /// <param name="elevator">The elevator to be moved</param>
        private async Task MoveElevator(Elevator elevator)
        {
            try
            {
                while (true) // Runs indefinitely to continuously process requests
                {
                    // Determine the elevator's direction if it is idle
                    if (elevator.CurrentDirection == Direction.None)
                    {
                        lock (_upStops[elevator.Id])
                        {
                            if (_upStops[elevator.Id].Count > 0)
                            {
                                elevator.CurrentDirection = Direction.Up;
                            }
                            else
                            {
                                lock (_downStops[elevator.Id])
                                {
                                    if (_downStops[elevator.Id].Count > 0)
                                    {
                                        elevator.CurrentDirection = Direction.Down;
                                    }
                                }
                            }
                        }
                    }

                    // Select the current stops based on elevator direction
                    SortedSet<int>? currentStops = elevator.CurrentDirection == Direction.Up
                        ? _upStops[elevator.Id]
                        : _downStops[elevator.Id];

                    // Process each floor request
                    while (currentStops != null && currentStops.Count > 0)
                    {
                        int nextFloor;
                        lock (currentStops)
                        {
                            nextFloor = currentStops.First(); // Get the next closest floor
                            currentStops.Remove(nextFloor); // Remove it from the stop list
                        }

                        // Move the elevator floor by floor
                        while (elevator.CurrentFloor != nextFloor)
                        {
                            elevator.CurrentFloor += elevator.CurrentFloor < nextFloor ? 1 : -1;
                            Log.Information($"Elevator {elevator.Id} moving to Floor {elevator.CurrentFloor}");
                            await Task.Delay(_timetakenToMoveBetweenFloors * 1000); // Simulate time taken to move between floors 
                        }

                        // Open doors and simulate passenger entry/exit
                        Log.Information($"\x1b[32mElevator {elevator.Id} arrived at Floor {elevator.CurrentFloor}. Doors opening...");
                        await Task.Delay(_timetakenByPassengersToEnterAndLeave * 1000); // Simulate door opening , passenger entering/leaving and door closing time
                    }

                    // Prevent "Yo-Yo" movement: Elevators continue in the same direction if more requests exist
                    if (elevator.CurrentDirection == Direction.Up)
                    {
                        lock (_downStops[elevator.Id])
                        {
                            if (_downStops[elevator.Id].Count > 0)
                            {
                                elevator.CurrentDirection = Direction.Down;
                            }
                            else
                            {
                                elevator.CurrentDirection = Direction.None;
                            }
                        }
                    }
                    else if (elevator.CurrentDirection == Direction.Down)
                    {
                        lock (_upStops[elevator.Id])
                        {
                            if (_upStops[elevator.Id].Count > 0)
                            {
                                elevator.CurrentDirection = Direction.Up;
                            }
                            else
                            {
                                elevator.CurrentDirection = Direction.None;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occurred while moving elevator.");
            }
        }
    }
}