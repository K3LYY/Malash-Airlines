using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Malash_Airlines {
    public partial class SeatLayout : Window {
        private List<Button> allSeats = new List<Button>();
        private Button selectedSeat = null;
        public string SelectedSeatNumber { get; private set; }

        public SeatLayout() {
            InitializeComponent();
            CreateFirstClassSeats();
            CreateEconomySeats();
        }

        public SeatLayout(string layoutType, List<string> occupiedSeats) : this() {
            MarkOccupiedSeats(occupiedSeats);
        }

        private void CreateFirstClassSeats() {
            for (int row = 1; row <= 4; row++) {
                TextBlock rowNumber = new TextBlock {
                    Text = row.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(rowNumber, row);
                Grid.SetColumn(rowNumber, 0);
                FirstClassGrid.Children.Add(rowNumber);

                CreateSeat(FirstClassGrid, row, 1, $"{row}A", true);
                CreateSeat(FirstClassGrid, row, 2, $"{row}B", true);
                CreateSeat(FirstClassGrid, row, 4, $"{row}C", true);
                CreateSeat(FirstClassGrid, row, 5, $"{row}D", true);
            }
        }

        private void CreateEconomySeats() {
            for (int i = 0; i < 26; i++) {
                EconomyGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
            }

            for (int row = 5; row <= 30; row++) {
                int gridRow = row - 4;

                TextBlock rowNumber = new TextBlock {
                    Text = row.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(rowNumber, gridRow);
                Grid.SetColumn(rowNumber, 0);
                EconomyGrid.Children.Add(rowNumber);

                CreateSeat(EconomyGrid, gridRow, 1, $"{row}A", false);
                CreateSeat(EconomyGrid, gridRow, 2, $"{row}B", false);
                CreateSeat(EconomyGrid, gridRow, 3, $"{row}C", false);
                CreateSeat(EconomyGrid, gridRow, 5, $"{row}D", false);
                CreateSeat(EconomyGrid, gridRow, 6, $"{row}E", false);
                CreateSeat(EconomyGrid, gridRow, 7, $"{row}F", false);
            }
        }

        private void CreateSeat(Grid parentGrid, int row, int column, string seatNumber, bool isFirstClass) {
            Button seat = new Button {
                Content = seatNumber[^1..],
                Width = isFirstClass ? 40 : 30,
                Height = isFirstClass ? 40 : 30,
                Margin = new Thickness(2),
                Background = isFirstClass ? Brushes.LightBlue : Brushes.LightGreen,
                Tag = new SeatInfo { SeatNumber = seatNumber, IsFirstClass = isFirstClass }
            };

            seat.Click += Seat_Click;

            Grid.SetRow(seat, row);
            Grid.SetColumn(seat, column);

            parentGrid.Children.Add(seat);
            allSeats.Add(seat);
        }

        private void MarkOccupiedSeats(List<string> occupiedSeats) {
            if (occupiedSeats == null) return;

            foreach (var seat in allSeats) {
                if (seat.Tag is SeatInfo seatInfo && occupiedSeats.Contains(seatInfo.SeatNumber)) {
                    seat.Background = Brushes.Gray;
                    seat.IsEnabled = false;
                    seat.Content = "X";
                }
            }
        }

        private void Seat_Click(object sender, RoutedEventArgs e) {
            if (sender is Button clickedSeat && clickedSeat.Tag is SeatInfo seatInfo) {
                if (!clickedSeat.IsEnabled) return;

                if (selectedSeat != null) {
                    SeatInfo prevSeatInfo = selectedSeat.Tag as SeatInfo;
                    selectedSeat.Background = prevSeatInfo.IsFirstClass ? Brushes.LightBlue : Brushes.LightGreen;
                    selectedSeat.Content = prevSeatInfo.SeatNumber[^1..];
                }

                selectedSeat = clickedSeat;
                selectedSeat.Background = Brushes.Orange;
                selectedSeat.Content = "✓";
                SelectedSeatNumber = seatInfo.SeatNumber;

                MessageBox.Show($"Selected seat: {seatInfo.SeatNumber} ({(seatInfo.IsFirstClass ? "First Class" : "Economy")})",
                    "Seat Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
            if (selectedSeat != null && selectedSeat.Tag is SeatInfo seatInfo) {
                SelectedSeatNumber = seatInfo.SeatNumber;
                DialogResult = true;
                Close();
            } else {
                MessageBox.Show("Please select a seat first.", "No Seat Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class SeatInfo {
        public string SeatNumber { get; set; }
        public bool IsFirstClass { get; set; }
    }
}