using System.Drawing; // ȷ������System.Drawing�����ռ���ʹ��Rectangle��
using System.Drawing.Imaging;
using System.Net.Http;
using System.Text;
using System.Windows.Forms; // ����System.Windows.Forms�����ռ�
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Reflection.Metadata;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using static System.Windows.Forms.DataFormats;
using System.Net.Http.Json;
using RestSharp;
using Newtonsoft.Json;

namespace OverlayForm // ���������ռ䲻��
{
    public partial class OverlayFormWindow : Form // �޸�����ΪOverlayFormWindow
    {
        public Rectangle Selection { get; private set; } = Rectangle.Empty;
        private string appId;
        private string appSecret;
        private string accessToken;
        private HttpClient httpClient = new HttpClient();
        private string selectedService;
        public int DisplayDuration { get; set; } = 5000; // Ĭ��5��

        public OverlayFormWindow(string appId, string appSecret, string accessToken, string selectedService)
        {
            this.appId = appId;
            this.appSecret = appSecret;
            this.accessToken = accessToken;
            this.selectedService = selectedService; // ���ղ����÷������ѡ��
            InitializeComponent();
            this.TopMost = true; // ȷ���������������
            this.FormBorderStyle = FormBorderStyle.None; // �ޱ߿�
            this.WindowState = FormWindowState.Maximized; // ���
            this.BackColor = Color.Black; // ����ɫ
            this.Opacity = 0.5; // ��͸��
            // ��ʼ��������Ҫ�����ã�������¼�����
        }

        private bool isSelecting = false;
        private Point startPoint;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            // ��ʼ����ѡ��
            isSelecting = true;
            startPoint = e.Location;
            Selection = Rectangle.Empty;
            //this.Opacity = 0;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isSelecting)
            {
                // ����ѡ�������ػ�
                Selection = new Rectangle(
                    Math.Min(startPoint.X, e.X),
                    Math.Min(startPoint.Y, e.Y),
                    Math.Abs(e.X - startPoint.X),
                    Math.Abs(e.Y - startPoint.Y));
                this.Refresh(); // �����ػ�
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (isSelecting)
            {
                // �������ѡ��
                isSelecting = false;
                this.Selection = new Rectangle(
                    Math.Min(startPoint.X, e.X),
                    Math.Min(startPoint.Y, e.Y),
                    Math.Abs(e.X - startPoint.X),
                    Math.Abs(e.Y - startPoint.Y));

                // ���� CaptureSelectedArea ���н�ͼ
                CaptureSelectedArea();

                // ѡ����ɺ�ر� OverlayForm ���������������
                //this.Opacity = 0.5;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (isSelecting)
            {
                // ����ѡ������
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, Selection);
                }
            }
        }
        private async void CaptureSelectedArea()
        {
            if (Selection.Width > 0 && Selection.Height > 0)
            {
                this.Hide();
                await Task.Delay(100); // ȷ����ͼǰOverlay����������

                using (Bitmap bitmap = new Bitmap(Selection.Width, Selection.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(Selection.Location, Point.Empty, Selection.Size);
                    }

                    string base64Image = ImageToBase64(bitmap); // ��bitmapת��Ϊbase64�ַ���

                    if (selectedService == "�е�����")
                    {
                        await CaptureAndTranslateWithYoudao(base64Image); // ���ڴ���base64Image
                    }
                    else if (selectedService == "�ٶȷ���")
                    {
                        await CaptureAndTranslateWithBaidu(bitmap, base64Image); // ����bitmap��base64Image
                    }
                }
                this.Show();
            }
            else
            {
                MessageBox.Show("δѡ����Ч����", "����", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void ShowTranslationResultInForm2(string translationText)
        {
            TranslationResultForm resultForm = new TranslationResultForm();
            resultForm.SetText(translationText); // ʹ�� SetText ���������÷�����
            resultForm.ShowDialog(); // ��ģ̬�Ի���ķ�ʽ��ʾ TranslationResultForm
        }


        // ��������ǩ���ĸ�������
        private string ComputeHash(string input, HashAlgorithm algorithm)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        // ���ڽ�ȡ�ַ����ĸ�������
        private string Truncate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Length <= 20 ? value : value.Substring(0, 10) + value.Length + value.Substring(value.Length - 10);
        }



        public class TranslationRegion
        {
            [JsonPropertyName("boundingBox")]
            public string? BoundingBox { get; set; }

            [JsonPropertyName("linesCount")]
            public int LinesCount { get; set; }

            [JsonPropertyName("lineheight")]
            public int LineHeight { get; set; }

            [JsonPropertyName("context")]
            public string? Context { get; set; }

            [JsonPropertyName("linespace")]
            public int LineSpace { get; set; }

            [JsonPropertyName("tranContent")]
            public string? TranContent { get; set; }
        }

        public class TranslationResult
        {
            [JsonPropertyName("orientation")]
            public string? Orientation { get; set; }

            [JsonPropertyName("lanFrom")]
            public string? LanFrom { get; set; }

            [JsonPropertyName("textAngle")]
            public string? TextAngle { get; set; }

            [JsonPropertyName("errorCode")]
            public string? ErrorCode { get; set; }

            [JsonPropertyName("lanTo")]
            public string? LanTo { get; set; }

            [JsonPropertyName("resRegions")]
            public List<TranslationRegion>? ResRegions { get; set; }
        }


        // ʾ������Bitmapת��ΪBase64�ַ���
        private string ImageToBase64(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] byteImage = ms.ToArray();
                return Convert.ToBase64String(byteImage);
            }
        }


        private void ResetSelectionState()
        {
            isSelecting = false; // ���ÿ�ѡ״̬����
            Selection = Rectangle.Empty; // �����ǰ�Ŀ�ѡ����
            this.Invalidate(); // �����ػ棬������ѻ��ƵĿ�ѡ��
        }

