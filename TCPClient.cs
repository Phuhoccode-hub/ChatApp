using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;


namespace SecureChatTCP_Client.Network
{
    class TCPClient
    {
        private TcpClient client;
        private NetworkStream stream;
        public Action<string> OnMessage;
        public bool Connect(string ip, int port, bool receive = false)
        {
            try
            {
                client = new TcpClient();
                client.Connect(ip, port);
                stream = client.GetStream();
                if (receive)
                {
                    StartReceive();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public string Send(string message)
        {
            if (client == null || !client.Connected)
            {
                return "";
            }
            byte[] data =Encoding.UTF8.GetBytes(message);
            
            stream.Write(data,0,data.Length);
            return "";
        }
        public string SendAndReceive(string message)
        {
            byte[] data =Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            byte[] buffer = new byte[131072];
            int length =stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, length);
        }
        public void SendOnly(string message)
        {
            byte[] data =Encoding.UTF8.GetBytes(message);
            stream.Write(data,0,data.Length);
        }
        private void StartReceive()
        {
            Thread t =
            new Thread(() =>
            {
                try
                {
                    while (client != null && client.Connected)
                    {
                        byte[] buffer =new byte[4096];
                        int length =stream.Read(buffer, 0, buffer.Length);
                        if (length == 0)
                            break;
                        string msg =Encoding.UTF8.GetString(buffer, 0, length);
                        OnMessage?.Invoke(msg);
                    }
                }
                catch
                {

                }
            });
            t.IsBackground = true;
            t.Start();
        }
        public void Close()
        {
            if(client != null)
                client.Close();
        }

    }
}