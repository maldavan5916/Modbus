using System;
using System.IO.Ports;
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

        private void SwithRelay_clik(object sender, RoutedEventArgs e)
        {
            byte adr = Convert.ToByte(AdressBox.Text);
            int rate = Convert.ToInt32(BaudRateCB.Text);
            string port = PortCB.Text;

            if ((bool)R1on.IsChecked) textBox1.Text = WriteSwithRelay(port, rate, adr, 0x00, 0xFF);
            if ((bool)R1off.IsChecked) textBox1.Text = WriteSwithRelay(port, rate, adr, 0x00, 0x00);
            if ((bool)R2on.IsChecked) textBox1.Text = WriteSwithRelay(port, rate, adr, 0x01, 0xFF);
            if ((bool)R2off.IsChecked) textBox1.Text = WriteSwithRelay(port, rate, adr, 0x01, 0x00);
        }

        static String WriteSwithRelay(string comport, int baudrate, byte slaveId, byte Nr, byte Sr)
        {
            try
            {
                using (var port = new SerialPort(comport))
                {
                    port.BaudRate = baudrate;
                    port.Parity = Parity.None;
                    port.DataBits = 8;
                    port.StopBits = StopBits.One;
                    port.Open();

                    // Формируем запрос
                    byte[] request = new byte[] { slaveId, 0x05, 0x00, Nr, Sr, 0x00 };

                    // Вычисляем CRC (контрольную сумму)
                    ushort crc = CalculateCRC(request);

                    // Добавляем CRC к запросу
                    byte[] fullRequest = new byte[request.Length + 2];
                    Array.Copy(request, fullRequest, request.Length);
                    fullRequest[request.Length] = (byte)(crc & 0xFF);
                    fullRequest[request.Length + 1] = (byte)(crc >> 8);

                    // Отправляем запрос
                    port.Write(fullRequest, 0, fullRequest.Length);

                    return "Запрос успешно отправлен.";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
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
    }
}
