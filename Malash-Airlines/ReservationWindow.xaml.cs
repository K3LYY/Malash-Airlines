using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
namespace Malash_Airlines
{
    public partial class ReservationWindow : Window
    {
        private SeatInfo selectedSeatInfo = null;

        public ReservationWindow()
        {
            InitializeComponent();
            LoadFlights();
        }

        private void LoadFlights()
        {
            try
            {
                List<Malash_Airlines.Flight> flights = Database.GetAvailableFlights();
                if (flights == null || flights.Count == 0)
                {
                    MessageBox.Show("No available flights found.", "No Flights", MessageBoxButton.OK, MessageBoxImage.Information);
                    return; // Exit if no flights are available
                }

                FlightComboBox.ItemsSource = flights;
                FlightComboBox.DisplayMemberPath = "FlightDisplay";
                FlightComboBox.SelectedValuePath = "ID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading flights: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void OpenSeatLayout_Click(object sender, RoutedEventArgs e)
        {
            SeatLayout seatLayoutWindow = new SeatLayout();
            seatLayoutWindow.ShowDialog();

            if (seatLayoutWindow.SelectedSeatInfo != null)
            {
                selectedSeatInfo = seatLayoutWindow.SelectedSeatInfo;
                SelectedSeatTextBlock.Text = $"Selected Seat: {selectedSeatInfo.SeatNumber} ({(selectedSeatInfo.IsFirstClass ? "First Class" : "Economy")})";
            }
        }

        private void ConfirmReservation_Click(object sender, RoutedEventArgs e)
        {
            if (FlightComboBox.SelectedItem is Flight selectedFlight && selectedSeatInfo != null)
            {
                MessageBox.Show($"Reservation confirmed!\n\nFlight: {selectedFlight.FlightDisplay}\nSeat: {selectedSeatInfo.SeatNumber} ({(selectedSeatInfo.IsFirstClass ? "First Class" : "Economy")})",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please select a flight and a seat before confirming.", "Missing Info", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public class Flight
        {
            public int ID { get; set; }
            public string Departure { get; set; }
            public string Destination { get; set; }
            public DateTime Date { get; set; }
            public string Time { get; set; }
            public decimal Price { get; set; }
            public string Plane { get; set; }

            public string FlightDisplay => $"{Departure} → {Destination} on {Date:dd/MM/yyyy} at {Time} ({Plane}) - ${Price}";
        }

    }
}
