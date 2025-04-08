using System;
using System.Windows;
using System.Windows.Controls;

namespace Malash_Airlines
{
    /// <summary>
    /// Interaction logic for UserProfile.xaml
    /// </summary>
    public partial class UserProfile : Window
    {
        private User originalUserData;

        public UserProfile()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AppSession.checkSession())
            {
                MessageBox.Show("You need to be logged in to access this page.", "Authentication Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                ReturnToMainWindow();
                return;
            }

            LoadUserData();
        }

        private void LoadUserData()
        {
            try
            {
                // Make sure we have current user data
                originalUserData = AppSession.CurrentUser;

                if (originalUserData != null)
                {
                    // Display user data in form
                    nameTextBox.Text = originalUserData.Name;
                    emailTextBox.Text = originalUserData.Email;
                    usernameTextBox.Text = originalUserData.Name; // Using Name as username
                    customerTypeTextBox.Text = originalUserData.CustomerType ?? "Standard";

                    statusMessage.Text = "User profile loaded successfully.";
                }
                else
                {
                    MessageBox.Show("Could not load user data.", "Data Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ReturnToMainWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                statusMessage.Text = "Error loading user profile.";
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(nameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(emailTextBox.Text) ||
                    string.IsNullOrWhiteSpace(usernameTextBox.Text))
                {
                    MessageBox.Show("All fields must be filled in.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Prepare updated user data (for database update)
                string updatedName = usernameTextBox.Text;
                string updatedName = nameTextBox.Text;
                string updatedEmail = emailTextBox.Text;

                // Update user in database
                // This would require adding a new method to the Database class
                bool success = UpdateUserInDatabase(AppSession.CurrentUser.ID, updatedName, updatedEmail);

                if (success)
                {
                    // Update local session
                    AppSession.CurrentUser.Name = updatedName;
                    AppSession.CurrentUser.Email = updatedEmail;

                    // If we got here, everything worked
                    MessageBox.Show("Your profile has been updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    statusMessage.Text = "Profile updated successfully.";
                }
                else
                {
                    MessageBox.Show("Failed to update profile.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    statusMessage.Text = "Failed to update profile.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                statusMessage.Text = "Error updating profile.";
            }
        }

        //private bool UpdateUserInDatabase(int userId, string name, string email)
        //{
        //    try
        //    {
        //        // This method needs to be added to the Database class
        //        // For now, we'll create a simple implementation here
        //        using (var connection = new MySql.Data.MySqlClient.MySqlConnection(GetConnectionString()))
        //        {
        //            connection.Open();
        //            string query = "UPDATE users SET Name = @Name, Email = @Email WHERE ID = @UserID;";
        //            using (var command = new MySql.Data.MySqlClient.MySqlCommand(query, connection))
        //            {
        //                command.Parameters.AddWithValue("@Name", name);
        //                command.Parameters.AddWithValue("@Email", email);
        //                command.Parameters.AddWithValue("@UserID", userId);
        //                int rowsAffected = command.ExecuteNonQuery();
        //                return rowsAffected > 0;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new ApplicationException($"Error updating user: {ex.Message}", ex);
        //    }
        //}

        private string GetConnectionString()
        {
            // This is a temporary solution - in production code, you should access
            // the connection string from the Database class directly if possible
            return Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ??
                throw new InvalidOperationException("DATABASE_CONNECTION_STRING is not set in the environment variables.");
        }

        //private void Cancel_Click(object sender, RoutedEventArgs e)
        //{
        //    // Reset form to original values
        //    nameTextBox.Text = originalUserData.Name;
        //    emailTextBox.Text = originalUserData.Email;
        //    usernameTextBox.Text = originalUserData.Name;

        //    statusMessage.Text = "Changes canceled.";
        //}

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ReturnToMainWindow();
        }

        private void ReturnToMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private bool UpdateUserInDatabase(int userId, string name, string email)
        {
            try
            {
                // Use the Database class method to update user
                return Database.UpdateUser(userId, name, email);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error updating user: {ex.Message}", ex);
            }
        }
    }
}
