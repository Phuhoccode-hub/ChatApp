using SecureChatTCP_Client.Network;
using SecureChatTCP_Client.Security;
using System;
using System.Windows.Forms;
using SecureChatTCP_Client;
namespace SecureChatTCP_Client.Forms
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
        }
        TCPClient tcp = new TCPClient();


        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Hide();
            Application.OpenForms["LoginForm"].Show();
        }
        private void btnCreate_Click(object sender, EventArgs e)
        {
            string user = txtRegUser.Text;
            string pass = txtRegPass.Text;
            string confirm = txtConfirm.Text;

            if (pass != confirm)
            {
                MessageBox.Show("Mật khẩu không khớp");
                return;
            }
            string hash = SHA256Helper.Hash(pass);
            if (!tcp.Connect(ServerConfig.IP, ServerConfig.Port))
            {
                MessageBox.Show("Không kết nối Server");
                return;
            }
            string data = "REGISTER|" + user + "|" + hash;
            string result =tcp.SendAndReceive(data);
            MessageBox.Show(result);
            tcp.Close();
        }

        private void chkShowPass_CheckedChanged(object sender, EventArgs e)
        {
            txtRegPass.UseSystemPasswordChar = !chkShowPass.Checked;
            txtConfirm.UseSystemPasswordChar = !chkShowPass.Checked;
        }

        private void txtRegUser_Leave(object sender, EventArgs e)
        {
            string user = txtRegUser.Text;

            if (user == "")
                return;

            if (!tcp.Connect(ServerConfig.IP, ServerConfig.Port))
            {
                return;
            }
            string result = tcp.SendAndReceive("CHECK|" + user);
            if (result == "EXIST")
            {
                lblStatus.Text = "Tài khoản đã tồn tại";
                btnCreate.Enabled = false;
            }
            else
            {
                lblStatus.Text = "Tài khoản hợp lệ";
                btnCreate.Enabled = true;
            }
        }
    }
    
}
