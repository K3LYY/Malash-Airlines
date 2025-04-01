using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Malash_Airlines {
    public partial class WorkerPanel : Window {
        private ObservableCollection<User> users;
        private ObservableCollection<ReservationViewModel> reservations;
        private ObservableCollection<Flight> flights;
        private ObservableCollection<Airport> airports;
        private ObservableCollection<PlaneViewModel> planes;
        private ObservableCollection<Flight> airportFlights;

        private string selectedSeatNumber;

        public WorkerPanel() {
            InitializeComponent();
            InitializeCollections();
            LoadAllData();
        }

        private void InitializeCollections() {
            users = new ObservableCollection<User>();
            reservations = new ObservableCollection<ReservationViewModel>();
            flights = new ObservableCollection<Flight>();
            airports = new ObservableCollection<Airport>();
            planes = new ObservableCollection<PlaneViewModel>();
            airportFlights = new ObservableCollection<Flight>();

            UsersDataGrid.ItemsSource = users;
            ReservationsDataGrid.ItemsSource = reservations;
            FlightsDataGrid.ItemsSource = flights;
            AirportsDataGrid.ItemsSource = airports;
            PlanesDataGrid.ItemsSource = planes;
            AirportFlightsDataGrid.ItemsSource = airportFlights;

            UserComboBox.ItemsSource = users;
            FlightComboBox.ItemsSource = flights;
            DepartureComboBox.ItemsSource = airports;
            DestinationComboBox.ItemsSource = airports;
            PlaneComboBox.ItemsSource = planes;
        }

        private void LoadAllData() {
            LoadAirports();
            LoadPlanes();
            LoadUsers();
            LoadFlights();
            LoadReservations();
        }

        // --- Data Loading Methods ---

        private void LoadUsers() {
            try {
                users.Clear();
                foreach (var user in Database.GetUsers()) {
                    users.Add(user);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error loading users: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadReservations() {
            try {
                reservations.Clear();
                var reservationList = Database.GetReservations();
                var userList = Database.GetUsers();
                var flightList = Database.GetAvailableFlights();

                foreach (var res in reservationList) {
                    var user = userList.FirstOrDefault(u => u.ID == res.UserID);
                    var flight = flightList.FirstOrDefault(f => f.ID == res.FlightID);
                    if (user != null && flight != null) {
                        reservations.Add(new ReservationViewModel {
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
                MessageBox.Show($"Error loading reservations: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFlights() {
            try {
                flights.Clear();
                foreach (var flight in Database.GetAvailableFlights()) {
                    flight.FlightDetails = $"{flight.ID}: {flight.Departure} -> {flight.Destination}, {flight.Date:yyyy-MM-dd} {flight.Time}";
                    flights.Add(flight);
                }
                FlightComboBox.Items.Refresh();
            } catch (Exception ex) {
                MessageBox.Show($"Error loading flights: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAirports() {
            try {
                airports.Clear();
                foreach (var airport in Database.GetAirports()) {
                    airports.Add(airport);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error loading airports: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPlanes() {
            try {
                planes.Clear();
                foreach (var plane in Database.GetPlanes()) {
                    planes.Add(new PlaneViewModel {
                        ID = plane.ID,
                        Name = plane.Name,
                        SeatsLayout = plane.SeatsLayout
                    });
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error loading planes: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFlightsForAirport(int airportId) {
            try {
                airportFlights.Clear();
                foreach (var flight in Database.GetFlightsByDepartureAirport(airportId)) {
                    airportFlights.Add(flight);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error loading flights for airport: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- Event Handlers ---

        private void RefreshUsersButton_Click(object sender, RoutedEventArgs e) {
            LoadUsers();
        }

        private void SearchUsersButton_Click(object sender, RoutedEventArgs e) {
            string searchEmail = SearchEmailTextBox.Text.Trim().ToLower();
            if (searchEmail == "wyszukaj po emailu") searchEmail = "";
            try {
                users.Clear();
                var allUsers = Database.GetUsers();
                foreach (var user in allUsers) {
                    if (string.IsNullOrEmpty(searchEmail) || user.Email.ToLower().Contains(searchEmail)) {
                        users.Add(user);
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error searching users: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchEmailTextBox_GotFocus(object sender, RoutedEventArgs e) {
            if (SearchEmailTextBox.Text == "Wyszukaj po emailu") {
                SearchEmailTextBox.Text = "";
                SearchEmailTextBox.Foreground = Brushes.Black;
            }
        }

        private void SearchEmailTextBox_LostFocus(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(SearchEmailTextBox.Text)) {
                SearchEmailTextBox.Text = "Wyszukaj po emailu";
                SearchEmailTextBox.Foreground = Brushes.Gray;
            }
        }

        private void RefreshReservationsButton_Click(object sender, RoutedEventArgs e) {
            LoadReservations();
        }

        private void SelectSeatButton_Click(object sender, RoutedEventArgs e) {
            if (FlightComboBox.SelectedItem is Flight selectedFlight) {
                try {
                    var occupiedSeats = Database.GetOccupiedSeatsForFlight(selectedFlight.ID);
                    var plane = Database.GetPlanes().FirstOrDefault(p => p.Name == selectedFlight.Plane);
                    string layoutType = plane?.SeatsLayout ?? "B737";

                    var seatLayoutWindow = new SeatLayout(layoutType, occupiedSeats);
                    if (seatLayoutWindow.ShowDialog() == true) {
                        selectedSeatNumber = seatLayoutWindow.SelectedSeatNumber;
                        SeatNumberTextBox.Text = selectedSeatNumber;
                    }
                } catch (Exception ex) {
                    MessageBox.Show($"Error opening seat selection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else {
                MessageBox.Show("Please select a flight first.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddReservationButton_Click(object sender, RoutedEventArgs e) {
            if (UserComboBox.SelectedItem == null || FlightComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(selectedSeatNumber)) {
                MessageBox.Show("Please select a user, flight, and seat.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try {
                int userId = ((User)UserComboBox.SelectedItem).ID;
                int flightId = ((Flight)FlightComboBox.SelectedItem).ID;
                bool success = Database.ReserveSeat(userId, flightId, selectedSeatNumber);

                if (success) {
                    MessageBox.Show("Reservation added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadReservations();
                    UserComboBox.SelectedIndex = -1;
                    FlightComboBox.SelectedIndex = -1;
                    SeatNumberTextBox.Text = string.Empty;
                    selectedSeatNumber = null;
                } else {
                    MessageBox.Show("Failed to add reservation.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error adding reservation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveReservationButton_Click(object sender, RoutedEventArgs e) {
            if (ReservationsDataGrid.SelectedItem is ReservationViewModel selectedReservation) {
                var confirm = MessageBox.Show($"Are you sure you want to remove reservation ID {selectedReservation.ReservationID}?",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes) {
                    try {
                        bool success = Database.RemoveReservation(selectedReservation.ReservationID);
                        if (success) {
                            MessageBox.Show("Reservation removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadReservations();
                        } else {
                            MessageBox.Show("Failed to remove reservation.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    } catch (Exception ex) {
                        MessageBox.Show($"Error removing reservation: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            } else {
                MessageBox.Show("Please select a reservation to remove.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ReservationsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ReservationsDataGrid.SelectedItem is ReservationViewModel selectedReservation) {
                // Auto-select the user in UserComboBox based on the selected reservation
                var user = users.FirstOrDefault(u => u.ID == selectedReservation.UserID);
                if (user != null) {
                    UserComboBox.SelectedItem = user;
                }
                // Clear previous flight and seat selections to allow new assignment
                FlightComboBox.SelectedIndex = -1;
                SeatNumberTextBox.Text = string.Empty;
                selectedSeatNumber = null;
            }
        }

        private void RefreshFlightsButton_Click(object sender, RoutedEventArgs e) {
            LoadFlights();
        }

        private void AddFlightButton_Click(object sender, RoutedEventArgs e) {
            if (DepartureComboBox.SelectedItem == null || DestinationComboBox.SelectedItem == null ||
                !FlightDatePicker.SelectedDate.HasValue || string.IsNullOrWhiteSpace(FlightTimeComboBox.Text) ||
                !decimal.TryParse(PriceTextBox.Text, out decimal price) || PlaneComboBox.SelectedItem == null) {
                MessageBox.Show("Please fill all fields correctly.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try {
                int departureId = ((Airport)DepartureComboBox.SelectedItem).ID;
                int destinationId = ((Airport)DestinationComboBox.SelectedItem).ID;
                DateTime date = FlightDatePicker.SelectedDate.Value;
                string time = FlightTimeComboBox.Text;
                int planeId = ((PlaneViewModel)PlaneComboBox.SelectedItem).ID;

                int flightId = Database.AddNewFlight(departureId, destinationId, date, time, price, planeId);
                if (flightId > 0) {
                    MessageBox.Show("Flight added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadFlights();
                    ClearFlightForm();
                } else {
                    MessageBox.Show("Failed to add flight.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error adding flight: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveFlightButton_Click(object sender, RoutedEventArgs e) {
            if (FlightsDataGrid.SelectedItem is Flight selectedFlight) {
                var confirm = MessageBox.Show($"Are you sure you want to remove flight ID {selectedFlight.ID}?",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes) {
                    try {
                        bool success = Database.RemoveFlight(selectedFlight.ID);
                        if (success) {
                            MessageBox.Show("Flight removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadFlights();
                        } else {
                            MessageBox.Show("Failed to remove flight.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    } catch (Exception ex) {
                        MessageBox.Show($"Error removing flight: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            } else {
                MessageBox.Show("Please select a flight to remove.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewSeatsButton_Click(object sender, RoutedEventArgs e) {
            if (FlightsDataGrid.SelectedItem is Flight selectedFlight) {
                var seatLayoutWindow = new SeatLayout(false);
                seatLayoutWindow.Show();
            } else {
                MessageBox.Show("Please select a flight to view its seat layout.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClearFlightForm() {
            DepartureComboBox.SelectedIndex = -1;
            DestinationComboBox.SelectedIndex = -1;
            FlightDatePicker.SelectedDate = null;
            FlightTimeComboBox.Text = string.Empty;
            PriceTextBox.Text = string.Empty;
            PlaneComboBox.SelectedIndex = -1;
        }

        private void RefreshAirportsButton_Click(object sender, RoutedEventArgs e) {
            LoadAirports();
        }

        private void AddAirportButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(AirportNameTextBox.Text) || string.IsNullOrWhiteSpace(AirportLocationTextBox.Text)) {
                MessageBox.Show("Please fill all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try {
                int airportId = Database.AddAirport(AirportNameTextBox.Text, AirportLocationTextBox.Text);
                if (airportId > 0) {
                    MessageBox.Show("Airport added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadAirports();
                    AirportNameTextBox.Text = string.Empty;
                    AirportLocationTextBox.Text = string.Empty;
                } else {
                    MessageBox.Show("Failed to add airport.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error adding airport: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveAirportButton_Click(object sender, RoutedEventArgs e) {
            if (AirportsDataGrid.SelectedItem is Airport selectedAirport) {
                var confirm = MessageBox.Show($"Are you sure you want to remove airport ID {selectedAirport.ID}?",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes) {
                    try {
                        bool success = Database.RemoveAirport(selectedAirport.ID);
                        if (success) {
                            MessageBox.Show("Airport removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadAirports();
                        } else {
                            MessageBox.Show("Failed to remove airport.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    } catch (Exception ex) {
                        MessageBox.Show($"Error removing airport: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            } else {
                MessageBox.Show("Please select an airport to remove.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AirportsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (AirportsDataGrid.SelectedItem is Airport selectedAirport) {
                LoadFlightsForAirport(selectedAirport.ID);
            } else {
                airportFlights.Clear();
            }
        }

        private void RefreshPlanesButton_Click(object sender, RoutedEventArgs e) {
            LoadPlanes();
        }

        private void AddPlaneButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(PlaneNameTextBox.Text) || SeatLayoutComboBox.SelectedItem == null) {
                MessageBox.Show("Please fill all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try {
                string name = PlaneNameTextBox.Text;
                string seatsLayout = ((ComboBoxItem)SeatLayoutComboBox.SelectedItem).Tag.ToString();
                int planeId = Database.AddPlane(name, seatsLayout);

                if (planeId > 0) {
                    MessageBox.Show("Plane added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadPlanes();
                    PlaneNameTextBox.Text = string.Empty;
                    SeatLayoutComboBox.SelectedIndex = -1;
                } else {
                    MessageBox.Show("Failed to add plane.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error adding plane: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemovePlaneButton_Click(object sender, RoutedEventArgs e) {
            if (PlanesDataGrid.SelectedItem is PlaneViewModel selectedPlane) {
                var confirm = MessageBox.Show($"Are you sure you want to remove plane ID {selectedPlane.ID}?",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes) {
                    try {
                        bool success = Database.RemovePlane(selectedPlane.ID);
                        if (success) {
                            MessageBox.Show("Plane removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadPlanes();
                        } else {
                            MessageBox.Show("Failed to remove plane.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    } catch (Exception ex) {
                        MessageBox.Show($"Error removing plane: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            } else {
                MessageBox.Show("Please select a plane to remove.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class PlaneViewModel {
        public int ID { get; set; }
        public string Name { get; set; }
        public string SeatsLayout { get; set; }
        public string SeatLayoutSummary => SeatsLayout switch {
            "B737" => "4x2 (First) + 26x3-3 (Economy)",
            "B787" => "6x2 (First) + 40x3-3 (Economy)",
            "A320" => "8x2 (First) + 30x3-3 (Economy)",
            _ => SeatsLayout
        };
    }

    public class ReservationViewModel {
        public int ReservationID { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string FlightDetails { get; set; }
        public string SeatNumber { get; set; }
        public string Status { get; set; }
        public int UserID { get; set; }
        public int FlightID { get; set; }
    }
}