using System.Drawing; // 确保引入System.Drawing命名空间以使用Rectangle等
using System.Drawing.Imaging;
using System.Net.Http;
using System.Text;
using System.Windows.Forms; // 引入System.Windows.Forms命名空间
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

namespace OverlayForm // 保持命名空间不变
{
    public partial class OverlayFormWindow : Form // 修改类名为OverlayFormWindow
    {
        public Rectangle Selection { get; private set; } = Rectangle.Empty;
        private string appId;
        private string appSecret;
        private string accessToken;
        private HttpClient httpClient = new HttpClient();
        private string selectedService;
        public int DisplayDuration { get; set; } = 5000; // 默认5秒

        public OverlayFormWindow(string appId, string appSecret, string accessToken, string selectedService)
        {
            this.appId = appId;
            this.appSecret = appSecret;
            this.accessToken = accessToken;
            this.selectedService = selectedService; // 接收并设置翻译服务选择
            InitializeComponent();
            this.TopMost = true; // 确保窗体总是在最顶层
            this.FormBorderStyle = FormBorderStyle.None; // 无边框
            this.WindowState = FormWindowState.Maximized; // 最大化
            this.BackColor = Color.Black; // 背景色
            this.Opacity = 0.5; // 半透明
            // 初始化其他必要的设置，如鼠标事件处理
        }

