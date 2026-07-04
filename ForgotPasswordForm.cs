using SecureChatTCP_Client.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SecureChatTCP_Client.Network;
using SecureChatTCP_Client;

namespace SecureChatTCP_Client.Forms
{
    public partial class ForgotPasswordForm : Form
    {
        TCPClient tcp = new TCPClient();
        public ForgotPasswordForm()
        {
            InitializeComponent();

        }
        
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtRegUser_TextChanged(object sender, EventArgs e)
        {

        }

        private void ForgotPasswordForm_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            LoginForm f = new LoginForm();
            f.Show();
            this.Close();
        }
        private void btnReset_Click(object sender, EventArgs e)
        {
            string user = txtForgotUser.Text.Trim();
            string pass = txtNewPass.Text;

            if (user == "" || pass == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin");
                return;
            }
            string hash = SHA256Helper.Hash(pass);
            if (!tcp.Connect(ServerConfig.IP, ServerConfig.Port, false))
            {
                MessageBox.Show("Không kết nối được Server");
                return;
            }

            string result = tcp.SendAndReceive("FORGOT|" + user + "|" + hash);
            tcp.Close();
            if (result == "RESET_SUCCESS")
            {
                MessageBox.Show("Đổi mật khẩu thành công");
                LoginForm f = new LoginForm();
                f.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Không tìm thấy tài khoản");
            }
        }

    }
            
}
