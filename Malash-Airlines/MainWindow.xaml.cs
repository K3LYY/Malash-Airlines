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
            WorkerPanel panel = new WorkerPanel();
            panel.Show();
            SeatLayout layout = new SeatLayout();
            layout.Show();
            Database db = new Database();
            MessageBox.Show(db.GetAirports().Count().ToString());
        }

        private void loginButtonClick(object sender, RoutedEventArgs e)
        {
            loginWindow window = new loginWindow();
            window.ShowDialog();
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

        
    }
}