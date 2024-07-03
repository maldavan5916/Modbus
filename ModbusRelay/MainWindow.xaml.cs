using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Windows;

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

        private void WriteSerial(string comport, int baudrate, string byteString)
        {
            using (var port = new SerialPort(comport))
            {
                port.BaudRate = baudrate;
                port.Parity = Parity.None;
                port.DataBits = 8;
                port.StopBits = StopBits.One;
                port.ReadTimeout = Properties.Settings.Default.TimeOut;
                port.Open();

                // Convert the string to a byte array
                byte[] request = byteString.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();

                // Calculate CRC (checksum)
                ushort crc = CalculateCRC(request);

                // Add CRC to the request
                byte[] fullRequest = new byte[request.Length + 2];
                Array.Copy(request, fullRequest, request.Length);
                fullRequest[request.Length] = (byte)(crc & 0xFF);
                fullRequest[request.Length + 1] = (byte)(crc >> 8);

                // Send the request
                OutputList.Items.Add($" ⬆️ : {String.Join(" ", fullRequest.Select(num => num.ToString("X2")))}");
                port.Write(fullRequest, 0, fullRequest.Length);

                // Read the response
                try
                {
                    // Buffer to store the response
                    byte[] buffer = new byte[256];
                    int bytesRead = port.Read(buffer, 0, buffer.Length);

                    // Convert the response to a readable string
                    string response = String.Join(" ", buffer.Take(bytesRead).Select(b => b.ToString("X2")));
                    OutputList.Items.Add($" ⬇️ : {response}");
                }
                catch (TimeoutException)
                {
                    OutputList.Items.Add("err: Read timeout");
                }
            }
        }

        private void WriteSerial(string comport, int baudrate, byte slaveId, byte Nr, byte Sr)
        {
            WriteSerial(comport, baudrate, $"{slaveId.ToString("X2")} 05 00 {Nr} {Sr.ToString("X2")} 00");
        }

        static ushort CalculateCRC(byte[] data)
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
            return crc;
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
