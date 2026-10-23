
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormProductManagement : Form
    {
        private TextBox txtSearch;
        private ComboBox cboFilterCategory;
        private Button btnSearch;
        private Button btnPrevious;
        private Button btnNext;
        private Label lblPageInfo;

        private int currentPage = 1;
        private const int pageSize = 5;
        private int totalPages = 1;
        private int totalItems = 0;

        private bool isLoading = false;

        public FormProductManagement()
        {
            InitializeComponent();

            dgvProducts.CellFormatting -= dgvProducts_CellFormatting;
            dgvProducts.CellFormatting += dgvProducts_CellFormatting;

            InitializePagingControls();
        }

        // =====================================================
        // 1. GIAO DIỆN
        // =====================================================

        private void InitializePagingControls()
        {
            Text = "QUẢN LÝ SẢN PHẨM";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1120, 700);
            MinimumSize = new Size(1050, 680);
            BackColor = Color.FromArgb(245, 247, 250);

            // Bảng sản phẩm
            dgvProducts.Parent = this;
            dgvProducts.Dock = DockStyle.None;
            dgvProducts.Location = new Point(15, 110);
            dgvProducts.Size = new Size(660, 510);
            dgvProducts.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dgvProducts.Visible = true;
            dgvProducts.BackgroundColor = Color.White;
            dgvProducts.BorderStyle = BorderStyle.FixedSingle;
            dgvProducts.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgvProducts.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.MultiSelect = false;
            dgvProducts.ReadOnly = true;
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.RowHeadersVisible = false;
            dgvProducts.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(39, 61, 86);
            dgvProducts.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;
            dgvProducts.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9, FontStyle.Bold);
            dgvProducts.EnableHeadersVisualStyles = false;
            dgvProducts.RowTemplate.Height = 32;

            // Khung nhập liệu
            panelRight.Parent = this;
            panelRight.Dock = DockStyle.None;
            panelRight.Location = new Point(690, 75);
            panelRight.Size = new Size(405, 570);
            panelRight.Anchor =
                AnchorStyles.Top | AnchorStyles.Bottom |
                AnchorStyles.Left | AnchorStyles.Right;
            panelRight.Visible = true;
            panelRight.BackColor = Color.White;
            panelRight.BorderStyle = BorderStyle.FixedSingle;

            Label lblTitle = new Label
            {
                Name = "lblProductTitle",
                Text = "QUẢN LÝ SẢN PHẨM",
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 55, 80),
                Location = new Point(15, 15),
                Size = new Size(450, 40)
            };

            Label lblSubtitle = new Label
            {
                Name = "lblProductSubtitle",
                Text = "Danh sách, tìm kiếm và cập nhật hàng hóa",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(17, 55),
                Size = new Size(500, 25)
            };

            Label lblSearch = new Label
            {
                Text = "Tìm kiếm",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(15, 85),
                Size = new Size(70, 22)
            };

            txtSearch = new TextBox
            {
                Location = new Point(85, 81),
                Size = new Size(205, 27),
                Font = new Font("Segoe UI", 10)
            };

            btnSearch = new Button
            {
                Text = "Tìm",
                Location = new Point(295, 79),
                Size = new Size(65, 31),
                BackColor = Color.FromArgb(41, 98, 160),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            Label lblFilter = new Label
            {
                Text = "Danh mục",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(375, 85),
                Size = new Size(75, 22)
            };

            cboFilterCategory = new ComboBox
            {
                Location = new Point(450, 81),
                Size = new Size(225, 27),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };

            btnPrevious = new Button
            {
                Text = "< Trước",
                Location = new Point(170, 630),
                Size = new Size(95, 34),
                Cursor = Cursors.Hand,
                Enabled = false
            };

            lblPageInfo = new Label
            {
                Text = "Trang 1/1 (0 sản phẩm)",
                Location = new Point(265, 635),
                Size = new Size(190, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            btnNext = new Button
            {
                Text = "Sau >",
                Location = new Point(455, 630),
                Size = new Size(95, 34),
                Cursor = Cursors.Hand,
                Enabled = false
            };

            Controls.Add(lblTitle);
            Controls.Add(lblSubtitle);
            Controls.Add(lblSearch);
            Controls.Add(txtSearch);
            Controls.Add(btnSearch);
            Controls.Add(lblFilter);
            Controls.Add(cboFilterCategory);
            Controls.Add(btnPrevious);
            Controls.Add(lblPageInfo);
            Controls.Add(btnNext);

            lblTitle.BringToFront();
            lblSubtitle.BringToFront();
            lblSearch.BringToFront();
            txtSearch.BringToFront();
            btnSearch.BringToFront();
            lblFilter.BringToFront();
            cboFilterCategory.BringToFront();
            dgvProducts.BringToFront();
            btnPrevious.BringToFront();
            lblPageInfo.BringToFront();
            btnNext.BringToFront();
            panelRight.BringToFront();

            // Tìm kiếm
            btnSearch.Click += async (s, e) =>
            {
                if (isLoading) return;

                currentPage = 1;
                await LoadProductsAsync();
            };

            // Nhấn Enter để tìm kiếm
            txtSearch.KeyDown += async (s, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;

                e.SuppressKeyPress = true;

                if (isLoading) return;

                currentPage = 1;
                await LoadProductsAsync();
            };

            // Lọc danh mục
            cboFilterCategory.SelectedIndexChanged += async (s, e) =>
            {
                if (!IsHandleCreated || isLoading) return;

                currentPage = 1;
                await LoadProductsAsync();
            };

            // Trang trước
            btnPrevious.Click += async (s, e) =>
            {
                if (isLoading || currentPage <= 1) return;

                currentPage--;
                await LoadProductsAsync();
            };

            // Trang sau
            btnNext.Click += async (s, e) =>
            {
                if (isLoading || currentPage >= totalPages) return;

                currentPage++;
                await LoadProductsAsync();
            };
        }

        // =====================================================
        // 2. TÔ MÀU SẢN PHẨM SẮP HẾT HÀNG
        // =====================================================

        private void dgvProducts_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (dgvProducts == null ||
                e.RowIndex < 0 ||
                e.RowIndex >= dgvProducts.Rows.Count)
            {
                return;
            }

            if (!dgvProducts.Columns.Contains("StockQuantity"))
                return;

            DataGridViewRow row = dgvProducts.Rows[e.RowIndex];
            object value = row.Cells["StockQuantity"].Value;

            if (value != null &&
                int.TryParse(value.ToString(), out int stock) &&
                stock <= 10)
            {
                row.DefaultCellStyle.BackColor = Color.LightCoral;
                row.DefaultCellStyle.ForeColor = Color.DarkRed;
                row.DefaultCellStyle.SelectionBackColor = Color.IndianRed;
                row.DefaultCellStyle.SelectionForeColor = Color.White;
            }
            else
            {
                row.DefaultCellStyle.BackColor =
                    dgvProducts.DefaultCellStyle.BackColor;
                row.DefaultCellStyle.ForeColor =
                    dgvProducts.DefaultCellStyle.ForeColor;
                row.DefaultCellStyle.SelectionBackColor =
                    dgvProducts.DefaultCellStyle.SelectionBackColor;
                row.DefaultCellStyle.SelectionForeColor =
                    dgvProducts.DefaultCellStyle.SelectionForeColor;
            }
        }

        // =====================================================
        // 3. KHỞI ĐỘNG FORM
        // =====================================================

        private async void FormProductManagement_Load(
            object sender, EventArgs e)
        {
            await LoadCategoriesToComboAsync();
            await LoadProductsAsync();
        }

        // =====================================================
        // 4. TẢI DANH MỤC
        // =====================================================

        private async Task LoadCategoriesToComboAsync()
        {
            isLoading = true;

            try
            {
                ApiClientService.SetAuthenticationToken();

                var categories = await ApiClientService.Client
                    .GetFromJsonAsync<List<CategoryDto>>("categories");

                if (categories == null)
                    categories = new List<CategoryDto>();

                // Danh mục dùng khi thêm/sửa
                cboCategory.DataSource = null;
                cboCategory.DisplayMember = "CategoryName";
                cboCategory.ValueMember = "CategoryId";
                cboCategory.DataSource = categories;

                // Danh mục dùng để lọc bảng
                cboFilterCategory.Items.Clear();

                cboFilterCategory.Items.Add(new CategoryFilterItem
                {
                    CategoryId = null,
                    CategoryName = "Tất cả danh mục"
                });

                foreach (CategoryDto category in categories)
                {
                    cboFilterCategory.Items.Add(new CategoryFilterItem
                    {
                        CategoryId = category.CategoryId,
                        CategoryName = category.CategoryName
                    });
                }

                cboFilterCategory.DisplayMember = "CategoryName";

                if (cboFilterCategory.Items.Count > 0)
                    cboFilterCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi nạp danh mục: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
            }
        }

        // =====================================================
        // 5. TẢI SẢN PHẨM VÀ PHÂN TRANG
        // =====================================================

        private async Task LoadProductsAsync()
        {
            if (isLoading) return;

            isLoading = true;
            UpdatePagingControls();

            try
            {
                ApiClientService.SetAuthenticationToken();

                string keyword = Uri.EscapeDataString(
                    txtSearch?.Text.Trim() ?? string.Empty);

                string url = "products/paged?keyword=" + keyword
                    + "&pageNumber=" + currentPage
                    + "&pageSize=" + pageSize;

                // Chỉ gửi categoryId khi chọn danh mục cụ thể.
                // "Tất cả danh mục" không được gửi categoryId.
                if (cboFilterCategory.SelectedItem
                        is CategoryFilterItem selected &&
                    selected.CategoryId.HasValue)
                {
                    url += "&categoryId=" + selected.CategoryId.Value;
                }

                var result = await ApiClientService.Client
                    .GetFromJsonAsync<PagedProductsDto>(url);

                if (result == null)
                {
                    dgvProducts.DataSource = null;
                    totalItems = 0;
                    totalPages = 1;
                    currentPage = 1;
                    return;
                }

                // Lưu tổng số bản ghi của toàn bộ kết quả đang lọc.
                // Không thay totalItems bằng số dòng của trang hiện tại.
                totalItems = result.TotalItems;
                totalPages = Math.Max(1, result.TotalPages);
                currentPage = Math.Max(1, result.CurrentPage);

                dgvProducts.DataSource = null;
                dgvProducts.DataSource =
                    result.Items ?? new List<ProductListItemDto>();

                SetColumnHeader("ProductId", "Mã SP");
                SetColumnHeader("Barcode", "Mã vạch");
                SetColumnHeader("ProductName", "Tên sản phẩm");
                SetColumnHeader("Price", "Đơn giá");
                SetColumnHeader("StockQuantity", "Tồn kho");
                SetColumnHeader("CategoryId", "Mã danh mục");
                SetColumnHeader("CategoryName", "Danh mục");
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "Không kết nối được API.\n" + ex.Message,
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi tải sản phẩm: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                UpdatePagingControls();
            }
        }

        private void SetColumnHeader(
            string columnName, string headerText)
        {
            if (dgvProducts.Columns.Contains(columnName))
                dgvProducts.Columns[columnName].HeaderText = headerText;
        }

        private void UpdatePagingControls()
        {
            if (lblPageInfo == null ||
                btnPrevious == null ||
                btnNext == null)
            {
                return;
            }

            lblPageInfo.Text = "Trang " + currentPage
                + "/" + totalPages
                + " (" + totalItems + " sản phẩm)";

            btnPrevious.Enabled =
                !isLoading && currentPage > 1;

            btnNext.Enabled =
                !isLoading && currentPage < totalPages;
        }

        // =====================================================
        // 6. CHỌN SẢN PHẨM TRONG BẢNG
        // =====================================================

        private void dgvProducts_CellClick(
            object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.RowIndex >= dgvProducts.Rows.Count)
            {
                return;
            }

            DataGridViewRow row = dgvProducts.Rows[e.RowIndex];

            try
            {
                txtId.Text = GetCellText(row, "ProductId");
                txtBarcode.Text = GetCellText(row, "Barcode");
                txtProductName.Text = GetCellText(row, "ProductName");

                if (decimal.TryParse(
                    GetCellText(row, "Price"), out decimal price))
                {
                    nudPrice.Value = Math.Max(
                        nudPrice.Minimum,
                        Math.Min(price, nudPrice.Maximum));
                }

                if (int.TryParse(
                    GetCellText(row, "StockQuantity"), out int stock))
                {
                    nudStock.Value = Math.Max(
                        nudStock.Minimum,
                        Math.Min(stock, nudStock.Maximum));
                }

                if (int.TryParse(
                    GetCellText(row, "CategoryId"), out int categoryId))
                {
                    cboCategory.SelectedValue = categoryId;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể lấy thông tin sản phẩm: " + ex.Message,
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private string GetCellText(
            DataGridViewRow row, string columnName)
        {
            if (!dgvProducts.Columns.Contains(columnName))
                return string.Empty;

            return row.Cells[columnName].Value?.ToString()
                ?? string.Empty;
        }

        // =====================================================
        // 7. KIỂM TRA DỮ LIỆU
        // =====================================================

        private bool ValidateProductInput()
        {
            if (string.IsNullOrWhiteSpace(txtBarcode.Text))
            {
                MessageBox.Show("Vui lòng nhập mã vạch.");
                txtBarcode.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên sản phẩm.");
                txtProductName.Focus();
                return false;
            }

            if (cboCategory.SelectedValue == null ||
                !int.TryParse(
                    cboCategory.SelectedValue.ToString(),
                    out int categoryId) ||
                categoryId <= 0)
            {
                MessageBox.Show("Vui lòng chọn danh mục sản phẩm.");
                cboCategory.Focus();
                return false;
            }

            return true;
        }

        private int GetSelectedCategoryId()
        {
            return Convert.ToInt32(cboCategory.SelectedValue);
        }

        private async Task<string> ReadApiErrorAsync(
            HttpResponseMessage response)
        {
            string body = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                return "HTTP " + (int)response.StatusCode
                    + " - " + response.ReasonPhrase;
            }

            return body;
        }

        // =====================================================
        // 8. THÊM SẢN PHẨM
        // =====================================================

        private async void btnAdd_Click(object sender, EventArgs e)
        {
            if (isLoading) return;
            if (!ValidateProductInput()) return;

            bool success = false;

            try
            {
                isLoading = true;
                SetCrudButtonsEnabled(false);

                ApiClientService.SetAuthenticationToken();

                var newProduct = new
                {
                    Barcode = txtBarcode.Text.Trim(),
                    ProductName = txtProductName.Text.Trim(),
                    Price = nudPrice.Value,
                    StockQuantity = (int)nudStock.Value,
                    CategoryId = GetSelectedCategoryId()
                };

                using (HttpResponseMessage response =
                    await ApiClientService.Client.PostAsJsonAsync(
                        "products", newProduct))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            "Thêm sản phẩm thất bại:\n"
                            + await ReadApiErrorAsync(response),
                            "Lỗi",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }

                    success = true;
                }

                // Tránh sự kiện lọc tự tải dữ liệu giữa lúc chuẩn bị tải lại.
                ClearInputs();
                txtSearch.Clear();
                currentPage = 1;

                if (cboFilterCategory.Items.Count > 0)
                    cboFilterCategory.SelectedIndex = 0;

                MessageBox.Show(
                    "Thêm sản phẩm thành công!",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi thêm sản phẩm: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                SetCrudButtonsEnabled(true);
            }

            if (success)
                await LoadProductsAsync();
        }

        // =====================================================
        // 9. SỬA SẢN PHẨM
        // =====================================================

        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            if (isLoading) return;

            if (!int.TryParse(txtId.Text, out int id) || id <= 0)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần cập nhật.");
                return;
            }

            if (!ValidateProductInput()) return;

            bool success = false;

            try
            {
                isLoading = true;
                SetCrudButtonsEnabled(false);

                ApiClientService.SetAuthenticationToken();

                var updateProduct = new
                {
                    ProductId = id,
                    Barcode = txtBarcode.Text.Trim(),
                    ProductName = txtProductName.Text.Trim(),
                    Price = nudPrice.Value,
                    StockQuantity = (int)nudStock.Value,
                    CategoryId = GetSelectedCategoryId()
                };

                using (HttpResponseMessage response =
                    await ApiClientService.Client.PutAsJsonAsync(
                        "products/" + id, updateProduct))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            "Cập nhật thất bại:\n"
                            + await ReadApiErrorAsync(response),
                            "Lỗi",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }

                    success = true;
                }

                ClearInputs();

                MessageBox.Show(
                    "Cập nhật sản phẩm thành công!",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi cập nhật sản phẩm: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                SetCrudButtonsEnabled(true);
            }

            if (success)
                await LoadProductsAsync();
        }

        // =====================================================
        // 10. XÓA SẢN PHẨM
        // =====================================================

        private async void btnDelete_Click(object sender, EventArgs e)
        {
            if (isLoading) return;

            if (!int.TryParse(txtId.Text, out int id) || id <= 0)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm cần xóa.");
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa sản phẩm ID = "
                    + id + " không?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes) return;

            bool success = false;

            try
            {
                isLoading = true;
                SetCrudButtonsEnabled(false);

                ApiClientService.SetAuthenticationToken();

                using (HttpResponseMessage response =
                    await ApiClientService.Client.DeleteAsync(
                        "products/" + id))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            "Xóa sản phẩm thất bại:\n"
                            + await ReadApiErrorAsync(response),
                            "Lỗi",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        return;
                    }

                    success = true;
                }

                ClearInputs();

                MessageBox.Show(
                    "Xóa sản phẩm thành công!",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi khi xóa sản phẩm: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                isLoading = false;
                SetCrudButtonsEnabled(true);
            }

            if (success)
                await LoadProductsAsync();
        }

        private void SetCrudButtonsEnabled(bool enabled)
        {
            btnAdd.Enabled = enabled;
            btnUpdate.Enabled = enabled;
            btnDelete.Enabled = enabled;
        }

        // =====================================================
        // 11. XÓA DỮ LIỆU Ô NHẬP
        // =====================================================

        private void ClearInputs()
        {
            txtId.Clear();
            txtBarcode.Clear();
            txtProductName.Clear();

            nudPrice.Value = Math.Max(
                nudPrice.Minimum,
                Math.Min(0m, nudPrice.Maximum));

            nudStock.Value = Math.Max(
                nudStock.Minimum,
                Math.Min(0m, nudStock.Maximum));

            dgvProducts.ClearSelection();
        }

        // =====================================================
        // 12. ĐỐI TƯỢNG LỌC DANH MỤC
        // =====================================================

        private class CategoryFilterItem
        {
            public int? CategoryId { get; set; }

            public string CategoryName { get; set; } = string.Empty;

            public override string ToString()
            {
                return CategoryName;
            }
        }
    }
}
