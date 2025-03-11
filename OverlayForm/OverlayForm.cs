using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using ZXing;
using ZXing.QrCode;
using System.Timers;

namespace DualScreenDemo
{
    public partial class OverlayForm : Form
    {
        
        private string marqueeText = "這是跑馬燈文字示例 - 歡迎使用MediaPlayerForm!";
        private Color marqueeTextColor = Color.White; 
        private string marqueeTextSecondLine = ""; 
        private string marqueeTextThirdLine = "";
        private int marqueeXPos;
        private int marqueeXPosSecondLine; 
        private int marqueeXPosThirdLine;
        private System.Windows.Forms.Timer marqueeTimer;
        private Image backgroundImage;
        private Image firstStickerImage;
        private Image secondStickerImage;
        private float firstStickerXPos;
        private float secondStickerXPos;
        private float imageYPos;
        private int screenHeight;
        private int topMargin;
        private int bottomMargin;
        public static System.Windows.Forms.Timer displayTimer = new System.Windows.Forms.Timer();
        public static System.Timers.Timer songDisplayTimer = new System.Timers.Timer();
        public static System.Timers.Timer unifiedTimer;
        private System.Windows.Forms.Timer stickerTimer1 = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer stickerTimer2 = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer keyUpTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer keyDownTimer = new System.Windows.Forms.Timer();
        public Label standardKeyLabel;
        private System.Windows.Forms.Timer standardKeyTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer messageTimer = new System.Windows.Forms.Timer();
        public Label maleKeyLabel;
        public Label messageLabel;
        private System.Windows.Forms.Timer maleKeyTimer;
        public Label femaleKeyLabel;
        private System.Windows.Forms.Timer femaleKeyTimer;
        private System.Windows.Forms.Timer secondLineTimer;
        private DateTime secondLineStartTime;
        private const int secondLineDuration = 20000; 
        private Image qrCodeImage;
        private bool showQRCode;
        private System.Windows.Forms.Timer segmentSwitchTimer = new System.Windows.Forms.Timer();
        private int currentSegmentIndex = 0;
        private List<string> textSegments = new List<string>();

        public enum MarqueeStartPosition
        {
            Middle,
            Right
        }

        
        private static OverlayForm _mainForm;

        public static OverlayForm MainForm
        {
            get { return _mainForm; }
            private set { _mainForm = value; }
        }

        public OverlayForm()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            MainForm = this;  
            InitializeFormSettings();
            ConfigureTimers();
            LoadBackgroundImage();
            ConfigureImageDisplay();
            InitializeLabels();
            ConfigureSegmentTimer();
        }
        private void ConfigureSegmentTimer()
        {
            segmentSwitchTimer.Interval = 3000;
            segmentSwitchTimer.Tick += (sender, e) =>
            {
                if (textSegments.Count > 1)
                {
                    currentSegmentIndex = (currentSegmentIndex + 1) % textSegments.Count;
                    blackBackgroundPanel.Invalidate();
                }
            };
        }
        private void SplitSecondLineText(string text)
        {
            textSegments.Clear();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (text.Length <= 16)
            {
                textSegments.Add(text);
            }
            else
            {
                for (int i = 0; i < text.Length; i += 16)
                {
                    int length = Math.Min(16, text.Length - i);
                    textSegments.Add(text.Substring(i, length));
                }
            }
            currentSegmentIndex = 0;
        }


        
        private void InitializeMaleKeyTimer()
        {
            maleKeyTimer = new System.Windows.Forms.Timer();
            maleKeyTimer.Interval = 3000; 
            maleKeyTimer.Tick += (s, e) => 
            {
                HideAllLabels();
            };
        }

        
        private void InitializeFemaleKeyTimer()
        {
            femaleKeyTimer = new System.Windows.Forms.Timer();
            femaleKeyTimer.Interval = 3000; 
            femaleKeyTimer.Tick += (s, e) => 
            {
                HideAllLabels();
            };
        }

private void AddCenteredPicture(Bitmap bitmap, int y)
{
    int x = (this.Width - bitmap.Width) / 2;
    AddPicture(bitmap, x, y);
}
private void AddPicture(Bitmap bitmap, int x, int y)
{
    PictureBox pictureBox = new PictureBox
    {
        Image = bitmap,
        Size = bitmap.Size,
        BackColor = Color.Transparent,
        Location = new Point(x, y)
    };
    this.Controls.Add(pictureBox);
    pictureBox.BringToFront();
}
private Bitmap GenerateTextImage(string text, Font font, Color textColor, Color backgroundColor)
{
    SizeF textSize;
    using (Bitmap tempBitmap = new Bitmap(1, 1))
    using (Graphics tempGraphics = Graphics.FromImage(tempBitmap))
    {
        textSize = tempGraphics.MeasureString(text, font);
    }

    // 創建一個稍大的位圖尺寸，以容納描邊
    int padding = 4; // 描邊寬度
    Bitmap bitmap = new Bitmap((int)textSize.Width + padding * 2, (int)textSize.Height + padding * 2);
    
    using (Graphics graphics = Graphics.FromImage(bitmap))
    {
        graphics.Clear(backgroundColor);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // 先繪製黑色描邊
        using (Brush outlineBrush = new SolidBrush(Color.Black))
        {
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    if (x != 0 || y != 0)
                    {
                        graphics.DrawString(text, font, outlineBrush, 
                            new PointF(padding + x, padding + y));
                    }
                }
            }
        }

        // 繪製中心文字
        using (Brush textBrush = new SolidBrush(textColor))
        {
            graphics.DrawString(text, font, textBrush, 
                new PointF(padding, padding));
        }
    }

    // 修剪圖片邊緣空白
    return TrimBitmap(bitmap);
}

private Bitmap TrimBitmap(Bitmap source)
{
    Rectangle rect = FindContentBounds(source);

    if (rect.Width == 0 || rect.Height == 0)
        return source; // 防止圖片完全空白時崩潰

    Bitmap trimmedBitmap = new Bitmap(rect.Width, rect.Height);
    using (Graphics g = Graphics.FromImage(trimmedBitmap))
    {
        g.DrawImage(source, 0, 0, rect, GraphicsUnit.Pixel);
    }

    return trimmedBitmap;
}

