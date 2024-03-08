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
            Timeout = TimeSpan.FromSeconds(30) // ����ȫ�ֳ�ʱʱ��Ϊ30��
        };
        // �����ȼ���API����
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Ϊ�ȼ�����һ��ID
        private int hotkeyId = 1;
        public Form1()
        {
            InitializeComponent();
            cmbTranslationService.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTranslationService.SelectedItem = "�ٶȷ���";
            LoadAppSettings(); // ����Ӧ������
            this.Load += new EventHandler(Form1_Load); // ���Load�¼�������
                                                       // ��ʼ��NotifyIcon
            notifyIcon1.Visible = false; // ��ʼ״̬���ɼ�
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
#pragma warning disable CS8622 // Disable warning
            // ����SizeChanged�¼�
            this.SizeChanged += MainForm_SizeChanged;
#pragma warning restore CS8622 // Re-enable warning

            // ����NotifyIcon�ĵ���¼�
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
            // ���Լ��ز���ʾ�����AppId��AppSecret
            txtAppId.Text = Properties.Settings.Default.AppId;
            txtAppSecret.Text = Properties.Settings.Default.AppSecret;
            txtAccessToken.Text = Properties.Settings.Default.AccessToken;

            // ���ʹ���������˵���ѡ�������Ҳ�����������
            cmbTranslationService.SelectedItem = Properties.Settings.Default.SelectedTranslationService;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // ע��F8Ϊȫ���ȼ� (F8�����������0x77)
            RegisterHotKey(this.Handle, hotkeyId, 0x0000, 0x77);

            // ���÷�������ѡ��
            cmbTranslationService.SelectedItem = Properties.Settings.Default.SelectedTranslationService;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // ȡ��ע���ȼ�
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


        // Form1��ѡ������񲢴�OverlayForm���߼�
        // ��fygj����Ŀ��
        private void OpenOverlayForm()
        {
            string appId = Properties.Settings.Default.AppId;
            string appSecret = Properties.Settings.Default.AppSecret;
            string accessToken = Properties.Settings.Default.AccessToken;

            // ȷ�� cmbTranslationService.SelectedItem ��Ϊ null
            string selectedService = cmbTranslationService.SelectedItem?.ToString() ?? "Ĭ�Ϸ������"; // �ṩһ��Ĭ��ֵ�Է�Ϊ null

            if (overlayForm == null || overlayForm.IsDisposed)
            {
                overlayForm = new OverlayFormWindow(appId, appSecret, accessToken, selectedService);
            }
            else
            {
                overlayForm.UpdateSettings(appId, appSecret, accessToken, selectedService);
            }

            // ���� sliderAutoClose �ǿ�����ʾʱ��Ļ���ؼ�
            overlayForm.DisplayDuration = trackBarDisplayDuration.Value * 1000; // ������ֵ���룩ת��Ϊ����

            overlayForm.Show();
            isScreenshotModeActive = true; // ȷ������״̬������
        }


        // ʾ�������ɼ򵥵�ǩ���߼�������Ҫ�����е�API�ĵ����е���
        private static string GenerateSignature(string appId, string appSecret, string query)
        {
            // ʾ��������ǩ����Ӧ����Կ�Ͳ�ѯ�ַ����򵥵�ƴ��
            // ע�⣺ʵ�ʵ�ǩ�����ɷ�ʽ���ܸ����ӣ���Ҫ�ο��е�API�ĵ�
            string signatureRaw = appId + query + appSecret;
            string signature = Convert.ToBase64String(Encoding.UTF8.GetBytes(signatureRaw));
            return signature;
        }


        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            // ���ı����ȡӦ��ID����Կ
            string appId = txtAppId.Text;
            string appSecret = txtAppSecret.Text;
            // ����txtAccessToken�Ǵ��access_token��TextBox������
            string accessToken = txtAccessToken.Text;

            // ����Щ��Ϣ���浽������
            Properties.Settings.Default.AppId = appId;
            Properties.Settings.Default.AppSecret = appSecret;
            Properties.Settings.Default.AccessToken = accessToken; // ����access_token

            // �����û�ѡ��ķ������
            if (cmbTranslationService.SelectedItem != null)
            {
                Properties.Settings.Default.SelectedTranslationService = cmbTranslationService.SelectedItem.ToString();
            }

            Properties.Settings.Default.Save(); // ��������

            // �ṩ�������û�
            MessageBox.Show("�����ѱ��档", "��Ϣ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected static string ComputeHash(string input, HashAlgorithm algorithm)
        {
            Byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        // �����ķ������ܷ���null�������ڷ������ͺ�ʹ��?
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
            // ��� SelectedItem �Ƿ�Ϊ null
            if (cmbTranslationService.SelectedItem != null)
            {
                // ��ȫ�ر����û���ѡ��������
                Properties.Settings.Default.SelectedTranslationService = cmbTranslationService.SelectedItem.ToString();
                Properties.Settings.Default.Save();
            }
            else
            {
                // ���� SelectedItem Ϊ null �����������������Ĭ��ֵ���������߼�
                // ���磬���������һ��Ĭ�ϵķ������
                // Properties.Settings.Default.SelectedTranslationService = "Ĭ�Ϸ������";
                // Properties.Settings.Default.Save();
            }
        }

        private void btnClearSettings_Click(object sender, EventArgs e)
        {
            // ����ı���
            txtAppId.Text = string.Empty;
            txtAppSecret.Text = string.Empty;
            txtAccessToken.Text = string.Empty;

            // ��ձ��������
            Properties.Settings.Default.AppId = string.Empty;
            Properties.Settings.Default.AppSecret = string.Empty;
            Properties.Settings.Default.AccessToken = string.Empty;
            Properties.Settings.Default.Save(); // �������ø���

            // �ṩ�û�����
            MessageBox.Show("�����������", "��Ϣ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void trackBarDisplayDuration_ValueChanged(object sender, EventArgs e)
        {
            labelDisplayDuration.Text = $"{trackBarDisplayDuration.Value} ��";
        }

        private void ShowTranslationResult(string text)
        {
            TranslationResultForm resultForm = new TranslationResultForm();
            resultForm.SetText(text);
            // ������ʾʱ��
            resultForm.SetAutoCloseInterval(trackBarDisplayDuration.Value * 1000); // ����ת��Ϊ����
            resultForm.ShowDialog();
        }

        private void labelDisplayDuration_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            // ���軬���Ĭ��ֵ��5
            trackBarDisplayDuration.Value = 5;
            labelDisplayDuration.Text = $"{trackBarDisplayDuration.Value} ��";
        }

        private void trackBarDisplayDuration_Scroll(object sender, EventArgs e)
        {

        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide(); // ���ش���
                notifyIcon1.Visible = true; // ��ʾ����ͼ��
            }
        }


        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) // ֻ�е���������ʱ����Ӧ
            {
                this.Show(); // ��ʾ����
                this.WindowState = FormWindowState.Normal; // ������״̬��Ϊ����
                notifyIcon1.Visible = false; // ��������ͼ��
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

