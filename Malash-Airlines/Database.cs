using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using DotNetEnv;
using MySql.Data.MySqlClient;

namespace Malash_Airlines {
    internal static class Database {
        private static string _connectionString;

        static Database() {
            string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".env");
            Env.Load(envPath);
            _connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ?? throw new InvalidOperationException("DATABASE_CONNECTION_STRING is not set in the environment variables.");
        }

        // Existing methods from the original file...
        public static List<Flight> GetAvailableFlights() {
            var flights = new List<Flight>();

            using (var connection = new MySqlConnection(_connectionString)) {
                try {
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
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            flights.Add(new Flight {
                                FlightDetails = $"{Convert.ToInt32(reader["ID"])} : {reader["Departure"]} -> {reader["Destination"]} dnia {reader["Date"].ToString().Substring(0, 10)} o {reader["Time"]}",
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
                } catch (Exception ex) {
                    throw new ApplicationException("Error retrieving available flights", ex);
                }
            }

            return flights;
        }

        public static List<Airport> GetAirports() {
            var airports = new List<Airport>();

            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "SELECT ID, Name, Location FROM airports ORDER BY Location;";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            airports.Add(new Airport {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = reader["Name"].ToString(),
                                Location = reader["Location"].ToString()
                            });
                        }
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error retrieving airports", ex);
                }
            }

