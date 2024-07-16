using System;
using System.IO.Ports;
using System.Linq;

namespace ModbusRelay
{
    internal class ModBus : IDisposable
    {
        private string comport;
        private int bautrate;
        private Parity parity = Parity.None;
        private int dataBits = 8;
        private StopBits stopBits = StopBits.One;
        SerialPort port;

        public ModBus(string comport, int bautrate)
        {
            this.comport = comport;
            this.bautrate = bautrate;

            port = new SerialPort
            {
                PortName = this.comport,
                BaudRate = this.bautrate,
                Parity = this.parity,
                DataBits = this.dataBits,
                StopBits = this.stopBits,
                ReadTimeout = Properties.Settings.Default.TimeOut,
                WriteTimeout = Properties.Settings.Default.TimeOut
            };
            port.Open();
        }

        public string Write(string bytes)
        {
            // Convert the string to a byte array
            byte[] request = bytes.Split(' ').Select(s => Convert.ToByte(s, 16)).ToArray();

            // Calculate CRC
            byte[] crc = CalculateCRC(request);

            // Add CRC to the request
            byte[] fullRequest = new byte[request.Length + 2];
            Array.Copy(request, fullRequest, request.Length);
            fullRequest[request.Length] = crc[0];
            fullRequest[request.Length + 1] = crc[1];

            // Отправка запроса
            port.Write(fullRequest, 0, fullRequest.Length);
            return $"{String.Join(" ", fullRequest.Select(num => num.ToString("X2")))}";
        }

        public string Read()
        {
            // Создание буфера для ответа
            byte[] buffer = new byte[255];
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

            // Уменьшаем буфер до фактически прочитанного объема данных
            Array.Resize(ref buffer, bytesRead);

            if (buffer.Length == 0) return "Нет данных";

            string crcCheck = CheckCRC(buffer) ? "OK" : "MISMATCH";
            return $"{String.Join(" ", buffer.Select(b => b.ToString("X2")))} [CRC {crcCheck}]";
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

        public void Dispose()
        {
            port.Close();
            port.Dispose();
        }
    }
}
