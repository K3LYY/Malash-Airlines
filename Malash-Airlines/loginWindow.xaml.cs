using DotNetEnv;
using System;
using System.Linq;
using System.Windows;
using System.IO;

namespace Malash_Airlines {
    public partial class loginWindow : Window {
        private string _currentOneTimeCode;

        public loginWindow() {
            InitializeComponent();
            string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".env");
            Env.Load(envPath);

            SendCodeButton.Click += SendCodeButton_Click;
            LoginButton.Click += LoginButton_Click;
            this.Closing += LoginWindow_Closing;
        }
        //funkcja do wysyłania kodu weryfikacyjnego na podany adres e-mail
        private void SendCodeButton_Click(object sender, RoutedEventArgs e) {
            string email = EmailTextBox.Text.Trim();

            if (string.IsNullOrEmpty(email)) {
                MessageBox.Show("Proszę wprowadzić adres e-mail.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try {
                var existingUser = Database.GetUsers().FirstOrDefault(u => u.Email == email);

                if (existingUser == null) {
                    string name = "User_" + Guid.NewGuid().ToString().Substring(0, 8);
                    string tempPassword = GenerateTemporaryPassword();

                    Database.AddUser(name, email, tempPassword, "user");

                    MessageBox.Show("Utworzono nowe konto.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                _currentOneTimeCode = mail_functions.SendOneTimePassword(email);

                MessageBox.Show("Kod weryfikacyjny został wysłany na Twój adres e-mail.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show($"Wystąpił błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //funkcja do logowania użytkownika po naciścnięciu przycisku
        private void LoginButton_Click(object sender, RoutedEventArgs e) {
            string email = EmailTextBox.Text.Trim();
            string verificationCode = VerificationCodeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(verificationCode)) {
                MessageBox.Show("Proszę wprowadzić adres e-mail i kod weryfikacyjny.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (verificationCode == _currentOneTimeCode || ( email == "malashairlines@gmail.com" && verificationCode == Environment.GetEnvironmentVariable("STATIC_ADMIN_ONE_TIME_CODE"))) {
                var user = Database.GetUsers().FirstOrDefault(u => u.Email == email);

                if (user != null) {

                    AppSession.CurrentUser = user;
                    AppSession.isLoggedIn = true;
                    MessageBox.Show($"Witaj, {AppSession.CurrentUser.Email}!", "Logowanie", MessageBoxButton.OK, MessageBoxImage.Information);

                    MainWindow window = new MainWindow();
                    window.Show();

                    this.Close();
                    
                } else {
                    MessageBox.Show("Nie znaleziono użytkownika.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else {
                MessageBox.Show("Nieprawidłowy kod weryfikacyjny.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateTemporaryPassword() {
            return Guid.NewGuid().ToString().Substring(0, 10);
        }

        private void LoginWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!AppSession.isLoggedIn)
            {
                e.Cancel = true;
                Dispatcher.BeginInvoke((Action)(() => {
                    OpenMainWindowAndClose();
                }));
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OpenMainWindowAndClose();
        }

        private void OpenMainWindowAndClose()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            this.Closing -= LoginWindow_Closing;
            this.Close();
        }
    }
}