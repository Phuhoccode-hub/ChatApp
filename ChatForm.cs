using SecureChatTCP_Client.Network;
using SecureChatTCP_Client.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SecureChatTCP_Client.Forms
{
    public partial class ChatForm : Form
    {
        TCPClient tcp = new TCPClient();
        private string username;

        public ChatForm(string user)
        {
            InitializeComponent();
            username = user;
            this.FormClosing += ChatForm_FormClosing;
            btnDownload.Click += btnDownload_Click;
        }
        private void ChatForm_Load(object sender, EventArgs e)
        {
            tcp.OnMessage = ReceiveMessage;
            if (tcp.Connect(ServerConfig.IP, ServerConfig.Port, true))
            {
                AddChat("Đã kết nối server");
                tcp.Send("ONLINE|" + username);
                LoadStickers();
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
                string encrypt = msg.Substring(5);
                try
                {
                    string plain = AESHelper.Decrypt(encrypt);
                    AddChat(plain);
                }
                catch
                {
                    AddChat("Lỗi giải mã.");
                }
            }
            else if (msg.StartsWith("STICKER|"))
            {
                AddChat(msg.Substring(8));
            }
            else if (msg.StartsWith("FILE|"))
            {
                string[] parts = msg.Split(new char[] { '|' }, 4);

                if (parts.Length == 4)
                {
                    string fileName = parts[1];
                    string base64 = parts[3];
                    // Chuyển Base64 về byte đã mã hóa
                    byte[] encryptData = Convert.FromBase64String(base64);
                    // Giải mã AES
                    byte[] data = AESHelper.Decrypt(encryptData);
                    string folder = Path.Combine(Application.StartupPath, "ReceivedFiles");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    string savePath = Path.Combine(folder, fileName);
                    // Lưu file đã giải mã
                    File.WriteAllBytes(savePath, data);
                    receivedFiles[fileName] = savePath;
                    if (!lstFiles.Items.Contains(fileName))
                    {
                        lstFiles.Items.Add(fileName);
                    }
                    AddChat("📎 Đã nhận file: " + fileName);
                }
            }
            else if (msg.StartsWith("ONLINE|"))
            {
                lstUsers.Items.Clear();
                string[] users = msg.Substring(7).Split(',');
                foreach (string u in users)
                {
                    if (u.Trim() != "")
                        lstUsers.Items.Add(u);
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
            string encrypt = AESHelper.Encrypt(msg);
            tcp.Send("CHAT|" + encrypt);
            AddChat("Me: " + msg);
            txtMessage.Clear();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult rs = MessageBox.Show("Bạn có muốn đăng xuất không?", "Logout",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
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
        private Dictionary<string, string> receivedFiles = new Dictionary<string, string>();
        private string lastReceivedFile = ""; // Biến này dùng để nhớ file cuối cùng nhận được.

        private void btnFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Chọn file";
            ofd.Filter = "All Files|*.*";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;
            FileInfo info = new FileInfo(ofd.FileName);
            if (info.Length > 20 * 1024)
            {
                MessageBox.Show("Chỉ được gửi file nhỏ hơn 20 KB.","Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }
            selectedFile = ofd.FileName;
            MessageBox.Show("Đã gửi file: " + info.Name, "Send File");
            byte[] fileBytes = File.ReadAllBytes(selectedFile);
            // AES Encrypt
            byte[] encryptBytes = AESHelper.Encrypt(fileBytes);
            // Convert sang Base64 để gửi TCP
            string base64 = Convert.ToBase64String(encryptBytes);
            string packet ="FILE|"+ info.Name+ "|"+ encryptBytes.Length+ "|"+ base64;
            tcp.Send(packet);
            AddChat("Me gửi file: " + info.Name);
        }
        // tránh lỗi khi đóng form
        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            tcp.Close();
        }
            private void btnDownload_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItem == null)
            {
                MessageBox.Show("Hãy chọn file cần tải.");
                return;
            }

            string fileName = lstFiles.SelectedItem.ToString();

            if (!receivedFiles.ContainsKey(fileName))
            {
                MessageBox.Show("Không tìm thấy file.");
                return;
            }

            string source = receivedFiles[fileName];
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = fileName;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.Copy(source, sfd.FileName, true);
                MessageBox.Show("Download thành công.");
            }
        }
        private void LoadStickers()
        {
            string[] stickers ={"😀","😂","😍","😎","😭","❤️","👍","👏","😡","😱"};
            foreach (string s in stickers)
            {
                Button btn = new Button();
                btn.Text = s;
                btn.Width = 45;
                btn.Height = 45;
                btn.Font = new Font("Segoe UI Emoji", 16);
                btn.Click += (sender, e) =>
                {
                    tcp.Send("STICKER|" + s);
                    AddChat("Me: " + s);
                };
                flowSticker.Controls.Add(btn);
            }
        }
        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void txtChatLog_TextChanged(object sender, EventArgs e)
        {

        }
        private void panelLeft_Paint(object sender, PaintEventArgs e)
        {

        }
    }
    

}
