using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CognexDriver
{
    class TCPClient
    {
        private int port;
        private IPAddress ipAddress;
        private TcpClient tcpClient;
        private ASCIIEncoding asen;
        private byte[] bufferSent;
        private byte[] bufferReceived;
        private int qtyBytesReceived;
        private Stream stream;

        public TCPClient(string IpAddress, int Port, int TimeOut)
        {
            ipAddress = IPAddress.Parse(IpAddress);
            port = Port;

            tcpClient = new TcpClient();
            tcpClient.ReceiveTimeout = TimeOut;
            tcpClient.SendTimeout = TimeOut;
            asen = new ASCIIEncoding();

            bufferSent = new byte[16384];
            bufferReceived = new byte[16384];
            qtyBytesReceived = 0;
        }       
        public string Connect()
        {
            tcpClient.Connect(ipAddress, port);
            stream = tcpClient.GetStream();

            return "Connected";
        }
        public void SendMessage(string Message)
        {
            bufferSent = asen.GetBytes(Message);
            stream.Write(bufferSent, 0, bufferSent.Length);
        }
        public string ReceiveMessage()
        {
            qtyBytesReceived = stream.Read(bufferReceived, 0, bufferReceived.Length);
            return Encoding.ASCII.GetString(bufferReceived).Substring(0, qtyBytesReceived);
        }
        public string Disconnect()
        {
            stream.Flush();
            stream.Close();
            tcpClient.Close();

            return "Disconnected";
        }
    }
}