        public void ExitScreenshotMode()
        {
            this.Hide(); // ���� this.Close(); ȡ��������������OverlayForm��ʵ��
        }

        // ��Bitmapת��Ϊbyte[]
        private byte[] BitmapToBytes(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }
        public class BaiduPicTransResponse
        {
            public string? error_code { get; set; }
            public string? error_msg { get; set; }
            public Data? data { get; set; }

            public class Data
            {
                public List<Content>? content { get; set; }
            }

            public class Content
            {
                public string? src { get; set; }
                public string? dst { get; set; }
            }
        }

        private async Task CaptureAndTranslateWithYoudao(string base64Image)
        {
            // ֱ���������ȡ���µ����ã�ȷ��ʹ�õ������µ�ֵ
            string appId = this.appId;
            string appSecret = this.appSecret;

            // ȷ��appId��appSecret��Ч
            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appSecret))
            {
                MessageBox.Show("App ID �� App Secret ����Ϊ�ա�", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ����ǩ���Ȳ���...
            string salt = DateTime.Now.Ticks.ToString();
            string curtime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            string signStr = appId + Truncate(base64Image) + salt + curtime + appSecret;
            string sign = ComputeHash(signStr, SHA256.Create());

            // �����������
            var parameters = new Dictionary<string, string>
            {
                {"appKey", appId},
                {"q", base64Image},
                {"salt", salt},
                {"from", "auto"},
                {"to", "zh-CHS"},
                {"curtime", curtime},
                {"sign", sign},
                {"signType", "v3"},
            };

            // ִ��HTTP����Ⱥ�������...
            using var content = new FormUrlEncodedContent(parameters);
            try
            {
                HttpResponseMessage response = await httpClient.PostAsync("https://openapi.youdao.com/ocrtransapi", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<TranslationResult>(responseContent);
                    if (result != null && result.ErrorCode == "0" && result.ResRegions != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var region in result.ResRegions)
                        {
                            sb.AppendLine(region.TranContent); // ƴ��ÿ������ķ�����
                        }

                        // ������������ʾ���ڵ�ʵ��
                        TranslationResultForm resultForm = new TranslationResultForm();
                        // ��StringBuilder�Ľ��ת��Ϊ�ַ�����������Ϊ���������ڵ��ı�
                        resultForm.SetText(sb.ToString()); // ʹ��StringBuilderƴ�ӵ��ַ���
                                                           // �����Զ��رյ�ʱ����
                        resultForm.SetAutoCloseInterval(this.DisplayDuration); // ʹ�����õ���ʾʱ��
                                                                               // ��ʾ����������
                        resultForm.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("����ʧ�ܣ�" + result?.ErrorCode, "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("���������ʧ�ܡ�", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"��������������⣬���������������ӻ��Ժ����ԡ�\n��������: {ex.Message}", "�������", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"�����쳣��{ex.Message}", "�쳣", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ���ðٶȷ���API
        private async Task CaptureAndTranslateWithBaidu(Bitmap bitmap, string base64Image)
        {
            // ʹ�����µ�accessToken
            string accessToken = this.accessToken;
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                MessageBox.Show("Access Token ����Ϊ�ա�", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string apiEndpoint = $"https://aip.baidubce.com/file/2.0/mt/pictrans/v1?access_token={accessToken}";
            var client = new RestClient(apiEndpoint);
            var request = new RestRequest(Method.POST);

            // �������������ļ�
            var bytes = BitmapToBytes(bitmap); // ��Bitmapת��Ϊbyte[]
            request.AddFileBytes("image", bytes, "image.jpg", "image/jpeg");
            request.AddParameter("from", "auto");
            request.AddParameter("to", "zh");
            request.AddParameter("v", "3");

            Debug.WriteLine("�������󵽰ٶ�ͼƬ����API");

            try
            {
                var response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    Debug.WriteLine("API��Ӧ�ɹ�");
                    var responseData = JsonConvert.DeserializeObject<BaiduPicTransResponse>(response.Content);
                    Debug.WriteLine($"��Ӧ���ݣ�{response.Content}");

                    if (responseData != null && responseData.error_code == "0" && responseData.data != null && responseData.data.content != null)
                    {
                        var translatedText = responseData.data.content.Select(c => c.dst).ToList();
                        if (translatedText.Any())
                        {
                            var translatedTextJoined = string.Join("\n", translatedText);
                            // �����´��ڵ�ʵ������ʾ������
                            // ��ʾ������֮ǰ
                            TranslationResultForm resultForm = new TranslationResultForm();
                            resultForm.SetText(translatedTextJoined); // ʹ�úϲ�����ַ���
                            resultForm.SetAutoCloseInterval(this.DisplayDuration); // ʹ�����õ���ʾʱ��
                            resultForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show($"û�з�������", "��֤���", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"API���ش���{responseData?.error_msg}", "��֤ʧ��", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    Debug.WriteLine($"API��֤ʧ�ܣ�{response.ErrorMessage}");
                    MessageBox.Show($"API��֤ʧ�ܣ�{response.ErrorMessage}", "��֤ʧ��", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"�����쳣��{ex.Message}");
                MessageBox.Show($"�����쳣��{ex.Message}", "�쳣", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Show();
            }
        }
        public void UpdateTranslationService(string newService)
        {
            this.selectedService = newService;
            // ���ܻ���Ҫ�������£���ȷ����һ�ν�ͼ����ʹ���µķ���
        }

        public void UpdateSettings(string appId, string appSecret, string accessToken, string selectedService)
        {
            this.appId = appId;
            this.appSecret = appSecret;
            this.accessToken = accessToken;
            this.selectedService = selectedService;
        }
    }
}
