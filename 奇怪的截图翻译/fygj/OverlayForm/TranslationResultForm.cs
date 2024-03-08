using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OverlayForm
{
    public partial class TranslationResultForm : Form
    {
        private string _textToDisplay = string.Empty;
        private Font _customFont = new Font("Microsoft Sans Serif", 15); // 默认字体大小为12，可以根据需要调整
        private System.Windows.Forms.Timer _closeTimer = new System.Windows.Forms.Timer();

        public TranslationResultForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None; // 无边框
            this.BackColor = Color.Silver; // 设置背景
#pragma warning disable CS8622 // Disable warning
            this.MouseClick += TranslationResultForm_MouseClick;
#pragma warning restore CS8622 // Re-enable warning
            // 初始化定时器，但不在这里设置间隔和启动
#pragma warning disable CS8622 // Disable warning
            this._closeTimer.Tick += _closeTimer_Tick; // 注册定时器Tick事件
#pragma warning restore CS8622 // Re-enable warning
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // 假设_textToDisplay包含了要显示的文本
            string text = _textToDisplay;
            Font font = _customFont; // 使用自定义字体
            Brush brush = Brushes.Fuchsia;
            int maxWidth = 600; // 假设最大宽度为300像素
            Point start = new Point(10, 10); // 起始绘制位置

            StringFormat stringFormat = new StringFormat();
            stringFormat.Trimming = StringTrimming.Word; // 单词边界换行
            stringFormat.FormatFlags = StringFormatFlags.LineLimit; // 控制文本换行

            // 使用MeasureString方法来测量文本的高度
            SizeF size = e.Graphics.MeasureString(text, font, maxWidth, stringFormat);

            // 调整窗体大小
            this.Width = maxWidth + 20; // 留出一些边距
            this.Height = (int)Math.Ceiling(size.Height) + 20; // 根据文本高度调整

            // 绘制文本
            RectangleF layoutRect = new RectangleF(start.X, start.Y, maxWidth, size.Height);
            e.Graphics.DrawString(text, font, brush, layoutRect, stringFormat);
            // 使用紫红色绘制文本
        }


        private void DrawTextWithLineBreaks(Graphics graphics, string text, Font font, Brush brush, Rectangle layoutRectangle)
        {
            StringFormat format = new StringFormat();
            format.Trimming = StringTrimming.Word;

            List<string> lines = new List<string>();
            int start = 0;
            while (start < text.Length)
            {
                string substring = text.Substring(start);
                SizeF size = graphics.MeasureString(substring, font, layoutRectangle.Width, format);
                if (size.Height > font.Height * 1.5)
                {
                    int lastSpace = substring.LastIndexOf(' ', Math.Min(substring.Length - 1, layoutRectangle.Width / (int)font.Size));
                    if (lastSpace > 0)
                    {
                        lines.Add(substring.Substring(0, lastSpace));
                        start += lastSpace + 1;
                    }
                    else
                    {
                        lines.Add(substring);
                        break;
                    }
                }
                else
                {
                    lines.Add(substring);
                    break;
                }
            }

            float y = layoutRectangle.Y;
            foreach (string line in lines)
            {
                graphics.DrawString(line, font, brush, new PointF(layoutRectangle.X, y));
                y += font.Height;
            }
        }


        public void SetText(string text)
        {
            _textToDisplay = text;
            SetTextAndAdjustSize(text);
            this.Invalidate(); // 触发重绘以显示新文本
                               // 不再这里启动定时器，移至 SetAutoCloseInterval 方法
        }

        public void SetTextAndAdjustSize(string text)
        {
            _textToDisplay = text;
            this.Invalidate(); // 触发重绘以更新显示

            // 检查Screen.PrimaryScreen是否为null
            if (Screen.PrimaryScreen != null)
            {
                // 将最大宽度统一设置为600像素
                int maxWidth = 600;

                using (Graphics g = this.CreateGraphics())
                {
                    SizeF size = g.MeasureString(text, _customFont, maxWidth); // 传入最大宽度作为测量的限制

                    // 调整窗体大小
                    this.Width = Math.Min((int)Math.Ceiling(size.Width) + 20, maxWidth); // 确保窗体宽度不超过最大宽度
                    this.Height = (int)Math.Ceiling(size.Height) + 20;

                    // 将窗体位置设置为屏幕上方中间
                    this.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2, 0);
                }
            }
        }

        public void AdjustFont(float fontSize)
        {
            _customFont = new Font(_customFont.FontFamily, fontSize);
            SetTextAndAdjustSize(_textToDisplay); // 重新调整文本和窗体大小
        }

        private void TranslationResultForm_MouseClick(object sender, MouseEventArgs e)
        {
            this.Close(); // 点击窗体任意位置关闭窗体
        }

        private void _closeTimer_Tick(object sender, EventArgs e)
        {
            // 在这里编写定时器触发时要执行的代码
            // 例如，自动关闭窗体
            this.Close();
        }

        public void SetAutoCloseInterval(int interval)
        {
            _closeTimer.Interval = interval; // 设置定时器间隔，单位为毫秒
            _closeTimer.Start(); // 启动定时器
        }
    }
}
