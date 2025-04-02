using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Malash_Airlines
{
    public partial class ReservationPanel : Window
    {
        private List<Flight> allFlights;
        private List<Airport> airports;
        private List<Plane> planes;
        private Flight selectedFlight;
        private int currentUserId;
        private string selectedSeatNumber;

        public ReservationPanel(int userId)
        {
            InitializeComponent();
            currentUserId = userId;
            LoadData();
            InitializeControls();

            // Handle seat selection events
            seatLayoutControl.SeatSelected += SeatLayoutControl_SeatSelected;
        }

        private void SeatLayoutControl_SeatSelected(object sender, SeatSelectedEventArgs e)
        {
            selectedSeatNumber = e.SeatNumber;
            reserveButton.IsEnabled = true;
        }

        private void LoadData()
        {
            try
            {
                allFlights = Database.GetAvailableFlights();
                airports = Database.GetAirports();
                planes = Database.GetPlanes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeControls()
        {
            // Initialize departure and destination combo boxes
            departureComboBox.ItemsSource = airports;
            departureComboBox.DisplayMemberPath = "Name";
            departureComboBox.SelectedValuePath = "ID";

            destinationComboBox.ItemsSource = airports;
            destinationComboBox.DisplayMemberPath = "Name";
            destinationComboBox.SelectedValuePath = "ID";

            // Initialize flights data grid
            flightsDataGrid.ItemsSource = allFlights;
        }

        private void DepartureComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterFlights();
        }

        private void DestinationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterFlights();
        }

        private void FilterFlights()
        {
            try
            {
                int? departureId = departureComboBox.SelectedValue as int?;
                int? destinationId = destinationComboBox.SelectedValue as int?;

                var filteredFlights = allFlights.AsEnumerable();

                if (departureId.HasValue)
                {
                    filteredFlights = filteredFlights.Where(f =>
                        airports.First(a => a.Name == f.Departure).ID == departureId.Value);
                }

                if (destinationId.HasValue)
                {
                    filteredFlights = filteredFlights.Where(f =>
                        airports.First(a => a.Name == f.Destination).ID == destinationId.Value);
                }

                flightsDataGrid.ItemsSource = filteredFlights.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering flights: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FlightsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedFlight = flightsDataGrid.SelectedItem as Flight;
            if (selectedFlight != null)
            {
                // Update flight details display
                selectedFlightDetails.Text = $"{selectedFlight.Departure} to {selectedFlight.Destination}\n" +
                                            $"Date: {selectedFlight.Date:yyyy-MM-dd}\n" +
                                            $"Time: {selectedFlight.Time}\n" +
                                            $"Aircraft: {selectedFlight.Plane}";

                // Update price
                totalPriceText.Text = $"{selectedFlight.Price:C}";

                // Load available seats for this flight
                LoadAvailableSeats();

                // Enable reserve button if seat is selected
                reserveButton.IsEnabled = !string.IsNullOrEmpty(selectedSeatNumber);
            }
            else
            {
                selectedFlightDetails.Text = "No flight selected";
                totalPriceText.Text = "$0.00";
                seatLayoutControl.ClearSeats();
                reserveButton.IsEnabled = false;
            }
        }

        private void LoadAvailableSeats()
        {
            try
            {
                if (selectedFlight == null) return;

                // Get the plane's seat layout
                var plane = planes.FirstOrDefault(p => p.Name == selectedFlight.Plane);
                if (plane == null)
                {
                    MessageBox.Show("Could not find seat configuration for this aircraft.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Get already reserved seats for this flight
                var reservations = Database.GetReservations(flightId: selectedFlight.ID);
                var reservedSeats = reservations.Select(r => r.SeatNumber).ToList();

                // Initialize the seat layout control
                seatLayoutControl.InitializeSeats(plane.SeatsLayout, reservedSeats);

                // Enable the control
                seatLayoutControl.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading available seats: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReserveButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFlight == null || string.IsNullOrEmpty(selectedSeatNumber))
            {
                MessageBox.Show("Please select a flight and a seat.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                bool success = Database.ReserveSeat(currentUserId, selectedFlight.ID, selectedSeatNumber);

                if (success)
                {
                    MessageBox.Show($"Seat {selectedSeatNumber} reserved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh the available seats
                    LoadAvailableSeats();
                    selectedSeatNumber = null;
                    reserveButton.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show("Failed to reserve the seat. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reserving seat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            // Close this window and return to main menu
            this.Close();
        }
    }
}