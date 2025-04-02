using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Malash_Airlines
{
    public partial class SeatLayout : UserControl
    {
        public event EventHandler<SeatSelectedEventArgs> SeatSelected;

        private List<Button> allSeats = new List<Button>();
        private Button selectedSeat = null;
        private List<string> reservedSeats = new List<string>();

        public SeatLayout()
        {
            InitializeComponent();
        }

        public void InitializeSeats(string seatLayout, List<string> reservedSeats)
        {
            this.reservedSeats = reservedSeats;
            ClearExistingSeats();

            // Parse seat layout and create seats
            var seatNumbers = ParseSeatLayout(seatLayout);

            foreach (var seatNumber in seatNumbers)
            {
                bool isFirstClass = seatNumber.StartsWith("1") || seatNumber.StartsWith("2") ||
                                  seatNumber.StartsWith("3") || seatNumber.StartsWith("4");

                int row = int.Parse(seatNumber.Substring(0, seatNumber.Length - 1));
                char column = seatNumber[^1];

                AddSeat(row, column, seatNumber, isFirstClass);
            }

            MarkReservedSeats();
        }

        private void ClearExistingSeats()
        {
            FirstClassGrid.Children.Clear();
            EconomyGrid.Children.Clear();
            allSeats.Clear();

            // Re-add headers
            InitializeHeaders();
        }

        private void InitializeHeaders()
        {
            // First Class Headers
            TextBlock fcA = new TextBlock { Text = "A", HorizontalAlignment = HorizontalAlignment.Center };
            TextBlock fcB = new TextBlock { Text = "B", HorizontalAlignment = HorizontalAlignment.Center };
            TextBlock fcC = new TextBlock { Text = "C", HorizontalAlignment = HorizontalAlignment.Center };
            TextBlock fcD = new TextBlock { Text = "D", HorizontalAlignment = HorizontalAlignment.Center };

            Grid.SetRow(fcA, 0); Grid.SetColumn(fcA, 1);
            Grid.SetRow(fcB, 0); Grid.SetColumn(fcB, 2);
            Grid.SetRow(fcC, 0); Grid.SetColumn(fcC, 4);
            Grid.SetRow(fcD, 0); Grid.SetColumn(fcD, 5);

            FirstClassGrid.Children.Add(fcA);
            FirstClassGrid.Children.Add(fcB);
            FirstClassGrid.Children.Add(fcC);
            FirstClassGrid.Children.Add(fcD);

            // Economy Headers
            TextBlock eA = new TextBlock { Text = "A", HorizontalAlignment = HorizontalAlignment.Center };
            TextBlock eB = new TextBlock { Text = "B", HorizontalAlignment = HorizontalAlignment.Center };
            TextBlock eC = new TextBlock { Text = "C", HorizontalAlignment = HorizontalAlignment.Center };
            TextBlock eD = new TextBlock { Text = "D", HorizontalAlignment = HorizontalAlignment.Center };
            TextBlock eE = new TextBlock { Text = "E", HorizontalAlignment = HorizontalAlignment.Center };
            TextBlock eF = new TextBlock { Text = "F", HorizontalAlignment = HorizontalAlignment.Center };

            Grid.SetRow(eA, 0); Grid.SetColumn(eA, 1);
            Grid.SetRow(eB, 0); Grid.SetColumn(eB, 2);
            Grid.SetRow(eC, 0); Grid.SetColumn(eC, 3);
            Grid.SetRow(eD, 0); Grid.SetColumn(eD, 5);
            Grid.SetRow(eE, 0); Grid.SetColumn(eE, 6);
            Grid.SetRow(eF, 0); Grid.SetColumn(eF, 7);

            EconomyGrid.Children.Add(eA);
            EconomyGrid.Children.Add(eB);
            EconomyGrid.Children.Add(eC);
            EconomyGrid.Children.Add(eD);
            EconomyGrid.Children.Add(eE);
            EconomyGrid.Children.Add(eF);
        }

        private void AddSeat(int row, char column, string seatNumber, bool isFirstClass)
        {
            Grid parentGrid = isFirstClass ? FirstClassGrid : EconomyGrid;
            int gridRow = isFirstClass ? row : row - 4;

            // Add row number if needed
            if (parentGrid.Children.OfType<TextBlock>().FirstOrDefault(t => Grid.GetRow(t) == gridRow && Grid.GetColumn(t) == 0) == null)
            {
                TextBlock rowNumber = new TextBlock
                {
                    Text = row.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(rowNumber, gridRow);
                Grid.SetColumn(rowNumber, 0);
                parentGrid.Children.Add(rowNumber);
            }

            // Determine column position based on seat letter
            int gridColumn = column switch
            {
                'A' => 1,
                'B' => 2,
                'C' => isFirstClass ? 4 : 3,
                'D' => isFirstClass ? 5 : 5,
                'E' => 6,
                'F' => 7,
                _ => 1
            };

            Button seat = new Button
            {
                Content = column.ToString(),
                Width = isFirstClass ? 40 : 30,
                Height = isFirstClass ? 40 : 30,
                Margin = new Thickness(2),
                Background = isFirstClass ? Brushes.LightBlue : Brushes.LightGreen,
                Tag = seatNumber
            };

            seat.Click += Seat_Click;
            Grid.SetRow(seat, gridRow);
            Grid.SetColumn(seat, gridColumn);
            parentGrid.Children.Add(seat);
            allSeats.Add(seat);
        }

        private void MarkReservedSeats()
        {
            foreach (Button seat in allSeats)
            {
                if (reservedSeats.Contains(seat.Tag.ToString()))
                {
                    seat.Background = Brushes.LightGray;
                    seat.Content = "X";
                    seat.IsEnabled = false;
                }
            }
        }

        private void Seat_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedSeat)
            {
                if (selectedSeat != null)
                {
                    // Reset previously selected seat
                    bool wasFirstClass = selectedSeat.Background == Brushes.LightBlue;
                    selectedSeat.Background = wasFirstClass ? Brushes.LightBlue : Brushes.LightGreen;
                    selectedSeat.Content = selectedSeat.Tag.ToString()[^1..];
                }

                // Select new seat
                selectedSeat = clickedSeat;
                selectedSeat.Background = Brushes.Orange;
                selectedSeat.Content = "✓";

                // Raise selection event
                SeatSelected?.Invoke(this, new SeatSelectedEventArgs(selectedSeat.Tag.ToString()));
            }
        }
    }

    public class SeatSelectedEventArgs : EventArgs
    {
        public string SeatNumber { get; }

        public SeatSelectedEventArgs(string seatNumber)
        {
            SeatNumber = seatNumber;
        }
    }
}