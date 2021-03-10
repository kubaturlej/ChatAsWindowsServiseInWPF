using System;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Threading;

namespace ServiceManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string ServiceName = "MojSerwis";
        ServiceController usluga;
        ServiceControllerStatus poprzedniStan;
        DispatcherTimer dt;
        public MainWindow()
        {
            InitializeComponent();
            usluga = new ServiceController(ServiceName);
            ServDispName.Text = usluga.DisplayName;
            ServName.Text = usluga.ServiceName;
            ServStatus.Text = usluga.Status.ToString();
            poprzedniStan = usluga.Status;
            if (usluga.Status == ServiceControllerStatus.Running)
            {
                Uruchom.IsEnabled = false;
            }
            if (usluga.Status == ServiceControllerStatus.Stopped)
            {
                Zatrzymaj.IsEnabled = false;
            }
            dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 0, 0, 500);
            dt.Tick += Dt_Tick;
            dt.Start();
        }

        private void Dt_Tick(object sender, EventArgs e)
        {
            usluga.Refresh();
            ServiceControllerStatus stan = usluga.Status;
            if (stan != poprzedniStan)
            {
                ServStatus.Text = stan.ToString();
                if (stan == ServiceControllerStatus.Running)
                {
                    Uruchom.IsEnabled = false;
                    Zatrzymaj.IsEnabled = true;
                }
                if (stan == ServiceControllerStatus.Stopped)
                {
                    Zatrzymaj.IsEnabled = false;
                    Uruchom.IsEnabled = true;
                }

                poprzedniStan = stan;
            }
        }
        private void Uruchom_Click(object sender, RoutedEventArgs e)
        {
            usluga.Start();
            Uruchom.IsEnabled = false;
            usluga.WaitForStatus(ServiceControllerStatus.Running);
            //usluga.Refresh();
            ServStatus.Text = usluga.Status.ToString();
            Zatrzymaj.IsEnabled = true;
        }

        private void Zatrzymaj_Click(object sender, RoutedEventArgs e)
        {
            usluga.Stop();
            Zatrzymaj.IsEnabled = false;
            usluga.WaitForStatus(ServiceControllerStatus.Stopped);
            //usluga.Refresh();
            ServStatus.Text = usluga.Status.ToString();
            Uruchom.IsEnabled = true;
        }
    }
}
