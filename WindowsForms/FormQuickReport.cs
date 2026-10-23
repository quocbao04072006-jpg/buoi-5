using System;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    public partial class FormQuickReport : Form
    {
        public FormQuickReport()
        {
            InitializeComponent();
            this.Load += FormQuickReport_Load;
        }

        private async void FormQuickReport_Load(object sender, EventArgs e)
        {
            await LoadReportDataAsync();
        }

        private async void btnRunReport_Click(object sender, EventArgs e)
        {
            await LoadReportDataAsync();
        }

        private async Task LoadReportDataAsync()
        {
            try
            {
                var dtp = this.Controls.OfType<DateTimePicker>().FirstOrDefault();
                DateTime selectedDate = dtp != null ? dtp.Value.Date : DateTime.Today;

                // BỎ CHỮ "api/" Ở ĐẦU ĐI VÌ BASEADDRESS ĐÃ CÓ SẴN
                string url = $"reports/daily?date={selectedDate:yyyy-MM-dd}";

                var result = await ApiClientService.Client.GetFromJsonAsync<ReportResponseDto>(url);

                if (result != null)
                {
                    lblTotalOrders.Text = $"{result.TotalOrders} Hóa đơn";
                    lblTotalRevenue.Text = $"{result.TotalRevenue:N0} đ";
                    lblBestSeller.Text = $"{result.BestSellingProduct}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu báo cáo: " + ex.Message, "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class ReportResponseDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public string BestSellingProduct { get; set; } = string.Empty;
    }
}