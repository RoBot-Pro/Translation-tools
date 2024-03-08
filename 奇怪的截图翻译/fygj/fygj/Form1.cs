                           using OverlayForm;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using RestSharp;
using Newtonsoft.Json;


namespace fygj
{
    public partial class Form1 : Form
    {
        private OverlayFormWindow? overlayForm = null;
        private bool isScreenshotModeActive = false;
        private readonly HttpClient httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30) // 设置全局超时时间为30秒
        };
        // 定义热键的API函数
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // 为热键分配一个ID
        private int hotkeyId = 1;
        public Form1()
        {
            InitializeComponent();
            cmbTranslationService.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTranslationService.SelectedItem = "百度翻译";
            LoadAppSettings(); // 加载应用设置
            this.Load += new EventHandler(Form1_Load); // 添加Load事件处理器
                                                       // 初始化NotifyIcon
            notifyIcon1.Visible = false; // 初始状态不可见
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
#pragma warning disable CS8622 // Disable warning
            // 订阅SizeChanged事件
            this.SizeChanged += MainForm_SizeChanged;
#pragma warning restore CS8622 // Re-enable warning

            // 订阅NotifyIcon的点击事件
#pragma warning disable CS8622 // Disable warning
            notifyIcon1.MouseClick += notifyIcon1_MouseClick;
#pragma warning restore CS8622 // Re-enable warning
        }

        private void txtAppId_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtAppSecret_TextChanged(object sender, EventArgs e)
        {

        }

        private void LoadAppSettings()
        {
            // 尝试加载并显示保存的AppId和AppSecret
            txtAppId.Text = Properties.Settings.Default.AppId;
            txtAppSecret.Text = Properties.Settings.Default.AppSecret;
            txtAccessToken.Text = Properties.Settings.Default.AccessToken;

            // 如果使用了下拉菜单来选择翻译服务，也加载这个设置
            cmbTranslationService.SelectedItem = Properties.Settings.Default.SelectedTranslationService;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // 注册F8为全局热键 (F8的虚拟键码是0x77)
            RegisterHotKey(this.Handle, hotkeyId, 0x0000, 0x77);

            // 设置翻译服务的选择
            cmbTranslationService.SelectedItem = Properties.Settings.Default.SelectedTranslationService;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 取消注册热键
            UnregisterHotKey(this.Handle, hotkeyId);
            base.OnFormClosing(e);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312 && m.WParam.ToInt32() == hotkeyId)
            {
                if (!isScreenshotModeActive)
                {
                    OpenOverlayForm();
                }
                else
                {
                    if (overlayForm != null && !overlayForm.IsDisposed)
                    {
                        overlayForm.ExitScreenshotMode();
                        isScreenshotModeActive = false;
                    }
                }
            }
        }


        // Form1中选择翻译服务并打开OverlayForm的逻辑
        // 在fygj主项目中
        private void OpenOverlayForm()
        {
            string appId = Properties.Settings.Default.AppId;
            string appSecret = Properties.Settings.Default.AppSecret;
            string accessToken = Properties.Settings.Default.AccessToken;

            // 确保 cmbTranslationService.SelectedItem 不为 null
            string selectedService = cmbTranslationService.SelectedItem?.ToString() ?? "默认翻译服务"; // 提供一个默认值以防为 null

            if (overlayForm == null || overlayForm.IsDisposed)
            {
                overlayForm = new OverlayFormWindow(appId, appSecret, accessToken, selectedService);
            }
            else
            {
                overlayForm.UpdateSettings(appId, appSecret, accessToken, selectedService);
            }

            // 假设 sliderAutoClose 是控制显示时间的滑块控件
            overlayForm.DisplayDuration = trackBarDisplayDuration.Value * 1000; // 将滑块值（秒）转换为毫秒

            overlayForm.Show();
            isScreenshotModeActive = true; // 确保激活状态被更新
        }


        // 示例：生成简单的签名逻辑，这需要根据有道API文档进行调整
        private static string GenerateSignature(string appId, string appSecret, string query)
        {
            // 示例：假设签名是应用密钥和查询字符串简单的拼接
            // 注意：实际的签名生成方式可能更复杂，需要参考有道API文档
            string signatureRaw = appId + query + appSecret;
            string signature = Convert.ToBase64String(Encoding.UTF8.GetBytes(signatureRaw));
            return signature;
        }


        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            // 从文本框读取应用ID和密钥
            string appId = txtAppId.Text;
            string appSecret = txtAppSecret.Text;
            // 假设txtAccessToken是存放access_token的TextBox的名字
            string accessToken = txtAccessToken.Text;

            // 将这些信息保存到设置中
            Properties.Settings.Default.AppId = appId;
            Properties.Settings.Default.AppSecret = appSecret;
            Properties.Settings.Default.AccessToken = accessToken; // 保存access_token

            // 保存用户选择的翻译服务
            if (cmbTranslationService.SelectedItem != null)
            {
                Properties.Settings.Default.SelectedTranslationService = cmbTranslationService.SelectedItem.ToString();
            }

            Properties.Settings.Default.Save(); // 保存设置

            // 提供反馈给用户
            MessageBox.Show("设置已保存。", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected static string ComputeHash(string input, HashAlgorithm algorithm)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        // 如果你的方法可能返回null，可以在返回类型后使用?
        protected static string? Truncate(string? q)
        {
            if (q == null)
            {
                return null;
            }
            int len = q.Length;
            return len <= 20 ? q : q.Substring(0, 10) + len.ToString() + q.Substring(len - 10, 10);
        }


        private void txtBaiduAppId_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtBaiduAppSecret_TextChanged(object sender, EventArgs e)
        {

        }

        private void cmbTranslationService_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 检查 SelectedItem 是否为 null
            if (cmbTranslationService.SelectedItem != null)
            {
                // 安全地保存用户的选择到设置中
                Properties.Settings.Default.SelectedTranslationService = cmbTranslationService.SelectedItem.ToString();
                Properties.Settings.Default.Save();
            }
            else
            {
                // 处理 SelectedItem 为 null 的情况，可能是设置默认值或者其他逻辑
                // 例如，你可以设置一个默认的翻译服务
                // Properties.Settings.Default.SelectedTranslationService = "默认翻译服务";
                // Properties.Settings.Default.Save();
            }
        }

        private void btnClearSettings_Click(object sender, EventArgs e)
        {
            // 清空文本框
            txtAppId.Text = string.Empty;
            txtAppSecret.Text = string.Empty;
            txtAccessToken.Text = string.Empty;

            // 清空保存的设置
            Properties.Settings.Default.AppId = string.Empty;
            Properties.Settings.Default.AppSecret = string.Empty;
            Properties.Settings.Default.AccessToken = string.Empty;
            Properties.Settings.Default.Save(); // 保存设置更改

            // 提供用户反馈
            MessageBox.Show("设置已清除。", "信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void trackBarDisplayDuration_ValueChanged(object sender, EventArgs e)
        {
            labelDisplayDuration.Text = $"{trackBarDisplayDuration.Value} 秒";
        }

        private void ShowTranslationResult(string text)
        {
            TranslationResultForm resultForm = new TranslationResultForm();
            resultForm.SetText(text);
            // 设置显示时间
            resultForm.SetAutoCloseInterval(trackBarDisplayDuration.Value * 1000); // 将秒转换为毫秒
            resultForm.ShowDialog();
        }

        private void labelDisplayDuration_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            // 假设滑块的默认值是5
            trackBarDisplayDuration.Value = 5;
            labelDisplayDuration.Text = $"{trackBarDisplayDuration.Value} 秒";
        }

        private void trackBarDisplayDuration_Scroll(object sender, EventArgs e)
        {

        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide(); // 隐藏窗体
                notifyIcon1.Visible = true; // 显示托盘图标
            }
        }


        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) // 只有当是左键点击时才响应
            {
                this.Show(); // 显示窗体
                this.WindowState = FormWindowState.Normal; // 将窗体状态改为正常
                notifyIcon1.Visible = false; // 隐藏托盘图标
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

