using System;
using System.Drawing;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormMainShell : Form
    {
        private Form _activeForm;

        // Dashboard
        private Panel panelDashboard;
        private FlowLayoutPanel flowKpiCards;
        private Label lblRevenueValue;
        private Label lblOrdersValue;
        private Label lblLowStockValue;
        private Label lblDashboardStatus;
        private Button btnDashboard;
        private Button btnRefreshDashboard;

        // Mau giao dien
        private readonly Color sidebarColor = Color.FromArgb(24, 30, 48);
        private readonly Color activeColor = Color.FromArgb(43, 105, 190);
        private readonly Color pageColor = Color.FromArgb(244, 247, 251);
        private readonly Color textColor = Color.FromArgb(35, 48, 68);

        public FormMainShell()
        {
            InitializeComponent();

            StyleMainWindow();
            CreateDashboardButton();
            CreateDashboard();
        }

        // =====================================================
        // GIAO DIEN CHINH
        // =====================================================

        private void StyleMainWindow()
        {
            Text = "MiniSupermarket Sakura | Quan ly sieu thi";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1050, 650);
            BackColor = pageColor;

            panelSidebar.BackColor = sidebarColor;
            panelTopHeader.BackColor = Color.White;
            panelMainContent.BackColor = pageColor;

            lblLogo.Text = "SAKURA MART";
            lblLogo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblLogo.ForeColor = Color.White;
            lblLogo.TextAlign = ContentAlignment.MiddleCenter;

            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle.ForeColor = textColor;

            lblUserInfo.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblUserInfo.ForeColor = Color.FromArgb(90, 102, 120);

            StyleMenuButton(btnPOS);
            StyleMenuButton(btnCategory);
            StyleMenuButton(btnProduct);
            StyleMenuButton(btnCustomer);
            StyleMenuButton(btnReports);
            StyleMenuButton(btnUserManage);

            btnLogout.Dock = DockStyle.Fill;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.BackColor = Color.FromArgb(35, 43, 63);
            btnLogout.ForeColor = Color.FromArgb(255, 220, 220);
            btnLogout.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnLogout.Text = "   Dang xuat";
            btnLogout.TextAlign = ContentAlignment.MiddleLeft;
            btnLogout.Cursor = Cursors.Hand;
        }

        private void StyleMenuButton(Button button)
        {
            button.Dock = DockStyle.Top;
            button.Height = 46;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(39, 50, 74);

            button.BackColor = sidebarColor;
            button.ForeColor = Color.FromArgb(222, 229, 240);
            button.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(15, 0, 4, 0);
            button.Cursor = Cursors.Hand;
        }

        // Tao nut bang code, khong can sua Designer
        private void CreateDashboardButton()
        {
            btnDashboard = new Button
            {
                Name = "btnDashboard",
                Text = "   Dashboard",
                Height = 46,
                Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = sidebarColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 4, 0),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            btnDashboard.FlatAppearance.BorderSize = 0;
            btnDashboard.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(39, 50, 74);

            btnDashboard.Click += btnDashboard_Click;

            // Chen Dashboard ngay sau logo, truoc cac muc chuc nang
            panelSidebar.Controls.Add(btnDashboard);
            panelSidebar.Controls.SetChildIndex(btnDashboard, 1);
        }

        // =====================================================
        // DASHBOARD
        // =====================================================

        private void CreateDashboard()
        {
            panelDashboard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = pageColor,
                Padding = new Padding(26),
                Visible = false,
                AutoScroll = true
            };

            Label heading = new Label
            {
                Text = "Dashboard",
                Font = new Font("Segoe UI", 23, FontStyle.Bold),
                ForeColor = textColor,
                AutoSize = true,
                Location = new Point(26, 22)
            };

            Label subtitle = new Label
            {
                Text = "Tong quan hoat dong kinh doanh cua Sakura Mart",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(112, 124, 143),
                AutoSize = true,
                Location = new Point(29, 66)
            };

            Panel headerLine = new Panel
            {
                BackColor = Color.FromArgb(225, 231, 240),
                Height = 1,
                Location = new Point(26, 98),
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                         AnchorStyles.Right,
                Width = 900
            };

            Label sectionTitle = new Label
            {
                Text = "CHI SO KINH DOANH",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(74, 88, 109),
                AutoSize = true,
                Location = new Point(28, 120)
            };

            flowKpiCards = new FlowLayoutPanel
            {
                Location = new Point(26, 153),
                Size = new Size(930, 185),
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                         AnchorStyles.Right,
                BackColor = Color.Transparent,
                WrapContents = true,
                AutoScroll = false,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            Panel revenueCard = CreateKpiCard(
                "DOANH THU HOM NAY",
                "Dang tai...",
                "VND",
                Color.FromArgb(43, 105, 190),
                out lblRevenueValue);

            Panel ordersCard = CreateKpiCard(
                "TONG SO HOA DON",
                "Dang tai...",
                "DON",
                Color.FromArgb(16, 150, 105),
                out lblOrdersValue);

            Panel stockCard = CreateKpiCard(
                "SAN PHAM SAP HET HANG",
                "Dang tai...",
                "SP",
                Color.FromArgb(220, 137, 38),
                out lblLowStockValue);

            flowKpiCards.Controls.Add(revenueCard);
            flowKpiCards.Controls.Add(ordersCard);
            flowKpiCards.Controls.Add(stockCard);

            Panel infoPanel = new Panel
            {
                Location = new Point(26, 365),
                Size = new Size(930, 125),
                Anchor = AnchorStyles.Top | AnchorStyles.Left |
                         AnchorStyles.Right,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label infoHeading = new Label
            {
                Text = "TRANG THAI HE THONG",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = textColor,
                AutoSize = true,
                Location = new Point(20, 18)
            };

            lblDashboardStatus = new Label
            {
                Text = "San sang ket noi API.",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(80, 95, 115),
                AutoSize = true,
                Location = new Point(22, 53)
            };

            Label infoNote = new Label
            {
                Text = "So lieu duoc lay tu API MiniSupermarket.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(22, 82)
            };

            infoPanel.Controls.Add(infoHeading);
            infoPanel.Controls.Add(lblDashboardStatus);
            infoPanel.Controls.Add(infoNote);

            btnRefreshDashboard = new Button
            {
                Text = "  Lam moi du lieu",
                Size = new Size(175, 42),
                Location = new Point(26, 515),
                BackColor = activeColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            btnRefreshDashboard.FlatAppearance.BorderSize = 0;
            btnRefreshDashboard.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(31, 83, 157);

            btnRefreshDashboard.Click += async (s, e) =>
                await LoadDashboardDataAsync();

            panelDashboard.Controls.Add(heading);
            panelDashboard.Controls.Add(subtitle);
            panelDashboard.Controls.Add(headerLine);
            panelDashboard.Controls.Add(sectionTitle);
            panelDashboard.Controls.Add(flowKpiCards);
            panelDashboard.Controls.Add(infoPanel);
            panelDashboard.Controls.Add(btnRefreshDashboard);

            panelMainContent.Controls.Add(panelDashboard);
        }

        private Panel CreateKpiCard(
            string title,
            string value,
            string unit,
            Color accent,
            out Label valueLabel)
        {
            Panel card = new Panel
            {
                Size = new Size(280, 160),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 16, 12),
                BorderStyle = BorderStyle.FixedSingle
            };

            Panel accentBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 5,
                BackColor = accent
            };

            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(104, 116, 134),
                AutoSize = false,
                Location = new Point(18, 24),
                Size = new Size(245, 30)
            };

            valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 21, FontStyle.Bold),
                ForeColor = accent,
                AutoSize = false,
                Location = new Point(18, 64),
                Size = new Size(245, 42),
                AutoEllipsis = true
            };

            Label unitLabel = new Label
            {
                Text = unit,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(130, 140, 155),
                AutoSize = true,
                Location = new Point(20, 119)
            };

            card.Controls.Add(accentBar);
            card.Controls.Add(titleLabel);
            card.Controls.Add(valueLabel);
            card.Controls.Add(unitLabel);

            return card;
        }

        private async Task LoadDashboardDataAsync()
        {
            if (lblRevenueValue == null)
                return;

            lblRevenueValue.Text = "Dang tai...";
            lblOrdersValue.Text = "Dang tai...";
            lblLowStockValue.Text = "Dang tai...";

            if (btnRefreshDashboard != null)
                btnRefreshDashboard.Enabled = false;

            try
            {
                if (string.IsNullOrWhiteSpace(SessionManager.JwtToken))
                {
                    lblDashboardStatus.Text =
                        "Khong co phien dang nhap. Vui long dang nhap lai.";
                    return;
                }

                ApiClientService.SetAuthenticationToken();

                string json =
                    await ApiClientService.GetDataWithTokenAsync(
                        "Reports/dashboard-summary");

                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    JsonElement root = document.RootElement;

                    decimal revenue = ReadDecimal(root, "todayRevenue");
                    int orders = ReadInt(root, "totalOrders");
                    int lowStock = ReadInt(root, "lowStockProducts");

                    lblRevenueValue.Text = revenue.ToString("N0") + " d";
                    lblOrdersValue.Text = orders.ToString("N0");
                    lblLowStockValue.Text = lowStock.ToString("N0");

                    lblDashboardStatus.Text =
                        "Cap nhat thanh cong luc " +
                        DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                lblDashboardStatus.Text = "Phien dang nhap khong hop le.";
                MessageBox.Show(
                    ex.Message,
                    "Loi xac thuc",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                lblDashboardStatus.Text = "Khong the lay du lieu tu API.";

                MessageBox.Show(
                    "Khong the tai Dashboard: " + ex.Message,
                    "Loi ket noi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (btnRefreshDashboard != null &&
                    !btnRefreshDashboard.IsDisposed)
                {
                    btnRefreshDashboard.Enabled = true;
                }
            }
        }

        private decimal ReadDecimal(JsonElement root, string name)
        {
            if (root.TryGetProperty(name, out JsonElement value))
            {
                if (value.ValueKind == JsonValueKind.Number &&
                    value.TryGetDecimal(out decimal number))
                    return number;

                if (value.ValueKind == JsonValueKind.String &&
                    decimal.TryParse(value.GetString(), out number))
                    return number;
            }

            return 0;
        }

        private int ReadInt(JsonElement root, string name)
        {
            if (root.TryGetProperty(name, out JsonElement value))
            {
                if (value.ValueKind == JsonValueKind.Number &&
                    value.TryGetInt32(out int number))
                    return number;

                if (value.ValueKind == JsonValueKind.String &&
                    int.TryParse(value.GetString(), out number))
                    return number;
            }

            return 0;
        }

        // =====================================================
        // LOAD FORM VA PHAN QUYEN
        // =====================================================

        private async void FormMainShell_Load(object sender, EventArgs e)
        {
            string role = SessionManager.CurrentRole ?? "";

            lblUserInfo.Text =
                "Nhan vien: " + SessionManager.CurrentUsername +
                "   |   Vai tro: " + role;

            ApplyRolePermissions(role);

            if (role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                ShowDashboard();
                await LoadDashboardDataAsync();
            }
            else
            {
                OpenDefaultScreenByRole(role);
            }
        }

        private void ApplyRolePermissions(string role)
        {
            switch ((role ?? "").ToUpperInvariant())
            {
                case "ADMIN":
                    btnDashboard.Visible = true;
                    btnPOS.Visible = true;
                    btnCategory.Visible = true;
                    btnProduct.Visible = true;
                    btnCustomer.Visible = true;
                    btnReports.Visible = true;
                    btnUserManage.Visible = true;
                    break;

                case "CASHIER":
                    btnDashboard.Visible = false;
                    btnPOS.Visible = true;
                    btnCategory.Visible = false;
                    btnProduct.Visible = false;
                    btnCustomer.Visible = true;
                    btnReports.Visible = false;
                    btnUserManage.Visible = false;
                    break;

                case "WAREHOUSE":
                    btnDashboard.Visible = false;
                    btnPOS.Visible = false;
                    btnCategory.Visible = true;
                    btnProduct.Visible = true;
                    btnCustomer.Visible = false;
                    btnReports.Visible = false;
                    btnUserManage.Visible = false;
                    break;

                default:
                    MessageBox.Show(
                        "Tai khoan chua duoc cap quyen hop le!",
                        "Canh bao",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    Close();
                    break;
            }
        }

        // =====================================================
        // DIEU HUONG
        // =====================================================

        private void ShowDashboard()
        {
            if (_activeForm != null)
            {
                _activeForm.Dispose();
                _activeForm = null;
            }

            panelMainContent.Controls.Clear();
            panelMainContent.Controls.Add(panelDashboard);

            panelDashboard.Visible = true;
            panelDashboard.Dock = DockStyle.Fill;
            panelDashboard.BringToFront();

            lblTitle.Text = "DASHBOARD - TONG QUAN";
            HighlightActiveButton(btnDashboard);
        }

        private void OpenChildForm(
            Form childForm,
            string screenTitle,
            Button senderButton)
        {
            if (_activeForm != null)
            {
                _activeForm.Dispose();
                _activeForm = null;
            }

            panelMainContent.Controls.Clear();

            _activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panelMainContent.Controls.Add(childForm);
            panelMainContent.Tag = childForm;

            lblTitle.Text = screenTitle;
            HighlightActiveButton(senderButton);

            childForm.Show();
            childForm.BringToFront();
        }

        private void HighlightActiveButton(Button activeButton)
        {
            foreach (Control control in panelSidebar.Controls)
            {
                if (control is Button button && button != btnLogout)
                {
                    button.BackColor = sidebarColor;
                    button.ForeColor = Color.FromArgb(222, 229, 240);
                    button.Font = new Font(
                        "Segoe UI", 9.5F, FontStyle.Regular);
                }
            }

            if (activeButton != null)
            {
                activeButton.BackColor = activeColor;
                activeButton.ForeColor = Color.White;
                activeButton.Font = new Font(
                    "Segoe UI", 9.5F, FontStyle.Bold);
            }
        }

        private void OpenDefaultScreenByRole(string role)
        {
            switch ((role ?? "").ToUpperInvariant())
            {
                case "ADMIN":
                    ShowDashboard();
                    break;

                case "WAREHOUSE":
                    OpenChildForm(
                        new FormProductManagement(),
                        "QUAN LY SAN PHAM & KHO HANG",
                        btnProduct);
                    break;

                case "CASHIER":
                    OpenChildForm(
                        new FormPOS(),
                        "BAN HANG - POS",
                        btnPOS);
                    break;
            }
        }

        // =====================================================
        // CAC NUT MENU
        // =====================================================

        private async void btnDashboard_Click(object sender, EventArgs e)
        {
            ShowDashboard();
            await LoadDashboardDataAsync();
        }

        private void btnPOS_Click(object sender, EventArgs e)
        {
            OpenChildForm(
                new FormPOS(),
                "BAN HANG - POS",
                btnPOS);
        }

        private void btnCategory_Click(object sender, EventArgs e)
        {
            OpenChildForm(
                new FormCategoryManagement(),
                "QUAN LY DANH MUC NHOM HANG",
                btnCategory);
        }

        private void btnProduct_Click(object sender, EventArgs e)
        {
            OpenChildForm(
                new FormProductManagement(),
                "QUAN LY SAN PHAM & KHO HANG",
                btnProduct);
        }

        private void btnCustomer_Click(object sender, EventArgs e)
        {
            OpenChildForm(
                new FormCustomerManagement(),
                "QUAN LY KHACH HANG",
                btnCustomer);
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            if (!SessionManager.CurrentRole.Equals(
                "ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Ban khong co quyen xem bao cao doanh thu!",
                    "Tu choi truy cap",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
                return;
            }

            OpenChildForm(
                new FormQuickReport(),
                "BAO CAO DOANH THU",
                btnReports);
        }

        private void btnUserManage_Click(object sender, EventArgs e)
        {
            if (!SessionManager.CurrentRole.Equals(
                "ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Ban khong co quyen quan tri tai khoan!",
                    "Tu choi truy cap",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Stop);
                return;
            }

            OpenChildForm(
                new FormUserManagement(),
                "QUAN TRI TAI KHOAN",
                btnUserManage);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Ban co chac chan muon dang xuat?",
                "Xac nhan dang xuat",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SessionManager.ClearSession();
                Close();
            }
        }
    }
}
