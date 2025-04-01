using System.Diagnostics;
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
            if(loginButton.Content.ToString() == "Log In")
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
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            UpdateLoginButtonVisibility();


        }

        public void Timer_Tick(object sender, EventArgs e)
        {
            
            timelbl.Content = DateTime.Now.ToLongTimeString();
            datelbl.Content = DateTime.Now.ToLongDateString();
        }

        private void UpdateLoginButtonVisibility()
        {
            
            if(AppSession.isLoggedIn == true)
            {
                //loginButton.Visibility = AppSession.isLoggedIn ? Visibility.Collapsed : Visibility.Visible;
                loginButton.Content = "Wyloguj";

                Label lbl = new Label();

                Label etykieta = new Label();
                etykieta.Content = "Zalogowano mailem "+ AppSession.eMail;
                etykieta.FontSize = 18;
                etykieta.Margin = new Thickness(10);
                Grid.SetRow(etykieta, 2); // Umieszczenie etykiety w drugim wierszu Grid
                Grid.SetColumn(etykieta, 2); // Umieszczenie etykiety w pierwszej kolumnie Grid
                windowGrid.Children.Add(etykieta); // MojGrid to nazwa elementu Grid w XAML

            }

            

        }
    }
}