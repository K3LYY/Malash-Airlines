using System;
using System.Collections.Generic;
using System.Diagnostics;
using QuestPDF.Infrastructure;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using pdfColor = QuestPDF.Infrastructure.Color;
using Color = System.Windows.Media.Color;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using VerticalAlignment = System.Windows.VerticalAlignment;

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

            QuestPDF.Settings.License = LicenseType.Community;

        }

        private void loginButtonClick(object sender, RoutedEventArgs e)
        {
            if (!AppSession.isLoggedIn)
            {
                loginWindow window = new loginWindow();
                window.Show();
                this.Close();
            }
            else
            {
                var button = sender as Button;
                if (button != null)
                {
                    //button.ContextMenu.PlacementTarget = button;
                    //button.ContextMenu.IsOpen = true;
                }
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                Application.Current.Shutdown();
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            LoadFlights();

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


        private void Category1Button_Click(object sender, RoutedEventArgs e) {
            ReservationPanel reservationPanel = new ReservationPanel();
            reservationPanel.Show();
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
                loginButton.Content = "Wyloguj";

                Label lbl = new Label();

                Label etykieta = new Label();
                etykieta.Content = AppSession.eMail;
                etykieta.FontSize = 18;
                etykieta.Margin = new Thickness(10);
                Grid.SetRow(etykieta, 2); 
                Grid.SetColumn(etykieta, 2);
                windowGrid.Children.Add(etykieta);

                UpdateCategoryVisibility();
            }
        }

        private void UpdateCategoryVisibility()
        {
            if (AppSession.isLoggedIn && (AppSession.userRole == "employee" || AppSession.userRole == "admin"))
            {
                category4Button.Visibility = Visibility.Visible;
            }
            else
            {
                category4Button.Visibility = Visibility.Collapsed;
            }
        }

        private void Category4Button_Click(object sender, RoutedEventArgs e)
        {
            WorkerPanel panel = new WorkerPanel();
            panel.Show();
        }

        private void LoadFlights()
        {
            try
            {
                currentFlights = Database.GetSoonestFlights(10);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading flights: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                currentFlights = new List<Flight>();
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

            flightDisplayPanel.Children.Clear();

            Border mainContainer = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
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

            TextBlock flightHeader = new TextBlock
            {
                Text = $"Flight #{flight.ID}",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            TextBlock routeInfo = new TextBlock
            {
                Text = $"{flight.Departure} → {flight.Destination}",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(70, 130, 180)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };

            TextBlock dateTimeInfo = new TextBlock
            {
                Text = $"{flight.Date.ToString("dddd, MMMM d, yyyy")} at {flight.Time}",
                FontSize = 18,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };

            TextBlock priceInfo = new TextBlock
            {
                Text = $"Price: ${flight.Price}",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 15)
            };

            TextBlock aircraftInfo = new TextBlock
            {
                Text = $"Aircraft: {flight.Plane}",
                FontSize = 16,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 25)
            };

            StackPanel buttonContainer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };

            Button bookButton = new Button
            {
                Content = "Book Now",
                FontSize = 16,
                Width = 180,
                Height = 45,
                Margin = new Thickness(5, 0, 5, 0)
            };

            bookButton.Click += (s, e) => BookFlight_Click(flight.ID);

            Button viewMapButton = new Button
            {
                Content = "View Route Map",
                FontSize = 16,
                Width = 180,
                Height = 45,
                Margin = new Thickness(5, 0, 5, 0)
            };

            viewMapButton.Click += (s, e) => ViewFlightMap_Click(flight);

            buttonContainer.Children.Add(bookButton);
            buttonContainer.Children.Add(viewMapButton);

            TextBlock navIndicator = new TextBlock
            {
                Text = $"Flight {index + 1} of {currentFlights.Count}",
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            flightInfo.Children.Add(flightHeader);
            flightInfo.Children.Add(routeInfo);
            flightInfo.Children.Add(dateTimeInfo);
            flightInfo.Children.Add(priceInfo);
            flightInfo.Children.Add(aircraftInfo);
            flightInfo.Children.Add(buttonContainer);
            flightInfo.Children.Add(navIndicator);

            mainContainer.Child = flightInfo;

            flightDisplayPanel.Children.Add(mainContainer);

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

            prevButton.IsEnabled = false;
            nextButton.IsEnabled = false;
        }

        private void UpdateNavigationButtons()
        {
            prevButton.IsEnabled = currentFlightIndex > 0;

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

        private void BookFlight_Click(int flightId) {
            try {
                ReservationPanel reservationPanel = new ReservationPanel(flightId);
                reservationPanel.Show();
            } catch (Exception ex) {
                MessageBox.Show($"Błąd podczas otwierania panelu rezerwacji: {ex.Message}",
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshSessionData() {
            if (AppSession.isLoggedIn && AppSession.CurrentUser != null) {
                var refreshedUser = Database.GetUsers().FirstOrDefault(u => u.ID == AppSession.CurrentUser.ID);
                if (refreshedUser != null) {
                    AppSession.CurrentUser = refreshedUser;
                    UpdateLoginButtonVisibility(); 
                }
            }
        }

 
        private void RefreshButton_Click(object sender, RoutedEventArgs e) {
            RefreshSessionData();
            LoadFlights(); 
            if (currentFlights.Count > 0) {
                DisplayFlight(currentFlightIndex);
            } else {
                DisplayNoFlightsMessage();
            }
        }


        private void ViewFlightMap_Click(Flight flight)
        {
            FlightMapVisualization mapWindow = new FlightMapVisualization(flight);
            mapWindow.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UserProfile userProfile = new UserProfile(AppSession.CurrentUser);
            userProfile.Show();
            this.Close();
        }
    }
}