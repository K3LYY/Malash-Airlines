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
        private ObservableCollection<ReservationViewModel> pendingReservations;

        private string selectedSeatNumber;

        public WorkerPanel() {
            InitializeComponent();
            InitializeCollections();
            LoadAllData();
            
            UpdateRoleManagementTabVisibility();
        }

        private void UpdateRoleManagementTabVisibility() {
            TabItem roleManagementTab = MainTabControl.Items[MainTabControl.Items.Count - 1] as TabItem;
            
            if (roleManagementTab != null) {
                if (AppSession.isLoggedIn && AppSession.userRole == "admin") {
                    roleManagementTab.Visibility = Visibility.Visible;
                } else {
                    roleManagementTab.Visibility = Visibility.Collapsed;
                }
            }
        }

        private ObservableCollection<User> businessClients;

        private void InitializeCollections() {
            users = new ObservableCollection<User>();
            reservations = new ObservableCollection<ReservationViewModel>();
            flights = new ObservableCollection<Flight>();
            airports = new ObservableCollection<Airport>();
            planes = new ObservableCollection<PlaneViewModel>();
            airportFlights = new ObservableCollection<Flight>();
            businessClients = new ObservableCollection<User>();
            pendingReservations = new ObservableCollection<ReservationViewModel>();
            PendingReservationsDataGrid.ItemsSource = pendingReservations;

            UsersDataGrid.ItemsSource = users;
            ReservationsDataGrid.ItemsSource = reservations;
            FlightsDataGrid.ItemsSource = flights;
            AirportsDataGrid.ItemsSource = airports;
            PlanesDataGrid.ItemsSource = planes;
            AirportFlightsDataGrid.ItemsSource = airportFlights;
            BusinessClientsDataGrid.ItemsSource = businessClients;

            UserComboBox.ItemsSource = users;
            FlightComboBox.ItemsSource = flights;
            DepartureComboBox.ItemsSource = airports;
            DestinationComboBox.ItemsSource = airports;
            PlaneComboBox.ItemsSource = planes;
            BusinessDepartureComboBox.ItemsSource = airports;
            BusinessDestinationComboBox.ItemsSource = airports;
            BusinessClientComboBox.ItemsSource = businessClients;
            FullPlaneDepartureComboBox.ItemsSource = airports;
            FullPlaneDestinationComboBox.ItemsSource = airports;
            FullPlanePlaneComboBox.ItemsSource = planes;

        }

        private void LoadAllData() {
            LoadAirports();
            LoadPlanes();
            LoadUsers();
            LoadFlights();
            LoadReservations();
            LoadBusinessClients();
            LoadPendingReservations();
            LoadRoleUsers();
        }

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

        private void LoadBusinessClients() {
            try {
                businessClients.Clear();
                var allUsers = Database.GetUsers();
                foreach (var user in allUsers) {
                    if (user.CustomerType.ToLower() == "business") {
                        businessClients.Add(user);
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error loading business clients: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void LoadPendingReservations() {
            try {
                pendingReservations.Clear();
                var unconfirmedList = Database.GetUnconfirmedReservations();
                foreach (var res in unconfirmedList) {
                    pendingReservations.Add(res);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error loading pending reservations: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFlights() {
            try {
                flights.Clear();
                foreach (var flight in Database.GetAvailableFlights()) {
                    flight.FlightDetails = $"{flight.ID}: {flight.Departure} -> {flight.Destination}, {flight.Date:yyyy-MM-dd} {flight.Time}";
                    flights.Add(flight);
                }
                
                var publicFlights = flights.Where(f => f.FlightType.ToLower() == "public").ToList();
                FlightComboBox.ItemsSource = publicFlights;
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

        private void RefreshBusinessClientsButton_Click(object sender, RoutedEventArgs e) {
            LoadBusinessClients();
        }

        private void RoleSearchEmailTextBox_GotFocus(object sender, RoutedEventArgs e) {
            if (RoleSearchEmailTextBox.Text == "Wyszukaj po emailu") {
                RoleSearchEmailTextBox.Text = "";
                RoleSearchEmailTextBox.Foreground = Brushes.Black;
            }
        }

        private void RoleSearchEmailTextBox_LostFocus(object sender, RoutedEventArgs e) {
            if (string.IsNullOrWhiteSpace(RoleSearchEmailTextBox.Text)) {
                RoleSearchEmailTextBox.Text = "Wyszukaj po emailu";
                RoleSearchEmailTextBox.Foreground = Brushes.Gray;
            }
        }

        private void RoleSearchUsersButton_Click(object sender, RoutedEventArgs e) {
            string searchEmail = RoleSearchEmailTextBox.Text.Trim().ToLower();
            if (searchEmail == "wyszukaj po emailu") searchEmail = "";
            try {
                LoadRoleUsers(searchEmail);
            } catch (Exception ex) {
                MessageBox.Show($"Błąd wyszukiwania użytkowników: {ex.Message}", "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshRoleUsersButton_Click(object sender, RoutedEventArgs e) {
            LoadRoleUsers();
        }

        private void RoleUsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (RoleUsersDataGrid.SelectedItem is User selectedUser) {
                SelectedUserTextBox.Text = $"{selectedUser.Name} ({selectedUser.Email})";
                CurrentRoleTextBox.Text = selectedUser.Role;
                NewRoleComboBox.SelectedItem = NewRoleComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == selectedUser.Role);
            } else {
                SelectedUserTextBox.Text = string.Empty;
                CurrentRoleTextBox.Text = string.Empty;
                NewRoleComboBox.SelectedIndex = -1;
            }
        }

        private void ChangeRoleButton_Click(object sender, RoutedEventArgs e) {
            if (RoleUsersDataGrid.SelectedItem is User selectedUser && NewRoleComboBox.SelectedItem is ComboBoxItem selectedRole) {
                string newRole = selectedRole.Content.ToString();

                if (newRole == selectedUser.Role) {
                    MessageBox.Show("Wybrana rola jest taka sama jak obecna.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var confirm = MessageBox.Show($"Czy na pewno chcesz zmienić rolę użytkownika {selectedUser.Name} z {selectedUser.Role} na {newRole}?",
                    "Potwierdź zmianę", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes) {
                    try {
                        bool success = Database.UpdateUserRole(selectedUser.ID, newRole);
                        if (success) {
                            MessageBox.Show($"Rola użytkownika została zmieniona pomyślnie na {newRole}.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadRoleUsers();
                        } else {
                            MessageBox.Show("Nie udało się zmienić roli użytkownika.", "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    } catch (Exception ex) {
                        MessageBox.Show($"Błąd podczas zmiany roli użytkownika: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            } else {
                MessageBox.Show("Wybierz użytkownika i nową rolę.", "Wymagane dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadRoleUsers(string searchEmail = "") {
            try {
                var allUsers = Database.GetUsers();
                var filteredUsers = allUsers
                    .Where(u => u.Role.ToLower() != "admin")
                    .Where(u => string.IsNullOrEmpty(searchEmail) || u.Email.ToLower().Contains(searchEmail.ToLower()))
                    .ToList();

                RoleUsersDataGrid.ItemsSource = new ObservableCollection<User>(filteredUsers);
            } catch (Exception ex) {
                MessageBox.Show($"Błąd ładowania użytkowników: {ex.Message}", "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshPendingReservationsButton_Click(object sender, RoutedEventArgs e) {
            LoadPendingReservations();
        }

        private void ConfirmReservationButton_Click(object sender, RoutedEventArgs e) {
            if (PendingReservationsDataGrid.SelectedItem is ReservationViewModel selectedReservation) {
                try {
                    Flight flight = Database.GetFlightById(selectedReservation.FlightID);
                    if (flight == null) {
                        throw new ApplicationException("Nie znaleziono lotu powiązanego z rezerwacją.");
                    }

                    User user = Database.GetUsers().FirstOrDefault(u => u.ID == selectedReservation.UserID);
                    if (user == null) {
                        throw new ApplicationException("Nie znaleziono użytkownika powiązanego z rezerwacją.");
                    }

                    var priceInputDialog = new InputDialog(
                        "Podaj cenę rezerwacji",
                        "Cena (PLN):",
                        flight.Price.ToString());

                    if (priceInputDialog.ShowDialog() == true) {
                        if (!decimal.TryParse(priceInputDialog.ResponseText, out decimal price)) {
                            MessageBox.Show("Wprowadzona wartość nie jest prawidłową ceną.",
                                "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        bool priceUpdateSuccess = Database.UpdateFlightPrice(selectedReservation.FlightID, price);
                        if (!priceUpdateSuccess) {
                            MessageBox.Show("Nie udało się zaktualizować ceny lotu.",
                                "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        bool updateSuccess = Database.UpdateReservation(selectedReservation.ReservationID, "pending");
                        if (updateSuccess) {
                            var reservation = Database.GetReservations().FirstOrDefault(r => r.ID == selectedReservation.ReservationID);
                            if (reservation == null) {
                                throw new ApplicationException("Nie udało się pobrać danych o rezerwacji.");
                            }

                            Invoice invoice = new Invoice {
                                ReservationID = selectedReservation.ReservationID,
                                Amount = price,
                                IssueDate = DateTime.Now,
                                DueDate = DateTime.Now.AddDays(7),
                                Status = "unpaid",
                                InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                                Notes = $"Faktura za rezerwację ID {selectedReservation.ReservationID}"
                            };

                            int invoiceId = Database.AddInvoice(invoice);
                            if (invoiceId <= 0) {
                                throw new ApplicationException("Nie udało się utworzyć faktury.");
                            }

                            try {
                                mail_functions.SendInvoice(user.Email, invoice, reservation, user, flight);
                                MessageBox.Show("Rezerwacja potwierdzona, cena lotu zaktualizowana, a faktura wystawiona i wysłana pomyślnie!",
                                    "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                            } catch (Exception mailEx) {
                                MessageBox.Show($"Rezerwacja potwierdzona, ale wystąpił błąd podczas wysyłania faktury: {mailEx.Message}",
                                    "Częściowy sukces", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }

                            LoadPendingReservations();
                            LoadReservations();
                            LoadFlights();
                        } else {
                            MessageBox.Show("Nie udało się zaktualizować statusu rezerwacji.",
                                "Błąd bazy danych", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                } catch (Exception ex) {
                    MessageBox.Show($"Błąd podczas potwierdzania rezerwacji: {ex.Message}",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else {
                MessageBox.Show("Proszę wybrać rezerwację do potwierdzenia.",
                    "Wymagane zaznaczenie", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OrderFullPlaneButton_Click(object sender, RoutedEventArgs e) {
            if (BusinessClientComboBox.SelectedItem == null || FullPlaneDepartureComboBox.SelectedItem == null ||
                FullPlaneDestinationComboBox.SelectedItem == null || !FullPlaneDatePicker.SelectedDate.HasValue ||
                string.IsNullOrWhiteSpace(FullPlaneTimeComboBox.Text) || FullPlanePlaneComboBox.SelectedItem == null) {
                MessageBox.Show("Please fill all fields correctly.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal price = 10000m;
            if (FullPlanePriceTextBox != null && !string.IsNullOrWhiteSpace(FullPlanePriceTextBox.Text)) {
                if (!decimal.TryParse(FullPlanePriceTextBox.Text, out price)) {
                    MessageBox.Show("Price must be a valid number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try {
                User client = (User)BusinessClientComboBox.SelectedItem;
                int userId = client.ID;
                int departureId = ((Airport)FullPlaneDepartureComboBox.SelectedItem).ID;
                int destinationId = ((Airport)FullPlaneDestinationComboBox.SelectedItem).ID;
                DateTime flightDate = FullPlaneDatePicker.SelectedDate.Value;
                string flightTime = FullPlaneTimeComboBox.Text;
                int planeId = ((PlaneViewModel)FullPlanePlaneComboBox.SelectedItem).ID;

                int flightId = Database.AddNewFlight(departureId, destinationId, flightDate, flightTime, price, planeId, "private");
                if (flightId > 0) {
                    string seatNumber = "FULL";

                    int reservationId = Database.AddReservation(userId, flightId, seatNumber, price);

                    if (reservationId > 0) {
                        Flight flight = Database.GetFlightById(flightId);
                        if (flight == null) {
                            throw new ApplicationException("Nie udało się pobrać danych o locie.");
                        }

                        Reservation reservation = Database.GetReservations().FirstOrDefault(r => r.ID == reservationId);
                        if (reservation == null) {
                            throw new ApplicationException("Nie udało się pobrać danych o rezerwacji.");
                        }

                        Invoice invoice = new Invoice {
                            ReservationID = reservationId,
                            Amount = price,
                            IssueDate = DateTime.Now,
                            DueDate = DateTime.Now.AddDays(7),
                            Status = "unpaid",
                            InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}",
                            Notes = $"Faktura za rezerwację prywatnego lotu ID {reservationId}"
                        };

                        int invoiceId = Database.AddInvoice(invoice);

                        try {
                            mail_functions.SendReservationDocuments(client.Email, reservation, client, flight);
                            MessageBox.Show($"Full plane ordered successfully for {client.Name}! Documents have been sent by email.",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        } catch (Exception ex) {
                            MessageBox.Show($"Full plane ordered successfully for {client.Name}, but there was an error sending email: {ex.Message}",
                                "Partial Success", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        LoadFlights();
                        LoadReservations();
                        LoadBusinessClients();
                        BusinessClientComboBox.SelectedIndex = -1;
                        FullPlaneDepartureComboBox.SelectedIndex = -1;
                        FullPlaneDestinationComboBox.SelectedIndex = -1;
                        FullPlaneDatePicker.SelectedDate = null;
                        FullPlaneTimeComboBox.Text = string.Empty;
                        FullPlanePlaneComboBox.SelectedIndex = -1;
                        if (FullPlanePriceTextBox != null) {
                            FullPlanePriceTextBox.Text = string.Empty;
                        }
                    } else {
                        MessageBox.Show("Failed to create reservation for the flight.", "Database Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                } else {
                    MessageBox.Show("Failed to order full plane.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error ordering full plane: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


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
                if (selectedFlight.FlightType.ToLower() != "public") {
                    MessageBox.Show("Cannot select seats for private flights.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
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
                
                var selectedFlight = FlightComboBox.SelectedItem as Flight;
                if (selectedFlight != null && selectedFlight.FlightType.ToLower() != "public") {
                    MessageBox.Show("Cannot make reservations for private flights.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
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
                var user = users.FirstOrDefault(u => u.ID == selectedReservation.UserID);
                if (user != null) {
                    UserComboBox.SelectedItem = user;
                }
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

        private void UserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (UserComboBox.SelectedItem is User selectedUser) {
                bool isBusiness = selectedUser.Role.ToLower() == "business";
                BusinessDepartureLabel.Visibility = isBusiness ? Visibility.Visible : Visibility.Collapsed;
                BusinessDepartureComboBox.Visibility = isBusiness ? Visibility.Visible : Visibility.Collapsed;
                BusinessDestinationLabel.Visibility = isBusiness ? Visibility.Visible : Visibility.Collapsed;
                BusinessDestinationComboBox.Visibility = isBusiness ? Visibility.Visible : Visibility.Collapsed;
                OrderBusinessFlightButton.Visibility = isBusiness ? Visibility.Visible : Visibility.Collapsed;

                FlightComboBox.Visibility = isBusiness ? Visibility.Collapsed : Visibility.Visible;
                SeatNumberTextBox.Visibility = isBusiness ? Visibility.Collapsed : Visibility.Visible;
                SelectSeatButton.Visibility = isBusiness ? Visibility.Collapsed : Visibility.Visible;
                AddReservationButton.Visibility = isBusiness ? Visibility.Collapsed : Visibility.Visible;

                if (isBusiness) {
                    BusinessDepartureComboBox.ItemsSource = airports;
                    BusinessDestinationComboBox.ItemsSource = airports;
                }
            } else {
                BusinessDepartureLabel.Visibility = Visibility.Collapsed;
                BusinessDepartureComboBox.Visibility = Visibility.Collapsed;
                BusinessDestinationLabel.Visibility = Visibility.Collapsed;
                BusinessDestinationComboBox.Visibility = Visibility.Collapsed;
                OrderBusinessFlightButton.Visibility = Visibility.Collapsed;

                FlightComboBox.Visibility = Visibility.Visible;
                SeatNumberTextBox.Visibility = Visibility.Visible;
                SelectSeatButton.Visibility = Visibility.Visible;
                AddReservationButton.Visibility = Visibility.Visible;
            }
        }

        private void UpgradeToBusinessButton_Click(object sender, RoutedEventArgs e) {
            if (UsersDataGrid.SelectedItem is User selectedUser) {
                string currentCustomerType = Database.GetUserCustomerType(selectedUser.ID);
                string newCustomerType;

                if (currentCustomerType.ToLower() == "business") {
                    newCustomerType = "normal";
                } else {
                    newCustomerType = "business";
                }

                string actionText = newCustomerType == "business" ? "upgrade" : "downgrade";

                var confirm = MessageBox.Show($"Are you sure you want to {actionText} {selectedUser.Name} to {newCustomerType} customer type?",
                    $"Confirm {actionText}", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes) {
                    try {
                        bool success = Database.UpdateUserCustomerType(selectedUser.ID, newCustomerType);
                        if (success) {
                            MessageBox.Show($"User {actionText}d to {newCustomerType} successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadUsers();
                        } else {
                            MessageBox.Show($"Failed to {actionText} user.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    } catch (Exception ex) {
                        MessageBox.Show($"Error updating user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            } else {
                MessageBox.Show("Please select a user.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OrderBusinessFlightButton_Click(object sender, RoutedEventArgs e) {
            if (UserComboBox.SelectedItem == null || BusinessDepartureComboBox.SelectedItem == null || BusinessDestinationComboBox.SelectedItem == null) {
                MessageBox.Show("Please select a user, departure airport, and destination airport.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try {
                int userId = ((User)UserComboBox.SelectedItem).ID;
                int departureId = ((Airport)BusinessDepartureComboBox.SelectedItem).ID;
                int destinationId = ((Airport)BusinessDestinationComboBox.SelectedItem).ID;

                var availablePlanes = Database.GetPlanes();
                if (!availablePlanes.Any()) {
                    MessageBox.Show("No planes available to order a flight.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int planeId = availablePlanes.First().ID;
                DateTime flightDate = DateTime.Now.AddDays(1);
                string flightTime = "12:00";
                decimal price = 10000m;

                int flightId = Database.AddNewFlight(departureId, destinationId, flightDate, flightTime, price, planeId);
                if (flightId > 0) {
                    MessageBox.Show($"Flight ordered successfully for {((User)UserComboBox.SelectedItem).Name}! Flight ID: {flightId}",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadFlights();
                    UserComboBox.SelectedIndex = -1;
                } else {
                    MessageBox.Show("Failed to order flight.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Error ordering flight: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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