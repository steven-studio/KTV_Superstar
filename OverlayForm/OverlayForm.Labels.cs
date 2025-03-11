// OverlayForm/OverlayForm.Labels.cs
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DualScreenDemo
{
    public partial class OverlayForm
    {
        public Label displayLabel;
        public Label songDisplayLabel;
        private List<Label> displayLabels;
        public Label pauseLabel;
        public Label muteLabel; // New mute label
        public Label originalSongLabel;
        public Label nextSongLabel;
        private Label serviceBellLabel;
        public Label volumeUpLabel; // New volume up label
        public Label volumeDownLabel; // New volume down label
        private System.Windows.Forms.Timer volumeUpTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer volumeDownTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer micUpTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer micDownTimer = new System.Windows.Forms.Timer();
        public Label micUpLabel; // New microphone up label
        public Label micDownLabel; // New microphone down label
        public Label keyUpLabel; // New key up label
        public Label keyDownLabel; // New key down label
        public Label standardLabel; // New standard label
        public Label keyShiftLabel; // New keyShift  label
        public Label squareLabel; // New square label
        public Label professionalLabel; // New professional label
        public Label singDownLabel; // New sing down label
        public Label brightLabel; // New bright label
        public Label softLabel; // New soft label
        public Label autoLabel; // New auto label
        public Label romanticLabel; // New romantic label
        public Label dynamicLabel; // New dynamic label
        public Label tintLabel; // New tint label
        private System.Windows.Forms.Timer standardTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer squareTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer professionalTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer singDownTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer brightTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer softTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer autoTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer romanticTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer dynamicTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer tintTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer secondLineDisplayTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer qrCodeTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer delayTimer = new System.Windows.Forms.Timer();
        

        private void InitializeLabels()
        {   
            InitializeBlackBackgroundPanel();
            InitializeDisplayLabel();
            // InitializeSongDisplayLabel();
            InitializeDisplayLabels();
            InitializePauseLabel();
            InitializeMuteLabel(); // Initialize the new mute label
            InitializeOriginalSongLabel(); // 初始化原唱标签
            InitializeServiceBellLabel();
            InitializeVolumeUpLabel(); // Initialize the volume up label
            InitializeVolumeDownLabel(); // Initialize the volume down label
            InitializeMicUpLabel(); // Initialize the microphone up label
            InitializeMicDownLabel(); // Initialize the microphone down label
            InitializeKeyUpLabel(); // Initialize the key up label
            InitializeKeyDownLabel(); // Initialize the key down label
            InitializeStandardLabel(); // Initialize the standard label
            InitializeSquareLabel(); // Initialize the square label
            InitializeProfessionalLabel(); // Initialize the professional label
            InitializeSingDownLabel(); // Initialize the sing down label
            InitializeBrightLabel(); // Initialize the bright label
            InitializeSoftLabel(); // Initialize the soft label
            InitializeAutoLabel(); // Initialize the auto label
            InitializeRomanticLabel(); // Initialize the romantic label
            InitializeDynamicLabel(); // Initialize the dynamic label
            InitializeTintLabel(); // Initialize the tint label
            InitializeStandardKeyLabel();
            InitializeMaleKeyLabel();
            InitializeMaleKeyTimer();
            InitializeFemaleKeyLabel();
            InitializeNextSongLabel();
            InitializeFemaleKeyTimer();
            ConfigureKeyTimers();
            InitializemessageLabel();
        }
        

        private void ConfigureKeyTimers()
        {
            volumeUpTimer.Interval = 1000;
            volumeUpTimer.Tick += (sender, e) => {volumeUpLabel.Visible = false;volumeUpTimer.Stop();keepshowmic();};

            volumeDownTimer.Interval = 1000;
            volumeDownTimer.Tick += (sender, e) => {volumeDownLabel.Visible = false;volumeDownTimer.Stop();keepshowmic();};
            
            maleKeyTimer.Interval = 1000;
            maleKeyTimer.Tick += (sender, e) => {maleKeyLabel.Visible = false;maleKeyTimer.Stop();keepshowmic();};

            femaleKeyTimer.Interval = 1000;
            femaleKeyTimer.Tick += (sender, e) => {femaleKeyLabel.Visible = false;femaleKeyTimer.Stop();keepshowmic();};

            micUpTimer.Interval = 1000; 
            micUpTimer.Tick += (sender, e) => {micUpLabel.Visible = false;micUpTimer.Stop();keepshowmic();};

            micDownTimer.Interval =1000;
            micDownTimer.Tick += (sender, e) =>{micDownLabel.Visible = false;micDownTimer.Stop();keepshowmic();};

            brightTimer.Interval = 1000;
            brightTimer.Tick += (sender, e) => {brightLabel.Visible = false;brightTimer.Stop();keepshowmic();};

            softTimer.Interval = 1000;
            softTimer.Tick += (sender, e) => {softLabel.Visible = false;softTimer.Stop();keepshowmic();};

            autoTimer.Interval = 1000;
            autoTimer.Tick += (sender, e) => {autoLabel.Visible = false;autoTimer.Stop();keepshowmic();};

            romanticTimer.Interval = 1000;
            romanticTimer.Tick += (sender, e) => {romanticLabel.Visible = false;romanticTimer.Stop();keepshowmic();};

            dynamicTimer.Interval = 1000;
            dynamicTimer.Tick += (sender, e) => {dynamicLabel.Visible = false;dynamicTimer.Stop();keepshowmic();};

            tintTimer.Interval = 1000;
            tintTimer.Tick += (sender, e) => {tintLabel.Visible = false;tintTimer.Stop();keepshowmic();};

            standardKeyTimer.Interval = 1000;
            standardKeyTimer.Tick += (sender, e) => {standardKeyLabel.Visible = false;standardKeyTimer.Stop();keepshowmic();};

            messageTimer.Interval = 1000;
            messageTimer.Tick += (sender, e) => {messageLabel.Visible = false;messageTimer.Stop();keepshowmic();};

            keyUpTimer.Interval = 1000;
            keyUpTimer.Tick += (sender, e) => {keyUpLabel.Visible = false;keyUpTimer.Stop();keepshowmic();};
            
            keyDownTimer.Interval = 1000;
            keyDownTimer.Tick += (sender, e) => {keyDownLabel.Visible = false;keyDownTimer.Stop();keepshowmic();};

            secondLineDisplayTimer.Interval = 32000;
            secondLineDisplayTimer.Tick += SecondLineDisplayTimer_Tick;

            // Initialize Timer for hiding QR code
            qrCodeTimer.Interval = 10000; // 10 seconds
            qrCodeTimer.Tick += QrCodeTimer_Tick;
        }

        private void QrCodeTimer_Tick(object sender, EventArgs e)
        {
            showQRCode = false;
            qrCodeTimer.Stop();
            Invalidate(); // Trigger a repaint to hide the QR code
        }

        public void DisplayQRCodeOnOverlay(string randomFolderPath)
        {
            try
            {
                // Read the server address from the file
                string serverAddressFilePath = Path.Combine(Application.StartupPath, "txt", "ip.txt");
                if (!File.Exists(serverAddressFilePath))
                {
                    Console.WriteLine("Server address file not found: " + serverAddressFilePath);
                    return;
                }

                string serverAddress = File.ReadAllText(serverAddressFilePath).Trim();
                // Generate the URL content for the QR code
                string qrContent = serverAddress.Contains(":") ?
                    String.Format("http://{0}/{1}/windows.html", serverAddress, randomFolderPath) :
                    String.Format("http://{0}:{1}/{2}/windows.html", serverAddress, 9090, randomFolderPath);
                Console.WriteLine("QR Content: " + qrContent);
                string qrImagePath = Path.Combine(Application.StartupPath, "themes/superstar/_www", randomFolderPath, "qrcode.png");
                if (!File.Exists(qrImagePath))
                {
                    Console.WriteLine("QR code image not found: " + qrImagePath);
                    return;
                }
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        using (var fs = new FileStream(qrImagePath, FileMode.Open, FileAccess.Read))
                        {
                            qrCodeImage = Image.FromStream(fs);
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error loading QR code image: " + ex.Message);
                        System.Threading.Thread.Sleep(100); // Wait a bit before retrying
                    }
                }

                if (qrCodeImage == null)
                {
                    Console.WriteLine("Failed to load QR code image after multiple attempts.");
                    return;
                }

                showQRCode = true;
                qrCodeTimer.Start();
                Invalidate(); // Trigger a repaint to show the QR code
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in DisplayQRCodeOnOverlay: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                }
            }
        }

        private void SecondLineDisplayTimer_Tick(object sender, EventArgs e)
        {
            secondLineDisplayTimer.Stop();
            marqueeTextSecondLine = "";
            marqueeXPosSecondLine = this.Width;
            Invalidate();
            
        }

        private void InitializeDisplayLabel()
        {
            displayLabel = new Label();
            displayLabel.Location = new Point(100, 50);
            displayLabel.AutoSize = true;
            displayLabel.ForeColor = Color.White;
            displayLabel.Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold);
            displayLabel.BackColor = Color.Black;
            this.Controls.Add(displayLabel);
            displayLabel.Location = new Point(10, 56);
        }

        // private void InitializeSongDisplayLabel()
        // {
        //     songDisplayLabel = new Label();
        //     songDisplayLabel.Location = new Point(0, 50); // 設置顯示位置
        //     songDisplayLabel.AutoSize = true;
        //     songDisplayLabel.ForeColor = Color.White;
        //     songDisplayLabel.Font = new Font("Microsoft JhengHei", 125, FontStyle.Bold); // 設定字體樣式
        //     songDisplayLabel.BackColor = Color.Black;
        //     this.Controls.Add(songDisplayLabel);
        // }

        private void InitializeDisplayLabels()
        {
            displayLabels = new List<Label>();

            for (int i = 0; i < 6; i++) // Assuming a maximum of 6 lines
            {
                Label displayLabel = new Label
                {
                    AutoSize = true,
                    ForeColor = Color.White,
                    Font = new Font("Microsoft JhengHei", 25, FontStyle.Bold), // 設定字體樣式
                    BackColor = Color.Transparent
                };
                displayLabels.Add(displayLabel);
                this.Controls.Add(displayLabel);
            }
        }

        private void InitializePauseLabel()
        {
            pauseLabel = new Label();
            pauseLabel.AutoSize = false;
            pauseLabel.Font = new Font("Microsoft JhengHei", 125, FontStyle.Bold);
            pauseLabel.BackColor = Color.Transparent;
            pauseLabel.TextAlign = ContentAlignment.MiddleCenter;
            pauseLabel.Size = new Size(1080, 200);
            pauseLabel.Location = new Point(
            (this.Width - pauseLabel.Width) / 2,  // 水平居中
            (this.Height - pauseLabel.Height) / 2 + 500 // 垂直居中偏下 50 像素
            );  
                    
            pauseLabel.Paint += (s, e) =>
            {
                string text = "播放暫停";
                Font font = pauseLabel.Font;
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // 描邊（多次偏移繪製）
                using (Brush outlineBrush = new SolidBrush(Color.Black))
                {
                    for (int x = -2; x <= 2; x++)
                    {
                        for (int y = -2; y <= 2; y++)
                        {
                            if (x != 0 || y != 0)
                            {
                                g.DrawString(text, font, outlineBrush,
                                    new PointF((pauseLabel.Width - g.MeasureString(text, font).Width) / 2 + x,
                                            (pauseLabel.Height - g.MeasureString(text, font).Height) / 2 + y));
                            }
                        }
                    }
                }

                // 中心白色文字
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    g.DrawString(text, font, textBrush,
                        new PointF((pauseLabel.Width - g.MeasureString(text, font).Width) / 2,
                                (pauseLabel.Height - g.MeasureString(text, font).Height) / 2));
                }
            };
            this.Controls.Add(pauseLabel);
        }


        private void InitializeMuteLabel()
        {
            muteLabel = new Label();
            muteLabel.AutoSize = false;
            muteLabel.Visible = false;
            muteLabel.Font = new Font("Microsoft JhengHei", 125, FontStyle.Bold);
            muteLabel.BackColor = Color.Transparent;
            muteLabel.TextAlign = ContentAlignment.MiddleCenter;
            muteLabel.Size = new Size(1080, 200);
            muteLabel.Location = new Point(
            (this.Width - muteLabel.Width) / 2,  // 水平居中
            (this.Height - muteLabel.Height) / 2 + 250 // 垂直居中偏下 50 像素
            );  
                    
            muteLabel.Paint += (s, e) =>
            {
                string text =  "【全部靜音】";
                Font font = muteLabel.Font;
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // 描邊（多次偏移繪製）
                using (Brush outlineBrush = new SolidBrush(Color.Black))
                {
                    for (int x = -2; x <= 2; x++)
                    {
                        for (int y = -2; y <= 2; y++)
                        {
                            if (x != 0 || y != 0)
                            {
                                g.DrawString(text, font, outlineBrush,
                                    new PointF((muteLabel.Width - g.MeasureString(text, font).Width) / 2 + x,
                                            (muteLabel.Height - g.MeasureString(text, font).Height) / 2 + y));
                            }
                        }
                    }
                }

                // 中心白色文字
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    g.DrawString(text, font, textBrush,
                        new PointF((muteLabel.Width - g.MeasureString(text, font).Width) / 2,
                                (muteLabel.Height - g.MeasureString(text, font).Height) / 2));
                }
            };
            this.Controls.Add(muteLabel);
        }

        private void InitializeServiceBellLabel()
        {
            
                serviceBellLabel = new Label
                {
                    Text = " 服務鈴 ",
                    Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.Black,
                    AutoSize = true,
                    Visible = false // 初始设置为不可见
                };
                serviceBellLabel.PerformLayout();


                // Console.WriteLine($"初始化标签位置: {serviceBellLabel.Location}, 大小: {serviceBellLabel.Size}");
                serviceBellLabel.BringToFront();
                this.Controls.Add(serviceBellLabel);
                this.Resize += (s, e) =>
                {
                    serviceBellLabel.Location = new Point(this.Width - serviceBellLabel.Width - 10,56);
                    // Console.WriteLine($"调整标签位置: {serviceBellLabel.Location}, 大小: {serviceBellLabel.Size}");
                };
            
        }
        
        private void InitializeOriginalSongLabel()
        {
            originalSongLabel = new Label
            {
                Text = "有人聲",
                Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold), // 设置字体样式
                ForeColor = Color.White , // 设置文字颜色
                BackColor = Color.Black,
                AutoSize = true,
                Visible = false // 初始设置为不可见
            };
            this.Controls.Add(originalSongLabel);
            this.Resize += (s, e) =>
            {
                originalSongLabel.Location = new Point(this.Width - originalSongLabel.Width - 10, 56);
            };
        }

        private void InitializeVolumeUpLabel()
        {
            volumeUpLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold),
                BackColor = Color.Transparent,
                Text = "音量 ↑",
                Visible = false
            };
            blackBackgroundPanel.Controls.Add(volumeUpLabel);
            blackBackgroundPanel.Resize += (s, e) =>
            {
                volumeUpLabel.Location = new Point(blackBackgroundPanel.Width - volumeUpLabel.Width - 10, 56);
            };
        }
        private void InitializeVolumeDownLabel()
        {
            volumeDownLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei",34, FontStyle.Bold),
                BackColor = Color.Transparent,
                Text = "音量 ↓",
                Visible = false
            };
            blackBackgroundPanel.Controls.Add(volumeDownLabel);
            blackBackgroundPanel.Resize += (s, e) =>
            {
                volumeDownLabel.Location = new Point(blackBackgroundPanel.Width - volumeDownLabel.Width - 10, 56);
            };
        }
        private void InitializeMicUpLabel()
        {
            micUpLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold),
                BackColor = Color.Transparent,
                Text = "麥克風 ↑",
                Visible = false
            };
            blackBackgroundPanel.Controls.Add(micUpLabel);
            blackBackgroundPanel.Resize += (s, e) =>
            {
                micUpLabel.Location = new Point(blackBackgroundPanel.Width - micUpLabel.Width - 10, 56);
            };
        }
        public Panel blackBackgroundPanel;

        private void InitializeBlackBackgroundPanel()
        {
            blackBackgroundPanel = new Panel
            {
                BackColor = Color.FromArgb(255, 0, 0, 0), // 黑色背景
                Size = new Size(this.Width, 120), // 高度 100 像素
                Location = new Point(0, 0), // 从画面顶部开始
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top // 固定顶部
            };

            // 設置 Paint 事件處理程序
            blackBackgroundPanel.Paint += (sender, e) =>
            {
                using (Font largeFont = new Font("微軟正黑體", 34, FontStyle.Bold))
                using (Font secondLineFont = new Font("微軟正黑體", 34, FontStyle.Bold))
                using (Brush whiteBrush = new SolidBrush(Color.White))
                using (Brush limeGreenBrush = new SolidBrush(Color.LimeGreen))
                using (Brush marqueeBrush = new SolidBrush(marqueeTextColor))
                using (Brush backgroundBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0)))
                {
                    // 第一行文字的代码保持不变
                    SizeF textSize = e.Graphics.MeasureString(marqueeText, largeFont);
                    float yPosition1 = 0;
                    e.Graphics.FillRectangle(backgroundBrush, marqueeXPos, yPosition1, textSize.Width, textSize.Height);
                    e.Graphics.DrawString(marqueeText, largeFont, marqueeBrush, new PointF(marqueeXPos, yPosition1));

                    // 修改第二行文字的部分
                    float yPosition2 = 56;
                    Rectangle clipRect = new Rectangle(
                        (int)(this.Width / 32),  // 从1/8改成1/16（因为要居中）
                        (int)yPosition2, 
                        (int)(15 * this.Width / 16),  // 从3/4改成7/8
                        (int)textSize.Height
                    );
                    Region originalClip = e.Graphics.Clip;
                    e.Graphics.SetClip(clipRect);

                    // 获取当前应该显示的文字段落
                    string displayText = textSegments.Count > 0 ? textSegments[currentSegmentIndex] : marqueeTextSecondLine;
                    SizeF textSizeSecondLine = e.Graphics.MeasureString(displayText, secondLineFont);
                    float centeredXPos = (this.Width - textSizeSecondLine.Width) / 2;
                    e.Graphics.FillRectangle(backgroundBrush, centeredXPos, yPosition2, textSizeSecondLine.Width, textSizeSecondLine.Height);
                    e.Graphics.DrawString(displayText, secondLineFont, limeGreenBrush, new PointF(centeredXPos, yPosition2));

                    // 还原裁剪区域
                    e.Graphics.Clip = originalClip;

                }
            };

            this.Controls.Add(blackBackgroundPanel);

            blackBackgroundPanel.SendToBack();
        }


        private void InitializeMicDownLabel()
        {

            micDownLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold),
                BackColor = Color.Transparent,
                Text = "麥克風 ↓",
                Visible = false
            };
            blackBackgroundPanel.Controls.Add(micDownLabel);
            blackBackgroundPanel.Resize += (s, e) =>
            {
                micDownLabel.Location = new Point(blackBackgroundPanel.Width - micDownLabel.Width - 15, 56);
            };
        }


        private void InitializeNextSongLabel()
        {
            nextSongLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 32, FontStyle.Regular),
                BackColor = Color.Transparent,
                Text = "",
                Visible = true
            };
            blackBackgroundPanel.Controls.Add(nextSongLabel);

            nextSongLabel.Location = new Point(10, 56);
        }



