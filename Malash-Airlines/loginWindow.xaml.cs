using System;
using System.Linq;
using System.Windows;

namespace Malash_Airlines {
    public partial class loginWindow : Window {
        //private Database ;
        private string _currentOneTimeCode;

        public loginWindow() {
            InitializeComponent();
            // = new Database();

            // Wire up event handlers
            SendCodeButton.Click += SendCodeButton_Click;
            LoginButton.Click += LoginButton_Click;
        }

        private void SendCodeButton_Click(object sender, RoutedEventArgs e) {
            string email = EmailTextBox.Text.Trim();

            if (string.IsNullOrEmpty(email)) {
                MessageBox.Show("Proszę wprowadzić adres e-mail.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try {
                // Check if user exists
                var existingUser = Database.GetUsers().FirstOrDefault(u => u.Email == email);

                if (existingUser == null) {
                    // User doesn't exist, create a new user
                    string name = "User_" + Guid.NewGuid().ToString().Substring(0, 8);
                    string tempPassword = GenerateTemporaryPassword();

                    // Add user to database with a default role
                    Database.AddUser(name, email, tempPassword, "user");

                    MessageBox.Show("Utworzono nowe konto.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Send one-time password
                _currentOneTimeCode = mail_functions.SendOneTimePassword(email);

                MessageBox.Show("Kod weryfikacyjny został wysłany na Twój adres e-mail.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e) {
            string email = EmailTextBox.Text.Trim();
            string verificationCode = VerificationCodeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(verificationCode)) {
                MessageBox.Show("Proszę wprowadzić adres e-mail i kod weryfikacyjny.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (verificationCode == _currentOneTimeCode) {
                // Find the user by email
                var user = Database.GetUsers().FirstOrDefault(u => u.Email == email);

                if (user != null) {
                    // Successful login
                    MessageBox.Show($"Witaj, {user.Name}!", "Logowanie", MessageBoxButton.OK, MessageBoxImage.Information);

                    // TODO: Open main application window
                    // For now, just close the login window
                    this.Close();
                } else {
                    MessageBox.Show("Nie znaleziono użytkownika.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else {
                MessageBox.Show("Nieprawidłowy kod weryfikacyjny.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateTemporaryPassword() {
            // Simple temporary password generation 
            // In a real-world scenario, use a more secure method
            return Guid.NewGuid().ToString().Substring(0, 10);
        }
    }
}