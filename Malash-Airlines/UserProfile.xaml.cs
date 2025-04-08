//using System;
//using System.Windows;
//using System.Windows.Controls;

//namespace Malash_Airlines
//{
//    /// <summary>
//    /// Interaction logic for UserProfile.xaml
//    /// </summary>
//    public partial class UserProfile : Window
//    {
//        private User originalUserData;

//        public UserProfile()
//        {
//            InitializeComponent();
//        }

//        private void Window_Loaded(object sender, RoutedEventArgs e)
//        {
//            if (!AppSession.checkSession())
//            {
//                MessageBox.Show("You need to be logged in to access this page.", "Authentication Required", MessageBoxButton.OK, MessageBoxImage.Warning);
//                ReturnToMainWindow();
//                return;
//            }

//            LoadUserData();
//        }

//        private void LoadUserData()
//        {
//            try
//            {
//                // Make sure we have current user data
//                originalUserData = AppSession.CurrentUser;

//                if (originalUserData != null)
//                {
//                    // Display user data in form
//                    nameTextBox.Text = originalUserData.Name;
//                    emailTextBox.Text = originalUserData.Email;
//                    //usernameTextBox.Text = originalUserData.Name; // Using Name as username
//                    //customerTypeTextBox.Text = originalUserData.CustomerType ?? "Standard";

//                    statusMessage.Text = "User profile loaded successfully.";
//                }
//                else
//                {
//                    MessageBox.Show("Could not load user data.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error);
//                    ReturnToMainWindow();
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Error loading user data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//                statusMessage.Text = "Error loading user profile.";
//            }
//        }

//        private void SaveChanges_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                // Validate inputs
//                if (string.IsNullOrWhiteSpace(nameTextBox.Text) ||
//                    string.IsNullOrWhiteSpace(emailTextBox.Text)/* ||
//                    string.IsNullOrWhiteSpace(usernameTextBox.Text)*/)
//                {
//                    MessageBox.Show("All fields must be filled in.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
//                    return;
//                }

//                // Prepare updated user data (for database update)
//                //string updatedName = usernameTextBox.Text;
//                string updatedName = nameTextBox.Text;
//                string updatedEmail = emailTextBox.Text;

//                // Update user in database
//                // This would require adding a new method to the Database class
//                bool success = UpdateUserInDatabase(AppSession.CurrentUser.ID, updatedName, updatedEmail);

//                if (success)
//                {
//                    // Update local session
//                    AppSession.CurrentUser.Name = updatedName;
//                    AppSession.CurrentUser.Email = updatedEmail;

//                    // If we got here, everything worked
//                    MessageBox.Show("Your profile has been updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
//                    statusMessage.Text = "Profile updated successfully.";
//                }
//                else
//                {
//                    MessageBox.Show("Failed to update profile.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//                    statusMessage.Text = "Failed to update profile.";
//                }
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Error updating profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
//                statusMessage.Text = "Error updating profile.";
//            }
//        }

//        //private bool UpdateUserInDatabase(int userId, string name, string email)
//        //{
//        //    try
//        //    {
//        //        // This method needs to be added to the Database class
//        //        // For now, we'll create a simple implementation here
//        //        using (var connection = new MySql.Data.MySqlClient.MySqlConnection(GetConnectionString()))
//        //        {
//        //            connection.Open();
//        //            string query = "UPDATE users SET Name = @Name, Email = @Email WHERE ID = @UserID;";
//        //            using (var command = new MySql.Data.MySqlClient.MySqlCommand(query, connection))
//        //            {
//        //                command.Parameters.AddWithValue("@Name", name);
//        //                command.Parameters.AddWithValue("@Email", email);
//        //                command.Parameters.AddWithValue("@UserID", userId);
//        //                int rowsAffected = command.ExecuteNonQuery();
//        //                return rowsAffected > 0;
//        //            }
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        throw new ApplicationException($"Error updating user: {ex.Message}", ex);
//        //    }
//        //}