private Rectangle FindContentBounds(Bitmap bmp)
{
    int width = bmp.Width;
    int height = bmp.Height;

    int top = 0, left = 0, bottom = height - 1, right = width - 1;

    bool found = false;

    // 找到頂邊界
    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            if (bmp.GetPixel(x, y).A > 0)
            {
                top = y;
                found = true;
                break;
            }
        }
        if (found) break;
    }

    found = false;
    // 找到底邊界
    for (int y = height - 1; y >= 0; y--)
    {
        for (int x = 0; x < width; x++)
        {
            if (bmp.GetPixel(x, y).A > 0)
            {
                bottom = y;
                found = true;
                break;
            }
        }
        if (found) break;
    }

    found = false;
    // 找到左邊界
    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            if (bmp.GetPixel(x, y).A > 0)
            {
                left = x;
                found = true;
                break;
            }
        }
        if (found) break;
    }

    found = false;
    // 找到右邊界
    for (int x = width - 1; x >= 0; x--)
    {
        for (int y = 0; y < height; y++)
        {
            if (bmp.GetPixel(x, y).A > 0)
            {
                right = x;
                found = true;
                break;
            }
        }
        if (found) break;
    }

    return Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
}



        public static void DisplayNumberAtTopLeft(string newText)
        {
            if (MainForm != null)
            {
                MainForm.Invoke(new System.Action(() =>
                {
                    
                    string currentText = MainForm.displayLabel.Text;
                    MainForm.nextSongLabel.Visible = false;
                    string combinedText = currentText + newText;

                    
                    if (combinedText.Length > 6)
                    {
                        
                        combinedText = combinedText.Substring(0, 6);
                    }

                    
                    MainForm.displayLabel.Text = combinedText;
                    MainForm.nextSongLabel.Visible = false;

                    
                    displayTimer.Stop();
                    displayTimer.Start();
                }));
            }
        }

        private void InitializeFormSettings()
        {
            
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.BackColor = Color.Magenta; 
            this.TransparencyKey = this.BackColor; 
            this.Height = 50; 

            
            if (Screen.AllScreens.Length > 1)
            {
                var secondaryScreen = Screen.AllScreens[1]; 
                this.Width = secondaryScreen.Bounds.Width; 
                this.Location = new Point(secondaryScreen.Bounds.Location.X, this.Location.Y); 
                screenHeight = secondaryScreen.Bounds.Height;
                topMargin = screenHeight / 3;
                bottomMargin = screenHeight * 2 / 3;
            }
            else
            {
                this.Width = Screen.PrimaryScreen.Bounds.Width; 
                this.screenHeight = Screen.PrimaryScreen.Bounds.Height;
            }

            
            marqueeXPos = this.Width;
            marqueeXPosSecondLine = 7 * this.Width / 8;
            marqueeXPosThirdLine = this.Width;

            
            marqueeTimer = new System.Windows.Forms.Timer();
            marqueeTimer.Interval = 20; 
            marqueeTimer.Tick += MarqueeTimer_Tick;
            marqueeTimer.Start();

            secondLineTimer = new System.Windows.Forms.Timer();
            secondLineTimer.Interval = 100; 
            secondLineTimer.Tick += SecondLineTimer_Tick;

            
            try
            {
                
                string filePath = Path.Combine(Application.StartupPath, "WelcomeMessage.txt");
                marqueeText = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine("Error reading marquee text from file: " + ex.Message);
                marqueeText = "這是跑馬燈文字示例 - 歡迎使用MediaPlayerForm!";
            }

            
            
            
            

            this.DoubleBuffered = true;
        }

        private void ConfigureTimers()
        {
            
            displayTimer.Interval = 15000; 
            displayTimer.Tick += DisplayTimer_Tick;

            songDisplayTimer = new System.Timers.Timer(30000); 
            songDisplayTimer.Elapsed += new ElapsedEventHandler(SongDisplayTimer_Elapsed);
            songDisplayTimer.AutoReset = true;
            songDisplayTimer.Enabled = true;


            unifiedTimer = new System.Timers.Timer(15000);
            
            unifiedTimer.Elapsed += new ElapsedEventHandler(UnifiedTimer_Elapsed);
            unifiedTimer.AutoReset = true;
            unifiedTimer.Enabled = true;

            stickerTimer1.Interval = 15000;  
            stickerTimer1.Tick += (sender, e) => {
                lock (imageLock)
                {
                    firstStickerImage = null;
                    this.Invalidate();
                }
                if (secondStickerImage == null) 
                    LoadBackgroundImage();
                stickerTimer1.Stop();
                HideImages();  
            };

            stickerTimer2.Interval = 15000;  
            stickerTimer2.Tick += (sender, e) => {
                lock (imageLock)
                {
                    secondStickerImage = null;
                    this.Invalidate();
                }
                if (firstStickerImage == null) 
                    LoadBackgroundImage();
                stickerTimer2.Stop();
                HideImages();  
            };
        }

private static void DisplayTimer_Tick(object sender, EventArgs e)
{
    if (MainForm.InvokeRequired)
    {
        MainForm.Invoke(new System.Action(() =>
        {
            MainForm.displayLabel.Text = "";  
            MainForm.nextSongLabel.Visible = true; // 显示 nextSongLabel
        }));
    }
    else
    {
        MainForm.displayLabel.Text = "";  
        MainForm.nextSongLabel.Visible = true; // 显示 nextSongLabel
    }

    displayTimer.Stop(); // 停止计时器
}

