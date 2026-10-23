using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormPOS : Form
    {
        private static readonly List<CartItemDto> _cart =
            new List<CartItemDto>();

        private bool isSearchingBarcode = false;
        private bool isCheckingOut = false;

        public FormPOS()
        {
            InitializeComponent();
            SetupCartGrid();
            this.Load += FormPOS_Load;
        }

        private void FormPOS_Load(object sender, EventArgs e)
        {
            UpdateCartDisplay();
            txtBarcode.Focus();
        }

        // =====================================================
        // 1. THIẾT LẬP BẢNG GIỎ HÀNG
        // =====================================================

        private void SetupCartGrid()
        {
            dgvCart.AutoGenerateColumns = false;
            dgvCart.Columns.Clear();
            dgvCart.AllowUserToAddRows = false;
            dgvCart.ReadOnly = true;
            dgvCart.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            // Cho phép chọn nhiều sản phẩm cùng lúc.
            dgvCart.MultiSelect = true;
            dgvCart.RowHeadersVisible = false;

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colProductId",
                DataPropertyName = "ProductId",
                HeaderText = "Mã SP",
                Width = 60
            });

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colProductName",
                DataPropertyName = "ProductName",
                HeaderText = "Tên sản phẩm",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 100
            });

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colUnitPrice",
                DataPropertyName = "UnitPrice",
                HeaderText = "Đơn giá",
                Width = 85,
                DefaultCellStyle = { Format = "N0" }
            });

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colQuantity",
                DataPropertyName = "Quantity",
                HeaderText = "SL",
                Width = 40
            });

            dgvCart.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTotalPrice",
                DataPropertyName = "TotalPrice",
                HeaderText = "Thành tiền",
                Width = 100,
                DefaultCellStyle = { Format = "N0" }
            });
        }

        // =====================================================
        // 2. QUÉT BARCODE
        // =====================================================

        private async void txtBarcode_KeyDown(
            object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.Handled = true;
            e.SuppressKeyPress = true;

            if (isSearchingBarcode || isCheckingOut)
                return;

            string barcode = txtBarcode.Text.Trim();

            if (string.IsNullOrWhiteSpace(barcode))
            {
                MessageBox.Show(
                    "Vui lòng nhập hoặc quét mã vạch.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtBarcode.Focus();
                return;
            }

            txtBarcode.Clear();

            await AddProductToCartByBarcodeAsync(barcode);

            if (!IsDisposed && txtBarcode.CanFocus)
                txtBarcode.Focus();
        }

        private async Task AddProductToCartByBarcodeAsync(
            string barcode)
        {
            if (isSearchingBarcode || isCheckingOut)
                return;

            isSearchingBarcode = true;

            try
            {
                ApiClientService.SetAuthenticationToken();

                string encodedBarcode =
                    Uri.EscapeDataString(barcode);

                using (HttpResponseMessage response =
                    await ApiClientService.Client.GetAsync(
                        "products/barcode/" + encodedBarcode))
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        MessageBox.Show(
                            "Không tìm thấy sản phẩm có mã vạch: "
                            + barcode,
                            "Không tìm thấy sản phẩm",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        MessageBox.Show(
                            "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
                            "Chưa xác thực",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        MessageBox.Show(
                            "Tài khoản không có quyền tra cứu sản phẩm.",
                            "Không có quyền",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        string error =
                            await response.Content.ReadAsStringAsync();

                        MessageBox.Show(
                            "API tra cứu mã vạch bị lỗi.\nHTTP: "
                            + (int)response.StatusCode
                            + "\nChi tiết: " + error,
                            "Lỗi API",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    ProductDto product =
                        await response.Content.ReadFromJsonAsync<ProductDto>();

                    if (product == null || product.ProductId <= 0)
                    {
                        MessageBox.Show(
                            "Dữ liệu sản phẩm không hợp lệ.",
                            "Lỗi dữ liệu",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    if (product.StockQuantity <= 0)
                    {
                        MessageBox.Show(
                            "Sản phẩm \"" + product.ProductName
                            + "\" đã hết hàng.",
                            "Hết hàng",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    CartItemDto existingItem = _cart.FirstOrDefault(
                        x => x.ProductId == product.ProductId);

                    int quantityInCart = existingItem == null
                        ? 0
                        : existingItem.Quantity;

                    if (quantityInCart >= product.StockQuantity)
                    {
                        MessageBox.Show(
                            "Số lượng trong giỏ đã đạt tồn kho hiện tại: "
                            + product.StockQuantity,
                            "Không đủ tồn kho",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    if (existingItem != null)
                    {
                        existingItem.Quantity++;
                    }
                    else
                    {
                        _cart.Add(new CartItemDto
                        {
                            ProductId = product.ProductId,
                            ProductName = product.ProductName,
                            UnitPrice = product.Price,
                            Quantity = 1
                        });
                    }

                    UpdateCartDisplay();
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "Không kết nối được API.\n" + ex.Message,
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (System.Text.Json.JsonException ex)
            {
                MessageBox.Show(
                    "Dữ liệu sản phẩm trả về không đúng định dạng.\n"
                    + ex.Message,
                    "Lỗi dữ liệu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi tra cứu mã vạch:\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                isSearchingBarcode = false;
            }
        }

        // =====================================================
        // 3. CẬP NHẬT GIỎ HÀNG
        // =====================================================

        private void UpdateCartDisplay()
        {
            dgvCart.DataSource = null;
            dgvCart.DataSource = _cart.ToList();

            decimal total = _cart.Sum(x => x.TotalPrice);
            lblTotalAmount.Text = $"{total:N0} đ";

            CalculateChange();
        }

        // =====================================================
        // 4. XÓA MỘT HOẶC NHIỀU SẢN PHẨM ĐÃ CHỌN
        // =====================================================

        private void btnRemoveSelected_Click(
            object sender, EventArgs e)
        {
            RemoveSelectedCartItems();
        }

        private void RemoveSelectedCartItems()
        {
            if (isSearchingBarcode || isCheckingOut)
            {
                MessageBox.Show(
                    "Hệ thống đang xử lý. Vui lòng đợi.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (dgvCart.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Vui lòng chọn một hoặc nhiều sản phẩm cần xóa.\n\n"
                    + "Giữ Ctrl và nhấp vào các dòng để chọn nhiều sản phẩm.",
                    "Chưa chọn sản phẩm",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Lấy mã các sản phẩm được chọn trước khi cập nhật bảng.
            HashSet<int> selectedIds = new HashSet<int>();

            foreach (DataGridViewRow row in dgvCart.SelectedRows)
            {
                object value = row.Cells["colProductId"].Value;

                if (value != null &&
                    int.TryParse(value.ToString(), out int productId))
                {
                    selectedIds.Add(productId);
                }
            }

            List<CartItemDto> selectedItems = _cart
                .Where(x => selectedIds.Contains(x.ProductId))
                .ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show(
                    "Không tìm thấy sản phẩm được chọn.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string names = string.Join(
                "\n",
                selectedItems.Select(x =>
                    "- " + x.ProductName + " (SL: " + x.Quantity + ")"));

            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn xóa " + selectedItems.Count
                + " sản phẩm sau khỏi giỏ hàng?\n\n"
                + names,
                "Xác nhận xóa sản phẩm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            foreach (CartItemDto item in selectedItems)
            {
                _cart.Remove(item);
            }

            UpdateCartDisplay();

            MessageBox.Show(
                "Đã xóa " + selectedItems.Count
                + " sản phẩm khỏi giỏ hàng.",
                "Thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            txtBarcode.Focus();
        }

        // =====================================================
        // 5. HỦY TOÀN BỘ GIỎ HÀNG
        // =====================================================

        private void btnClearCart_Click(object sender, EventArgs e)
        {
            if (isSearchingBarcode || isCheckingOut)
            {
                MessageBox.Show(
                    "Hệ thống đang xử lý. Vui lòng đợi.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (_cart.Count == 0)
            {
                MessageBox.Show(
                    "Giỏ hàng đang trống.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn hủy TOÀN BỘ giỏ hàng không?",
                "Xác nhận hủy giỏ hàng",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            _cart.Clear();
            UpdateCartDisplay();

            txtCashReceived.Clear();
            txtCustomerPhone.Clear();
            lblCustomerName.Text = "Khách vãng lai";
            txtBarcode.Clear();
            txtBarcode.Focus();

            MessageBox.Show(
                "Đã hủy toàn bộ giỏ hàng thành công!",
                "Thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // =====================================================
        // 6. TÍNH TIỀN THỪA
        // =====================================================

        private void txtCashReceived_TextChanged(
            object sender, EventArgs e)
        {
            CalculateChange();
        }

        private void CalculateChange()
        {
            if (lblChange == null || txtCashReceived == null)
                return;

            decimal total = _cart.Sum(x => x.TotalPrice);

            if (string.IsNullOrWhiteSpace(txtCashReceived.Text))
            {
                lblChange.Text = total > 0 ? "Chưa đủ tiền!" : "0 đ";
                lblChange.ForeColor =
                    total > 0 ? Color.Red : Color.Black;
                return;
            }

            if (decimal.TryParse(
                txtCashReceived.Text.Trim(),
                out decimal cashReceived))
            {
                decimal change = cashReceived - total;

                if (change >= 0)
                {
                    lblChange.Text = $"{change:N0} đ";
                    lblChange.ForeColor = Color.Black;
                }
                else
                {
                    lblChange.Text = "Chưa đủ tiền!";
                    lblChange.ForeColor = Color.Red;
                }
            }
            else
            {
                lblChange.Text = "Số tiền không hợp lệ";
                lblChange.ForeColor = Color.Red;
            }
        }

        // =====================================================
        // 7. THANH TOÁN
        // =====================================================

        private async void btnCheckout_Click(
            object sender, EventArgs e)
        {
            if (isCheckingOut || isSearchingBarcode)
                return;

            if (_cart.Count == 0)
            {
                MessageBox.Show(
                    "Giỏ hàng đang trống!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            decimal total = _cart.Sum(x => x.TotalPrice);

            if (!decimal.TryParse(
                txtCashReceived.Text.Trim(),
                out decimal cashReceived))
            {
                MessageBox.Show(
                    "Vui lòng nhập số tiền khách đưa hợp lệ.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtCashReceived.Focus();
                return;
            }

            if (cashReceived < total)
            {
                MessageBox.Show(
                    "Tiền khách đưa chưa đủ thanh toán.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtCashReceived.Focus();
                return;
            }

            var orderRequest = new
            {
                CashierUsername = SessionManager.CurrentUsername,
                CustomerPhone = txtCustomerPhone.Text.Trim(),
                Items = _cart.Select(i => new
                {
                    i.ProductId,
                    i.Quantity,
                    i.UnitPrice
                }).ToList()
            };

            isCheckingOut = true;
            btnCheckout.Enabled = false;
            btnClearCart.Enabled = false;
            btnRemoveSelected.Enabled = false;

            try
            {
                ApiClientService.SetAuthenticationToken();

                using (HttpResponseMessage response =
                    await ApiClientService.Client.PostAsJsonAsync(
                        "orders/checkout", orderRequest))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            "Thanh toán thành công!",
                            "Thành công",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        _cart.Clear();
                        UpdateCartDisplay();

                        txtCashReceived.Clear();
                        txtCustomerPhone.Clear();
                        lblCustomerName.Text = "Khách vãng lai";
                        txtBarcode.Clear();
                        txtBarcode.Focus();
                    }
                    else
                    {
                        string error =
                            await response.Content.ReadAsStringAsync();

                        MessageBox.Show(
                            "Thanh toán thất bại.\nHTTP: "
                            + (int)response.StatusCode
                            + "\nChi tiết: " + error,
                            "Lỗi thanh toán",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show(
                    "Không kết nối được máy chủ khi thanh toán.\n"
                    + ex.Message,
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi thanh toán:\n" + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                isCheckingOut = false;

                if (!IsDisposed)
                {
                    btnCheckout.Enabled = true;
                    btnClearCart.Enabled = true;
                    btnRemoveSelected.Enabled = true;
                }
            }
        }
    }

    public class CartItemDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public class ProductDto
    {
        public int ProductId { get; set; }

        public string Barcode { get; set; } = string.Empty;

        public string ProductName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }
    }
}
