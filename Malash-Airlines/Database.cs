using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Malash_Airlines
{
    public class Database
    {
        private readonly string _connectionString;

        public Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Flight> GetAvailableFlights()
        {
            var flights = new List<Flight>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        SELECT F.ID, A1.Name AS Departure, A2.Name AS Destination, 
                               F.Date, F.Time, F.Price, P.Name AS Plane
                        FROM flights F
                        JOIN airports A1 ON F.Departure = A1.ID
                        JOIN airports A2 ON F.Destination = A2.ID
                        JOIN planes P ON F.PlaneID = P.ID
                        WHERE F.Date >= CURDATE()
                        ORDER BY F.Date, F.Time;";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            flights.Add(new Flight
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Departure = reader["Departure"].ToString(),
                                Destination = reader["Destination"].ToString(),
                                Date = Convert.ToDateTime(reader["Date"]),
                                Time = reader["Time"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Plane = reader["Plane"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Error retrieving available flights", ex);
                }
            }

            return flights;
        }

        public List<Airport> GetAirports()
        {
            var airports = new List<Airport>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT ID, Name, Location FROM airports ORDER BY Location;";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            airports.Add(new Airport
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = reader["Name"].ToString(),
                                Location = reader["Location"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Error retrieving airports", ex);
                }
            }

            return airports;
        }

        public List<Plane> GetPlanes()
        {
            var planes = new List<Plane>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT ID, Name, SeatsLayout FROM planes ORDER BY ID;";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            planes.Add(new Plane
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = reader["Name"].ToString(),
                                SeatsLayout = reader["SeatsLayout"].ToString()
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Error retrieving planes", ex);
                }
            }

            return planes;
        }

        public int AddNewFlight(int departureId, int destinationId, DateTime flightDate,
                                 string time, decimal price, int planeId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // Validate airports and plane
                    string validateQuery = @"
                        SELECT 
                            (SELECT COUNT(*) FROM airports WHERE ID = @Departure) +
                            (SELECT COUNT(*) FROM airports WHERE ID = @Destination) +
                            (SELECT COUNT(*) FROM planes WHERE ID = @PlaneID) AS ValidCount;";

                    using (var validateCommand = new MySqlCommand(validateQuery, connection))
                    {
                        validateCommand.Parameters.AddWithValue("@Departure", departureId);
                        validateCommand.Parameters.AddWithValue("@Destination", destinationId);
                        validateCommand.Parameters.AddWithValue("@PlaneID", planeId);

                        int validCount = Convert.ToInt32(validateCommand.ExecuteScalar());
                        if (validCount != 3)
                        {
                            throw new ArgumentException("Invalid airport or plane IDs");
                        }
                    }

                    // Insert new flight
                    string query = @"
                        INSERT INTO flights (Departure, Destination, Date, Time, Price, PlaneID)
                        VALUES (@Departure, @Destination, @Date, @Time, @Price, @PlaneID);
                        SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Departure", departureId);
                        command.Parameters.AddWithValue("@Destination", destinationId);
                        command.Parameters.AddWithValue("@Date", flightDate.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@Time", time);
                        command.Parameters.AddWithValue("@Price", price);
                        command.Parameters.AddWithValue("@PlaneID", planeId);

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Error adding new flight", ex);
                }
            }
        }

        public bool ReserveSeat(int userId, int flightId, string seatNumber)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // Validate user exists
                    string checkUserQuery = "SELECT COUNT(*) FROM users WHERE ID = @UserID";
                    using (var checkUserCommand = new MySqlCommand(checkUserQuery, connection))
                    {
                        checkUserCommand.Parameters.AddWithValue("@UserID", userId);
                        int userExists = Convert.ToInt32(checkUserCommand.ExecuteScalar());

                        if (userExists == 0)
                        {
                            throw new ArgumentException("User does not exist");
                        }
                    }

                    // Insert reservation
                    string query = @"
                        INSERT INTO reservations (UserID, FlightID, SeatNumber, Status)
                        VALUES (@UserID, @FlightID, @SeatNumber, 'confirmed');";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.Parameters.AddWithValue("@FlightID", flightId);
                        command.Parameters.AddWithValue("@SeatNumber", seatNumber);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Error making reservation", ex);
                }
            }
        }
    }

    // Data models
    public class Flight
    {
        public int ID { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public decimal Price { get; set; }
        public string Plane { get; set; }
    }

    public class Airport
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
    }

    public class Plane
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string SeatsLayout { get; set; }
    }
}