private static void SongDisplayTimer_Elapsed(object sender, EventArgs e)
{
    if (MainForm.InvokeRequired)
    {
        Console.WriteLine("SongDisplayTimer_Tick invoked on UI thread.");

        MainForm.Invoke(new System.Action(() =>
        {
            MainForm.songDisplayLabel.Text = "";  
            MainForm.nextSongLabel.Visible = true; // 恢复显示 nextSongLabel
        }));
    }
    else
    {
        Console.WriteLine("SongDisplayTimer_Tick invoked on background thread.");
        MainForm.songDisplayLabel.Text = "";  
        MainForm.nextSongLabel.Visible = true; // 恢复显示 nextSongLabel
    }

    songDisplayTimer.Stop(); 
}


        private readonly object _lockObject = new object();

        private void UnifiedTimer_Elapsed(object sender, EventArgs e)
        {
            // Console.WriteLine("UnifiedTimer_Elapsed called"); 

            if (MainForm.InvokeRequired)
            {
                MainForm.Invoke(new System.Action<object, EventArgs>(UnifiedTimer_Elapsed), new object[] { sender, e });
            }
            else
            {
                displayLabel.Text = "";

                switch (CurrentUIState)
                {
                    case UIState.SelectingLanguage:
                        
                        SetUIState(UIState.Initial);
                        HandleTimeout("");
                        break;
                    case UIState.SelectingArtistCategory:
                        SetUIState(UIState.Initial);
                        HandleTimeout("");
                        break;
                    case UIState.SelectingAction:
                        SetUIState(UIState.Initial);
                        HandleTimeout("");
                        break;
                    case UIState.SelectingSong:
                        
                        SetUIState(UIState.Initial);
                        HandleTimeout("");
                        break;
                    case UIState.SelectingArtist:
                        SetUIState(UIState.Initial);
                        HandleTimeout("");
                        break;
                    case UIState.PlayHistory:
                        SetUIState(UIState.Initial);
                        HandleTimeout("");
                        break;
                }
            }
        }

        private async Task HandleTimeout(string message)
        {
            Console.WriteLine("HandleTimeout called with message: " + message);
            SetUIState(UIState.Initial);
            DisplayMessage(message, 2000);
            await Task.Delay(2000); 
        }

        private void DisplayMessage(string message, int duration)
        {   
            MainForm.nextSongLabel.Visible = false;
            displayLabel.Text = message;
            unifiedTimer.Interval = duration;
            unifiedTimer.Start();
        }
        
        private void LoadBackgroundImage()
        {
            // try
            // {
            //     backgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, "themes\\superstar\\images.jpg"));
            // }
            // catch (Exception ex)
            // {
            //     Console.WriteLine("Error loading background image: " + ex.Message);
            //     backgroundImage = null;
            // }
        }

        private void ConfigureImageDisplay()
        {
            try
            {
                firstStickerImage = Image.FromFile(Path.Combine(Application.StartupPath, "superstar-pic/1-1.png"));
                firstStickerXPos = this.Width / 2;
                imageYPos = (screenHeight / 3) - firstStickerImage.Height / 6;

                
                LoadBackgroundImage();
                
                
                stickerTimer1.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading initial sticker image: " + ex.Message);
                firstStickerImage = null; 
            }
        }

        private void HideImages()
        {
            bool anyStickersActive = false;

            lock (imageLock)
            {
                
                if (firstStickerImage != null)
                {
                    firstStickerImage = null;
                    stickerTimer1.Stop();
                    anyStickersActive = true;  
                }

                
                if (secondStickerImage != null)
                {
                    secondStickerImage = null;
                    stickerTimer2.Stop();
                    anyStickersActive = true;  
                }

                
                if (!anyStickersActive)  
                {
                    
                    backgroundImage = null;
                }

                this.Invalidate();  
            }
        }

        public void UpdateSongDisplayLabel(string newText)
        {
            
            songDisplayLabel.Text = newText;

            
            songDisplayLabel.Font = new Font("Arial", 125);

            songDisplayLabel.ForeColor = Color.White;

            
            songDisplayTimer.Stop();

            
            songDisplayTimer.Start();

            
            this.Invalidate();
        }


        private void SecondLineTimer_Tick(object sender, EventArgs e)
        {
            if ((DateTime.Now - secondLineStartTime).TotalMilliseconds >= secondLineDuration)
            {
                marqueeTextSecondLine = "";
                secondLineTimer.Stop();
            }
        }

        public void ResetMarqueeTextToWelcomeMessage()
        {
            try
            {
                string filePath = Path.Combine(Application.StartupPath, "WelcomeMessage.txt");
                string welcomeMessage = File.ReadAllText(filePath);
                this.UpdateMarqueeText(welcomeMessage, MarqueeStartPosition.Right, Color.White);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading marquee text from file: " + ex.Message);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            blackBackgroundPanel.SendToBack();

            base.OnPaint(e);

            using (Font largeFont = new Font("微軟正黑體", 34, FontStyle.Bold))
            using (Brush whiteBrush = new SolidBrush(Color.White))
            using (Brush limeGreenBrush = new SolidBrush(Color.LimeGreen))
            using (Brush marqueeBrush = new SolidBrush(marqueeTextColor)) 
            using (Brush backgroundBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0))) 
            {
                SizeF textSize = e.Graphics.MeasureString(marqueeText, largeFont);

                float yPosition1 = 10;  
                float yPosition2 = 55; 
                float yPosition3 = 100; 

                e.Graphics.FillRectangle(backgroundBrush, marqueeXPos, yPosition1, textSize.Width, textSize.Height);
                e.Graphics.DrawString(marqueeText, largeFont, marqueeBrush, new PointF(marqueeXPos, yPosition1));
                Rectangle clipRect = new Rectangle((int)(this.Width / 8), (int)yPosition2, (int)(3 * this.Width / 4), (int)textSize.Height);
                Region originalClip = e.Graphics.Clip;
                e.Graphics.SetClip(clipRect);

                SizeF textSizeSecondLine = e.Graphics.MeasureString(marqueeTextSecondLine, largeFont);
                float centeredXPos = (this.Width - textSizeSecondLine.Width) / 2;

                e.Graphics.FillRectangle(backgroundBrush, centeredXPos, yPosition2, textSizeSecondLine.Width, textSizeSecondLine.Height);
                e.Graphics.DrawString(marqueeTextSecondLine, largeFont, limeGreenBrush, new PointF(centeredXPos, yPosition2));

                e.Graphics.Clip = originalClip;

                if (string.IsNullOrEmpty(marqueeTextSecondLine))
                {
                    SizeF textSizeThirdLine = e.Graphics.MeasureString(marqueeTextThirdLine, largeFont);
                    e.Graphics.FillRectangle(backgroundBrush, marqueeXPosThirdLine, yPosition2, textSizeThirdLine.Width, textSizeThirdLine.Height);
                    e.Graphics.DrawString(marqueeTextThirdLine, largeFont, whiteBrush, new PointF(marqueeXPosThirdLine, yPosition2));
                }
                else
                {
                    SizeF textSizeThirdLine = e.Graphics.MeasureString(marqueeTextThirdLine, largeFont);
                    e.Graphics.FillRectangle(backgroundBrush, marqueeXPosThirdLine, yPosition3, textSizeThirdLine.Width, textSizeThirdLine.Height);
                    e.Graphics.DrawString(marqueeTextThirdLine, largeFont, whiteBrush, new PointF(marqueeXPosThirdLine, yPosition3));
                }
            }

            lock (imageLock)
            {
                if (backgroundImage != null)
                {
                    e.Graphics.DrawImage(backgroundImage, new Rectangle(25, 100, this.Width - 50, (int)(this.Height * 2 / 3) - 100));
                }
                if (firstStickerImage != null)
                {
                    e.Graphics.DrawImage(firstStickerImage, firstStickerXPos, imageYPos);
                }
                if (secondStickerImage != null)
                {
                    e.Graphics.DrawImage(secondStickerImage, secondStickerXPos, imageYPos);
                }
                if (showQRCode && qrCodeImage != null)
                {
                    int qrCodeSize = screenHeight / 6; 
                    int qrCodeX = 10; 
                    int qrCodeY = this.Height - qrCodeSize - 20; 

                    Rectangle qrCodeRect = new Rectangle(qrCodeX, qrCodeY, qrCodeSize, qrCodeSize);

                    e.Graphics.DrawImage(qrCodeImage, qrCodeRect);
                }
            }

                // 確保跑馬燈第二行在最前
            // blackBackgroundPanel.SendToBack();

        }


        
        public void DisplayBarrage(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new System.Action(() => DisplayBarrage(text)));  
                return;
            }

            
            Random rnd = new Random();

            
            for (int i = 0; i < 30; i++) 
            {
                Label lblBarrage = new Label
                {
                    Text = text, 
                    AutoSize = true, 
                    ForeColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256)), 
                    Font = new Font("Arial", rnd.Next(10, 50)), 
                    Location = new Point(rnd.Next(0, this.Width), rnd.Next(0, this.Height)) 
                };

                
                this.Controls.Add(lblBarrage);
                lblBarrage.BringToFront(); 

                
                System.Windows.Forms.Timer moveTimer = new System.Windows.Forms.Timer { Interval = 50 }; 
                moveTimer.Tick += (sender, e) =>
                {
                    lblBarrage.Left -= 5; 

                    if (lblBarrage.Right < 0) 
                    {
                        lblBarrage.Dispose();
                        moveTimer.Dispose();
                    }
                };
                moveTimer.Start();

                
                int duration = rnd.Next(3000, 7000); 
                System.Windows.Forms.Timer durationTimer = new System.Windows.Forms.Timer { Interval = duration };
                durationTimer.Tick += (sender, e) =>
                {
                    if (moveTimer.Enabled)
                    {
                        moveTimer.Stop(); 
                        moveTimer.Dispose();
                    }

                    this.Controls.Remove(lblBarrage);
                    lblBarrage.Dispose();
                    durationTimer.Stop(); 
                    durationTimer.Dispose();
                };
                durationTimer.Start();
            }
        }

        public void DisplaySticker(string stickerId)
        {
            Console.WriteLine("Attempting to display sticker.");
            this.Invoke((MethodInvoker)delegate {
                
                Console.WriteLine("Form Width: " + this.Width);
                Console.WriteLine("Form Height: " + this.Height);

                
                string imagePath = String.Format("{0}\\superstar-pic\\{1}.png", Application.StartupPath, stickerId);
                Console.WriteLine("Image path: " + imagePath);
                try
                {
                    Image newSticker = Image.FromFile(imagePath);
                    lock (imageLock)
                    {
                        if (firstStickerImage == null)
                        {
                            firstStickerImage = newSticker;
                            firstStickerXPos = this.Width / 2 - firstStickerImage.Width / 2;
                            LoadBackgroundImage();  
                            stickerTimer1.Start();  
                        }
                        else if (secondStickerImage == null)
                        {
                            firstStickerXPos = this.Width * 3 / 10f - firstStickerImage.Width / 8;
                            secondStickerImage = newSticker;
                            secondStickerXPos = this.Width * 7 / 10f - secondStickerImage.Width / 8;
                            
                            backgroundImage = null;
                            stickerTimer2.Start();  
                        }
                        else
                        {
                            
                            firstStickerImage = secondStickerImage;
                            firstStickerXPos = this.Width * 3 / 10f - firstStickerImage.Width / 8;
                            
                            secondStickerImage = newSticker;
                            secondStickerXPos = this.Width * 7 / 10f - secondStickerImage.Width / 8;
                            stickerTimer2.Start();  
                        }
                    }
                    this.Invalidate(); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading sticker image: " + ex.Message);
                }
            });
        }

        
        public static string ReadSongNumber()
        {
            
            string songNumber = MainForm.displayLabel.Text; 
            MainForm.nextSongLabel.Visible = false;
            return songNumber; 
        }

        private List<SongData> LanguageSongList;

        
        public enum UIState
        {
            Initial,
            SelectingLanguage,
            SelectingSong,
            SelectingAction, 
            PlayingSong,
            SelectingArtistCategory,   
            SelectingStrokeCount, 
            SelectingArtist,
            PlayHistory 
        }

        public enum Category
        {
            NewSongs, 
            HotSongs, 
            Artists   
            
        }

        
        public static UIState CurrentUIState = UIState.Initial;
        private string currentLanguage = "";  
        
        public static Category CurrentCategory { get; set; }
        private SongData selectedSong;  

        public static void SetUIState(UIState newState)
        {
            CurrentUIState = newState;
            
            displayTimer.Stop();

            
            switch (newState)
            {
                case UIState.Initial:
                    
                    foreach (var control in MainForm.Controls.OfType<Control>().ToArray())
                    {
                        if (control != MainForm.displayLabel &&
                            control != MainForm.pauseLabel &&
                            control != MainForm.muteLabel &&
                            control != MainForm.volumeUpLabel &&
                            control != MainForm.volumeDownLabel &&
                            control != MainForm.micUpLabel &&
                            control != MainForm.micDownLabel &&
                            control != MainForm.standardKeyLabel &&
                            control != MainForm.keyUpLabel &&
                            control != MainForm.keyDownLabel &&
                            control != MainForm.maleKeyLabel &&
                            control != MainForm.femaleKeyLabel &&
                            control != MainForm.squareLabel &&
                            control != MainForm.professionalLabel &&
                            control != MainForm.standardLabel &&
                            control != MainForm.singDownLabel &&
                            control != MainForm.brightLabel &&
                            control != MainForm.softLabel &&
                            control != MainForm.autoLabel &&
                            control != MainForm.romanticLabel &&
                            control != MainForm.dynamicLabel &&
                            control != MainForm.tintLabel &&
                            control != MainForm.blackBackgroundPanel &&
                            control != MainForm.nextSongLabel) 
                        {
                            MainForm.Controls.Remove(control);
                            control.Dispose();
                        }
                    }

                    MainForm.displayLabel.Text = ""; 
                    CommandHandler.readyForSongListInput = false;
                    unifiedTimer.Stop();
                    break;
                case UIState.SelectingLanguage:
                    unifiedTimer.Interval = 10000;
                    unifiedTimer.Enabled = true;
                    unifiedTimer.Start();
                    break;
                case UIState.SelectingSong:
                    unifiedTimer.Interval = 10000;
                    unifiedTimer.Enabled = true;
                    unifiedTimer.Start();
                    break;
                case UIState.SelectingArtistCategory:
                    unifiedTimer.Interval = 10000;
                    unifiedTimer.Enabled = true;
                    unifiedTimer.Start();
                    break;
                case UIState.SelectingStrokeCount:
                    unifiedTimer.Interval = 10000;
                    unifiedTimer.Enabled = true;
                    unifiedTimer.Start();
                    break;
                case UIState.SelectingArtist:
                    unifiedTimer.Interval = 10000;
                    unifiedTimer.Enabled = true;
                    unifiedTimer.Start();
                    break;
                case UIState.PlayHistory:
                    unifiedTimer.Interval = 10000;
                    unifiedTimer.Enabled = true;
                    unifiedTimer.Start();
                    break;
                default:
                    break;
            }
        }

        int number;
        int inputNumber;

        public void OnUserInput(string input)
        {
            bool isNumber = int.TryParse(input, out number); 

            if (isNumber)
            {
                if (CurrentCategory == Category.NewSongs)
                {
                    switch (CurrentUIState)
                    {
                        case UIState.SelectingLanguage:
                            HandleLanguageSelection(number);
                            break;

                        case UIState.SelectingSong:
                            HandleSongSelection(number);
                            break;

                        default:
                            displayLabel.Text = "無效的狀態";
                            displayLabel.BringToFront();
                            break;
                    }
                }
                else if (CurrentCategory == Category.HotSongs)
                {
                    switch (CurrentUIState)
                    {
                        case UIState.SelectingLanguage:
                            HandleLanguageSelection(number);
                            break;

                        case UIState.SelectingSong:
                            HandleSongSelection(number);
                            break;

                        default:
                            displayLabel.Text = "無效的狀態";
                            displayLabel.BringToFront();
                            break;
                    }
                }
                else if (CurrentCategory == Category.Artists)
                {
                    switch (CurrentUIState)
                    {
                        case UIState.SelectingArtistCategory:
                            ProcessArtistCategorySelection(number);
                            break;

                        case UIState.SelectingStrokeCount:
                            ProcessStrokeCountSelection(number);
                            break;

                        case UIState.SelectingArtist:
                            HandleArtistSelection(number);
                            break;

                        case UIState.SelectingSong:
                            HandleSongSelection(number);
                            break;

                        default:
                            displayLabel.Text = "無效的狀態";
                            displayLabel.BringToFront();
                            break;
                    }
                }
            }
            else if (input == "a")
            {
                try
                {
                    if (CurrentUIState == UIState.SelectingSong)
                    {
                        Console.WriteLine("Current State is SelectingSong, ready to process song selection.");
                        Console.WriteLine("Number: " + inputNumber);
                        int songIndex = (currentPage - 1) * songsPerPage + (inputNumber - 1);
                        Console.WriteLine("Calculated Song Index: " + songIndex + ", Total Songs: " + totalSongs);

                        if (songIndex >= 0 && songIndex < totalSongs)
                        {
                            selectedSong = LanguageSongList[songIndex];  
                            Console.WriteLine("Adding song to playlist: " + LanguageSongList[songIndex].Song);

                            
                            // DisplayActionWithSong(currentPage, songIndex, "點播");
                            
                            
                            AddSongToPlaylist(selectedSong);
                        }
                        else
                        {
                            Console.WriteLine("Song index out of range.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine("An error occurred while processing input 'a': " + ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }
            else if (input == "b")
            {
                if (CurrentUIState == UIState.SelectingSong)
                {
                    Console.WriteLine("Current State is SelectingSong, ready to process song selection.");
                    int songIndex = (currentPage - 1) * songsPerPage + (inputNumber - 1);
                    Console.WriteLine("Calculated Song Index: " + songIndex + ", Total Songs: " + totalSongs);

                    if (songIndex < totalSongs)
                    {
                        selectedSong = LanguageSongList[songIndex];  
                        Console.WriteLine("Adding song to playlist: " + LanguageSongList[songIndex].Song);

                        
                        // DisplayActionWithSong(currentPage, songIndex, "插播");
                        
                        
                        InsertSongToPlaylist(selectedSong);
                    }
                    else
                    {
                        Console.WriteLine("Song index out of range.");
                    }
                }
            }
        }

        private void HandleLanguageSelection(int number)
        {
            switch (number)
            {
                case 1:
                    currentLanguage = "國語";
                    break;
                case 2:
                    currentLanguage = "台語";
                    break;
                case 3:
                    currentLanguage = "粵語";
                    break;
                case 4:
                    currentLanguage = "英文";
                    break;
                case 5:
                    currentLanguage = "日語";
                    break;
                case 6:
                    currentLanguage = "韓語";
                    break;
                default:
                    displayLabel.Text = "輸入錯誤!!!";
                    displayLabel.BringToFront();
                    OverlayForm.displayTimer.Start();
                    return;
            }
            Console.WriteLine("Language selected: " + currentLanguage);
            DisplaySongsInLanguage(currentLanguage, CurrentCategory);
            CurrentUIState = UIState.SelectingSong;
            Console.WriteLine("State changed to SelectingSong");
        }

        private Artist selectedArtist;

        private void HandleArtistSelection(int number)
        {
            int artistIndex = (currentPage - 1) * artistsPerPage + (number - 1);
            inputNumber = number;
            if (artistIndex < totalArtists)
            {
                selectedArtist = currentArtistList[artistIndex];
                currentLanguage = selectedArtist.Name; 
                SetUIState(UIState.SelectingSong); 
                LanguageSongList = SongListManager.Instance.GetSongsByArtist(selectedArtist.Name);
                currentPage = 1;
                totalSongs = LanguageSongList.Count;
                DisplaySongs(currentPage);
            }
            else
            {
                Console.WriteLine("Song index out of range.");
            }
        }

        private void HandleSongSelection(int number)
        {
            Console.WriteLine("Current State is SelectingSong, ready to process song selection.");
            int songIndex = (currentPage - 1) * songsPerPage + (number - 1);
            inputNumber = number;
            Console.WriteLine("Calculated Song Index: " + songIndex + ", Total Songs: " + totalSongs);

            if (songIndex < totalSongs)
            {
                selectedSong = LanguageSongList[songIndex];  
                Console.WriteLine("Adding song to playlist: " + LanguageSongList[songIndex].Song);

                
                DisplaySongsWithArrows(currentPage, songIndex);
                
                
            }
            else
            {
                Console.WriteLine("Song index out of range.");
            }
        }

        private string currentArtistCategory;

        private void ProcessArtistCategorySelection(int number)
        {
            switch (number)
            {
                case 1:
                    currentArtistCategory = "男";
                    break;
                case 2:
                    currentArtistCategory = "女";
                    break;
                case 3:
                    currentArtistCategory = "團";
                    break;
                case 4:
                    currentArtistCategory = "外";
                    break;
                case 5:
                    currentArtistCategory = "全部";
                    break;
                default:
                    Console.WriteLine("Invalid selection");
                    return;
            }

            ClearDisplay();
            DisplayStrokeCountOptions();
        }

        private void ClearDisplay()
        {
            
            
            foreach (var control in this.Controls.OfType<Control>().ToArray())
            {
                if (control != displayLabel &&
                    control != pauseLabel &&
                    control != muteLabel &&
                    control != volumeUpLabel &&
                    control != volumeDownLabel &&
                    control != micUpLabel &&
                    control != micDownLabel &&
                    control != standardKeyLabel &&
                    control != keyUpLabel &&
                    control != keyDownLabel &&
                    control != maleKeyLabel &&
                    control != femaleKeyLabel &&
                    control != squareLabel &&
                    control != professionalLabel &&
                    control != standardLabel &&
                    control != singDownLabel &&
                    control != brightLabel &&
                    control != softLabel &&
                    control != autoLabel &&
                    control != romanticLabel &&
                    control != dynamicLabel &&
                    control != tintLabel &&
                    control != blackBackgroundPanel &&
                    control != nextSongLabel) 
                {
                    this.Controls.Remove(control);
                    control.Dispose();
                }
            }
        }
public void UpdateHistoryLabel(List<SongData> historySongs, List<PlayState> playStates, int currentPage, int totalPages)
{
    this.Controls.OfType<PictureBox>().ToList().ForEach(p => this.Controls.Remove(p));
    string headerText = $"已點歌曲 ({currentPage} / {totalPages})";
    Font headerFont = new Font("Microsoft JhengHei", 50, FontStyle.Bold);
    Bitmap headerBitmap = GenerateTextImage(headerText, headerFont, Color.White, Color.Transparent);
    AddCenteredPicture(headerBitmap, 150);

    int startY = 230;
    int verticalSpacing = 6;
    int leftMargin = 100 ;
    int rightMargin = this.Width - 100;
    for (int i = 0; i < historySongs.Count; i++)
    {
        var song = historySongs[i];
        var state = playStates[i];
        string status;
        Color textColor;

        switch (state)
        {
            case PlayState.Played:
                status = "(播畢)";
                textColor = Color.FromArgb(200, 75, 125); // RGB: (200, 75, 125)
                break;
            case PlayState.Playing:
                status = "(播放中)";
                textColor = Color.LimeGreen;
                break;
            case PlayState.NotPlayed:
                status = "";
                textColor = Color.White;
                break;
            default:
                status = "";
                textColor = Color.White;
                break;
        }
        string songText = $"{i + 1}. {song.Song}";
        Font songFont = new Font("Microsoft JhengHei", 50, FontStyle.Bold);
        Bitmap songBitmap = GenerateTextImage(songText, songFont, textColor, Color.Transparent);
        Bitmap statusBitmap = null;
        if (!string.IsNullOrEmpty(status))
        {
            Font statusFont = new Font("Microsoft JhengHei", 50, FontStyle.Bold);
            statusBitmap = GenerateTextImage(status, statusFont, textColor, Color.Transparent);
        }

        int y = startY + i * (songBitmap.Height + verticalSpacing);
        AddPicture(songBitmap, leftMargin, y);
        if (statusBitmap != null)
        {
            int statusX = rightMargin - statusBitmap.Width;
            AddPicture(statusBitmap, statusX, y);
        }
    }
}



        public void UpdateDisplayLabels(string[] messages)//新歌歌星排行首頁
        {
            // 清除舊的圖片控件
            this.Controls.OfType<PictureBox>().ToList().ForEach(p => this.Controls.Remove(p));

            if (messages.Length == 0) return;

            int mainTitleFontSize = 60; 
            int optionFontSize = 50; 
            int lineSpacing = 15; 
            int columnSpacing = 400;

            // 主標題
            string mainTitle = messages[0];
            Font mainTitleFont = new Font("Microsoft JhengHei", mainTitleFontSize, FontStyle.Bold);
            Bitmap mainTitleBitmap = GenerateTextImage(mainTitle, mainTitleFont, Color.White, Color.Transparent);
            int startY = 130;
            AddCenteredPicture(mainTitleBitmap, startY);
            startY += mainTitleBitmap.Height + lineSpacing;

            // 選項
            string[] options = messages.Skip(1).ToArray();
            int optionsStartY = startY;
            int maxItemsPerColumn = (int)Math.Ceiling(options.Length / 2.0);

            int leftColumnX = 200; 
            int rightColumnX = this.Width / 2 + 150;

            for (int i = 0; i < options.Length; i++)
            {
                Font optionFont = new Font("Microsoft JhengHei", optionFontSize, FontStyle.Bold);
                Bitmap optionBitmap = GenerateTextImage(options[i], optionFont, Color.White, Color.Transparent);

                int x = (i < maxItemsPerColumn) ? leftColumnX : rightColumnX;
                int currentY = optionsStartY + ((i % maxItemsPerColumn) * (optionBitmap.Height + lineSpacing));

                AddPicture(optionBitmap, x, currentY);
            }
        }

        private string strokeRange; 
        private int totalArtists = 0;
        private const int artistsPerPage = 10;
        private List<Artist> currentArtistList = new List<Artist>();

        private void ProcessStrokeCountSelection(int number)
        {
            List<Artist> selectedArtists = null;
            switch (number)
            {
                case 1:
                    selectedArtists = ArtistManager.Instance.GetArtistsByCategoryAndStrokeCountRange(currentArtistCategory ,0, 3);
                    strokeRange = "00~03";
                    break;
                case 2:
                    selectedArtists = ArtistManager.Instance.GetArtistsByCategoryAndStrokeCountRange(currentArtistCategory, 4, 7);
                    strokeRange = "04~07";
                    break;
                case 3:
                    selectedArtists = ArtistManager.Instance.GetArtistsByCategoryAndStrokeCountRange(currentArtistCategory, 8, 11);
                    strokeRange = "08~11";
                    break;
                case 4:
                    selectedArtists = ArtistManager.Instance.GetArtistsByCategoryAndStrokeCountRange(currentArtistCategory, 12, 15);
                    strokeRange = "12~15";
                    break;
                case 5:
                    selectedArtists = ArtistManager.Instance.GetArtistsByCategoryAndStrokeCountRange(currentArtistCategory, 16, int.MaxValue);
                    strokeRange = "16以上";
                    break;
                default:
                    Console.WriteLine("Invalid selection");
                    return;
            }

            if (selectedArtists != null && selectedArtists.Count > 0)
            {
                SetUIState(OverlayForm.UIState.SelectingArtist); 
                DisplayArtists(selectedArtists, currentPage);
            }
            else
            {
                Console.WriteLine("No artists found for the selected stroke count range.");
            }
        }

private void DisplayArtists(List<Artist> artists, int page)//歌星點進去後的畫面
{
    currentArtistList = artists;
    totalArtists = artists.Count;

    if (artists == null || artists.Count == 0)
    {
        Console.WriteLine("Artist list is null or empty.");
        return;
    }

    this.Controls.OfType<PictureBox>().ToList().ForEach(p => this.Controls.Remove(p));

    int artistsPerColumn = 5;
    int startIndex = (page - 1) * artistsPerPage;
    int endIndex = Math.Min(startIndex + artistsPerPage, artists.Count);

    int totalPages = (int)Math.Ceiling((double)artists.Count / artistsPerPage);

    string categoryDisplayText = currentArtistCategory switch
    {
        "男" => "男歌星",
        "女" => "女歌星",
        "團" => "團體",
        "外" => "外語",
        "全部" => "全部",
        _ => currentArtistCategory
    };

    string headerText = String.Format("{0} -- {1} ({2} / {3})", categoryDisplayText, strokeRange, page, totalPages);
    Font headerFont = new Font("Microsoft JhengHei", 50, FontStyle.Bold);
    Bitmap headerBitmap = GenerateTextImage(headerText, headerFont, Color.White, Color.Transparent);
    AddCenteredPicture(headerBitmap, 150);

    int startY = 250;
    int verticalSpacing = 15;
    int leftColumnX = 200; // 左邊列的起始 X 偏移量
    int rightColumnX = this.Width / 2 + 150; // 右邊列的起始 X 偏移量

    for (int i = startIndex; i < endIndex; i++)
    {
        int artistNumber = i - startIndex + 1;
        string artistNumberText = $"{artistNumber}.";
        string artistNameText = artists[i].Name;

        Font numberFont = new Font("Microsoft JhengHei", 45 , FontStyle.Bold);
        Bitmap numberBitmap = GenerateTextImage(artistNumberText, numberFont, Color.White, Color.Transparent);

        Font nameFont = new Font("Microsoft JhengHei", 45, FontStyle.Bold);
        Bitmap nameBitmap = GenerateTextImage(artistNameText, nameFont, Color.White, Color.Transparent);

        int x = (i - startIndex) < artistsPerColumn ? leftColumnX : rightColumnX;
        int y = startY + ((i - startIndex) % artistsPerColumn) * (numberBitmap.Height + verticalSpacing);

        AddPicture(numberBitmap, x, y);
        AddPicture(nameBitmap, x + numberBitmap.Width, y);
    }
}

        private void DisplayStrokeCountOptions()
        {
            string categoryDisplayText;
            switch (currentArtistCategory)
            {
                case "男":
                    categoryDisplayText = "男歌星";
                    break;
                case "女":
                    categoryDisplayText = "女歌星";
                    break;
                case "團":
                    categoryDisplayText = "團體";
                    break;
                case "外":
                    categoryDisplayText = "外語";
                    break;
                case "全部":
                    categoryDisplayText = "全部";
                    break;
                default:
                    categoryDisplayText = currentArtistCategory; 
                    break;
            }
            string[] messages = new string[]
            {
                categoryDisplayText,
                "1. 0~3",
                "2. 4~7",
                "3. 8~11",
                "4. 12~15",
                "5. 16以上"
            };
            SetUIState(OverlayForm.UIState.SelectingStrokeCount);
            UpdateDisplayLabels(messages);
        }

private void DisplaySongsInLanguage(string language, Category category)
{
    Dictionary<string, List<SongData>> selectedSongList;

    // 確定類別
    if (category == Category.NewSongs)
    {
        selectedSongList = SongListManager.NewSongLists;
    }
    else if (category == Category.HotSongs)
    {
        selectedSongList = SongListManager.HotSongLists;
    }
    else
    {
        ClearDisplay();
        displayLabel.Text = "無效的類別";
        return;
    }

    if (!selectedSongList.TryGetValue(language, out var songsInLanguage) || songsInLanguage == null || songsInLanguage.Count == 0)
    {
        ClearDisplay();
        displayLabel.Text = $"{language} - 熱門中未找到歌曲";
        LanguageSongList = null;
        totalSongs = 0;
        currentPage = 0;
        return;
    }
    LanguageSongList = songsInLanguage;
    totalSongs = songsInLanguage.Count; 
    currentPage = 1; 

    ClearDisplay();
    DisplaySongs(currentPage);
}

        public void AddSongToPlaylist(SongData songData)
        {
            try
            {
                var filePath1 = songData.SongFilePathHost1;
                var filePath2 = songData.SongFilePathHost2;

                if (!File.Exists(filePath1) && !File.Exists(filePath2))
                {
                    PrimaryForm.WriteLog(String.Format("File not found on both hosts: {0} and {1}", filePath1, filePath2));
                }
                else
                {
                    var pathToPlay = File.Exists(filePath1) ? filePath1 : filePath2;

                    bool wasEmpty = PrimaryForm.userRequestedSongs.Count == 0;

                    PrimaryForm.userRequestedSongs.Add(songData);
                    PrimaryForm.playedSongsHistory.Add(songData);
                    PrimaryForm.playStates.Add(wasEmpty ? PlayState.Playing : PlayState.NotPlayed);

                    if (wasEmpty)
                    {
                        VideoPlayerForm.Instance.SetPlayingSongList(PrimaryForm.userRequestedSongs);
                        PrimaryForm.currentSongIndexInHistory += 1;
                    }
                    VideoPlayerForm.Instance.UpdateNextSongFromPlaylist();
                    PrimaryForm.PrintPlayingSongList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
            }
            // OverlayForm.MainForm.displayLabel.Text = String.Format("已點歌曲:{0}", selectedSong);
        }



        public void InsertSongToPlaylist(SongData songData)
        {
            try
            {
                var filePath1 = songData.SongFilePathHost1;
                var filePath2 = songData.SongFilePathHost2;

                if (!File.Exists(filePath1) && !File.Exists(filePath2))
                {
                    PrimaryForm.WriteLog(String.Format("File not found on both hosts: {0} and {1}", filePath1, filePath2));
                }
                else
                {
                    var pathToPlay = File.Exists(filePath1) ? filePath1 : filePath2;

                    bool wasEmpty = PrimaryForm.userRequestedSongs.Count == 0;

                    if (wasEmpty)
                    {
                        PrimaryForm.userRequestedSongs.Add(songData);
                        VideoPlayerForm.Instance.SetPlayingSongList(PrimaryForm.userRequestedSongs);
                        PrimaryForm.playedSongsHistory.Add(songData);
                        PrimaryForm.playStates.Add(PlayState.Playing);
                        PrimaryForm.currentSongIndexInHistory += 1;
                    }
                    else if (PrimaryForm.userRequestedSongs.Count == 1)
                    {
                        PrimaryForm.userRequestedSongs.Insert(1, songData);
                        PrimaryForm.playedSongsHistory.Insert(PrimaryForm.currentSongIndexInHistory + 1, songData);
                        PrimaryForm.playStates.Insert(PrimaryForm.currentSongIndexInHistory + 1, PlayState.NotPlayed);
                    }
                    else
                    {
                        PrimaryForm.userRequestedSongs.Insert(1, songData);
                        PrimaryForm.playedSongsHistory.Insert(PrimaryForm.currentSongIndexInHistory + 1, songData);
                        PrimaryForm.playStates.Insert(PrimaryForm.currentSongIndexInHistory + 1, PlayState.NotPlayed);
                    }

                    VideoPlayerForm.Instance.UpdateNextSongFromPlaylist();
                    PrimaryForm.PrintPlayingSongList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
            }
        }


        public int currentPage = 1;
        public int songsPerPage = 5;
        public int totalSongs = 0;  


public void DisplaySongs(int page)
{
    if (LanguageSongList == null || LanguageSongList.Count == 0)
    {
        Console.WriteLine("LanguageSongList is null or empty.");
        return;
    }

    this.Controls.OfType<PictureBox>().ToList().ForEach(p => this.Controls.Remove(p));

    int songsPerColumn = 5;
    int startIndex = (page - 1) * songsPerPage;
    int endIndex = Math.Min(startIndex + songsPerPage, LanguageSongList.Count);

    int totalPages = (int)Math.Ceiling((double)LanguageSongList.Count / songsPerPage);

    string categoryText = OverlayForm.CurrentCategory switch
    {
        OverlayForm.Category.NewSongs => "新歌",
        OverlayForm.Category.HotSongs => "熱門",
        _ => ""
    };
    string headerText = $"{currentLanguage} - {categoryText} ({page} / {totalPages})";
    Font headerFont = new Font("Microsoft JhengHei", 60, FontStyle.Bold);
    Bitmap headerBitmap = GenerateTextImage(headerText, headerFont, Color.White, Color.Transparent);
    AddCenteredPicture(headerBitmap, 150);

    int startY = 250;
    int leftColumnX = 100;
    int rightColumnX = this.Width / 2 + 100;

    // 計算當前頁面最大歌名和歌手文字長度
    int maxSongLength = 0;
    int maxArtistLength = 0;
    for (int i = startIndex; i < endIndex; i++)
    {
        string songText = $"{i - startIndex + 1}. {LanguageSongList[i].Song}";
        string artistText = !string.IsNullOrWhiteSpace(LanguageSongList[i].ArtistB)
            ? $"{LanguageSongList[i].ArtistA} - {LanguageSongList[i].ArtistB}"
            : LanguageSongList[i].ArtistA;

        maxSongLength = Math.Max(maxSongLength, songText.Length);
        maxArtistLength = Math.Max(maxArtistLength, artistText.Length);
    }

    int songFontSize = maxSongLength > 20 ? 35 : 45;
    int artistFontSize = maxArtistLength > 20 ? 30 : 35;
    int verticalSpacing = songFontSize == 30 ? 25 : 10;

    // 統一行高
    int rowHeight = 0;

    // 計算行高
    for (int i = startIndex; i < endIndex; i++)
    {
        string songText = $"{i - startIndex + 1}. {LanguageSongList[i].Song}";
        string artistText = !string.IsNullOrWhiteSpace(LanguageSongList[i].ArtistB)
            ? $"{LanguageSongList[i].ArtistA} - {LanguageSongList[i].ArtistB}"
            : LanguageSongList[i].ArtistA;

        Font songFont = new Font("Microsoft JhengHei", songFontSize, FontStyle.Bold);
        Font artistFont = new Font("Microsoft JhengHei", artistFontSize, FontStyle.Bold);

        Bitmap songBitmap = GenerateTextImage(songText, songFont, Color.White, Color.Transparent);
        Bitmap artistBitmap = GenerateTextImage(artistText, artistFont, Color.White, Color.Transparent);

        rowHeight = Math.Max(rowHeight, Math.Max(songBitmap.Height, artistBitmap.Height));
    }

    for (int i = startIndex; i < endIndex; i++)
    {
        int songNumber = i - startIndex + 1;

        string songText = $"{songNumber}. {LanguageSongList[i].Song}";
        string artistText = !string.IsNullOrWhiteSpace(LanguageSongList[i].ArtistB)
            ? $"{LanguageSongList[i].ArtistA} - {LanguageSongList[i].ArtistB}"
            : LanguageSongList[i].ArtistA;

        Font songFont = new Font("Microsoft JhengHei", songFontSize, FontStyle.Bold);
        Font artistFont = new Font("Microsoft JhengHei", artistFontSize, FontStyle.Bold);

        Bitmap songBitmap = GenerateTextImage(songText, songFont, Color.White, Color.Transparent);
        Bitmap artistBitmap = GenerateTextImage(artistText, artistFont, Color.White, Color.Transparent);

        int x = (i - startIndex) < songsPerColumn ? leftColumnX : rightColumnX;
        int y = startY + ((i - startIndex) % songsPerColumn) * (rowHeight + verticalSpacing);

        AddPicture(songBitmap, x, y);
        AddPicture(artistBitmap, x + songBitmap.Width + 20, y);
    }
}

public void DisplaySongsWithArrows(int page, int highlightIndex)
{
    if (LanguageSongList == null || LanguageSongList.Count == 0)
    {
        Console.WriteLine("Error: LanguageSongList is null or empty.");
        return;
    }

    this.Controls.OfType<PictureBox>().ToList().ForEach(p => this.Controls.Remove(p));

    int songsPerColumn = 5;
    int startIndex = (page - 1) * songsPerPage;
    int endIndex = Math.Min(startIndex + songsPerPage, LanguageSongList.Count);

    int totalPages = (int)Math.Ceiling((double)LanguageSongList.Count / songsPerPage);

    string categoryText = OverlayForm.CurrentCategory switch
    {
        OverlayForm.Category.NewSongs => "新歌",
        OverlayForm.Category.HotSongs => "熱門",
        _ => ""
    };

    string headerText = $"{currentLanguage} - {categoryText} ({page} / {totalPages})";
    Font headerFont = new Font("Microsoft JhengHei", 60, FontStyle.Bold);
    Bitmap headerBitmap = GenerateTextImage(headerText, headerFont, Color.White, Color.Transparent);
    AddCenteredPicture(headerBitmap, 150);

    int startY = 250;
    int leftColumnX = 100;
    int rightColumnX = this.Width / 2 + 100;

    // 找到当前页面中最长的 songText 和 artistText 长度
    int maxSongLength = 0;
    int maxArtistLength = 0;
    for (int i = startIndex; i < endIndex; i++)
    {
        string songText = $"{i - startIndex + 1}. {LanguageSongList[i].Song}";
        string artistText = !string.IsNullOrWhiteSpace(LanguageSongList[i].ArtistB)
            ? $"{LanguageSongList[i].ArtistA} - {LanguageSongList[i].ArtistB}"
            : LanguageSongList[i].ArtistA;

        maxSongLength = Math.Max(maxSongLength, songText.Length);
        maxArtistLength = Math.Max(maxArtistLength, artistText.Length);
    }

    // 动态调整字体大小
    int songFontSize = maxSongLength > 20 ? 35 : 45;
    int artistFontSize = maxArtistLength > 20 ? 30 : 35;
    int verticalSpacing = songFontSize == 30 ? 25 : 10;

    // 统一行高計算
    int rowHeight = 0;
    for (int i = startIndex; i < endIndex; i++)
    {
        string songText = $"{i - startIndex + 1}. {LanguageSongList[i].Song}";
        string artistText = !string.IsNullOrWhiteSpace(LanguageSongList[i].ArtistB)
            ? $"{LanguageSongList[i].ArtistA} - {LanguageSongList[i].ArtistB}"
            : LanguageSongList[i].ArtistA;

        Font tempSongFont = new Font("Microsoft JhengHei", songFontSize, FontStyle.Bold);
        Font tempArtistFont = new Font("Microsoft JhengHei", artistFontSize, FontStyle.Bold);

        Bitmap tempSongBitmap = GenerateTextImage(songText, tempSongFont, Color.White, Color.Transparent);
        Bitmap tempArtistBitmap = GenerateTextImage(artistText, tempArtistFont, Color.White, Color.Transparent);

        rowHeight = Math.Max(rowHeight, Math.Max(tempSongBitmap.Height, tempArtistBitmap.Height));
    }

    for (int i = startIndex; i < endIndex; i++)
    {
        int songNumber = i - startIndex + 1;

        string songText = $"{songNumber}. {LanguageSongList[i].Song}";
        string artistText = !string.IsNullOrWhiteSpace(LanguageSongList[i].ArtistB)
            ? $"{LanguageSongList[i].ArtistA} - {LanguageSongList[i].ArtistB}"
            : LanguageSongList[i].ArtistA;

        // 设置颜色，选中的索引显示为亮绿色
        Color songColor = (i == highlightIndex) ? Color.LimeGreen : Color.White;
        Color artistColor = (i == highlightIndex) ? Color.LimeGreen : Color.White;

        Font songFont = new Font("Microsoft JhengHei", songFontSize, FontStyle.Bold);
        Bitmap songBitmap = GenerateTextImage(songText, songFont, songColor, Color.Transparent);

        Font artistFont = new Font("Microsoft JhengHei", artistFontSize, FontStyle.Bold);
        Bitmap artistBitmap = GenerateTextImage(artistText, artistFont, artistColor, Color.Transparent);

        int x = (i - startIndex) < songsPerColumn ? leftColumnX : rightColumnX;
        int y = startY + ((i - startIndex) % songsPerColumn) * (rowHeight + verticalSpacing);

        AddPicture(songBitmap, x, y);
        AddPicture(artistBitmap, x + songBitmap.Width + 20, y);
    }
}



public void DisplayActionWithSong(int page, int songIndex, string actionType)
{
    // try
    // {
    //     if (LanguageSongList == null || LanguageSongList.Count == 0)
    //     {
    //         Console.WriteLine("Error: LanguageSongList is null or empty.");
    //         return;
    //     }

    //     SongData song = LanguageSongList[songIndex];

    //     this.Controls.OfType<PictureBox>().ToList().ForEach(p => this.Controls.Remove(p));

    //     int songsPerColumn = 5;
    //     int startIndex = (page - 1) * songsPerPage;
    //     int endIndex = Math.Min(startIndex + songsPerPage, LanguageSongList.Count);

    //     int totalPages = (int)Math.Ceiling((double)LanguageSongList.Count / songsPerPage);

    //     string headerText = $"{actionType}: {song.ArtistA} - {song.Song} ({page} / {totalPages})";
    //     Font headerFont = new Font("Microsoft JhengHei", 40, FontStyle.Bold);
    //     Color headerColor = actionType == "點播" ? Color.LimeGreen : Color.Yellow;
    //     Bitmap headerBitmap = GenerateTextImage(headerText, headerFont, headerColor, Color.Transparent);
    //     AddCenteredPicture(headerBitmap, 150);

    //     int startY = 250;
    //     int verticalSpacing = 10;
    //     int leftColumnX = 200;
    //     int rightColumnX = this.Width / 2 + 150;

    //     for (int i = startIndex; i < endIndex; i++)
    //     {
    //         int songNumber = i - startIndex + 1;
    //         string songText = $"{songNumber}. {LanguageSongList[i].Song}";
    //         string artistText = !string.IsNullOrWhiteSpace(LanguageSongList[i].ArtistB)
    //             ? $"{LanguageSongList[i].ArtistA} - {LanguageSongList[i].ArtistB}"
    //             : LanguageSongList[i].ArtistA;

    //         Font songFont = new Font("Microsoft JhengHei", 40, FontStyle.Bold);
    //         Bitmap songBitmap = GenerateTextImage(songText, songFont, Color.White, Color.Transparent);

    //         Font artistFont = new Font("Microsoft JhengHei", 30, FontStyle.Bold);
    //         Bitmap artistBitmap = GenerateTextImage(artistText, artistFont, Color.White, Color.Transparent);

    //         int x = (i - startIndex) < songsPerColumn ? leftColumnX : rightColumnX;
    //         int y = startY + ((i - startIndex) % songsPerColumn) * (songBitmap.Height + verticalSpacing);

    //         AddPicture(songBitmap, x, y);
    //         AddPicture(artistBitmap, x + songBitmap.Width + 20, y);
    //     }
    // }
    // catch (Exception ex)
    // {
    //     Console.WriteLine($"Error in DisplayActionWithSong: {ex.Message}");
    //     Console.WriteLine(ex.StackTrace);
    // }
}

        public void NextPage()
        {
            
            unifiedTimer.Stop();

            if (CurrentUIState == UIState.SelectingArtist)
            {
                if (currentPage * artistsPerPage < totalArtists)
                {
                    currentPage++;
                    DisplayArtists(currentArtistList, currentPage);
                }
            }
            else
            {                    
                if (currentPage * songsPerPage < totalSongs)
                {
                    currentPage++;
                    DisplaySongs(currentPage);
                }
            }

            
            unifiedTimer.Start();
        }

        public void PreviousPage()
        {
            
            unifiedTimer.Stop();

            if (CurrentUIState == UIState.SelectingArtist)
            {
                if (currentPage > 1)
                {
                    currentPage--;
                    DisplayArtists(currentArtistList, currentPage);
                }
            }
            else
            {   
                if (currentPage > 1)
                {
                    currentPage--;
                    DisplaySongs(currentPage);
                }
            }

            
            unifiedTimer.Start();
        }
    }
}