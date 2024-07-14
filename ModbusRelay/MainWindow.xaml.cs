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

        private void SwithRelay_clik1(object sender, RoutedEventArgs e)
        {
            try
            {
                byte adr = Convert.ToByte(AdressBox.Text);
                int rate = Convert.ToInt32(BaudRateCB.Text);
                string port = PortCB.Text;

                if ((bool)R1on.IsChecked) WriteSerial(port, rate, adr, 0, 0xFF);
                if ((bool)R1off.IsChecked) WriteSerial(port, rate, adr, 0, 0);
            }
            catch (Exception ex)
            {
                OutputList.Items.Add($"err: {ex.Message}");
            }
        }

        private void SwithRelay_clik2(object sender, RoutedEventArgs e)
        {
            try
            {
                byte adr = Convert.ToByte(AdressBox.Text);
                int rate = Convert.ToInt32(BaudRateCB.Text);
                string port = PortCB.Text;

                if ((bool)R2on.IsChecked) WriteSerial(port, rate, adr, 1, 0xFF);
                if ((bool)R2off.IsChecked) WriteSerial(port, rate, adr, 1, 0);
            }
            catch (Exception ex)
            {
                OutputList.Items.Add($"err: {ex.Message}");
            }
        }


        private void SwithRelay1_click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                SwithRelay(btn, 0);
                switch (btn.Content.ToString())
                {
                    case "ON": R1on.IsChecked = true; break;
                    case "OFF": R1off.IsChecked = true; break;
                }
            }
        }

        private void SwithRelay2_click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                SwithRelay(btn, 1);
                switch (btn.Content.ToString())
                {
                    case "ON": R2on.IsChecked = true; break;
                    case "OFF": R2off.IsChecked = true; break;
                }
            }
        }

        private void SwithRelay(Button btn, byte Nrelay)
        {
            try
            {
                byte adr = Convert.ToByte(AdressBox.Text);
                int rate = Convert.ToInt32(BaudRateCB.Text);
                string port = PortCB.Text;

                if (btn.Content.ToString() == "OFF")
                {
                    WriteSerial(port, rate, adr, Nrelay, 0xFF);
                    btn.Content = "ON";
                    btn.Background = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    WriteSerial(port, rate, adr, Nrelay, 0);
                    btn.Content = "OFF";
                    btn.Background = null;
                }
            }
            catch (Exception ex)
            {
                OutputList.Items.Add($"err: {ex.Message}");
            }
        }

        private void WriteSerial(string comport, int baudrate, string byteString)
        {
            using (var port = new SerialPort(comport))
            {
                port.BaudRate = baudrate;
                port.Parity = Parity.None;
                port.DataBits = 8;
                port.StopBits = StopBits.One;
                port.ReadTimeout = Properties.Settings.Default.TimeOut;
                port.WriteTimeout = Properties.Settings.Default.TimeOut;

                try
                {
                    port.Open();

                    // Convert the string to a byte array
                    byte[] request = byteString.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();

                    // Calculate CRC
                    byte[] crc = CalculateCRC(request);

                    // Add CRC to the request
                    byte[] fullRequest = new byte[request.Length + 2];
                    Array.Copy(request, fullRequest, request.Length);
                    fullRequest[request.Length] = crc[0];
                    fullRequest[request.Length + 1] = crc[1];

                    // Отправка запроса
                    OutputList.Items.Add($"<- : {String.Join(" ", fullRequest.Select(num => num.ToString("X2")))}");
                    port.Write(fullRequest, 0, fullRequest.Length);

                    // Создание буфера для ответа
                    byte[] buffer = new byte[8];
                    int bytesRead = 0;
                    int totalBytesToRead = buffer.Length;
                    int attempts = 0;

                    // Чтение данных из последовательного порта
                    while (bytesRead < totalBytesToRead && attempts < 10)
                    {
                        if (port.BytesToRead > 0)
                        {
                            bytesRead += port.Read(buffer, bytesRead, totalBytesToRead - bytesRead);
                        }
                        else
                        {
                            System.Threading.Thread.Sleep(50); // Пауза для ожидания данных
                            attempts++;
                        }
                    }

                    // Проверка результата чтения
                    if (bytesRead > 0)
                    {
                        string crcCheck = CheckCRC(buffer) ? "OK" : "MISMATCH";
                        OutputList.Items.Add($" ->: {String.Join(" ", buffer.Select(b => b.ToString("X2")))} [CRC {crcCheck}]");
                    }
                }
                catch (TimeoutException)
                {
                    OutputList.Items.Add("err: Timeout");
                }
                catch (Exception ex)
                {
                    OutputList.Items.Add($"err: {ex.Message}");
                }
                finally
                {
                    if (port.IsOpen) port.Close();
                }
            }
        }

        private void WriteSerial(string comport, int baudrate, byte slaveId, byte Nr, byte Sr)
        {
            WriteSerial(comport, baudrate, $"{slaveId.ToString("X2")} 05 00 {Nr} {Sr.ToString("X2")} 00");
        }

        static byte[] CalculateCRC(byte[] data)
        {
            ushort crc = 0xFFFF;
            foreach (byte b in data)
            {
                crc ^= b;
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x0001) != 0)
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    else
                        crc >>= 1;
                }
            }

            // Преобразование 16-битного целого в массив из 2 байт
            byte[] crcBytes = new byte[2];
            crcBytes[0] = (byte)(crc & 0xFF); // Младший байт
            crcBytes[1] = (byte)((crc >> 8) & 0xFF); // Старший байт
            return crcBytes;
        }

        static bool CheckCRC(byte[] data)
        {
            byte[] receivedCrc = data.Skip(data.Length - 2).Take(2).ToArray();
            byte[] calculatedCrc = CalculateCRC(data.Take(data.Length - 2).ToArray());
            return receivedCrc.SequenceEqual(calculatedCrc);
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
                OutputList.Items.Add($"err: {ex.Message}");
            }
        }

        private void ClearOutList(object sender, RoutedEventArgs e)
        {
            OutputList.Items.Clear();
        }

        private void Send_click(object sender, RoutedEventArgs e)
        {
            try
            {
                int rate = Convert.ToInt32(BaudRateCB.Text);
                string port = PortCB.Text;

                WriteSerial(port, rate, CommandBox.Text);
            }
            catch (Exception ex)
            {
                OutputList.Items.Add($"err: {ex.Message}");
            }
        }

        private void GetAdress_click(object sender, RoutedEventArgs e)
        {

        }
    }
}