        private bool isSelecting = false;
        private Point startPoint;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            // 开始区域选择
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
                // 更新选择区域并重绘
                Selection = new Rectangle(
                    Math.Min(startPoint.X, e.X),
                    Math.Min(startPoint.Y, e.Y),
                    Math.Abs(e.X - startPoint.X),
                    Math.Abs(e.Y - startPoint.Y));
                this.Refresh(); // 触发重绘
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (isSelecting)
            {
                // 完成区域选择
                isSelecting = false;
                this.Selection = new Rectangle(
                    Math.Min(startPoint.X, e.X),
                    Math.Min(startPoint.Y, e.Y),
                    Math.Abs(e.X - startPoint.X),
                    Math.Abs(e.Y - startPoint.Y));

                // 调用 CaptureSelectedArea 进行截图
                CaptureSelectedArea();

                // 选择完成后关闭 OverlayForm 或进行其他清理工作
                //this.Opacity = 0.5;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (isSelecting)
            {
                // 绘制选择区域
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
                await Task.Delay(100); // 确保截图前Overlay窗口已隐藏

                using (Bitmap bitmap = new Bitmap(Selection.Width, Selection.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(Selection.Location, Point.Empty, Selection.Size);
                    }

                    string base64Image = ImageToBase64(bitmap); // 将bitmap转换为base64字符串

                    if (selectedService == "有道翻译")
                    {
                        await CaptureAndTranslateWithYoudao(base64Image); // 现在传递base64Image
                    }
                    else if (selectedService == "百度翻译")
                    {
                        await CaptureAndTranslateWithBaidu(bitmap, base64Image); // 传递bitmap和base64Image
                    }
                }
                this.Show();
            }
            else
            {
                MessageBox.Show("未选择有效区域。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void ShowTranslationResultInForm2(string translationText)
        {
            TranslationResultForm resultForm = new TranslationResultForm();
            resultForm.SetText(translationText); // 使用 SetText 方法来设置翻译结果
            resultForm.ShowDialog(); // 以模态对话框的方式显示 TranslationResultForm
        }


        // 用于生成签名的辅助方法
        private string ComputeHash(string input, HashAlgorithm algorithm)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashedBytes = algorithm.ComputeHash(inputBytes);
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }

        // 用于截取字符串的辅助方法
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


        // 示例：将Bitmap转换为Base64字符串
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
            isSelecting = false; // 重置框选状态变量
            Selection = Rectangle.Empty; // 清除当前的框选区域
            this.Invalidate(); // 请求重绘，以清除已绘制的框选框
        }

        public void ExitScreenshotMode()
        {
            this.Hide(); // 或者 this.Close(); 取决于您如何想管理OverlayForm的实例
        }

        // 将Bitmap转换为byte[]
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
            // 直接在这里读取最新的设置，确保使用的是最新的值
            string appId = this.appId;
            string appSecret = this.appSecret;

            // 确保appId和appSecret有效
            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appSecret))
            {
                MessageBox.Show("App ID 和 App Secret 不能为空。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 生成签名等操作...
            string salt = DateTime.Now.Ticks.ToString();
            string curtime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            string signStr = appId + Truncate(base64Image) + salt + curtime + appSecret;
            string sign = ComputeHash(signStr, SHA256.Create());

            // 构建请求参数
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

            // 执行HTTP请求等后续操作...
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
                            sb.AppendLine(region.TranContent); // 拼接每个区域的翻译结果
                        }

                        // 创建翻译结果显示窗口的实例
                        TranslationResultForm resultForm = new TranslationResultForm();
                        // 将StringBuilder的结果转换为字符串，并设置为翻译结果窗口的文本
                        resultForm.SetText(sb.ToString()); // 使用StringBuilder拼接的字符串
                                                           // 设置自动关闭的时间间隔
                        resultForm.SetAutoCloseInterval(this.DisplayDuration); // 使用设置的显示时间
                                                                               // 显示翻译结果窗口
                        resultForm.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("翻译失败：" + result?.ErrorCode, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("请求翻译服务失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"网络请求出现问题，请检查您的网络连接或稍后重试。\n错误详情: {ex.Message}", "网络错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发生异常：{ex.Message}", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 调用百度翻译API
        private async Task CaptureAndTranslateWithBaidu(Bitmap bitmap, string base64Image)
        {
            // 使用最新的accessToken
            string accessToken = this.accessToken;
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                MessageBox.Show("Access Token 不能为空。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string apiEndpoint = $"https://aip.baidubce.com/file/2.0/mt/pictrans/v1?access_token={accessToken}";
            var client = new RestClient(apiEndpoint);
            var request = new RestRequest(Method.POST);

            // 添加请求参数和文件
            var bytes = BitmapToBytes(bitmap); // 将Bitmap转换为byte[]
            request.AddFileBytes("image", bytes, "image.jpg", "image/jpeg");
            request.AddParameter("from", "auto");
            request.AddParameter("to", "zh");
            request.AddParameter("v", "3");

            Debug.WriteLine("发送请求到百度图片翻译API");

            try
            {
                var response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    Debug.WriteLine("API响应成功");
                    var responseData = JsonConvert.DeserializeObject<BaiduPicTransResponse>(response.Content);
                    Debug.WriteLine($"响应内容：{response.Content}");

                    if (responseData != null && responseData.error_code == "0" && responseData.data != null && responseData.data.content != null)
                    {
                        var translatedText = responseData.data.content.Select(c => c.dst).ToList();
                        if (translatedText.Any())
                        {
                            var translatedTextJoined = string.Join("\n", translatedText);
                            // 创建新窗口的实例并显示翻译结果
                            // 显示翻译结果之前
                            TranslationResultForm resultForm = new TranslationResultForm();
                            resultForm.SetText(translatedTextJoined); // 使用合并后的字符串
                            resultForm.SetAutoCloseInterval(this.DisplayDuration); // 使用设置的显示时间
                            resultForm.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show($"没有翻译结果。", "验证结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"API返回错误：{responseData?.error_msg}", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    Debug.WriteLine($"API验证失败：{response.ErrorMessage}");
                    MessageBox.Show($"API验证失败：{response.ErrorMessage}", "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"请求异常：{ex.Message}");
                MessageBox.Show($"请求异常：{ex.Message}", "异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Show();
            }
        }
        public void UpdateTranslationService(string newService)
        {
            this.selectedService = newService;
            // 可能还需要其他更新，以确保下一次截图翻译使用新的服务
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
