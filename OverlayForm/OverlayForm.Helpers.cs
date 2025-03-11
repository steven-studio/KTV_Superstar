using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DualScreenDemo
{
    public partial class OverlayForm
    {
        private readonly object imageLock = new object();        

        private void AdjustLabelPositions()
        {
            int labelHeight = displayLabels.First().Height;
            int totalHeight = displayLabels.Count * labelHeight;
            int startY = 100;

            for (int i = 0; i < displayLabels.Count; i++)
            {
                Label label = displayLabels[i];
                int centerX = (this.Width - label.Width) / 2;
                int centerY = startY + i * labelHeight;
                label.Location = new Point(centerX, centerY);
            }

            if (pauseLabel != null)
            {
                
                pauseLabel.Location = new Point(this.Width - pauseLabel.Width - 10, 100);
            }

            if (muteLabel != null)
            {
                
                muteLabel.Location = new Point(this.Width - muteLabel.Width - 10, 140);
            }
        }

        public void UpdateMarqueeText(string newText, MarqueeStartPosition startPosition, Color textColor)
        {
            this.marqueeText = newText;
            this.marqueeTextColor = textColor;

            // 使用顯示字體進行測量
            Font displayFont = new Font("Arial", 25, FontStyle.Bold);

            using (Graphics graphics = this.CreateGraphics())
            {
                SizeF textSize = graphics.MeasureString(marqueeText, displayFont);
                int textWidth = (int)textSize.Width;
                switch (startPosition)
                {
                    case MarqueeStartPosition.Middle:
                        this.marqueeXPos = (this.Width / 2) - (textWidth / 2) - 100;
                        break;
                    case MarqueeStartPosition.Right:
                        this.marqueeXPos = this.Width;
                        break;
                }
            }

            this.Invalidate();
            blackBackgroundPanel.Invalidate();
        }

        public void UpdateMarqueeTextSecondLine(string newText)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => UpdateMarqueeTextSecondLine(newText)));
                return;
            }

            marqueeTextSecondLine = newText;
            SplitSecondLineText(newText);

            using (Graphics graphics = this.CreateGraphics())
            {
                float textWidth = MeasureDisplayStringWidth(graphics, marqueeTextSecondLine, new Font("微軟正黑體", 40, FontStyle.Bold));
                marqueeXPosSecondLine = (int)((this.Width - textWidth) / 2);
            }

            if (textSegments.Count > 1)
            {
                segmentSwitchTimer.Start();
            }
            else
            {
                segmentSwitchTimer.Stop();
            }

            // 重置計時器
            if (secondLineTimer != null)
            {
                secondLineTimer.Stop();
                secondLineTimer.Dispose();
            }

            secondLineTimer = new System.Windows.Forms.Timer();
            secondLineTimer.Interval = 100;
            secondLineStartTime = DateTime.Now;

            secondLineTimer.Tick += (sender, e) =>
            {
                if ((DateTime.Now - secondLineStartTime).TotalMilliseconds >= 30000) // 30秒
                {
                    marqueeTextSecondLine = "";
                    textSegments.Clear();  // 清除分段文本
                    if (segmentSwitchTimer != null)
                    {
                        segmentSwitchTimer.Stop();  // 停止分段切換計時器
                    }
                    secondLineTimer.Stop();
                    secondLineTimer.Dispose();
                    this.Invalidate();
                    blackBackgroundPanel.Invalidate();
                }
            };

            secondLineTimer.Start();
            blackBackgroundPanel.Invalidate();
        }

        public void UpdateMarqueeTextThirdLine(string newText)
        {
            
            Console.WriteLine("UpdateMarqueeTextThirdLine called with text: " + newText);

            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => UpdateMarqueeTextThirdLine(newText)));
                return;
            }
            marqueeTextThirdLine = newText;
            marqueeXPosThirdLine = this.Width; 

            
            Console.WriteLine("Marquee text position reset to: " + marqueeXPosThirdLine);

            
            Invalidate();
        }

        private void MarqueeTimer_Tick(object sender, EventArgs e)
        {
            marqueeXPos -= 2; // 調整移動速度

            // 使用與顯示相同的字體來計算文本寬度
            using (Graphics graphics = this.CreateGraphics())
            {
                float textWidth = MeasureDisplayStringWidth(graphics, marqueeText, new Font("微軟正黑體", 34, FontStyle.Bold));

                // 當文本完全移出屏幕時重置位置
                if (marqueeXPos < -textWidth)
                {
                    marqueeXPos = this.Width;
                }
            }

            this.Invalidate();
            blackBackgroundPanel.Invalidate();
        }

        private float MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
        {
            // 使用提供的字體來測量文本寬度
            SizeF textSize = graphics.MeasureString(text, font);
            return textSize.Width;
        }
    }
}