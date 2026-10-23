using System;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        // Sự kiện khi người dùng bấm nút Đăng nhập.
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Vui lòng nhập đầy đủ tài khoản và mật khẩu!",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            btnLogin.Enabled = false;

            try
            {
                // Không dùng HttpClient riêng ở FormLogin.
                // ApiClientService sẽ lưu token và gắn Authorization: Bearer <token>
                // cho cùng HttpClient mà các màn hình nghiệp vụ đang dùng.
                bool loginSuccess = await ApiClientService.LoginAsync(username, password);

                if (!loginSuccess)
                {
                    MessageBox.Show(
                        ApiClientService.LastLoginError,
                        "Đăng nhập thất bại",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    txtPass.Clear();
                    txtPass.Focus();
                    return;
                }

                MessageBox.Show(
                    "Đăng nhập thành công với quyền: " + SessionManager.CurrentRole,
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Ẩn Form đăng nhập trong khi FormMainShell hoạt động.
                // Khi đăng xuất, MainShell sẽ xóa token và đóng lại; FormLogin hiện lại để đăng nhập tài khoản khác.
                this.Hide();
                using (FormMainShell mainForm = new FormMainShell())
                {
                    mainForm.ShowDialog();
                }

                if (string.IsNullOrWhiteSpace(SessionManager.JwtToken))
                {
                    this.Show();
                    txtPass.Clear();
                    txtUser.Focus();
                    return;
                }

                // Người dùng đóng hẳn cửa sổ chính (không chọn đăng xuất).
                this.Close();
            }
            catch (Exception ex)
            {
                SessionManager.ClearSession();
                MessageBox.Show(
                    "Không thể đăng nhập hoặc kết nối Web API.\n" + ex.Message +
                    "\n\nHãy kiểm tra API đang chạy tại https://localhost:7198/swagger.",
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (!this.IsDisposed)
                {
                    btnLogin.Enabled = true;
                }
            }
        }
    }
}
