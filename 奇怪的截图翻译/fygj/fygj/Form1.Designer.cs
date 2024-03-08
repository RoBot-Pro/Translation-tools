namespace fygj
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            txtAppId = new TextBox();
            txtAppSecret = new TextBox();
            btnSaveSettings = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            cmbTranslationService = new ComboBox();
            label6 = new Label();
            txtAccessToken = new TextBox();
            btnClearSettings = new Button();
            trackBarDisplayDuration = new TrackBar();
            labelDisplayDuration = new Label();
            label5 = new Label();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            exitToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)trackBarDisplayDuration).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // txtAppId
            // 
            txtAppId.Location = new Point(57, 59);
            txtAppId.Name = "txtAppId";
            txtAppId.Size = new Size(287, 23);
            txtAppId.TabIndex = 0;
            txtAppId.TextChanged += txtAppId_TextChanged;
            // 
            // txtAppSecret
            // 
            txtAppSecret.Location = new Point(57, 127);
            txtAppSecret.Name = "txtAppSecret";
            txtAppSecret.Size = new Size(287, 23);
            txtAppSecret.TabIndex = 1;
            txtAppSecret.TextChanged += txtAppSecret_TextChanged;
            // 
            // btnSaveSettings
            // 
            btnSaveSettings.BackColor = Color.FromArgb(0, 192, 192);
            btnSaveSettings.ForeColor = Color.Black;
            btnSaveSettings.Location = new Point(414, 164);
            btnSaveSettings.Name = "btnSaveSettings";
            btnSaveSettings.Size = new Size(98, 55);
            btnSaveSettings.TabIndex = 2;
            btnSaveSettings.Text = "保存设置";
            btnSaveSettings.UseVisualStyleBackColor = false;
            btnSaveSettings.Click += btnSaveSettings_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.ForeColor = Color.DodgerBlue;
            label1.Location = new Point(57, 24);
            label1.Name = "label1";
            label1.Size = new Size(108, 17);
            label1.TabIndex = 4;
            label1.Text = "请输入有道翻译ID:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.ForeColor = Color.DodgerBlue;
            label2.Location = new Point(57, 98);
            label2.Name = "label2";
            label2.Size = new Size(125, 17);
            label2.TabIndex = 5;
            label2.Text = "请输入有道翻译Key：";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.ForeColor = Color.Violet;
            label3.Location = new Point(256, 315);
            label3.Name = "label3";
            label3.Size = new Size(274, 68);
            label3.TabIndex = 6;
            label3.Text = "确保已经输入有道翻译的ID和Key\r\n或者百度翻译的access_token\r\n并且保存之后！\r\n截图翻译快捷键F8，再次按下F8则退出截图翻译！";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.ForeColor = Color.Goldenrod;
            label4.Location = new Point(57, 167);
            label4.Name = "label4";
            label4.Size = new Size(132, 17);
            label4.TabIndex = 10;
            label4.Text = "百度翻译access_token";
            // 
            // cmbTranslationService
            // 
            cmbTranslationService.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTranslationService.FormattingEnabled = true;
            cmbTranslationService.Items.AddRange(new object[] { "百度翻译", "有道翻译" });
            cmbTranslationService.Location = new Point(61, 263);
            cmbTranslationService.Name = "cmbTranslationService";
            cmbTranslationService.Size = new Size(121, 25);
            cmbTranslationService.TabIndex = 12;
            cmbTranslationService.SelectedIndexChanged += cmbTranslationService_SelectedIndexChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.ForeColor = Color.Sienna;
            label6.Location = new Point(57, 232);
            label6.Name = "label6";
            label6.Size = new Size(75, 17);
            label6.TabIndex = 13;
            label6.Text = "翻译API选项";
            // 
            // txtAccessToken
            // 
            txtAccessToken.Location = new Point(57, 196);
            txtAccessToken.Name = "txtAccessToken";
            txtAccessToken.Size = new Size(287, 23);
            txtAccessToken.TabIndex = 14;
            // 
            // btnClearSettings
            // 
            btnClearSettings.BackColor = Color.FromArgb(0, 192, 192);
            btnClearSettings.Location = new Point(414, 59);
            btnClearSettings.Name = "btnClearSettings";
            btnClearSettings.Size = new Size(98, 56);
            btnClearSettings.TabIndex = 15;
            btnClearSettings.Text = "重置输入";
            btnClearSettings.UseVisualStyleBackColor = false;
            btnClearSettings.Click += btnClearSettings_Click;
            // 
            // trackBarDisplayDuration
            // 
            trackBarDisplayDuration.Location = new Point(57, 338);
            trackBarDisplayDuration.Maximum = 20;
            trackBarDisplayDuration.Name = "trackBarDisplayDuration";
            trackBarDisplayDuration.Size = new Size(104, 45);
            trackBarDisplayDuration.TabIndex = 16;
            trackBarDisplayDuration.Scroll += trackBarDisplayDuration_Scroll;
            trackBarDisplayDuration.ValueChanged += trackBarDisplayDuration_ValueChanged;
            // 
            // labelDisplayDuration
            // 
            labelDisplayDuration.AutoSize = true;
            labelDisplayDuration.ForeColor = Color.Red;
            labelDisplayDuration.Location = new Point(61, 318);
            labelDisplayDuration.Name = "labelDisplayDuration";
            labelDisplayDuration.Size = new Size(31, 17);
            labelDisplayDuration.TabIndex = 17;
            labelDisplayDuration.Text = "5 秒";
            labelDisplayDuration.Click += labelDisplayDuration_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.ForeColor = Color.Teal;
            label5.Location = new Point(57, 301);
            label5.Name = "label5";
            label5.Size = new Size(104, 17);
            label5.TabIndex = 18;
            label5.Text = "翻译结果时间设置";
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "这是一个奇怪的截图工具";
            notifyIcon1.Visible = true;
            notifyIcon1.MouseClick += notifyIcon1_MouseClick;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { exitToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(125, 26);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(124, 22);
            exitToolStripMenuItem.Text = "点击退出";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(224, 224, 224);
            ClientSize = new Size(533, 392);
            Controls.Add(label5);
            Controls.Add(labelDisplayDuration);
            Controls.Add(trackBarDisplayDuration);
            Controls.Add(btnClearSettings);
            Controls.Add(txtAccessToken);
            Controls.Add(label6);
            Controls.Add(cmbTranslationService);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnSaveSettings);
            Controls.Add(txtAppSecret);
            Controls.Add(txtAppId);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            Text = "奇怪的截图翻译工具";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)trackBarDisplayDuration).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtAppId;
        private TextBox txtAppSecret;
        private Button btnSaveSettings;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private ComboBox cmbTranslationService;
        private Label label6;
        private TextBox txtAccessToken;
        private Button btnClearSettings;
        private TrackBar trackBarDisplayDuration;
        private Label labelDisplayDuration;
        private Label label5;
        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem exitToolStripMenuItem;
    }
}
