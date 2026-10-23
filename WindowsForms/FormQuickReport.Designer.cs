namespace MiniSupermarket.WinForms
{
    partial class FormQuickReport
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.dtpReportDate = new System.Windows.Forms.DateTimePicker();
            this.btnRunReport = new System.Windows.Forms.Button();
            this.lblTotalOrders = new System.Windows.Forms.Label();
            this.lblTotalRevenue = new System.Windows.Forms.Label();
            this.lblBestSeller = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // dtpReportDate
            // 
            this.dtpReportDate.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.dtpReportDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpReportDate.Location = new System.Drawing.Point(30, 30);
            this.dtpReportDate.Name = "dtpReportDate";
            this.dtpReportDate.Size = new System.Drawing.Size(200, 32);
            this.dtpReportDate.TabIndex = 0;
            // 
            // btnRunReport
            // 
            this.btnRunReport.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRunReport.Location = new System.Drawing.Point(250, 30);
            this.btnRunReport.Name = "btnRunReport";
            this.btnRunReport.Size = new System.Drawing.Size(150, 35);
            this.btnRunReport.TabIndex = 1;
            this.btnRunReport.Text = "Xem Báo Cáo";
            this.btnRunReport.UseVisualStyleBackColor = true;
            this.btnRunReport.Click += new System.EventHandler(this.btnRunReport_Click);
            // 
            // lblTotalOrders
            // 
            this.lblTotalOrders.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(100)))), ((int)(((byte)(180)))));
            this.lblTotalOrders.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTotalOrders.ForeColor = System.Drawing.Color.White;
            this.lblTotalOrders.Location = new System.Drawing.Point(30, 100);
            this.lblTotalOrders.Name = "lblTotalOrders";
            this.lblTotalOrders.Size = new System.Drawing.Size(300, 120);
            this.lblTotalOrders.TabIndex = 2;
            this.lblTotalOrders.Text = "Tổng số hóa đơn: 0";
            this.lblTotalOrders.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblTotalRevenue
            // 
            this.lblTotalRevenue.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(157)))), ((int)(((byte)(88)))));
            this.lblTotalRevenue.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTotalRevenue.ForeColor = System.Drawing.Color.White;
            this.lblTotalRevenue.Location = new System.Drawing.Point(360, 100);
            this.lblTotalRevenue.Name = "lblTotalRevenue";
            this.lblTotalRevenue.Size = new System.Drawing.Size(350, 120);
            this.lblTotalRevenue.TabIndex = 3;
            this.lblTotalRevenue.Text = "Doanh thu: 0 đ";
            this.lblTotalRevenue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblBestSeller
            // 
            this.lblBestSeller.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(68)))), ((int)(((byte)(55)))));
            this.lblBestSeller.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblBestSeller.ForeColor = System.Drawing.Color.White;
            this.lblBestSeller.Location = new System.Drawing.Point(30, 250);
            this.lblBestSeller.Name = "lblBestSeller";
            this.lblBestSeller.Size = new System.Drawing.Size(680, 100);
            this.lblBestSeller.TabIndex = 4;
            this.lblBestSeller.Text = "Mặt hàng bán chạy nhất: Đang cập nhật";
            this.lblBestSeller.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormQuickReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1050, 660);
            this.Controls.Add(this.lblBestSeller);
            this.Controls.Add(this.lblTotalRevenue);
            this.Controls.Add(this.lblTotalOrders);
            this.Controls.Add(this.btnRunReport);
            this.Controls.Add(this.dtpReportDate);
            this.Name = "FormQuickReport";
            this.Text = "FormQuickReport";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dtpReportDate;
        private System.Windows.Forms.Button btnRunReport;
        private System.Windows.Forms.Label lblTotalOrders;
        private System.Windows.Forms.Label lblTotalRevenue;
        private System.Windows.Forms.Label lblBestSeller;
    }
}