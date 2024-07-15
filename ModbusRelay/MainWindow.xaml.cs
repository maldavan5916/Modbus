using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModbusRelay
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PortCB.ItemsSource = SerialPort.GetPortNames();
        }

        private void SwithRelay1_click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                SwithRelay(btn, 0);
            }
        }

        private void SwithRelay2_click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                SwithRelay(btn, 1);
            }
        }

        private void SwithRelay(Button btn, byte Nrelay)
        {
            try
            {
                byte adr = Convert.ToByte(AdressBox.Text);
                int rate = Convert.ToInt32(BaudRateCB.Text);
                string port = PortCB.Text;
                string result;

                ModBus modBus = new ModBus(port, rate);

                if (btn.Content.ToString() == "OFF")
                {
                    result = modBus.Write($"{adr.ToString("X2")} 05 00 {Nrelay} FF 00");
                    btn.Content = "ON";
                    btn.Background = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    result =  modBus.Write($"{adr.ToString("X2")} 05 00 {Nrelay} 00 00");
                    btn.Content = "OFF";
                    btn.Background = null;
                }

                OutputList.Text += $"<- : {result}\n" +
                                   $" ->: {modBus.Read()}\n";
                modBus.Dispose();
            }
            catch (Exception ex)
            {
                OutputList.Text += $"err: {ex.Message}\n";
            }
        }

        static void OpenBrowser(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void AbotBoxOpen(object sender, RoutedEventArgs e)
        {
            new AboutBox1().ShowDialog();
        }

        private void GitHubOpen(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenBrowser("https://github.com/maldavan5916/Modbus");
            }
            catch (Exception ex)
            {
                OutputList.Text += $"err: {ex.Message}\n";
            }
        }

        private void ClearOutList(object sender, RoutedEventArgs e)
        {
            OutputList.Text = string.Empty;
        }

        private void Send_click(object sender, RoutedEventArgs e)
        {
            try
            {
                int rate = Convert.ToInt32(BaudRateCB.Text);
                string port = PortCB.Text;

                ModBus modBus = new ModBus(port, rate);
                OutputList.Text += $"<- : {modBus.Write(CommandBox.Text)}\n" +
                                   $" ->: {modBus.Read()}\n";
                modBus.Dispose();
            }
            catch (Exception ex)
            {
                OutputList.Text += $"err: {ex.Message}\n";
            }
        }

        private void GetAdress_click(object sender, RoutedEventArgs e)
        {

        }
    }
}
