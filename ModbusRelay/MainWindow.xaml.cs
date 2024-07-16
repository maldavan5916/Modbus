using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text.RegularExpressions;
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
                    result = modBus.Write($"{adr.ToString("X2")} 05 00 {Nrelay} 00 00");
                    btn.Content = "OFF";
                    btn.Background = null;
                }

                OutputList.Text += $"<- : {result}\n";
                OutputList.Text += $" ->: {modBus.Read()}\n\n";

                modBus.Dispose();
            }
            catch (Exception ex)
            {
                OutputList.Text += $"err: {ex.Message}\n\n";
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
                OutputList.Text += $"err: {ex.Message}\n\n";
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

                OutputList.Text += $"<- : {modBus.Write(CommandBox.Text)}\n";
                OutputList.Text += $" ->: {modBus.Read()}\n\n";

                modBus.Dispose();
            }
            catch (Exception ex)
            {
                OutputList.Text += $"err: {ex.Message}\n\n";
            }
        }

        private void GetAdress_click(object sender, RoutedEventArgs e)
        {
            try
            {
                int rate = Convert.ToInt32(BaudRateCB.Text);
                string port = PortCB.Text;

                ModBus modBus = new ModBus(port, rate);

                string writeResponse = modBus.Write("00 03 00 00 00 01");
                OutputList.Text += $"<- : {writeResponse}\n";

                string readResponse = modBus.Read();
                OutputList.Text += $" ->: {readResponse}\n\n";

                AdressBox.Text = GetAdressFromResponse(readResponse).ToString();

                modBus.Dispose();
            }
            catch (Exception ex)
            {
                OutputList.Text += $"err: {ex.Message}\n\n";
            }
        }

        static byte GetAdressFromResponse(string response)
        {
            if (response.Contains("[CRC MISMATCH]")) throw new ArgumentException("Контрольная сумма (CRC) не совпала. Адрес не получен");

            // Регулярное выражение для извлечения пятого байта
            Regex byteRegex = new Regex(@"(?:[0-9A-Fa-f]{2}\s+){4}([0-9A-Fa-f]{2})");
            Match byteMatch = byteRegex.Match(response);

            if (!byteMatch.Success) throw new ArgumentException("Неверный формат строки: не найден пятый байт.");

            string AdressHex = byteMatch.Groups[1].Value;
            byte AdressDec = Convert.ToByte(AdressHex, 16);

            return AdressDec;
        }

        private void ChangeAdress_MenuClick(object sender, RoutedEventArgs e)
        {
            AdressLabel.Content = "Новый адрес";
            R1gb.Visibility = Visibility.Collapsed;
            R2gb.Visibility = Visibility.Collapsed;
            AdressChangeBtn.Visibility = Visibility.Visible;
            CancelAdressChangeBtn.Visibility = Visibility.Visible;
        }

        private void ChangeAdress_BtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                OutputList.Text += $"err: {ex.Message}\n\n";
            }

            AdressLabel.Content = "Aдрес";
            R1gb.Visibility = Visibility.Visible;
            R2gb.Visibility = Visibility.Visible;
            AdressChangeBtn.Visibility = Visibility.Collapsed;
            CancelAdressChangeBtn.Visibility = Visibility.Collapsed;
        }

        private void CancelChangeAdress_BtnClick(object sender, RoutedEventArgs e)
        {
            AdressLabel.Content = "Aдрес";
            R1gb.Visibility = Visibility.Visible;
            R2gb.Visibility = Visibility.Visible;
            AdressChangeBtn.Visibility = Visibility.Collapsed;
            CancelAdressChangeBtn.Visibility = Visibility.Collapsed;
        }

        private void ChangeBautRate_MenuClick(object sender, RoutedEventArgs e)
        {
            BautRateLabel.Content = "Новая скорость";
            R1gb.Visibility = Visibility.Collapsed;
            R2gb.Visibility = Visibility.Collapsed;
            BautRateChangeBtn.Visibility = Visibility.Visible;
            CancelBautRateChangeBtn.Visibility = Visibility.Visible;
        }

        private void ChangeBautRate_BtnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                OutputList.Text += $"err: {ex.Message}\n\n";
            }

            BautRateLabel.Content = "Cкорость";
            R1gb.Visibility = Visibility.Visible;
            R2gb.Visibility = Visibility.Visible;
            BautRateChangeBtn.Visibility = Visibility.Collapsed;
            CancelBautRateChangeBtn.Visibility = Visibility.Collapsed;
        }

        private void CancelChangeBautRate_BtnClick(object sender, RoutedEventArgs e)
        {
            BautRateLabel.Content = "Cкорость";
            R1gb.Visibility = Visibility.Visible;
            R2gb.Visibility = Visibility.Visible;
            BautRateChangeBtn.Visibility = Visibility.Collapsed;
            CancelBautRateChangeBtn.Visibility = Visibility.Collapsed;
        }
    }
}
