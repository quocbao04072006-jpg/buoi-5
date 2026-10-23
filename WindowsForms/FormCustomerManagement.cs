using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormCustomerManagement : Form
    {
        public FormCustomerManagement()
        {
            InitializeComponent();
            this.Load += FormCustomerManagement_Load;
        }

        private async void FormCustomerManagement_Load(object sender, EventArgs e)
        {
            await LoadCustomersAsync();
        }

        private async Task LoadCustomersAsync()
        {
            try
            {
                var customers = await ApiClientService.Client.GetFromJsonAsync<List<CustomerDto>>("customers");
                if (customers != null && dgvCustomers != null)
                {
                    dgvCustomers.DataSource = customers;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải danh sách khách hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnLoad_Click(object sender, EventArgs e)
        {
            await LoadCustomersAsync();
        }

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCustomerName.Text) || string.IsNullOrWhiteSpace(txtPhoneNumber.Text))
                {
                    MessageBox.Show("Tên khách hàng và số điện thoại không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var newCustomer = new
                {
                    CustomerName = txtCustomerName.Text.Trim(),
                    PhoneNumber = txtPhoneNumber.Text.Trim(),
                    Address = txtAddress.Text.Trim(),
                    RewardPoints = int.TryParse(txtRewardPoints.Text, out int pts) ? pts : 0 // Lấy chính xác số điểm đã nhập
                };

                var response = await ApiClientService.Client.PostAsJsonAsync("customers", newCustomer);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Thêm khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadCustomersAsync();
                    ClearInputs();
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Thêm thất bại: " + errorMsg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtCustomerId.Text))
                {
                    MessageBox.Show("Vui lòng chọn khách hàng cần sửa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int customerId = int.Parse(txtCustomerId.Text);
                var updatedCustomer = new
                {
                    CustomerId = customerId,
                    CustomerName = txtCustomerName.Text.Trim(),
                    PhoneNumber = txtPhoneNumber.Text.Trim(),
                    Address = txtAddress.Text.Trim(),
                    RewardPoints = int.TryParse(txtRewardPoints.Text, out int pts) ? pts : 0
                };

                var response = await ApiClientService.Client.PutAsJsonAsync($"customers/{customerId}", updatedCustomer);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadCustomersAsync();
                    ClearInputs();
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Cập nhật thất bại: " + errorMsg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtCustomerId.Text))
                {
                    MessageBox.Show("Vui lòng chọn khách hàng cần xóa!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int customerId = int.Parse(txtCustomerId.Text);
                var confirm = MessageBox.Show("Bạn có chắc chắn muốn xóa khách hàng này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm == DialogResult.Yes)
                {
                    var response = await ApiClientService.Client.DeleteAsync($"customers/{customerId}");
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await LoadCustomersAsync();
                        ClearInputs();
                    }
                    else
                    {
                        MessageBox.Show("Xóa thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string keyword = txtSearch != null ? txtSearch.Text.Trim() : "";

                if (string.IsNullOrEmpty(keyword))
                {
                    await LoadCustomersAsync();
                    return;
                }

                var customers = await ApiClientService.Client.GetFromJsonAsync<List<CustomerDto>>($"customers/search?keyword={keyword}");
                if (customers != null && dgvCustomers != null)
                {
                    dgvCustomers.DataSource = customers;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tìm kiếm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvCustomers != null)
            {
                var row = dgvCustomers.Rows[e.RowIndex];
                txtCustomerId.Text = row.Cells["CustomerId"].Value?.ToString() ?? "";
                txtCustomerName.Text = row.Cells["CustomerName"].Value?.ToString() ?? "";
                txtPhoneNumber.Text = row.Cells["PhoneNumber"].Value?.ToString() ?? "";
                txtAddress.Text = row.Cells["Address"].Value?.ToString() ?? "";
                txtRewardPoints.Text = row.Cells["RewardPoints"].Value?.ToString() ?? "0";
            }
        }

        private void ClearInputs()
        {
            txtCustomerId.Clear();
            txtCustomerName.Clear();
            txtPhoneNumber.Clear();
            txtAddress.Clear();
            txtRewardPoints.Clear();
            txtSearch.Clear();
        }
    }

    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int RewardPoints { get; set; }
    }
}