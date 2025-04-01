using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Malash_Airlines
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //WorkerPanel panel = new WorkerPanel();
            //panel.Show();
            //SeatLayout layout = new SeatLayout();
            //layout.Show();
            //Database db = new Database();
            //MessageBox.Show(db.GetAirports().Count().ToString());
            //mail_functions.SendOneTimePassword("kacper.zaluska7@gmail.com");
           
        }

        private void loginButtonClick(object sender, RoutedEventArgs e)
        {
            loginWindow window = new loginWindow();
            window.ShowDialog();
            MainWindow main = new MainWindow();
            main.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            


        }

        public void Timer_Tick(object sender, EventArgs e)
        {
            
            timelbl.Content = DateTime.Now.ToLongTimeString();
            datelbl.Content = DateTime.Now.ToLongDateString();
        }

        private void UpdateLoginButtonVisibility()
        {
            // Sprawdź, czy kontrolka LoginButton została już zainicjalizowana
            if (loginButton != null)
            {
                // Jeśli użytkownik jest zalogowany (IsLoggedIn == true), ukryj przycisk (Collapsed)
                // W przeciwnym razie (IsLoggedIn == false), pokaż przycisk (Visible)
                loginButton.Visibility = AppSession.isLoggedIn ? Visibility.Collapsed : Visibility.Visible;

                // Możesz tutaj dodać logikę dla innych elementów, np. przycisku "Wyloguj"
                // lub wyświetlania nazwy zalogowanego użytkownika
                // if (LogoutButton != null)
                // {
                //     LogoutButton.Visibility = AppSession.IsLoggedIn ? Visibility.Visible : Visibility.Collapsed;
                // }
                // if (UserInfoLabel != null)
                // {
                //     UserInfoLabel.Content = AppSession.IsLoggedIn ? $"Zalogowano jako: {AppSession.Username}" : string.Empty;
                //     UserInfoLabel.Visibility = AppSession.IsLoggedIn ? Visibility.Visible : Visibility.Collapsed;
                // }
            }
        }
    }
}