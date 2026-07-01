using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace SecureChatTCP_Server
{
    public partial class server : Form
    {
        TcpListener listener;
        Thread serverThread;
        List<TcpClient> clients = new List<TcpClient>();
             public server()
        {
            InitializeComponent();
            
        }
 /// /////////
        private void StartServer()
        {
            serverThread = new Thread(() =>
            {
                listener =new TcpListener(IPAddress.Any, 9999);
                listener.Start();
                AddLog("Server started");
                while (true)
                {
                    TcpClient client =listener.AcceptTcpClient();
                    clients.Add(client);
                    AddLog("Client connected");
                    Thread t =
                    new Thread(() =>
                    {
                        ReceiveClient(client);
                    });
                    t.IsBackground = true;
                    t.Start();
                }
            });
            serverThread.IsBackground = true;
            serverThread.Start();
        }
        private void ReceiveClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                while (true)
                {
                    byte[] buffer = new byte[4096];
                    int length = stream.Read(buffer, 0, buffer.Length);
                    if (length == 0)
                        break;
                    string data = Encoding.UTF8.GetString(
                    buffer, 0, length);
                    AddLog("Nhan: " + data);
                    // login register vẫn xử lý
                    if (data.StartsWith("LOGIN") ||
                        data.StartsWith("REGISTER") ||
                        data.StartsWith("CHECK") ||
                        data.StartsWith("FORGOT"))

                    {
                        string result = ProcessData(data);
                        byte[] send = Encoding.UTF8.GetBytes(result);
                        stream.Write(
                        send, 0, send.Length);
                    }
                    else if (data.StartsWith("FILE"))
                    {
                        AddLog("File: " + data);
                        SendToAll(data, client);
                    }
                    else
                    {
                        AddLog("Chat: " + data);
                        SendToAll(data, client);
                    }
                }
            }
            catch
            {
                AddLog("Client disconnected");
            }

            clients.Remove(client);
        }
        /// /////////////////////
        private void Form1_Load(object sender, EventArgs e)
        {
            StartServer();
        }
        private string ProcessData(string data)
        {
            string[] arr = data.Split('|');
            // kiểm tra username tồn tại
            if (arr[0] == "CHECK")
            {
                string username = arr[1];
                DatabaseHelper db = new DatabaseHelper();
                bool exist = db.CheckUser(username);
                if (exist)
                {
                    AddLog("Username đã tồn tại: " + username);
                    return "EXIST";
                }
                else
                {
                    AddLog("Username hợp lệ: " + username);
                    return "OK";
                }
            }
            // đăng ký tài khoản
            if (arr[0] == "REGISTER")
            {
                string username = arr[1];
                string password = arr[2];
                DatabaseHelper db = new DatabaseHelper();
                bool ok = db.Register(username, password);
                if (ok)
                {
                    AddLog("User đăng ký: " + username);
                    return "Register successful";
                }
                else
                {
                    AddLog("Đăng ký thất bại: " + username);
                    return "Register failed";
                }
            }
            if (arr[0] == "LOGIN")
            {
                string username = arr[1];
                string password = arr[2];
                DatabaseHelper db =new DatabaseHelper();
                bool ok =db.Login(username, password);
                if (ok)
                {
                    AddLog("Login thành công: " + username);
                    return "LOGIN_SUCCESS";
                }
                else
                {
                    AddLog("Login thất bại: " + username);
                    return "LOGIN_FAIL";
                }
            }
            if (arr[0] == "FORGOT")
            {
                string username = arr[1];
                string password = arr[2];
                DatabaseHelper db = new DatabaseHelper();
                bool ok = db.ResetPassword(username, password);
                if (ok)
                {
                    AddLog("Reset password: " + username);
                    return "RESET_SUCCESS";
                }
                else
                {
                    return "RESET_FAIL";
                }
            }
            return "Unknown command";
        }
        private void SendToAll(string msg, TcpClient sender)
        {
            foreach (TcpClient c in clients)
            {
                if (c != sender)
                {
                    NetworkStream ns = c.GetStream();
                    byte[] data =Encoding.UTF8.GetBytes(msg);
                    ns.Write(data, 0, data.Length);
                }
            }
        }
        private void AddLog(string text)
        {
            if (txtServerLog.InvokeRequired)
            {
                txtServerLog.Invoke(new Action<string>(AddLog),text);
            }
            else
            {
                txtServerLog.AppendText(DateTime.Now.ToString("HH:mm:ss")+ " : "+ text+ Environment.NewLine);
            }
        }
    }
} 