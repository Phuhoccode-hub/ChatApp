using SecureChatTCP_Client.Network;
using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace SecureChatTCP_Client.Forms
{
    public partial class ChatForm : Form
    {
        TCPClient tcp = new TCPClient();

        public ChatForm()
        {
            InitializeComponent();
            this.FormClosing += ChatForm_FormClosing;

        }


        private void ChatForm_Load(object sender, EventArgs e)
        {
            tcp.OnMessage = ReceiveMessage;
            if (tcp.Connect(ServerConfig.IP, ServerConfig.Port, true))
            {
                AddChat("Đã kết nối server");
            }
            else
            {
                MessageBox.Show("Không kết nối được Server");
            }
        }

        private void ReceiveMessage(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(ReceiveMessage), msg);
                return;
            }

            if (msg.StartsWith("CHAT|"))
            {
                AddChat(msg.Substring(5));
            }
            else if (msg.StartsWith("FILE|"))
            {
                string[] parts = msg.Split(new char[] { '|' }, 4);

                if (parts.Length == 4)
                {
                    string fileName = parts[1];
                    string base64 = parts[3];

                    byte[] data = Convert.FromBase64String(base64);

                    string folder = Path.Combine(Application.StartupPath, "ReceivedFiles");

                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    string savePath = Path.Combine(folder, fileName);

                    File.WriteAllBytes(savePath, data);

                    AddChat("📎 Đã nhận file: " + fileName);
                    AddChat("Đã lưu tại: " + savePath);
                }
            }
            else
            {
                AddChat(msg);
            }
        }
        private void AddChat(string text)
        {
            txtChatLog.AppendText(DateTime.Now.ToString("HH:mm") + " : " + text + Environment.NewLine);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string msg = txtMessage.Text.Trim();
            if (msg == "")
            {
                MessageBox.Show("Chưa nhập tin nhắn");
                return;
            }
            tcp.Send("CHAT|" + msg);
            AddChat("Me: " + msg);
            txtMessage.Clear();
        }

        private void txtChatLog_TextChanged(object sender, EventArgs e)
        {

        }

        private void panelLeft_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult rs = MessageBox.Show("Bạn có muốn đăng xuất không?", "Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (rs == DialogResult.Yes)
            {
                tcp.Close();
                LoginForm login = new LoginForm();
                login.Show();
                this.Close();
            }
        }
        // chon file gui 
        private string selectedFile = ""; // chi luu duong dan file duoc chon
 
        private void btnFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Chọn file";
            ofd.Filter = "All Files|*.*";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;
            FileInfo info = new FileInfo(ofd.FileName);
            if (info.Length > 300 * 1024)
            {
                MessageBox.Show("Chỉ được gửi file nhỏ hơn 300 KB.","Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }
            selectedFile = ofd.FileName;
            MessageBox.Show("Đã gửi file: " + info.Name, "Send File");
            byte[] fileBytes = File.ReadAllBytes(selectedFile);
            string base64 = Convert.ToBase64String(fileBytes);
            string packet ="FILE|"+ info.Name+ "|"+ info.Length+ "|"+ base64;
            tcp.Send(packet);
            AddChat("Me gửi file: " + info.Name);
        }
        // tránh lỗi khi đóng form
        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            tcp.Close();
        }
        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

}