            return airports;
        }

        public static List<Plane> GetPlanes() {
            var planes = new List<Plane>();

            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "SELECT ID, Name, SeatsLayout FROM planes ORDER BY ID;";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            planes.Add(new Plane {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = reader["Name"].ToString(),
                                SeatsLayout = reader["SeatsLayout"].ToString()
                            });
                        }
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error retrieving planes", ex);
                }
            }

            return planes;
        }

        public static int AddNewFlight(int departureId, int destinationId, DateTime flightDate,
                                 string time, decimal price, int planeId) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();

                    // Validate airports and plane
                    string validateQuery = @"
                        SELECT 
                            (SELECT COUNT(*) FROM airports WHERE ID = @Departure) +
                            (SELECT COUNT(*) FROM airports WHERE ID = @Destination) +
                            (SELECT COUNT(*) FROM planes WHERE ID = @PlaneID) AS ValidCount;";

                    using (var validateCommand = new MySqlCommand(validateQuery, connection)) {
                        validateCommand.Parameters.AddWithValue("@Departure", departureId);
                        validateCommand.Parameters.AddWithValue("@Destination", destinationId);
                        validateCommand.Parameters.AddWithValue("@PlaneID", planeId);

                        int validCount = Convert.ToInt32(validateCommand.ExecuteScalar());
                        if (validCount != 3) {
                            throw new ArgumentException("Invalid airport or plane IDs");
                        }
                    }

                    // Insert new flight
                    string query = @"
                        INSERT INTO flights (Departure, Destination, Date, Time, Price, PlaneID)
                        VALUES (@Departure, @Destination, @Date, @Time, @Price, @PlaneID);
                        SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@Departure", departureId);
                        command.Parameters.AddWithValue("@Destination", destinationId);
                        command.Parameters.AddWithValue("@Date", flightDate.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@Time", time);
                        command.Parameters.AddWithValue("@Price", price);
                        command.Parameters.AddWithValue("@PlaneID", planeId);

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error adding new flight", ex);
                }
            }
        }

        public static bool ReserveSeat(int userId, int flightId, string seatNumber) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();

                    // Validate user exists
                    string checkUserQuery = "SELECT COUNT(*) FROM users WHERE ID = @UserID";
                    using (var checkUserCommand = new MySqlCommand(checkUserQuery, connection)) {
                        checkUserCommand.Parameters.AddWithValue("@UserID", userId);
                        int userExists = Convert.ToInt32(checkUserCommand.ExecuteScalar());

                        if (userExists == 0) {
                            throw new ArgumentException("User does not exist");
                        }
                    }

                    // Insert reservation
                    string query = @"
                        INSERT INTO reservations (UserID, FlightID, SeatNumber, Status)
                        VALUES (@UserID, @FlightID, @SeatNumber, 'confirmed');";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.Parameters.AddWithValue("@FlightID", flightId);
                        command.Parameters.AddWithValue("@SeatNumber", seatNumber);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error making reservation", ex);
                }
            }
        }

        // New methods from the previous addition...
        public static List<User> GetUsers() {
            var users = new List<User>();

            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "SELECT ID, Name, Email, Role FROM users;";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            users.Add(new User {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                Role = reader["Role"].ToString()
                            });
                        }
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error retrieving users", ex);
                }
            }

            return users;
        }

        public static int AddUser(string name, string email, string password, string role) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = @"
                        INSERT INTO users (Name, Email, Password, Role)
                        VALUES (@Name, @Email, @Password, @Role);
                        SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);
                        command.Parameters.AddWithValue("@Role", role);

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error adding new user", ex);
                }
            }
        }

        public static int AddPlane(string name, string seatsLayout) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = @"
                        INSERT INTO planes (Name, SeatsLayout)
                        VALUES (@Name, @SeatsLayout);
                        SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@SeatsLayout", seatsLayout);

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error adding new plane", ex);
                }
            }
        }

        public static bool RemovePlane(int planeId) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "DELETE FROM planes WHERE ID = @PlaneID;";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@PlaneID", planeId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error removing plane", ex);
                }
            }
        }

        public static int AddAirport(string name, string location) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = @"
                        INSERT INTO airports (Name, Location)
                        VALUES (@Name, @Location);
                        SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@Name", name);
                        command.Parameters.AddWithValue("@Location", location);

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error adding new airport", ex);
                }
            }
        }

        public static bool RemoveAirport(int airportId) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "DELETE FROM airports WHERE ID = @AirportID;";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@AirportID", airportId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error removing airport", ex);
                }
            }
        }

        public static bool RemoveFlight(int flightId) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "DELETE FROM flights WHERE ID = @FlightID;";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@FlightID", flightId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error removing flight", ex);
                }
            }
        }

        public static List<Reservation> GetReservations(int? userId = null, int? flightId = null) {
            var reservations = new List<Reservation>();

            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = @"
                        SELECT ID, UserID, FlightID, SeatNumber, Status 
                        FROM reservations 
                        WHERE 1=1 
                        " + (userId.HasValue ? "AND UserID = @UserID " : "") +
                          (flightId.HasValue ? "AND FlightID = @FlightID" : "") + ";";

                    using (var command = new MySqlCommand(query, connection)) {
                        if (userId.HasValue)
                            command.Parameters.AddWithValue("@UserID", userId.Value);
                        if (flightId.HasValue)
                            command.Parameters.AddWithValue("@FlightID", flightId.Value);

                        using (var reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                reservations.Add(new Reservation {
                                    ID = Convert.ToInt32(reader["ID"]),
                                    UserID = Convert.ToInt32(reader["UserID"]),
                                    FlightID = Convert.ToInt32(reader["FlightID"]),
                                    SeatNumber = reader["SeatNumber"].ToString(),
                                    Status = reader["Status"].ToString()
                                });
                            }
                        }
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error retrieving reservations", ex);
                }
            }

            return reservations;
        }

        public static List<string> GetOccupiedSeatsForFlight(int flightId) {
            var occupiedSeats = new List<string>();
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "SELECT SeatNumber FROM reservations WHERE FlightID = @FlightID AND Status = 'confirmed';";
                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@FlightID", flightId);
                        using (var reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                occupiedSeats.Add(reader["SeatNumber"].ToString());
                            }
                        }
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error retrieving occupied seats", ex);
                }
            }
            return occupiedSeats;
        }

        public static List<Flight> GetFlightsByDepartureAirport(int airportId) {
            var flights = new List<Flight>();
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = @"
                SELECT F.ID, A1.Name AS Departure, A2.Name AS Destination, 
                       F.Date, F.Time, F.Price, P.Name AS Plane
                FROM flights F
                JOIN airports A1 ON F.Departure = A1.ID
                JOIN airports A2 ON F.Destination = A2.ID
                JOIN planes P ON F.PlaneID = P.ID
                WHERE F.Departure = @AirportID AND F.Date >= CURDATE()
                ORDER BY F.Date, F.Time;";
                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@AirportID", airportId);
                        using (var reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                flights.Add(new Flight {
                                    FlightDetails = $"{Convert.ToInt32(reader["ID"])} : {reader["Departure"]} -> {reader["Destination"]} dnia {reader["Date"].ToString().Substring(0, 10)} o {reader["Time"]}",
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
                } catch (Exception ex) {
                    throw new ApplicationException("Error retrieving flights for airport", ex);
                }
            }
            return flights;
        }

        public static bool RemoveReservation(int reservationId) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "DELETE FROM reservations WHERE ID = @ReservationID;";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@ReservationID", reservationId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error removing reservation", ex);
                }
            }
        }
    }

    // Data models
    public class Flight {
        public string FlightDetails { get; set; }
        public int ID { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public decimal Price { get; set; }
        public string Plane { get; set; }
    }

    public class Airport {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
    }

    public class Plane {
        public int ID { get; set; }
        public string Name { get; set; }
        public string SeatsLayout { get; set; }
    }

    // New Data Models
    public class User {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class Reservation {
        public int ID { get; set; }
        public int UserID { get; set; }
        public int FlightID { get; set; }
        public string SeatNumber { get; set; }
        public string Status { get; set; }
    }
}