public void UpdateNextSongLabelFromPlaylist(bool isUserPlaylistPlaying, SongData currentPlayingSong)
{
    // 获取播放列表
    List<SongData> currentPlaylist = isUserPlaylistPlaying ? 
        VideoPlayerForm.playingSongList : VideoPlayerForm.publicPlaylist;

    if (currentPlaylist == null || currentPlaylist.Count == 0)
    {
        nextSongLabel.Text = "目前沒有下一首，請踴躍點歌!!!";
        nextSongLabel.Visible = true;
        return;
    }

    // 找到当前歌曲的索引
    int currentSongIndex = currentPlaylist.IndexOf(currentPlayingSong);

    if (currentSongIndex == -1 || currentSongIndex + 1 >= currentPlaylist.Count)
    {
        nextSongLabel.Text = "目前沒有下一首，請踴躍點歌!!!";
    }
    else
    {
        SongData nextSong = currentPlaylist[currentSongIndex + 1];
        if (!string.IsNullOrEmpty(nextSong.ArtistA) && !string.IsNullOrEmpty(nextSong.Song))
        {
            nextSongLabel.Text = !string.IsNullOrWhiteSpace(nextSong.ArtistB) 
                ? String.Format("下一首：{0}  {1} {2}", nextSong.ArtistA, nextSong.ArtistB, nextSong.Song) 
                : String.Format("下一首：{0}  {1}", nextSong.ArtistA, nextSong.Song); 
        }
        else
        {
            nextSongLabel.Text = "";
        }
    }

    nextSongLabel.Visible = !string.IsNullOrEmpty(nextSongLabel.Text); // 僅在有內容時顯示標籤
}



        // 更新 nextSongLabel 標籤的文本
        public void UpdateNextSongLabel(string text)
        {
            if (nextSongLabel == null)
            {
                return;
            }

            nextSongLabel.Text = text;
            nextSongLabel.Visible = true; // 確保標籤顯示
            Console.WriteLine($"更新顯示: {text}");
        }

        private void InitializeKeyUpLabel()
        {
            keyUpLabel = new Label();
            keyUpLabel.AutoSize = true;
            keyUpLabel.ForeColor = Color.White;
            keyUpLabel.Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold); // 设置字体样式
            keyUpLabel.BackColor = Color.Black;
            keyUpLabel.Text = "升調 1# ";
            keyUpLabel.Visible = false; // 初始设置为不可见
            this.Controls.Add(keyUpLabel);
            this.Resize += (s, e) => 
            {
                keyUpLabel.Location = new Point(this.Width - keyUpLabel.Width - 10, 56);
            };
        }

        // Initialize the key down label
        private void InitializeKeyDownLabel()
        {
            keyDownLabel = new Label();
            keyDownLabel.AutoSize = true;
            keyDownLabel.ForeColor = Color.White;
            keyDownLabel.Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold); // 设置字体样式
            keyDownLabel.BackColor = Color.Black;
            keyDownLabel.Text = "降調 1# ";
            keyDownLabel.Visible = false;
            this.Controls.Add(keyDownLabel);
            this.Resize += (s, e) => 
            {
                keyDownLabel.Location = new Point(this.Width - keyDownLabel.Width - 10 ,56);
            };
        }


        private void InitializeStandardLabel()
        {
            standardLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold),
                BackColor = Color.Transparent,
                Text = "標準迴音",
                Visible = false
            };
            blackBackgroundPanel.Controls.Add(standardLabel);
            blackBackgroundPanel.Resize += (s, e) =>
            {
                standardLabel.Location = new Point(blackBackgroundPanel.Width - standardLabel.Width - 10, 56);
            };
        }
        private void InitializeSquareLabel()
        {
            squareLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold),
                BackColor = Color.Transparent,
                Text = "廣場迴音",
                Visible = false
            };
            blackBackgroundPanel.Controls.Add(squareLabel);
            blackBackgroundPanel.Resize += (s, e) =>
            {
                squareLabel.Location = new Point(blackBackgroundPanel.Width - squareLabel.Width - 10, 56);
            };
        }
        private void InitializeProfessionalLabel()
        {
            professionalLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold),
                BackColor = Color.Transparent,
                Text = "專業迴音",
                Visible = false
            };
            blackBackgroundPanel.Controls.Add(professionalLabel);
            blackBackgroundPanel.Resize += (s, e) =>
            {
                professionalLabel.Location = new Point(blackBackgroundPanel.Width - professionalLabel.Width - 10, 56);
            };
        }
        private void InitializeSingDownLabel()
        {
            singDownLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold),
                BackColor = Color.Transparent,
                Text = "唱將迴音",
                Visible = false
            };
            blackBackgroundPanel.Controls.Add(singDownLabel);
            blackBackgroundPanel.Resize += (s, e) =>
            {
                singDownLabel.Location = new Point(blackBackgroundPanel.Width - singDownLabel.Width - 10, 56);
            };
        }

        private void InitializeBrightLabel()
        {
            brightLabel = new Label();
            brightLabel.AutoSize = true;
            brightLabel.ForeColor = Color.LightYellow;
            brightLabel.Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold); // 设置字体样式
            brightLabel.BackColor = Color.Transparent;
            brightLabel.Text = " 明亮 ";
            brightLabel.Visible = false; // 初始设置为不可见
            this.Controls.Add(brightLabel);
            this.Resize += (s, e) => 
            {
                brightLabel.Location = new Point(this.Width - brightLabel.Width - 10, 27);
            };
        }


        private void InitializeSoftLabel()
        {
            softLabel = new Label();
            softLabel.AutoSize = true;
            softLabel.ForeColor = Color.LightGreen;
            softLabel.Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold); // 设置字体样式
            softLabel.BackColor = Color.Transparent;
            softLabel.Text = " 柔和 ";
            softLabel.Visible = false; // 初始设置为不可见
            this.Controls.Add(softLabel);
            this.Resize += (s, e) => 
            {
                softLabel.Location = new Point(this.Width - softLabel.Width - 10,27);
            };
        }

        // Initialize the auto label
        private void InitializeAutoLabel()
        {
            autoLabel = new Label();
            autoLabel.AutoSize = true;
            autoLabel.ForeColor = Color.Cyan;
            autoLabel.Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold); // 设置字体样式
            autoLabel.BackColor = Color.Transparent;
            autoLabel.Text = " 自動 ";
            autoLabel.Visible = false; // 初始设置为不可见
            this.Controls.Add(autoLabel);
            this.Resize += (s, e) => 
            {
                autoLabel.Location = new Point(this.Width - autoLabel.Width - 10, 27);
            };
        }
        private void InitializeRomanticLabel()
        {
            romanticLabel = new Label();
            romanticLabel.AutoSize = true;
            romanticLabel.ForeColor = Color.Pink;
            romanticLabel.Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold); // 设置字体样式
            romanticLabel.BackColor = Color.Transparent;
            romanticLabel.Text = " 浪漫 ";
            romanticLabel.Visible = false;
            this.Controls.Add(romanticLabel);
            this.Resize += (s, e) => 
            {
                romanticLabel.Location = new Point(this.Width - romanticLabel.Width - 10, 27);
            };
        }
        private void InitializeDynamicLabel()
        {
            dynamicLabel = new Label();
            dynamicLabel.AutoSize = true;
            dynamicLabel.ForeColor = Color.Red;
            dynamicLabel.Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold); // 设置字体样式
            dynamicLabel.BackColor = Color.Transparent;
            dynamicLabel.Text = " 動感 ";
            dynamicLabel.Visible = false; // 初始设置为不可见
            this.Controls.Add(dynamicLabel);
            this.Resize += (s, e) => 
            {
                dynamicLabel.Location = new Point(this.Width - dynamicLabel.Width - 10, 27);
            };
        }


        // Initialize the tint label
        private void InitializeTintLabel()
        {
            tintLabel = new Label();
            tintLabel.AutoSize = true;
            tintLabel.ForeColor = Color.Blue;
            tintLabel.Font = new Font("Microsoft JhengHei",34, FontStyle.Bold); // 设置字体样式
            tintLabel.BackColor = Color.Transparent;
            tintLabel.Text = " 調色 ";
            tintLabel.Visible = false;
            this.Controls.Add(tintLabel);
            this.Resize += (s, e) => 
            {
                tintLabel.Location = new Point(this.Width - tintLabel.Width - 10, 27);
            };
        }
        // 显示服务铃标签
        public void ShowServiceBellLabel()
        {
            if (serviceBellLabel != null)
            {
                whichoneshowmic();
                HideAllLabels();
                serviceBellLabel.Visible = true;
                // Console.WriteLine("服務鈴顯示: " + serviceBellLabel.Text); // 输出标签文本内容
            }
        }

        public void HideServiceBellLabel()
        {
            if (serviceBellLabel != null)
            {
                serviceBellLabel.Visible = false;
                // Console.WriteLine("服務鈴隱藏: " + serviceBellLabel.Text); // 输出标签文本内容
                keepshowmic();
            }
        }

        // 显示标准标签
        public void ShowStandardLabel()
        {
            HideAllLabels();
            standardLabel.Visible = true;
            whichoneshowmic();
            standardTimer.Start();
        }
        public void ShowSquareLabel()
        {
            HideAllLabels();
            squareLabel.Visible = true;
            whichoneshowmic();
            squareTimer.Start();
        }

        public void ShowProfessionalLabel()
        {
            HideAllLabels();
            professionalLabel.Visible = true;
            whichoneshowmic();
            professionalTimer.Start();
        }

        // 显示唱將标签
        public void ShowSingDownLabel()
        {
            HideAllLabels();
            singDownLabel.Visible = true;
            whichoneshowmic();
            singDownTimer.Start();
        }



        // 显示明亮标签
        public void ShowBrightLabel()
        {
            HideAllLabels();
            brightLabel.Visible = true;
            whichoneshowmic();
            brightTimer.Start();
        }

        // 显示柔和标签
        public void ShowSoftLabel()
        {
            
            HideAllLabels();
            softLabel.Visible = true;
            whichoneshowmic();
            softTimer.Start();
        }



        public void ShowAutoLabel()
        {
            HideAllLabels();
            autoLabel.Visible = true;
            whichoneshowmic();
            autoTimer.Start();
        }

        // 显示浪漫标签
        public void ShowRomanticLabel()
        {
            
            HideAllLabels();
            romanticLabel.Visible = true;
            whichoneshowmic();
            romanticTimer.Start();
        }

        public void ShowDynamicLabel()
        {
            HideAllLabels();
            dynamicLabel.Visible = true;
            whichoneshowmic();
            dynamicTimer.Start();
        }

        public void ShowTintLabel()
        {
            HideAllLabels();
            tintLabel.Visible = true;
            whichoneshowmic();
            tintTimer.Start();
        }

        private void InitializeStandardKeyLabel()
        {

            standardKeyLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold),
                BackColor = Color.Black,
                Text = "  標準調 ",
                Visible = false
            };
            blackBackgroundPanel.Controls.Add(standardKeyLabel);
            blackBackgroundPanel.Resize += (s, e) =>
            {
                standardKeyLabel.Location = new Point(blackBackgroundPanel.Width - standardKeyLabel.Width - 10, 56);
            };
        }
        private void InitializeFemaleKeyLabel()
        {

            femaleKeyLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold),
                BackColor = Color.Transparent,
                Text = " 女調 ",
                Visible = false
            };
            blackBackgroundPanel.Controls.Add(femaleKeyLabel);
            blackBackgroundPanel.Resize += (s, e) =>
            {
                femaleKeyLabel.Location = new Point(blackBackgroundPanel.Width - femaleKeyLabel.Width - 10, 56);
            };
        }
        private void InitializeMaleKeyLabel()
        {

            maleKeyLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold),
                BackColor = Color.Transparent,
                Text = " 男調 ",
                Visible = false
            };
            blackBackgroundPanel.Controls.Add(maleKeyLabel);
            blackBackgroundPanel.Resize += (s, e) =>
            {
                maleKeyLabel.Location = new Point(blackBackgroundPanel.Width - maleKeyLabel.Width - 10, 56);
            };
        }

        public void ShowPauseLabel()
        {
            pauseLabel.Visible = true;
        }

        // 隐藏暂停标签
        public void HidePauseLabel()
        {
            pauseLabel.Visible = false;
        }

        // 显示静音标签
        public void ShowMuteLabel()
        {
            muteLabel.Visible = true;
        }

        // 隐藏静音标签
        public void HideMuteLabel()
        {
            muteLabel.Visible = false;
        }

        public void ShowOriginalSongLabel(string text="有人聲")
        {
            HideAllLabels();
            originalSongLabel.Text = text;
            originalSongLabel.Visible = true;
        }

        public void HideOriginalSongLabel()
        {
            originalSongLabel.Visible = false;
            keepshowmic();
        }

        public void ToggleOriginalSongLabel()
        {
            originalSongLabel.Visible = !originalSongLabel.Visible;
        }
        public void ShowMaleKeyLabel()
        {
            maleKeyLabel.Visible = true;
            maleKeyTimer.Start();
        }
        public void ShowFemaleKeyLabel()
        {
            femaleKeyLabel.Visible = true;
            femaleKeyTimer.Start();
        }
 
        public void ShowVolumeUpLabel()
        {
            volumeUpLabel.Visible = true;
            volumeUpTimer.Start();
        }
        // 显示音量-标签
        public void ShowVolumeDownLabel()
        {
            volumeDownLabel.Visible = true;
            volumeDownTimer.Start();
        }
        // 显示麥克風+标签
        public void ShowMicUpLabel()
        {
            micUpLabel.Visible = true;
            micUpTimer.Start();
        }

        // 显示麥克風-标签
        public void ShowMicDownLabel()
        {
            micDownLabel.Visible = true;
            micDownTimer.Start();
        }

        // 显示標準調标签
        public void ShowStandardKeyLabel()
        {

            standardKeyLabel.Visible = true;
            standardKeyTimer.Start();

        }


        public void ShowKeyUpLabel(string customText = "")
        {
            // keyUpLabel.Text = string.IsNullOrWhiteSpace(customText) ? "升 1# 調" : customText;
            keyUpLabel.Text = customText; 
            keyUpLabel.Visible = true;
            keyUpTimer.Start();
        }


        public void ShowKeyDownLabel(string customText = "")
        {
            keyDownLabel.Text = customText; 
            keyDownLabel.Visible = true;
            keyDownTimer.Start();
        }
        private string switchmic = "Standard" ;
        // 隐藏所有标签
        public void HideAllLabels()
        {
            HidemicLabels();
        }
        public void whichoneshowmic()
        {
            if(singDownLabel.Visible){ 
                switchmic="SingDown";
            }else if(professionalLabel.Visible){   
                switchmic="Professional";
            }else if(squareLabel.Visible){ 
                switchmic="Square";
            }else if(standardLabel.Visible){    
                switchmic="Standard";
            }
        }
        public void keepshowmic()
        {
            switch (switchmic)
            {
                case "SingDown":
                    HidemicLabels();
                    ShowSingDownLabel();
                    break;
                case "Professional":
                    HidemicLabels();
                    ShowProfessionalLabel();
                    break;
                case "Square":
                    HidemicLabels();
                    ShowSquareLabel();
                    break;
                case "Standard":
                    HidemicLabels();
                    ShowStandardLabel();
                    break;
                default:
                    break;
            }
        }

        public void HidemicLabels()
        {
            singDownLabel.Visible = false;
            singDownTimer.Stop();
            professionalLabel.Visible = false;
            professionalTimer.Stop();
            squareLabel.Visible = false;
            squareTimer.Stop();
            standardLabel.Visible = false;
            standardTimer.Stop();
            
        }
        private void InitializemessageLabel()
        {
            messageLabel = new Label();
            messageLabel.AutoSize = true;
            messageLabel.ForeColor = Color.White;
            messageLabel.Font = new Font("Microsoft JhengHei", 34, FontStyle.Bold);
            messageLabel.BackColor = Color.Transparent;
            messageLabel.Text = "";
            messageLabel.Visible = false;
            this.Controls.Add(messageLabel);
            UpdateMessageLabelPosition();
            this.Resize += (s, e) => UpdateMessageLabelPosition();
        }
        public void ShowmessageLabel(string customText)
        {
            messageLabel.Visible = true;
            messageLabel.Text =customText;
            messageTimer.Start();
        }

        // 隐藏標準調标签
        public void HidemessageLabel()
        {
            messageLabel.Visible = false;
            messageTimer.Stop();
        }
        private void UpdateMessageLabelPosition()
        {
            int yPosition = (this.Height - messageLabel.Height) / 2;
            int xPosition = 10;
            messageLabel.Location = new Point(xPosition, yPosition);
        }
    }
}