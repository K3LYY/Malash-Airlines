using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Malash_Airlines
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Flight> currentFlights;
        private int currentFlightIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
            WorkerPanel panel = new WorkerPanel();
            panel.Show();
            //SeatLayout layout = new SeatLayout();
            //layout.Show();
            //Database db = new Database();
            //MessageBox.Show(db.GetAirports().Count().ToString());
            //mail_functions.SendOneTimePassword("kacper.zaluska7@gmail.com");

        }

        private void loginButtonClick(object sender, RoutedEventArgs e)
        {
            if (loginButton.Content.ToString() == "Log In")
            {
                loginWindow window = new loginWindow();
                window.Show();
                this.Close();
            }
            else
            {
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                Application.Current.Shutdown();
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set up timer for clock and date
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            // Load flights from database
            LoadFlights();

            // Display the first flight
            if (currentFlights.Count > 0)
            {
                DisplayFlight(currentFlightIndex);
            }
            else
            {
                DisplayNoFlightsMessage();
            }
            UpdateLoginButtonVisibility();
        }
        

    
        

        public void Timer_Tick(object sender, EventArgs e)
        {
            timelbl.Content = DateTime.Now.ToLongTimeString();
            datelbl.Content = DateTime.Now.ToLongDateString();
        }
        private void UpdateLoginButtonVisibility()
        {

            if (AppSession.isLoggedIn == true)
            {
                //loginButton.Visibility = AppSession.isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
                loginButton.Content = "Wyloguj";

                Label lbl = new Label();

                Label etykieta = new Label();
                etykieta.Content = "Zalogowano mailem " + AppSession.eMail;
                etykieta.FontSize = 18;
                etykieta.Margin = new Thickness(10);
                Grid.SetRow(etykieta, 2); // Umieszczenie etykiety w drugim wierszu Grid
                Grid.SetColumn(etykieta, 2); // Umieszczenie etykiety w pierwszej kolumnie Grid
                windowGrid.Children.Add(etykieta); // MojGrid to nazwa elementu Grid w XAML

            }
        }

        private void LoadFlights()
        {
            try
            {
                // Get flights from the database
                currentFlights = Database.GetSoonestFlights(10); // Get 10 soonest flights
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading flights: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                currentFlights = new List<Flight>(); // Empty list if there's an error
            }
        }

        private void DisplayFlight(int index)
        {
            if (currentFlights.Count == 0 || index < 0 || index >= currentFlights.Count)
            {
                DisplayNoFlightsMessage();
                return;
            }

       

            Flight flight = currentFlights[index];

            // Clear previous content
            flightDisplayPanel.Children.Clear();

            // Create flight display
            Border mainContainer = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.FromRgb(44, 62, 80)), // #2C3E50
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20),
                Margin = new Thickness(10)
            };

            StackPanel flightInfo = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Flight header with flight ID
            TextBlock flightHeader = new TextBlock
            {
                Text = $"Flight #{flight.ID}",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)), // #2C3E50
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            // Route info
            TextBlock routeInfo = new TextBlock
            {
                Text = $"{flight.Departure} → {flight.Destination}",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(70, 130, 180)), // #4682B4 (SteelBlue)
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            // Date and time info
            TextBlock dateTimeInfo = new TextBlock
            {
                Text = $"{flight.Date.ToString("dddd, MMMM d, yyyy")} at {flight.Time}",
                FontSize = 18,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)), // #2C3E50
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };

            // Price info
            TextBlock priceInfo = new TextBlock
            {
                Text = $"Price: ${flight.Price}",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)), // #2C3E50
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };

            // Aircraft info
            TextBlock aircraftInfo = new TextBlock
            {
                Text = $"Aircraft: {flight.Plane}",
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)), // #2C3E50
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 25)
            };

            // Button container for multiple buttons
            StackPanel buttonContainer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };

            // Book now button
            Button bookButton = new Button
            {
                Content = "Book Now",
                FontSize = 16,
                Width = 180,
                Height = 45,
                Margin = new Thickness(5, 0, 5, 0)
            };

            bookButton.Click += (s, e) => BookFlight_Click(flight.ID);

            // View Route Map button
            Button viewMapButton = new Button
            {
                Content = "View Route Map",
                FontSize = 16,
                Width = 180,
                Height = 45,
                Margin = new Thickness(5, 0, 5, 0)
            };

            viewMapButton.Click += (s, e) => ViewFlightMap_Click(flight);

            // Add buttons to container
            buttonContainer.Children.Add(bookButton);
            buttonContainer.Children.Add(viewMapButton);

            // Navigation indicator
            TextBlock navIndicator = new TextBlock
            {
                Text = $"Flight {index + 1} of {currentFlights.Count}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)), // Gray
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            // Add all elements to the stack panel
            flightInfo.Children.Add(flightHeader);
            flightInfo.Children.Add(routeInfo);
            flightInfo.Children.Add(dateTimeInfo);
            flightInfo.Children.Add(priceInfo);
            flightInfo.Children.Add(aircraftInfo);
            flightInfo.Children.Add(buttonContainer);
            flightInfo.Children.Add(navIndicator);

            // Add the stack panel to the container
            mainContainer.Child = flightInfo;

            // Add the container to the display panel
            flightDisplayPanel.Children.Add(mainContainer);

            // Update button states
            UpdateNavigationButtons();
        }

        private void DisplayNoFlightsMessage()
        {
            flightDisplayPanel.Children.Clear();

            TextBlock noFlightsMessage = new TextBlock
            {
                Text = "No upcoming flights available",
                FontSize = 24,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            flightDisplayPanel.Children.Add(noFlightsMessage);

            // Disable both navigation buttons
            prevButton.IsEnabled = false;
            nextButton.IsEnabled = false;
        }

        private void UpdateNavigationButtons()
        {
            // Enable/disable prev button based on current index
            prevButton.IsEnabled = currentFlightIndex > 0;

            // Enable/disable next button based on current index
            nextButton.IsEnabled = currentFlightIndex < currentFlights.Count - 1;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentFlightIndex > 0)
            {
                currentFlightIndex--;
                DisplayFlight(currentFlightIndex);
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentFlightIndex < currentFlights.Count - 1)
            {
                currentFlightIndex++;
                DisplayFlight(currentFlightIndex);
            }
        }

        private void BookFlight_Click(int flightId)
        {
            // You can implement flight booking functionality here
            // For example, open a seat selection window
            MessageBox.Show($"Booking flight #{flightId}", "Book Flight", MessageBoxButton.OK, MessageBoxImage.Information);

            // Uncomment and modify this to implement actual booking
            // SeatLayout seatLayout = new SeatLayout(flightId);
            // seatLayout.ShowDialog();
        }

        private void ViewFlightMap_Click(Flight flight)
        {
            // Open the flight map visualization window
            FlightMapVisualization mapWindow = new FlightMapVisualization(flight);
            mapWindow.ShowDialog();
        }
    }
}