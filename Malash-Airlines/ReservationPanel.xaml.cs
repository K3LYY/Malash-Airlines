﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Malash_Airlines {
    public partial class ReservationPanel : Window {
        private List<Flight> availableFlights;
        private Flight selectedFlight;
        private string selectedSeatNumber;
        private List<Airport> airports;
        private List<Plane> planes;

        public ReservationPanel() {
            InitializeComponent();
            LoadData();
            CheckUserAccess();
        }

        private void LoadData() {
            LoadAvailableFlights();
            LoadAirportsAndPlanes();
            SetupTimeComboBox();
        }

        private void CheckUserAccess() {
            // Sprawdź, czy użytkownik ma dostęp do zakładki rezerwacji prywatnych
            bool isBusinessUser = AppSession.isLoggedIn && AppSession.CurrentUser != null &&
                                  AppSession.CurrentUser.CustomerType?.ToLower() == "business";

            // Jeśli użytkownik nie jest zalogowany lub nie jest klientem biznesowym, ukryj drugą zakładkę
            PrivateTabItem.Visibility = isBusinessUser ? Visibility.Visible : Visibility.Collapsed;

            // Ustaw domyślnie pierwszą zakładkę
            MainTabControl.SelectedIndex = 0;
        }

        private void LoadAvailableFlights() {
            try {
                // Pobierz dostępne loty z bazy danych (tylko publiczne)
                availableFlights = Database.GetAvailableFlights()
                    .Where(f => f.FlightType.ToLower() == "public")
                    .ToList();

                // Przypisz źródło danych do listy lotów
                FlightsListBox.ItemsSource = availableFlights;
                FlightsListBox.DisplayMemberPath = "FlightDetails";
            } catch (Exception ex) {
                MessageBox.Show($"Błąd podczas ładowania lotów: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadAirportsAndPlanes() {
            try {
                // Pobierz lotniska i samoloty dla zakładki rezerwacji prywatnych
                airports = Database.GetAirports();
                planes = Database.GetPlanes();

                // Przypisz dane do kontrolek
                DepartureAirportComboBox.ItemsSource = airports;
                DestinationAirportComboBox.ItemsSource = airports;
                AircraftTypeComboBox.ItemsSource = planes;

                // Ustaw domyślną datę na jutro
                FlightDatePicker.SelectedDate = DateTime.Now.AddDays(1);
            } catch (Exception ex) {
                MessageBox.Show($"Błąd podczas ładowania danych: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupTimeComboBox() {
            // Wypełnij ComboBox z godzinami lotów
            FlightTimeComboBox.Items.Clear();
            for (int hour = 0; hour < 24; hour++) {
                FlightTimeComboBox.Items.Add($"{hour:D2}:00");
                FlightTimeComboBox.Items.Add($"{hour:D2}:30");
            }
            // Domyślnie ustaw na 12:00
            FlightTimeComboBox.SelectedItem = "12:00";
        }

        private void FlightsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (FlightsListBox.SelectedItem is Flight flight) {
                selectedFlight = flight;
                UpdateFlightDetails(flight);
                EnableBookingControls(true);
            } else {
                ClearFlightDetails();
                EnableBookingControls(false);
            }
        }

        private void UpdateFlightDetails(Flight flight) {
            FlightDetailsPanel.Visibility = Visibility.Visible;
            DepartureTextBlock.Text = flight.Departure;
            DestinationTextBlock.Text = flight.Destination;
            DateTextBlock.Text = flight.Date.ToString("dd.MM.yyyy");
            TimeTextBlock.Text = flight.Time;
            PlaneTextBlock.Text = flight.Plane;
            PriceTextBlock.Text = $"{flight.Price:C}";

            // Jeśli użytkownik jest zalogowany, wyświetl cenę z uwzględnieniem rabatu
            if (AppSession.isLoggedIn && AppSession.CurrentUser != null) {
                if (AppSession.CurrentUser.CustomerType?.ToLower() == "business") {
                    // 20% rabatu dla klientów biznesowych
                    decimal discountedPrice = flight.Price * 0.8m;
                    PriceTextBlock.Text = $"{discountedPrice:C} (rabat 20%)";
                }
            }
        }

        private void ClearFlightDetails() {
            FlightDetailsPanel.Visibility = Visibility.Collapsed;
            DepartureTextBlock.Text = "";
            DestinationTextBlock.Text = "";
            DateTextBlock.Text = "";
            TimeTextBlock.Text = "";
            PlaneTextBlock.Text = "";
            PriceTextBlock.Text = "";
        }

        private void EnableBookingControls(bool enable) {
            SelectSeatButton.IsEnabled = enable;
            ViewMapButton.IsEnabled = enable;
            BookButton.IsEnabled = enable && !string.IsNullOrEmpty(selectedSeatNumber);
        }

        private void SelectSeatButton_Click(object sender, RoutedEventArgs e) {
            if (selectedFlight == null) return;

            try {
                // Pobierz zajęte miejsca dla wybranego lotu
                var occupiedSeats = Database.GetOccupiedSeatsForFlight(selectedFlight.ID);

                // Pobierz informacje o samolocie
                var plane = Database.GetPlanes().FirstOrDefault(p => p.Name == selectedFlight.Plane);
                string layoutType = plane?.SeatsLayout ?? "B737";

                // Otwórz okno wyboru miejsc
                var seatLayoutWindow = new SeatLayout(layoutType, occupiedSeats);
                if (seatLayoutWindow.ShowDialog() == true) {
                    selectedSeatNumber = seatLayoutWindow.SelectedSeatNumber;
                    SeatNumberTextBox.Text = selectedSeatNumber;
                    EnableBookingControls(true);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Błąd podczas otwierania wyboru miejsc: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewMapButton_Click(object sender, RoutedEventArgs e) {
            if (selectedFlight == null) return;

            try {
                // Otwórz okno wizualizacji mapy lotu
                FlightMapVisualization mapWindow = new FlightMapVisualization(selectedFlight);
                mapWindow.ShowDialog();
            } catch (Exception ex) {
                MessageBox.Show($"Błąd podczas otwierania mapy: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BookButton_Click(object sender, RoutedEventArgs e) {
            if (selectedFlight == null || string.IsNullOrEmpty(selectedSeatNumber)) {
                MessageBox.Show("Proszę wybrać lot i miejsce.", "Wymagane dane", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Sprawdź, czy użytkownik jest zalogowany
            if (!AppSession.isLoggedIn || AppSession.CurrentUser == null) {
                var result = MessageBox.Show("Aby dokonać rezerwacji, musisz być zalogowany/a. Czy chcesz się zalogować?",
                    "Wymagane logowanie", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes) {
                    // Otwórz okno logowania
                    loginWindow loginWin = new loginWindow();
                    this.Close(); // Zamknij to okno
                    loginWin.ShowDialog();
                }
                return;
            }

            try {
                // Oblicz cenę
                decimal price = selectedFlight.Price;
                if (AppSession.CurrentUser.CustomerType?.ToLower() == "business") {
                    // 20% rabatu dla klientów biznesowych
                    price *= 0.8m;
                }

                // Dodaj rezerwację do bazy danych (status: pending)
                int reservationId = Database.AddReservation(
                    AppSession.CurrentUser.ID,
                    selectedFlight.ID,
                    selectedSeatNumber,
                    price,
                    "pending");

                if (reservationId > 0) {
                    // Dodaj fakturę
                    Invoice invoice = new Invoice {
                        ReservationID = reservationId,
                        Amount = price,
                        Status = "unpaid",
                        IssueDate = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(7),
                        Notes = $"Rezerwacja miejsca {selectedSeatNumber} na lot {selectedFlight.ID}"
                    };

                    int invoiceId = Database.AddInvoice(invoice);

                    MessageBox.Show("Rezerwacja została utworzona pomyślnie! Faktura została wystawiona.",
                        "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearSelections();
                } else {
                    MessageBox.Show("Nie udało się utworzyć rezerwacji. Miejsce może być już zajęte.",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Błąd podczas tworzenia rezerwacji: {ex.Message}",
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SendPrivateRequestButton_Click(object sender, RoutedEventArgs e) {
            // Sprawdź, czy użytkownik jest zalogowany jako klient biznesowy
            if (!AppSession.isLoggedIn || AppSession.CurrentUser == null ||
                AppSession.CurrentUser.CustomerType?.ToLower() != "business") {
                MessageBox.Show("Tylko zalogowani klienci biznesowi mogą składać zapytania o prywatne loty.",
                    "Brak uprawnień", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Sprawdź, czy wszystkie wymagane pola są wypełnione
            if (DepartureAirportComboBox.SelectedItem == null ||
                DestinationAirportComboBox.SelectedItem == null ||
                !FlightDatePicker.SelectedDate.HasValue ||
                string.IsNullOrEmpty(FlightTimeComboBox.Text) ||
                AircraftTypeComboBox.SelectedItem == null) {
                MessageBox.Show("Proszę wypełnić wszystkie wymagane pola.",
                    "Brakujące dane", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try {
                // Przygotuj dane do zapytania
                int userId = AppSession.CurrentUser.ID;
                int departureId = ((Airport)DepartureAirportComboBox.SelectedItem).ID;
                int destinationId = ((Airport)DestinationAirportComboBox.SelectedItem).ID;
                DateTime flightDate = FlightDatePicker.SelectedDate.Value;
                string flightTime = FlightTimeComboBox.Text;
                int planeId = ((Plane)AircraftTypeComboBox.SelectedItem).ID;

                // Dodaj lot z typem "pending_private" (cena będzie ustalona później)
                int flightId = Database.AddNewFlight(departureId, destinationId, flightDate, flightTime, 0, planeId, "private");

                if (flightId > 0) {
                    // Dodaj rezerwację ze statusem "pending" i miejscem "FULL" (cały samolot)
                    int reservationId = Database.AddReservation(userId, flightId, "FULL", 0, "unconfirmed");

                    if (reservationId > 0) {
                        MessageBox.Show("Zapytanie o prywatny lot zostało złożone pomyślnie. " +
                            "Zostaniesz poinformowany o decyzji pracownika linii lotniczych.",
                            "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Wyczyść formularz
                        DepartureAirportComboBox.SelectedIndex = -1;
                        DestinationAirportComboBox.SelectedIndex = -1;
                        FlightDatePicker.SelectedDate = DateTime.Now.AddDays(1);
                        FlightTimeComboBox.SelectedItem = "12:00";
                        AircraftTypeComboBox.SelectedIndex = -1;
                        NotesTextBox.Text = string.Empty;
                    } else {
                        // Jeśli rezerwacja się nie powiodła, usuń utworzony lot
                        Database.RemoveFlight(flightId);
                        MessageBox.Show("Nie udało się złożyć zapytania o prywatny lot. Spróbuj ponownie później.",
                            "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                } else {
                    MessageBox.Show("Nie udało się utworzyć zapytania o prywatny lot.",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } catch (Exception ex) {
                MessageBox.Show($"Wystąpił błąd podczas składania zapytania: {ex.Message}",
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearSelections() {
            selectedFlight = null;
            selectedSeatNumber = null;
            FlightsListBox.SelectedIndex = -1;
            SeatNumberTextBox.Text = string.Empty;
            ClearFlightDetails();
            EnableBookingControls(false);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) {
            LoadAvailableFlights();
            ClearSelections();
        }
    }
}
