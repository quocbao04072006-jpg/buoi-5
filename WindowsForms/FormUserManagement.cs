using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormUserManagement : Form
    {
        private bool _isLoadingUsers;

        public FormUserManagement()
        {
            InitializeComponent();

            cboRole.Items.AddRange(new string[] { "Admin", "Cashier", "Warehouse" });
            cboRole.SelectedIndex = 1;

            // Checkbox phải commit ngay khi vừa bấm, để sự kiện CellValueChanged chạy.
            dgvUsers.CurrentCellDirtyStateChanged += dgvUsers_CurrentCellDirtyStateChanged;
            dgvUsers.CellValueChanged += dgvUsers_CellValueChanged;
        }

        private async void FormUserManagement_Load(object sender, EventArgs e)
        {
            await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            _isLoadingUsers = true;
            try
            {
                var users = await ApiClientService.Client
                    .GetFromJsonAsync<List<UserDto>>("users");

                dgvUsers.DataSource = null;
                dgvUsers.DataSource = users ?? new List<UserDto>();

                // Làm rõ cột checkbox trạng thái tài khoản.
                if (dgvUsers.Columns[nameof(UserDto.IsActive)] != null)
                {
                    dgvUsers.Columns[nameof(UserDto.IsActive)].HeaderText = "IsActive";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi lấy danh sách tài khoản: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                _isLoadingUsers = false;
            }
        }

        // Đảm bảo thao tác tick/bỏ tick được ghi nhận ngay.
        private void dgvUsers_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvUsers.IsCurrentCellDirty &&
                dgvUsers.CurrentCell is DataGridViewCheckBoxCell)
            {
                dgvUsers.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        // Lưu IsActive xuống database qua PUT /api/users/{id}/active.
        private async void dgvUsers_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_isLoadingUsers || e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var column = dgvUsers.Columns[e.ColumnIndex];
            if (column.DataPropertyName != nameof(UserDto.IsActive) &&
                column.Name != nameof(UserDto.IsActive))
            {
                return;
            }

            var user = dgvUsers.Rows[e.RowIndex].DataBoundItem as UserDto;
            if (user == null)
                return;

            bool isActive = Convert.ToBoolean(
                dgvUsers.Rows[e.RowIndex].Cells[e.ColumnIndex].Value ?? false);

            try
            {
                var response = await ApiClientService.Client.PutAsJsonAsync(
                    $"users/{user.Id}/active",
                    new UpdateUserActiveRequest { IsActive = isActive });

                if (!response.IsSuccessStatusCode)
                {
                    string detail = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(
                        $"Không lưu được trạng thái tài khoản. HTTP {(int)response.StatusCode}.\n{detail}",
                        "Cập nhật thất bại",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    await LoadUsersAsync();
                    return;
                }

                // Tải lại từ API để chắc chắn giao diện phản ánh dữ liệu đã lưu trong DB.
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi cập nhật trạng thái tài khoản: " + ex.Message,
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                await LoadUsersAsync();
            }
        }

        private async void btnAddUser_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show(
                    "Tên đăng nhập và mật khẩu không được trống!",
                    "Cảnh báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var newUser = new
            {
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                Role = cboRole.SelectedItem?.ToString() ?? "Cashier"
            };

            try
            {
                var res = await ApiClientService.Client.PostAsJsonAsync("users", newUser);
                if (res.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Tạo tài khoản mới thành công!",
                        "Thông báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    await LoadUsersAsync();
                    txtUsername.Clear();
                    txtPassword.Clear();
                    txtFullName.Clear();
                }
                else
                {
                    string detail = await res.Content.ReadAsStringAsync();
                    MessageBox.Show(
                        "Không tạo được tài khoản.\n" + detail,
                        "Thất bại",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi tạo tài khoản: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class UpdateUserActiveRequest
    {
        public bool IsActive { get; set; }
    }
}
