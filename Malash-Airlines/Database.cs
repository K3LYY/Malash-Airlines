using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
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

        public static List<Flight> GetAvailableFlights() {
            var flights = new List<Flight>();

            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = @"
                            SELECT F.ID, A1.Name AS Departure, A2.Name AS Destination, 
                                   F.Date, F.Time, F.Price, P.Name AS Plane, F.FlightType
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
                                Plane = reader["Plane"].ToString(),
                                FlightType = reader["FlightType"].ToString()
                            });
                        }
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error retrieving available flights: " + ex.Message, ex);
                }
            }

            return flights;
        }

        public static List<Airport> GetAirports() {
            var airports = new List<Airport>();

            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "SELECT ID, Name, Location, GatesCount FROM airports ORDER BY Location;";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            airports.Add(new Airport {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = reader["Name"].ToString(),
                                Location = reader["Location"].ToString(),
                                GatesCount = Convert.ToInt32(reader["GatesCount"])
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

        public static List<Flight> GetSoonestFlights(int limit = 5) {
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
                    WHERE F.Date >= CURDATE() OR (F.Date = CURDATE() AND F.Time >= CURTIME())
                    ORDER BY F.Date, F.Time
                    LIMIT @Limit;";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@Limit", limit);
                        using (var reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                flights.Add(new Flight {
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
                    throw new ApplicationException("Error retrieving soonest flights", ex);
                }
            }

            return flights;
        }

        public static int AddNewFlight(int departureId, int destinationId, DateTime flightDate,
                                 string time, decimal price, int planeId, string FlightType = "public") {
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
                        INSERT INTO flights (Departure, Destination, Date, Time, Price, PlaneID, FlightType)
                        VALUES (@Departure, @Destination, @Date, @Time, @Price, @PlaneID, @FlightType);
                        SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@Departure", departureId);
                        command.Parameters.AddWithValue("@Destination", destinationId);
                        command.Parameters.AddWithValue("@Date", flightDate.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@Time", time);
                        command.Parameters.AddWithValue("@Price", price);
                        command.Parameters.AddWithValue("@PlaneID", planeId);
                        command.Parameters.AddWithValue("@FlightType", FlightType);

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

                    // Sprawdź, czy lot nie jest prywatny
                    string checkFlightTypeQuery = "SELECT FlightType FROM flights WHERE ID = @FlightID";
                    using (var checkFlightTypeCommand = new MySqlCommand(checkFlightTypeQuery, connection)) {
                        checkFlightTypeCommand.Parameters.AddWithValue("@FlightID", flightId);
                        var flightType = checkFlightTypeCommand.ExecuteScalar()?.ToString();

                        if (flightType?.ToLower() == "private") {
                            throw new ArgumentException("Cannot reserve seats on private flights");
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

        public static int AddReservation(int userId, int flightId, string seatNumber, decimal price, string status = "unconfirmed") {
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    connection.Open();

                    // Sprawdzamy, czy rezerwacja "FULL" już istnieje
                    if (seatNumber == "FULL") {
                        string checkQuery = "SELECT COUNT(*) FROM reservations WHERE FlightID = @FlightID";
                        using (var checkCommand = new MySqlCommand(checkQuery, connection)) {
                            checkCommand.Parameters.AddWithValue("@FlightID", flightId);
                            int existingReservations = Convert.ToInt32(checkCommand.ExecuteScalar());
                            if (existingReservations > 0) return -1;
                        }
                    } else {
                        // Sprawdzenie czy miejsce nie jest zajęte / nie ma FULL
                        string checkSeatQuery = "SELECT COUNT(*) FROM reservations WHERE FlightID = @FlightID AND SeatNumber = @SeatNumber";
                        using (var seatCmd = new MySqlCommand(checkSeatQuery, connection)) {
                            seatCmd.Parameters.AddWithValue("@FlightID", flightId);
                            seatCmd.Parameters.AddWithValue("@SeatNumber", seatNumber);
                            if (Convert.ToInt32(seatCmd.ExecuteScalar()) > 0) return -1;
                        }

                        string checkFullQuery = "SELECT COUNT(*) FROM reservations WHERE FlightID = @FlightID AND SeatNumber = 'FULL'";
                        using (var fullCmd = new MySqlCommand(checkFullQuery, connection)) {
                            fullCmd.Parameters.AddWithValue("@FlightID", flightId);
                            if (Convert.ToInt32(fullCmd.ExecuteScalar()) > 0) return -1;
                        }
                    }

                    // Dodaj rezerwację z wybranym statusem
                    string insertQuery = @"
                INSERT INTO reservations (UserID, FlightID, SeatNumber, Status)
                VALUES (@UserID, @FlightID, @SeatNumber, @Status);
                SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(insertQuery, connection)) {
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.Parameters.AddWithValue("@FlightID", flightId);
                        command.Parameters.AddWithValue("@SeatNumber", seatNumber);
                        command.Parameters.AddWithValue("@Status", status);

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            } catch (Exception ex) {
                throw new ApplicationException($"Error adding reservation: {ex.Message}", ex);
            }
        }

        public static bool UpdateReservation(int reservationId, string newStatus) {
            try {
                using (var connection = new MySqlConnection(_connectionString)) {
                    connection.Open();
                    string updateQuery = @"
                UPDATE reservations
                SET Status = @Status
                WHERE ID = @ReservationID;";

                    using (var command = new MySqlCommand(updateQuery, connection)) {
                        command.Parameters.AddWithValue("@Status", newStatus);
                        command.Parameters.AddWithValue("@ReservationID", reservationId);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            } catch (Exception ex) {
                throw new ApplicationException($"Error updating reservation: {ex.Message}", ex);
            }
        }

        public static List<ReservationViewModel> GetUnconfirmedReservations() {
            var result = new List<ReservationViewModel>();
            try {
                var unconfirmedReservations = GetReservations().Where(r => r.Status.ToLower() == "unconfirmed").ToList();
                var userList = GetUsers();
                var flightList = GetAvailableFlights();

                foreach (var res in unconfirmedReservations) {
                    var user = userList.FirstOrDefault(u => u.ID == res.UserID);
                    var flight = flightList.FirstOrDefault(f => f.ID == res.FlightID)
                        ?? GetFlightById(res.FlightID); // Próba pobrania lotu, nawet jeśli nie jest już dostępny

                    if (user != null && flight != null) {
                        result.Add(new ReservationViewModel {
                            ReservationID = res.ID,
                            UserName = user.Name,
                            UserEmail = user.Email,
                            FlightDetails = $"{flight.Departure} -> {flight.Destination}, {flight.Date:yyyy-MM-dd} {flight.Time}",
                            SeatNumber = res.SeatNumber,
                            Status = res.Status,
                            UserID = res.UserID,
                            FlightID = res.FlightID
                        });
                    }
                }
            } catch (Exception ex) {
                throw new ApplicationException("Error retrieving unconfirmed reservations", ex);
            }
            return result;
        }

        public static bool UpdateUserRole(int userId, string newRole) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "UPDATE users SET Role = @Role WHERE ID = @UserID;";
                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@Role", newRole);
                        command.Parameters.AddWithValue("@UserID", userId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                } catch (Exception ex) {
                    throw new ApplicationException($"Error updating user role: {ex.Message}", ex);
                }
            }
        }

        public static List<User> GetUsers() {
            var users = new List<User>();

            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "SELECT ID, Name, Email, Role, CustomerType FROM users;";

                    using (var command = new MySqlCommand(query, connection))
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            users.Add(new User {
                                ID = Convert.ToInt32(reader["ID"]),
                                Name = reader["Name"].ToString(),
                                Email = reader["Email"].ToString(),
                                Role = reader["Role"].ToString(),
                                CustomerType = reader["CustomerType"].ToString()
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
                        INSERT INTO airports (Name, Location, GatesCount)
                        VALUES (@Name, @Location, 5);
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
                    throw new ApplicationException("Error removing airport: " + ex.ToString(), ex);
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
                       F.Date, F.Time, F.Price, P.Name AS Plane, F.FlightType
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
                                    Plane = reader["Plane"].ToString(),
                                    FlightType = reader["FlightType"].ToString()
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

        public static bool UpdateUserCustomerType(int userId, string newCustomerType) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "UPDATE users SET CustomerType = @CustomerType WHERE ID = @UserID;";
                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@CustomerType", newCustomerType);
                        command.Parameters.AddWithValue("@UserID", userId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                } catch (Exception ex) {
                    string innerErrorMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                    throw new ApplicationException($"Error updating user customer type: {ex.Message}. Inner error: {innerErrorMessage}", ex);
                }
            }
        }

        public static string GetUserCustomerType(int userId) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "SELECT CustomerType FROM users WHERE ID = @UserID;";
                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@UserID", userId);
                        var result = command.ExecuteScalar();
                        return result != null ? result.ToString() : "";
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Error getting user customer type", ex);
                }
            }
        }

        public static Flight GetFlightById(int flightId) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = @"
                        SELECT f.ID, a1.Name as Departure, a2.Name as Destination, 
                               f.Date, f.Time, f.Price, p.Name as Plane, f.FlightType
                        FROM flights f
                        JOIN airports a1 ON f.Departure = a1.ID
                        JOIN airports a2 ON f.Destination = a2.ID
                        JOIN planes p ON f.PlaneID = p.ID
                        WHERE f.ID = @FlightID;";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@FlightID", flightId);
                        using (var reader = command.ExecuteReader()) {
                            if (reader.Read()) {
                                return new Flight {
                                    ID = Convert.ToInt32(reader["ID"]),
                                    Departure = reader["Departure"].ToString(),
                                    Destination = reader["Destination"].ToString(),
                                    Date = Convert.ToDateTime(reader["Date"]),
                                    Time = reader["Time"].ToString(),
                                    Price = Convert.ToDecimal(reader["Price"]),
                                    Plane = reader["Plane"].ToString(),
                                    FlightType = reader["FlightType"].ToString()
                                };
                            }
                        }
                    }
                } catch (Exception ex) {
                    throw new ApplicationException($"Error retrieving flight with ID {flightId}: {ex.Message}", ex);
                }
            }
            return null;
        }

        public static int AddInvoice(Invoice invoice) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();

                    // Check if reservation exists
                    string checkReservationQuery = "SELECT COUNT(*) FROM reservations WHERE ID = @ReservationID";
                    using (var checkCommand = new MySqlCommand(checkReservationQuery, connection)) {
                        checkCommand.Parameters.AddWithValue("@ReservationID", invoice.ReservationID);
                        int reservationExists = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (reservationExists == 0) {
                            throw new ArgumentException("Rezerwacja nie istnieje");
                        }
                    }

                    // Generate a unique invoice number (format: INV-YYYYMMDD-XXXX) if not provided
                    string invoiceNumber = invoice.InvoiceNumber;
                    if (string.IsNullOrEmpty(invoiceNumber)) {
                        invoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

                        // Check if invoice number is unique
                        string checkInvoiceQuery = "SELECT COUNT(*) FROM invoices WHERE InvoiceNumber = @InvoiceNumber";
                        using (var checkCommand = new MySqlCommand(checkInvoiceQuery, connection)) {
                            checkCommand.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
                            while (Convert.ToInt32(checkCommand.ExecuteScalar()) > 0) {
                                // If not unique, generate a new one
                                invoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
                                checkCommand.Parameters["@InvoiceNumber"].Value = invoiceNumber;
                            }
                        }
                    }

                    // Insert new invoice
                    string query = @"
                INSERT INTO invoices (ReservationID, Amount, Status, IssueDate, DueDate, InvoiceNumber, Notes)
                VALUES (@ReservationID, @Amount, @Status, @IssueDate, @DueDate, @InvoiceNumber, @Notes);
                SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@ReservationID", invoice.ReservationID);
                        command.Parameters.AddWithValue("@Amount", invoice.Amount);
                        command.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(invoice.Status) ? "unpaid" : invoice.Status);
                        command.Parameters.AddWithValue("@IssueDate", invoice.IssueDate.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@DueDate", invoice.DueDate.ToString("yyyy-MM-dd"));
                        command.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
                        command.Parameters.AddWithValue("@Notes", invoice.Notes ?? (object)DBNull.Value);

                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                } catch (Exception ex) {
                    throw new ApplicationException($"Błąd dodawania faktury: {ex.Message}", ex);
                }
            }
        }

        public static bool RemoveInvoice(int invoiceId) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = "DELETE FROM invoices WHERE ID = @InvoiceID;";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@InvoiceID", invoiceId);
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                } catch (Exception ex) {
                    throw new ApplicationException($"Błąd usuwania faktury: {ex.Message}", ex);
                }
            }
        }

        public static bool UpdateInvoice(Invoice invoice) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();

                    // Build dynamic update query
                    var queryBuilder = new System.Text.StringBuilder("UPDATE invoices SET ");
                    var parameters = new List<string>();

                    // Check which properties should be updated
                    if (invoice.Amount > 0) {
                        parameters.Add("Amount = @Amount");
                    }

                    if (!string.IsNullOrEmpty(invoice.Status)) {
                        parameters.Add("Status = @Status");
                    }

                    if (invoice.DueDate != default(DateTime)) {
                        parameters.Add("DueDate = @DueDate");
                    }

                    // Add optional PaymentDate
                    if (invoice.PaymentDate != default(DateTime)) {
                        parameters.Add("PaymentDate = @PaymentDate");
                    }

                    if (invoice.Notes != null) {
                        parameters.Add("Notes = @Notes");
                    }

                    // If no parameters provided, return true without making any changes
                    if (parameters.Count == 0) {
                        return true;
                    }

                    queryBuilder.Append(string.Join(", ", parameters));
                    queryBuilder.Append(" WHERE ID = @InvoiceID;");

                    using (var command = new MySqlCommand(queryBuilder.ToString(), connection)) {
                        command.Parameters.AddWithValue("@InvoiceID", invoice.ID);

                        if (invoice.Amount > 0) {
                            command.Parameters.AddWithValue("@Amount", invoice.Amount);
                        }

                        if (!string.IsNullOrEmpty(invoice.Status)) {
                            command.Parameters.AddWithValue("@Status", invoice.Status);
                        }

                        if (invoice.DueDate != default(DateTime)) {
                            command.Parameters.AddWithValue("@DueDate", invoice.DueDate.ToString("yyyy-MM-dd"));
                        }

                        if (invoice.PaymentDate != default(DateTime)) {
                            command.Parameters.AddWithValue("@PaymentDate", invoice.PaymentDate.ToString("yyyy-MM-dd"));
                        }

                        if (invoice.Notes != null) {
                            command.Parameters.AddWithValue("@Notes", invoice.Notes);
                        }

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                } catch (Exception ex) {
                    throw new ApplicationException($"Błąd aktualizacji faktury: {ex.Message}", ex);
                }
            }
        }

        // Dodanie metody do pobierania faktur
        public static List<Invoice> GetInvoices(int? reservationId = null) {
            var invoices = new List<Invoice>();

            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = @"
                SELECT ID, ReservationID, Amount, Status, IssueDate, DueDate, PaymentDate, InvoiceNumber, Notes 
                FROM invoices 
                WHERE 1=1 " + (reservationId.HasValue ? "AND ReservationID = @ReservationID " : "") +
                        "ORDER BY IssueDate DESC;";

                    using (var command = new MySqlCommand(query, connection)) {
                        if (reservationId.HasValue)
                            command.Parameters.AddWithValue("@ReservationID", reservationId.Value);

                        using (var reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                invoices.Add(new Invoice {
                                    ID = Convert.ToInt32(reader["ID"]),
                                    ReservationID = Convert.ToInt32(reader["ReservationID"]),
                                    Amount = Convert.ToDecimal(reader["Amount"]),
                                    Status = reader["Status"].ToString(),
                                    IssueDate = Convert.ToDateTime(reader["IssueDate"]),
                                    DueDate = Convert.ToDateTime(reader["DueDate"]),
                                    PaymentDate = reader["PaymentDate"] != DBNull.Value ? Convert.ToDateTime(reader["PaymentDate"]) : default(DateTime),
                                    InvoiceNumber = reader["InvoiceNumber"].ToString(),
                                    Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : null
                                });
                            }
                        }
                    }
                } catch (Exception ex) {
                    throw new ApplicationException("Błąd pobierania faktur", ex);
                }
            }

            return invoices;
        }

        // Metoda pomocnicza do pobierania pojedynczej faktury
        public static Invoice GetInvoiceById(int invoiceId) {
            using (var connection = new MySqlConnection(_connectionString)) {
                try {
                    connection.Open();
                    string query = @"
                SELECT ID, ReservationID, Amount, Status, IssueDate, DueDate, PaymentDate, InvoiceNumber, Notes 
                FROM invoices 
                WHERE ID = @InvoiceID;";

                    using (var command = new MySqlCommand(query, connection)) {
                        command.Parameters.AddWithValue("@InvoiceID", invoiceId);
                        using (var reader = command.ExecuteReader()) {
                            if (reader.Read()) {
                                return new Invoice {
                                    ID = Convert.ToInt32(reader["ID"]),
                                    ReservationID = Convert.ToInt32(reader["ReservationID"]),
                                    Amount = Convert.ToDecimal(reader["Amount"]),
                                    Status = reader["Status"].ToString(),
                                    IssueDate = Convert.ToDateTime(reader["IssueDate"]),
                                    DueDate = Convert.ToDateTime(reader["DueDate"]),
                                    PaymentDate = reader["PaymentDate"] != DBNull.Value ? Convert.ToDateTime(reader["PaymentDate"]) : default(DateTime),
                                    InvoiceNumber = reader["InvoiceNumber"].ToString(),
                                    Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : null
                                };
                            }
                        }
                    }
                } catch (Exception ex) {
                    throw new ApplicationException($"Błąd pobierania faktury o ID {invoiceId}: {ex.Message}", ex);
                }
            }
            return null;
        }

    }
        public class Flight {
            public string FlightDetails { get; set; }
            public int ID { get; set; }
            public string Departure { get; set; }
            public string Destination { get; set; }
            public DateTime Date { get; set; }
            public string Time { get; set; }
            public decimal Price { get; set; }
            public string Plane { get; set; }
            public string FlightType { get; set; }
        }

        public class Airport {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }
            public int GatesCount { get; set; }
        }

        public class Plane {
            public int ID { get; set; }
            public string Name { get; set; }
            public string SeatsLayout { get; set; }
        }

        public class User {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public string CustomerType { get; set; }
        }

        public class Reservation {
            public int ID { get; set; }
            public int UserID { get; set; }
            public int FlightID { get; set; }
            public string SeatNumber { get; set; }
            public string Status { get; set; }
        }

        public class Invoice {
            public int ID { get; set; }
            public int ReservationID { get; set; }
            public decimal Amount { get; set; }
            public string Status { get; set; }
            public DateTime IssueDate { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime PaymentDate { get; set; }
            public string InvoiceNumber { get; set; }
            public string Notes { get; set; }
        }
    }