//        private string GetConnectionString()
//        {
//            // This is a temporary solution - in production code, you should access
//            // the connection string from the Database class directly if possible
//            return Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ??
//                throw new InvalidOperationException("DATABASE_CONNECTION_STRING is not set in the environment variables.");
//        }

//        private void Cancel_Click(object sender, RoutedEventArgs e)
//        {
//            // Reset form to original values
//            nameTextBox.Text = originalUserData.Name;
//            emailTextBox.Text = originalUserData.Email;
//            //usernameTextBox.Text = originalUserData.Name;

//            statusMessage.Text = "Changes canceled.";
//        }

//        private void BackButton_Click(object sender, RoutedEventArgs e)
//        {
//            ReturnToMainWindow();
//        }

//        private void ReturnToMainWindow()
//        {
//            MainWindow mainWindow = new MainWindow();
//            mainWindow.Show();
//            this.Close();
//        }
//        private bool UpdateUserInDatabase(int userId, string name, string email)
//        {
//            try
//            {
//                // Use the Database class method to update user
//                return Database.UpdateUser(userId, name, email);
//            }
//            catch (Exception ex)
//            {
//                throw new ApplicationException($"Error updating user: {ex.Message}", ex);
//            }
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Malash_Airlines
{
    /// <summary>
    /// Interaction logic for UserProfile.xaml
    /// </summary>
    public partial class UserProfile : Window
    {
        private User _currentUser;
        private string _originalName;
        private string _originalEmail;
        private ObservableCollection<UserReservation> _userReservations;

        public UserProfile(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _userReservations = new ObservableCollection<UserReservation>();
            reservationsDataGrid.ItemsSource = _userReservations;
        }
        private void ReturnToMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
           ReturnToMainWindow();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AppSession.checkSession())
            {
                MessageBox.Show("You need to be logged in to access this page.", "Authentication Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                ReturnToMainWindow();
            }
            else
            {
                LoadUserInfo();
                LoadUserReservations();
            }
                           
        }

        private void LoadUserInfo()
        {
            nameTextBox.Text = _currentUser.Name;
            emailTextBox.Text = _currentUser.Email;

            // Store original values for comparison when saving
            _originalName = _currentUser.Name;
            _originalEmail = _currentUser.Email;
        }

        private void LoadUserReservations()
        {
            try
            {
                // Clear existing reservations
                _userReservations.Clear();

                // Get user reservations from database
                var userReservations = Database.GetReservations(_currentUser.ID);

                foreach (var reservation in userReservations)
                {
                    // Get the flight details for this reservation
                    var flight = Database.GetFlightById(reservation.FlightID);

                    if (flight != null)
                    {
                        _userReservations.Add(new UserReservation
                        {
                            ReservationID = reservation.ID,
                            FlightID = flight.ID,
                            Departure = flight.Departure,
                            Destination = flight.Destination,
                            Date = flight.Date,
                            Time = flight.Time,
                            SeatNumber = reservation.SeatNumber,
                            Status = reservation.Status,
                            Price = flight.Price
                        });
                    }
                }

                // Update status message
                statusMessage.Text = $"Loaded {_userReservations.Count} reservation(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reservations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                statusMessage.Text = "Failed to load reservations";
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(nameTextBox.Text) || string.IsNullOrWhiteSpace(emailTextBox.Text))
                {
                    MessageBox.Show("Name and Email cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if there are any changes
                if (_originalName == nameTextBox.Text && _originalEmail == emailTextBox.Text)
                {
                    statusMessage.Text = "No changes to save";
                    return;
                }

                // Update user in database
                bool result = Database.UpdateUser(_currentUser.ID, nameTextBox.Text, emailTextBox.Text);

                if (result)
                {
                    // Update current user object
                    _currentUser.Name = nameTextBox.Text;
                    _currentUser.Email = emailTextBox.Text;

                    // Update original values
                    _originalName = _currentUser.Name;
                    _originalEmail = _currentUser.Email;

                    statusMessage.Text = "User information updated successfully";
                    MessageBox.Show("Your profile has been updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    statusMessage.Text = "Failed to update user information";
                    MessageBox.Show("Failed to update your profile. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                statusMessage.Text = "Error updating user profile";
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Restore original values
            nameTextBox.Text = _originalName;
            emailTextBox.Text = _originalEmail;
            statusMessage.Text = "Changes cancelled";
        }

       

        private void ReservationsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedReservation = reservationsDataGrid.SelectedItem as UserReservation;

            // Enable or disable buttons based on selection and status
            if (selectedReservation != null)
            {
                bool isPending = selectedReservation.Status.ToLower() == "unconfirmed" ||
                                 selectedReservation.Status.ToLower() == "pending";

                payReservationButton.IsEnabled = isPending;
                cancelReservationButton.IsEnabled = true;
            }
            else
            {
                payReservationButton.IsEnabled = false;
                cancelReservationButton.IsEnabled = false;
            }
        }

        private void PayReservation_Click(object sender, RoutedEventArgs e)
        {
            var selectedReservation = reservationsDataGrid.SelectedItem as UserReservation;

            if (selectedReservation != null)
            {
                try
                {
                    // Simple payment confirmation dialog
                    MessageBoxResult result = MessageBox.Show(
                        $"Confirm payment of {selectedReservation.Price:C} for your flight from {selectedReservation.Departure} to {selectedReservation.Destination}?",
                        "Payment Confirmation",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Update reservation status in database
                        bool updateResult = Database.UpdateReservation(selectedReservation.ReservationID, "confirmed");

                        if (updateResult)
                        {
                            // Update status in local collection
                            selectedReservation.Status = "confirmed";

                            // Refresh DataGrid
                            reservationsDataGrid.Items.Refresh();

                            // Update selection to refresh button states
                            ReservationsDataGrid_SelectionChanged(null, null);

                            statusMessage.Text = "Payment successful";
                            MessageBox.Show("Payment successful! Your reservation is now confirmed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            statusMessage.Text = "Payment failed";
                            MessageBox.Show("Payment processing failed. Please try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    statusMessage.Text = "Error processing payment";
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelReservation_Click(object sender, RoutedEventArgs e)
        {
            var selectedReservation = reservationsDataGrid.SelectedItem as UserReservation;

            if (selectedReservation != null)
            {
                try
                {
                    // Confirmation dialog
                    MessageBoxResult result = MessageBox.Show(
                        $"Are you sure you want to cancel your reservation for the flight from {selectedReservation.Departure} to {selectedReservation.Destination}?",
                        "Cancel Reservation",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Two options: update status to cancelled or remove completely
                        // Option 1: Update status to "cancelled"
                        bool updateResult = Database.UpdateReservation(selectedReservation.ReservationID, "cancelled");

                        // Option 2: Remove reservation completely (uncomment if preferred)
                        // bool updateResult = Database.RemoveReservation(selectedReservation.ReservationID);

                        if (updateResult)
                        {
                            // Update or remove from local collection
                            if (selectedReservation.Status.ToLower() == "cancelled")
                            {
                                // Remove from collection if fully cancelled
                                _userReservations.Remove(selectedReservation);
                            }
                            else
                            {
                                // Update status in local collection
                                selectedReservation.Status = "cancelled";
                                reservationsDataGrid.Items.Refresh();
                            }

                            // Update selection to refresh button states
                            ReservationsDataGrid_SelectionChanged(null, null);

                            statusMessage.Text = "Reservation cancelled";
                            MessageBox.Show("Your reservation has been cancelled successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            statusMessage.Text = "Failed to cancel reservation";
                            MessageBox.Show("Failed to cancel reservation. Please try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    statusMessage.Text = "Error cancelling reservation";
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshReservations_Click(object sender, RoutedEventArgs e)
        {
            LoadUserReservations();
        }
    }

    // ViewModel for user reservations
    public class UserReservation
    {
        public int ReservationID { get; set; }
        public int FlightID { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string SeatNumber { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
    }
}
