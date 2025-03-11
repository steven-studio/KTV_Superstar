using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using NAudio.Wave; 
using Microsoft.Ink; 
using System.Text.RegularExpressions; 
using WMPLib;

namespace DualScreenDemo
{
    public partial class PrimaryForm : Form
    {
        #region 防止閃屏
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }
        #endregion 

        
        public static PrimaryForm Instance { get; private set; }
        public bool isOnOrderedSongsPage = false;

        
        private ProgressBar progressBar;

        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private PictureBox pictureBox4;
        private PictureBox pictureBox5;
        private PictureBox pictureBox6;

        private const int offsetX = 100;

        private PictureBox pictureBoxArtistSearch;

        private Button[] numberButtonsArtistSearch;
        private Button modifyButtonArtistSearch, closeButtonArtistSearch;
        private RichTextBox inputBoxArtistSearch;
        private const int offsetXArtistSearch = 100; 
        private const int offsetYArtistSearch = 100; 

        private PictureBox pictureBoxWordCount;

        private Button[] numberButtonsWordCount;
        private Button modifyButtonWordCount, closeButtonWordCount;
        private RichTextBox inputBoxWordCount;
        private const int offsetXWordCount = 100;
        private const int offsetYWordCount = 100;

        private PictureBox pictureBoxSongIDSearch;

        private Button[] numberButtonsSongIDSearch;
        private Button modifyButtonSongIDSearch, closeButtonSongIDSearch;
        private RichTextBox inputBoxSongIDSearch;
        private const int offsetXSongID = 100;
        private const int offsetYSongID = 100;
        private const int offsetXPinYin = 100;

        private Button singerSearchButton;
        private Bitmap singerSearchNormalBackground;
        private Bitmap singerSearchActiveBackground;
        private Button songSearchButton;
        private Bitmap songSearchNormalBackground;
        private Bitmap songSearchActiveBackground;
        private Button serviceBellButton;
        private Button deliciousFoodButton;
        private Bitmap deliciousFoodNormalBackground;
        private Bitmap deliciousFoodActiveBackground;
        private Button mobileSongRequestButton;
        private Button qieGeButton;
        private Button musicUpButton;
        private Button musicDownButton;
        private Button micUpButton;
        private Button micDownButton;
        private Button originalSongButton;
        private Button replayButton;
        public Button pauseButton;
        public Button playButton;
        private Button muteButton;
        private Button maleKeyButton;
        private Button femaleKeyButton;
        private Button standardKeyButton;
        private Button soundEffectButton;

        private Button pitchUpButton;
        private Button pitchDownButton;
        private Button syncScreenButton;
        private Button toggleLightButton;

        private PictureBox promotionsPictureBox;

        private List<Image> promotions;
        private List<Image> menu;

        private PictureBox VodScreenPictureBox;

        private Panel overlayPanel;

        private Button btnPreviousPage;
        private Button btnReturn;
        private Button btnNextPage;
        private Button btnApplause;
        private Button btnSimplifiedChinese;
        private Button btnTraditionalChinese;
        private Button exitButton;
        
        private static Bitmap normalStateImage;
        private static Bitmap mouseOverImage;
        private static Bitmap mouseDownImage;

        
        private static Bitmap resizedNormalStateImage;
        private static Bitmap resizedMouseOverImage;
        private static Bitmap resizedMouseDownImage;

        private static Bitmap normalStateImageNewSongAlert;
        private static Bitmap mouseOverImageNewSongAlert;
        private static Bitmap mouseDownImageNewSongAlert;

        private static Bitmap resizedNormalStateImageForNewSongAlert;
        private static Bitmap resizedMouseOverImageForNewSongAlert;
        private static Bitmap resizedMouseDownImageForNewSongAlert;

        private static Bitmap normalStateImageArtistQuery;
        private static Bitmap mouseOverImageArtistQuery;
        private static Bitmap mouseDownImageArtistQuery;

        private static Bitmap resizedNormalStateImageForArtistQuery;
        private static Bitmap resizedMouseOverImageForArtistQuery;
        private static Bitmap resizedMouseDownImageForArtistQuery;

        private static Bitmap normalStateImageSongQuery;
        private static Bitmap mouseOverImageSongQuery;
        private static Bitmap mouseDownImageSongQuery;

        private static Bitmap resizedNormalStateImageForSongQuery;
        private static Bitmap resizedMouseOverImageForSongQuery;
        private static Bitmap resizedMouseDownImageForSongQuery;

        private static Bitmap normalStateImageLanguageQuery;
        private static Bitmap mouseOverImageLanguageQuery;
        private static Bitmap mouseDownImageLanguageQuery;

        private static Bitmap resizedNormalStateImageForLanguageQuery;
        private static Bitmap resizedMouseOverImageForLanguageQuery;
        private static Bitmap resizedMouseDownImageForLanguageQuery;

        private static Bitmap normalStateImage6_1;
        private static Bitmap mouseOverImage6_1;
        private static Bitmap mouseDownImage6_1;

        private static Bitmap resizedNormalStateImageFor6_1;
        private static Bitmap resizedMouseOverImageFor6_1;
        private static Bitmap resizedMouseDownImageFor6_1;

        private static Bitmap normalStateImageCategoryQuery;
        private static Bitmap mouseOverImageCategoryQuery;
        private static Bitmap mouseDownImageCategoryQuery;

        private static Bitmap resizedNormalStateImageForCategoryQuery;
        private static Bitmap resizedMouseOverImageForCategoryQuery;
        private static Bitmap resizedMouseDownImageForCategoryQuery;

        private static Bitmap normalStateImage7_1;
        private static Bitmap mouseOverImage7_1;
        private static Bitmap mouseDownImage7_1;

        private static Bitmap resizedNormalStateImageFor7_1;
        private static Bitmap resizedMouseOverImageFor7_1;
        private static Bitmap resizedMouseDownImageFor7_1;

        private static Bitmap normalStateImage7_1_1;
        private static Bitmap mouseOverImage7_1_1;
        private static Bitmap mouseDownImage7_1_1;

        private static Bitmap resizedNormalStateImageFor7_1_1;
        private static Bitmap resizedMouseOverImageFor7_1_1;
        private static Bitmap resizedMouseDownImageFor7_1_1;

        
        private static Bitmap normalStateImageForPromotionsAndMenu;
        private static Bitmap resizedNormalStateImageForPromotionsAndMenu;

        private static Bitmap normalStateImageForSyncScreen;
        private static Bitmap resizedNormalStateImageForSyncScreen;

        private static Bitmap normalStateImageForSceneSoundEffects;
        private static Bitmap resizedNormalStateImageForSceneSoundEffects;

        private static Bitmap normalStateImageForLightControl;
        private static Bitmap resizedNormalStateImageForLightControl;

        public VideoPlayerForm videoPlayerForm;

        
        public List<SongData> allSongs; 
        public List<Artist> allArtists;
        public List<SongData> currentSongList;
        public List<Artist> currentArtistList;
        public List<SongData> publicSongList;
        private List<SongData> guoYuSongs;
        private List<SongData> taiYuSongs;
        private List<SongData> yueYuSongs;
        private List<SongData> yingWenSongs;
        private List<SongData> riYuSongs;
        private List<SongData> hanYuSongs;
        private List<SongData> guoYuSongs2;
        private List<SongData> taiYuSongs2;
        private List<SongData> yueYuSongs2;
        private List<SongData> yingWenSongs2;
        private List<SongData> riYuSongs2;
        private List<SongData> hanYuSongs2;
        private List<SongData> loveDuetSongs;
        private List<SongData> talentShowSongs;
        private List<SongData> medleyDanceSongs;
        private List<SongData> ninetiesSongs;
        private List<SongData> nostalgicSongs;
        private List<SongData> chinaSongs;
        private List<SongData> vietnameseSongs;
        public static List<SongData> userRequestedSongs;
        public static List<SongData> playedSongsHistory;
        public static List<PlayState> playStates;
        public static int currentSongIndexInHistory = -1;
        public MultiPagePanel multiPagePanel;
        private List<Label> songLabels = new List<Label>();
        public int currentPage = 0;
        public int totalPages; 
        public const int itemsPerPage = 18;
        private const int RowsPerPage = 9;
        private const int Columns = 2;

        private WaveInEvent waveIn;
        private WaveFileWriter waveWriter;
        


        
        private const int PanelStartLeft = 25;    // 修改為實際需要的左邊距
        private const int PanelStartTop = 227;    // 修改為實際需要的上邊距
        private const int PanelEndLeft = 1175;    // 修改為實際需要的右邊界
        private const int PanelEndTop = 739;      // 修改為實際需要的下邊界

        private Timer lightControlTimer;
        public Timer volumeUpTimer;
        public Timer volumeDownTimer;
        private DateTime lastVolumeUpTime = DateTime.MinValue;
        private DateTime lastVolumeDownTime = DateTime.MinValue;
        public Timer micControlTimer;

        private SequenceManager sequenceManager = new SequenceManager();

        private PictureBox buttonMiddle;
        private PictureBox buttonTopRight;
        private PictureBox buttonTopLeft;
        private PictureBox buttonThanks;

        private Dictionary<Control, (Point Location, bool Visible)> initialControlStates = new Dictionary<Control, (Point Location, bool Visible)>();

        private Dictionary<Control, Point> initialControlPositions = new Dictionary<Control, Point>();

        private Panel sendOffPanel;
        private PictureBox sendOffPictureBox;

        private static Bitmap normalStateImageHotSong;
        private static Bitmap mouseDownImageHotSong;
        private static Bitmap resizedNormalStateImageForHotSong;
        private static Bitmap resizedMouseDownImageForHotSong;

        // 布局常量
        private const float LeftColumnX = 0.05f;    // 左列起始位置（屏幕宽度的5%）
        private const float RightColumnX = 0.55f;   // 右列起始位置（屏幕宽度的55%）
        private const float SongWidth = 0.4f;       // 歌名宽度（屏幕宽度的40%）
        private const float ArtistWidth = 0.1f;     // 歌手名宽度（屏幕宽度的10%）
        private const int ItemHeight = 64;          // 每个项目的高度
        private const int RowGap = 0;               // 行间距

        private PictureBox serviceBellPictureBox;  // 添加为类成员变量

        private Timer autoRefreshTimer;

        public PrimaryForm()
        {
            Instance = this;
            
            // 添加 DPI 感知支持
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }
            
            this.DoubleBuffered = true;
            
            InitializeComponent();
            InitializeProgressBar();

            // 初始化自动刷新Timer
            autoRefreshTimer = new Timer();
            autoRefreshTimer.Interval = 1000; // 1秒
            autoRefreshTimer.Tick += AutoRefreshTimer_Tick;
            
            // 設置窗體屬性
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            
            lightControlTimer = new Timer();
            lightControlTimer.Interval = 5; 
            lightControlTimer.Tick += LightControlTimer_Tick;
            volumeUpTimer = new Timer();
            volumeUpTimer.Interval = 100; 
            volumeUpTimer.Tick += VolumeUpTimer_Tick;
            volumeDownTimer = new Timer();
            volumeDownTimer.Interval = 100;
            volumeDownTimer.Tick += VolumeDownTimer_Tick;
            micControlTimer = new Timer();
            micControlTimer.Interval = 100;
            micControlTimer.Tick += MicControlTimer_Tick;
            
            InitializeRecording();
            InitializeMediaPlayer();
            LoadSongData();
            LoadImages(); 
            InitializeFormAndControls(); 
            InitializeMultiPagePanel();
            OverlayQRCodeOnImage(HttpServer.randomFolderPath);

            InitializeHandWritingForSingers();
            InitializeHandWritingForSongs();

            InitializeSendOffPanel();
            
            InitializePromotionsAndMenuPanel();
            SaveInitialControlStates(this);
            
            this.Paint += PrimaryForm_Paint;
            
            // 添加 Load 事件處理
            this.Load += PrimaryForm_Load;
        }

        // 添加 DPI 感知支持
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        private void SaveInitialControlStates(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                initialControlStates[control] = (control.Location, control.Visible);

                if (control.HasChildren)
                {
                    SaveInitialControlStates(control);
                }
            }
        }

        private void PrimaryForm_Paint(object sender, PaintEventArgs e)
        {
            
            string hostName = System.Net.Dns.GetHostName();

            
            string displayName = "包廂" + hostName.Substring(Math.Max(0, hostName.Length - 20));

            
            Font font = new Font("微軟正黑體", 24, FontStyle.Bold);
            Brush brush = new SolidBrush(Color.Red);

            
            PointF point = new PointF(500, 30);

            
            e.Graphics.DrawString(displayName, font, brush, point);
        }

        
        private void buttonMiddle_Click(object sender, EventArgs e)
        {
            sequenceManager.ProcessClick("巨");
        }

        private void buttonTopRight_Click(object sender, EventArgs e)
        {
            sequenceManager.ProcessClick("級");
        }

        private void buttonTopLeft_Click(object sender, EventArgs e)
        {
            sequenceManager.ProcessClick("超");
        }

        private void buttonThanks_Click(object sender, EventArgs e)
        {
            sequenceManager.ProcessClick("星");
        }

        public void ShowSendOffScreen()
        {
            sendOffPanel.BringToFront();
            sendOffPanel.Visible = true;
            
            // 確保按鈕可見並在最上層
            buttonMiddle.Visible = true;
            buttonTopRight.Visible = true;
            buttonTopLeft.Visible = true;
            buttonThanks.Visible = true;

            buttonMiddle.BringToFront();
            buttonTopRight.BringToFront();
            buttonTopLeft.BringToFront();
            buttonThanks.BringToFront();
        }

        public void HideSendOffScreen()
        {
            sendOffPanel.Visible = false;
        }

        private void HideAllButtons()
        {
            HideControlsRecursively(this); 
        }

        private void HideControlsRecursively(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is Button) 
                {
                    control.Visible = false; 
                }
                else if (control.HasChildren) 
                {
                    HideControlsRecursively(control); 
                }
            }
        }

        private void UpdateProgress(TimeSpan currentPosition)
        {
            
            if (progressBar.InvokeRequired) {
                progressBar.Invoke(new System.Action(() => {
                    progressBar.Value = (int)currentPosition.TotalSeconds;
                }));
            } else {
                progressBar.Value = (int)currentPosition.TotalSeconds;
            }
        }

        private void InitializeComponent()
        {

        }

        private void InitializeProgressBar()
        {
            
            progressBar = new ProgressBar();

            
            progressBar.Location = new System.Drawing.Point(10, 10); 
            progressBar.Size = new System.Drawing.Size(300, 30); 

            
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            
            this.Controls.Add(progressBar);
        }

        private void EnableDoubleBuffering(Control control)
        {
            if (control != null)
            {
                var doubleBufferedProperty = control.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (doubleBufferedProperty != null)
                {
                    doubleBufferedProperty.SetValue(control, true, null);
                }
            }
        }

        private void InitializeRecording()
        {
            
            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                var caps = WaveIn.GetCapabilities(n);
                Console.WriteLine(String.Format("{0}: {1}", n, caps.ProductName));
            }

            waveIn = new WaveInEvent();
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.RecordingStopped += OnRecordingStopped; 
            waveIn.WaveFormat = new WaveFormat(44100, 1); 
            
            
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveWriter != null)
            {
                waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
                waveWriter.Flush();
            }
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            if (waveIn != null)
            {
                waveIn.Dispose();
                waveIn = null;
            }

            if (waveWriter != null)
            {
                waveWriter.Dispose();
                waveWriter = null;
            }
        }

        private void InitializeFormAndControls()
        {
            this.SuspendLayout();
            
            // 獲取螢幕尺寸
            Screen screen = Screen.PrimaryScreen;
            int screenWidth = screen.Bounds.Width;
            int screenHeight = screen.Bounds.Height;
            
            // 設置窗體大小
            this.Size = new Size(screenWidth, screenHeight);
            this.Location = new Point(0, 0);
            
            // 調整背景圖片
            string selectedTheme = ReadSelectedThemePath();
            if (!string.IsNullOrEmpty(selectedTheme))
            {
                string backgroundImagePath = Path.Combine(Application.StartupPath, "themes\\superstar\\555009.jpg");
                try
                {
                    using (Image originalImage = Image.FromFile(backgroundImagePath))
                    {
                        this.BackgroundImage = new Bitmap(originalImage);
                        this.BackgroundImageLayout = ImageLayout.Stretch;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加載背景圖片時發生錯誤: {ex.Message}");
                }
            }

            InitializePictureBox();

            
            InitializeButtonsForHotSong(); 
            InitializeButtonsForNewSong(); 
            InitializeButtonsForSingerSearch();
            InitializeButtonsForSongSearch();
            InitializeButtonsForPictureBoxLanguageQuery();
            InitializeButtonsForGroupPictureBox();
            InitializeCategorySearchButtons();
            InitializeButtonsForZhuYinSingers();
            InitializeButtonsForZhuYinSongs();
            InitializeButtonsForEnglishSingers();
            InitializeButtonsForEnglishSongs();
            InitializeButtonsForPictureBoxArtistSearch();
            InitializeButtonsForPictureBoxWordCount();
            InitializeButtonsForPinYinSingers();
            InitializeButtonsForPinYinSongs();
            InitializeButtonsForPictureBoxSongIDSearch();
            InitializeButtonsForFavoritePictureBox();
            InitializePromotionsButton();
            InitializeButtonsForPictureBoxToggleLight();
            InitializeButtonsForVodScreenPictureBox();
            InitializeSoundEffectButtons();
            InitializeSyncScreen();

            
            InitializeOtherControls(); 

            
            pictureBox1.BringToFront();
            pictureBox2.BringToFront();
            pictureBox3.BringToFront();
            pictureBox4.BringToFront();
            pictureBoxQRCode.BringToFront();
            pictureBoxZhuYinSingers.BringToFront();
            pictureBoxZhuYinSongs.BringToFront();
            pictureBoxEnglishSingers.BringToFront();
            pictureBoxEnglishSongs.BringToFront();
            pictureBoxWordCount.BringToFront();
            FavoritePictureBox.BringToFront();
            promotionsPictureBox.BringToFront();
            pictureBoxToggleLight.BringToFront();
            overlayPanel.BringToFront();
            VodScreenPictureBox.BringToFront();

            newSongAlertButton.BringToFront();
            hotPlayButton.BringToFront();
            singerSearchButton.BringToFront();
            songSearchButton.BringToFront();
            languageSearchButton.BringToFront();
            groupSearchButton.BringToFront();
            categorySearchButton.BringToFront();
            serviceBellButton.BringToFront();
            orderedSongsButton.BringToFront();
            myFavoritesButton.BringToFront();
            deliciousFoodButton.BringToFront();
            promotionsButton.BringToFront();
            mobileSongRequestButton.BringToFront();
            qieGeButton.BringToFront();
            musicUpButton.BringToFront();
            musicDownButton.BringToFront();
            micUpButton.BringToFront();
            micDownButton.BringToFront();
            originalSongButton.BringToFront();
            replayButton.BringToFront();
            pauseButton.BringToFront();
            playButton.BringToFront();
            muteButton.BringToFront();
            maleKeyButton.BringToFront();
            femaleKeyButton.BringToFront();
            standardKeyButton.BringToFront();
            soundEffectButton.BringToFront();
            pitchUpButton.BringToFront();
            pitchDownButton.BringToFront();
            syncScreenButton.BringToFront();
            toggleLightButton.BringToFront();
            btnPreviousPage.BringToFront();
            btnReturn.BringToFront();
            btnNextPage.BringToFront();
            btnApplause.BringToFront();
            btnSimplifiedChinese.BringToFront();
            btnTraditionalChinese.BringToFront();
            exitButton.BringToFront();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private string ReadSelectedThemePath()
        {
            
            string configFilePath = Path.Combine(Application.StartupPath, "theme_description.txt");
            try
            {
                
                string[] lines = File.ReadAllLines(configFilePath);

                
                foreach (string line in lines)
                {
                    if (line.StartsWith("Selected Theme: "))
                    {
                        
                        string themePath = line.Substring("Selected Theme: ".Length).Trim();
                        return themePath;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("讀取文件時發生錯誤: {0}", ex.Message));
            }

            return string.Empty; 
        }

        private void InitializePictureBox()
        {
            pictureBox1 = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            
            pictureBox2 = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            
            pictureBox3 = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            
            pictureBox4 = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            
            pictureBox5 = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            
            pictureBox6 = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };
            
            pictureBoxZhuYinSingers = new PictureBox
            {
                Name = "pictureBoxZhuYinSingers", 
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            pictureBoxZhuYinSongs = new PictureBox
            {
                Name = "pictureBoxZhuYinSongs", 
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            pictureBoxEnglishSingers = new PictureBox
            {
                Name = "pictureBoxEnglishSingers",
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            pictureBoxEnglishSongs = new PictureBox
            {
                Name = "pictureBoxEnglishSongs",
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            pictureBoxArtistSearch = new PictureBox
            {
                Name = "pictureBoxArtistSearch",
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            pictureBoxWordCount = new PictureBox
            {
                Name = "pictureBoxWordCount",
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            pictureBoxPinYinSingers = new PictureBox
            {
                Name = "pictureBoxPinYinSingers",
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            pictureBoxPinYinSongs = new PictureBox
            {
                Name = "pictureBoxPinYinSongs",
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            pictureBoxSongIDSearch = new PictureBox
            {
                Name = "pictureBoxSongIDSearch",
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            pictureBoxHandWritingSingers = new PictureBox
            {
                Name = "pictureBoxHandWritingSingers",
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            pictureBoxHandWritingSongs = new PictureBox
            {
                Name = "pictureBoxHandWritingSongs",
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            FavoritePictureBox = new PictureBox
            {
                Name = "FavoritePictureBox",
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false
            };
            FavoritePictureBox.Paint += new PaintEventHandler(FavoritePictureBox_Paint);

            promotionsPictureBox = new PictureBox
            {
                Name = "promotionsPictureBox",
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            pictureBoxToggleLight = new PictureBox
            {
                Name = "pictureBoxToggleLight",
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            VodScreenPictureBox = new PictureBox
            {
                Name = "VodScreenPictureBox",
                BackColor = Color.FromArgb(128, 0, 0, 0),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false
            };

            overlayPanel = new Panel
            {
                Name = "overlayPanel",
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(128, 0, 0, 0),
                
                Visible = false
            };

            pictureBoxQRCode = new PictureBox
            {
                Name = "pictureBoxQRCode",
                
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            pictureBoxSceneSoundEffects = new PictureBox
            {
                Name = "pictureBoxSceneSoundEffects",
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false 
            };

            this.Controls.Add(pictureBox1);
            this.Controls.Add(pictureBox2);
            this.Controls.Add(pictureBox3);
            this.Controls.Add(pictureBox4);
            this.Controls.Add(pictureBox5);
            this.Controls.Add(pictureBox6);
            this.Controls.Add(pictureBoxQRCode); 
            this.Controls.Add(pictureBoxZhuYinSingers);
            this.Controls.Add(pictureBoxZhuYinSongs);
            this.Controls.Add(pictureBoxEnglishSingers);
            this.Controls.Add(pictureBoxEnglishSongs);
            this.Controls.Add(pictureBoxArtistSearch);
            this.Controls.Add(pictureBoxWordCount);
            this.Controls.Add(pictureBoxPinYinSingers);
            this.Controls.Add(pictureBoxPinYinSongs);
            this.Controls.Add(pictureBoxHandWritingSingers);
            this.Controls.Add(pictureBoxHandWritingSongs);
            this.Controls.Add(pictureBoxSongIDSearch);
            this.Controls.Add(FavoritePictureBox);
            this.Controls.Add(promotionsPictureBox);
            this.Controls.Add(pictureBoxToggleLight);
            this.Controls.Add(VodScreenPictureBox);
            this.Controls.Add(overlayPanel);
            this.Controls.Add(pictureBoxSceneSoundEffects); 
        }

        
        private void PhoneticButton_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Tag != null)
            {
                
                if (inputBoxZhuYinSingers.Visible)
                {
                    inputBoxZhuYinSingers.Text += button.Tag.ToString();
                }
                else if (inputBoxZhuYinSongs.Visible)
                {
                    inputBoxZhuYinSongs.Text += button.Tag.ToString();
                }
            }
        }

        private void ModifyButtonArtist_Click(object sender, EventArgs e)
        {
            
            if (inputBoxArtistSearch.Text.Length > 0)
            {
                
                inputBoxArtistSearch.Text = inputBoxArtistSearch.Text.Substring(0, inputBoxArtistSearch.Text.Length - 1);
            }
        }

        private void ModifyButtonWordCount_Click(object sender, EventArgs e)
        {
            
            if (inputBoxWordCount.Text.Length > 0)
            {
                
                inputBoxWordCount.Text = inputBoxWordCount.Text.Substring(0, inputBoxWordCount.Text.Length - 1);
            }
        }

        private void BtnBrightnessUp1_MouseDown(object sender, MouseEventArgs e)
        {
            
            lightControlTimer.Tag = "a2 d9 a4";
            lightControlTimer.Start();
        }

        private void BtnBrightnessUp1_MouseUp(object sender, MouseEventArgs e)
        {
            lightControlTimer.Stop();
        }

        private void LightControlTimer_Tick(object sender, EventArgs e)
        {
            if(lightControlTimer.Tag != null)
            {
                SendCommandThroughSerialPort(lightControlTimer.Tag.ToString());
            }
        }

        private void VolumeUpTimer_Tick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    
                    SendCommandThroughSerialPort("a2 b3 a4");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to send command: " + ex.Message);
                }
            });
        }

        private void VolumeDownTimer_Tick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    
                    SendCommandThroughSerialPort("a2 b4 a4");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to send command: " + ex.Message);
                }
            });
        }

        private void MicControlTimer_Tick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    if(micControlTimer.Tag != null)
                    {
                        SendCommandThroughSerialPort(micControlTimer.Tag.ToString());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to send command: " + ex.Message);
                }
            });
        }

        public static void SendCommandThroughSerialPort(string command)
        {
            if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
            {
                try
                {
                    
                    
                    byte[] commandBytes = HexStringToByteArray(command);
                    SerialPortManager.mySerialPort.Write(commandBytes, 0, commandBytes.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to send command: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Serial port is not open.");
            }
        }

        
        public static byte[] HexStringToByteArray(string hex)
        {
            hex = hex.Replace(" ", ""); 
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        private void OptionButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            MessageBox.Show(String.Format("Clicked on option: {0}", clickedButton.Text));
            
        }

        private void RecognizeInk(InkOverlay inkOverlay, ListBox candidateListBox)
        {
            if (inkOverlay.Ink.Strokes.Count > 0)
            {
                using (RecognizerContext context = new RecognizerContext())
                {
                    context.Strokes = inkOverlay.Ink.Strokes;
                    RecognitionStatus status;
                    RecognitionResult result = context.Recognize(out status);

                    if (status == RecognitionStatus.NoError)
                    {
                        
                        

                        
                        List<string> candidates = new List<string>();
                        foreach (RecognitionAlternate alternate in result.GetAlternatesFromSelection())
                        {
                            candidates.Add(alternate.ToString());
                        }
                        ShowCandidates(candidates, candidateListBox);
                    }
                    else
                    {
                        
                        candidateListBox.Visible = false; 
                    }
                }
            }
            else
            {
                
                candidateListBox.Visible = false; 
            }
        }

        private void ShowCandidates(List<string> candidates, ListBox candidateListBox)
        {
            candidateListBox.Items.Clear();
            foreach (var candidate in candidates)
            {
                candidateListBox.Items.Add(candidate);
            }
            candidateListBox.Visible = true;
        }

        private void BtnShowAll_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Show All button clicked!");
            
        }

        private void SetPictureBoxArtistSearchAndButtonsVisibility(bool isVisible)
        {
            System.Action action = () =>
            {
                SuspendLayout();

                
                pictureBoxArtistSearch.Visible = isVisible;

                
                if (isVisible) pictureBoxArtistSearch.BringToFront();

                
                modifyButtonArtistSearch.Visible = isVisible;
                closeButtonArtistSearch.Visible = isVisible;

                
                if (isVisible)
                {
                    modifyButtonArtistSearch.BringToFront();
                    closeButtonArtistSearch.BringToFront();
                }

                
                foreach (Button button in numberButtonsArtistSearch)
                {
                    button.Visible = isVisible;
                    
                    if (isVisible)
                        button.BringToFront();
                }

                inputBoxArtistSearch.Visible = isVisible;
                if (isVisible) inputBoxArtistSearch.BringToFront();

                ResumeLayout();
            };

            if (this.InvokeRequired)
            {
                this.Invoke(action);
            }
            else
            {
                action();
            }
        }

        private void SetPictureBoxWordCountAndButtonsVisibility(bool isVisible)
        {
            System.Action action = () =>
            {
                SuspendLayout();

                
                pictureBoxWordCount.Visible = isVisible;

                
                if (isVisible) pictureBoxWordCount.BringToFront();

                
                modifyButtonWordCount.Visible = isVisible;
                closeButtonWordCount.Visible = isVisible;

                
                if (isVisible)
                {
                    modifyButtonWordCount.BringToFront();
                    closeButtonWordCount.BringToFront();
                }

                
                foreach (Button button in numberButtonsWordCount)
                {
                    button.Visible = isVisible;
                    
                    if (isVisible)
                        button.BringToFront();
                }

                inputBoxWordCount.Visible = isVisible;
                if (isVisible) inputBoxWordCount.BringToFront();

                ResumeLayout();
            };

            if (this.InvokeRequired)
            {
                this.Invoke(action);
            }
            else
            {
                action();
            }
        }

        private void ConfigureButton(Button button, int posX, int posY, int width, int height, 
                             Bitmap normalStateImage, Bitmap mouseOverImage, Bitmap mouseDownImage, 
                             EventHandler clickEventHandler)
        {
            
            ResizeAndPositionButton(button, posX, posY, width, height);

            
            Rectangle cropArea = new Rectangle(button.Location.X, button.Location.Y, button.Size.Width, button.Size.Height);

            
            button.BackgroundImage = normalStateImage.Clone(cropArea, normalStateImage.PixelFormat);
            button.BackgroundImageLayout = ImageLayout.Stretch;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0; 
            button.FlatAppearance.MouseDownBackColor = Color.Transparent; 
            button.FlatAppearance.MouseOverBackColor = Color.Transparent; 

            
            button.MouseEnter += (sender, e) => button.BackgroundImage = mouseOverImage.Clone(cropArea, mouseOverImage.PixelFormat);
            button.MouseLeave += (sender, e) => button.BackgroundImage = normalStateImage.Clone(cropArea, normalStateImage.PixelFormat);
            button.MouseDown += (sender, e) => button.BackgroundImage = mouseDownImage.Clone(cropArea, mouseDownImage.PixelFormat);
            button.MouseUp += (sender, e) => button.BackgroundImage = normalStateImage.Clone(cropArea, normalStateImage.PixelFormat);

            
            if (clickEventHandler != null)
            {
                button.Click += clickEventHandler;
            }

            
            this.Controls.Add(button);
        }

        private void InitializeOtherControls()
        {
            InitializeButton(ref newSongAlertButton, ref newSongAlertNormalBackground, ref newSongAlertActiveBackground, "newSongAlertButton", 25, 97, 99, 99, "themes\\superstar\\ICON上方\\上方ICON_新歌快報X.png", NewSongAlertButton_Click);

            InitializeButton(ref hotPlayButton, ref hotPlayNormalBackground, ref hotPlayActiveBackground, "hotPlayButton", 143, 97, 99, 99, "themes\\superstar\\ICON上方\\上方ICON_熱門排行-02.png", HotPlayButton_Click);

            InitializeButton(ref singerSearchButton, ref singerSearchNormalBackground, ref singerSearchActiveBackground, "singerSearchButton", 261, 97, 99, 99, "themes\\superstar\\ICON上方\\上方ICON_歌星查詢-03.png", SingerSearchButton_Click);

            InitializeButton(ref songSearchButton, ref songSearchNormalBackground, ref songSearchActiveBackground, "songSearchButton", 378, 97, 100, 99, "themes\\superstar\\ICON上方\\上方ICON_歌名查詢-04.png", SongSearchButton_Click);

            InitializeButton(ref languageSearchButton, ref languageSearchNormalBackground, ref languageSearchActiveBackground, "languageSearchButton", 496, 97, 99, 99, "themes\\superstar\\ICON上方\\上方ICON_語別查詢-05.png", LanguageSongSelectionButton_Click);
            
            InitializeButton(ref groupSearchButton, ref groupSearchNormalBackground, ref groupSearchActiveBackground, "groupSearchButton", 614, 97, 99, 100, "themes\\superstar\\ICON上方\\上方ICON_合唱查詢-06.png", GroupSongSelectionButton_Click);

            InitializeButton(ref categorySearchButton, ref categorySearchNormalBackground, ref categorySearchActiveBackground, "categorySearchButton", 731, 97, 99, 99, "themes\\superstar\\ICON上方\\上方ICON_類別查詢-07.png", CategorySearchButton_Click);

            
            serviceBellButton = new Button { Text = "" };
            serviceBellButton.Name = "serviceBellButton";
            ConfigureButton(serviceBellButton, 848, 96, 101, 102, 
                resizedNormalStateImage, resizedMouseOverImage, resizedMouseDownImage, 
                (sender, e) => OnServiceBellButtonClick(sender, e)); 

            this.Controls.Add(serviceBellButton);

            InitializeButton(ref orderedSongsButton, ref orderedSongsNormalBackground, ref orderedSongsActiveBackground, "orderedSongsButton", 966, 97, 100, 99, "themes\\superstar\\ICON上方\\上方ICON_已點歌曲-09.png", OrderedSongsButton_Click);

            InitializeButton(ref myFavoritesButton, ref myFavoritesNormalBackground, ref myFavoritesActiveBackground, "myFavoritesButton", 1084, 97, 99, 99, "themes\\superstar\\ICON上方\\上方ICON_我的最愛-10.png", MyFavoritesButton_Click);

            InitializeButton(ref promotionsButton, ref promotionsNormalBackground, ref promotionsActiveBackground, "promotionsButton", 1202, 97, 99, 99, "themes\\superstar\\ICON上方\\上方ICON_優惠活動-11.png", promotionsButton_Click);

            InitializeButton(ref deliciousFoodButton, ref deliciousFoodNormalBackground, ref deliciousFoodActiveBackground, "deliciousFoodButton", 1320, 97, 98, 99, "themes\\superstar\\ICON上方\\上方ICON_美味菜單-12.png", DeliciousFoodButton_Click);

            mobileSongRequestButton = new Button { Text = "" };
            mobileSongRequestButton.Name = "mobileSongRequestButton";
            ConfigureButton(mobileSongRequestButton, 1211, 669, 210, 70, 
                resizedNormalStateImage, resizedMouseOverImage, resizedMouseDownImage, 
                MobileSongRequestButton_Click);

            qieGeButton = new Button{ Text = "" };
            qieGeButton.Name = "qieGeButton";
            ConfigureButton(qieGeButton, 28, 755, 92, 132, 
                resizedNormalStateImage, resizedNormalStateImage, resizedNormalStateImage, 
                (sender, e) => videoPlayerForm.SkipToNextSong());
            this.Controls.Add(qieGeButton);

            
            musicUpButton = new Button{ Text = "" };
            musicUpButton.Name = "musicUpButton";
            ResizeAndPositionButton(musicUpButton, 136, 754, 92, 58);
            Rectangle musicUpButtonCropArea = new Rectangle(136, 754, 92, 58);
            musicUpButton.BackgroundImage = normalStateImage.Clone(musicUpButtonCropArea, normalStateImage.PixelFormat);
            musicUpButton.BackgroundImageLayout = ImageLayout.Stretch;
            musicUpButton.FlatStyle = FlatStyle.Flat;
            musicUpButton.FlatAppearance.BorderSize = 0; 
            musicUpButton.MouseDown += (sender, e) => { OverlayForm.MainForm.ShowVolumeUpLabel(); volumeUpTimer.Start(); };
            musicUpButton.MouseUp += (sender, e) => { OverlayForm.MainForm.HideAllLabels(); volumeUpTimer.Stop(); };
            this.Controls.Add(musicUpButton);

            
            musicDownButton = new Button{ Text = "" };
            musicDownButton.Name = "musicDownButton";
            ResizeAndPositionButton(musicDownButton, 136, 827, 92, 57);
            Rectangle musicDownButtonCropArea = new Rectangle(136, 827, 92, 57);
            musicDownButton.BackgroundImage = normalStateImage.Clone(musicDownButtonCropArea, normalStateImage.PixelFormat);
            musicDownButton.BackgroundImageLayout = ImageLayout.Stretch;
            musicDownButton.FlatStyle = FlatStyle.Flat;
            musicDownButton.FlatAppearance.BorderSize = 0; 
            musicDownButton.MouseDown += (sender, e) => { OverlayForm.MainForm.ShowVolumeDownLabel(); volumeDownTimer.Start(); };
            musicDownButton.MouseUp += (sender, e) => { OverlayForm.MainForm.HideAllLabels(); volumeDownTimer.Stop(); };
            this.Controls.Add(musicDownButton);

            
            micUpButton = new Button{ Text = "" };
            micUpButton.Name = "micUpButton";
            ResizeAndPositionButton(micUpButton, 244, 754, 92, 57);
            Rectangle micUpButtonCropArea = new Rectangle(244, 754, 92, 57);
            micUpButton.BackgroundImage = normalStateImage.Clone(micUpButtonCropArea, normalStateImage.PixelFormat);
            micUpButton.BackgroundImageLayout = ImageLayout.Stretch;
            micUpButton.FlatStyle = FlatStyle.Flat;
            micUpButton.FlatAppearance.BorderSize = 0; 
            micUpButton.MouseDown += (sender, e) => { OverlayForm.MainForm.ShowMicUpLabel(); micControlTimer.Tag = "a2 b5 a4"; micControlTimer.Start(); };
            micUpButton.MouseUp += (sender, e) => { OverlayForm.MainForm.HideAllLabels(); micControlTimer.Stop(); };
            this.Controls.Add(micUpButton);

            
            micDownButton = new Button{ Text = "" };
            micDownButton.Name = "micDownButton";
            ResizeAndPositionButton(micDownButton, 244, 827, 92, 57);
            Rectangle micDownButtonCropArea = new Rectangle(244, 827, 92, 57);
            micDownButton.BackgroundImage = normalStateImage.Clone(micDownButtonCropArea, normalStateImage.PixelFormat);
            micDownButton.BackgroundImageLayout = ImageLayout.Stretch;
            micDownButton.FlatStyle = FlatStyle.Flat;
            micDownButton.FlatAppearance.BorderSize = 0; 
            micDownButton.MouseDown += (sender, e) => { OverlayForm.MainForm.ShowMicDownLabel(); micControlTimer.Tag = "a2 b6 a4"; micControlTimer.Start(); };
            micDownButton.MouseUp += (sender, e) => { OverlayForm.MainForm.HideAllLabels(); micControlTimer.Stop(); };
            this.Controls.Add(micDownButton);

            
            originalSongButton = new Button { Text = "" };
            originalSongButton.Name = "originalSongButton";
            ResizeAndPositionButton(originalSongButton, 353, 756, 91, 55);
            Rectangle originalSongButtonCropArea = new Rectangle(353, 756, 91, 55);
            originalSongButton.BackgroundImage = normalStateImage.Clone(originalSongButtonCropArea, normalStateImage.PixelFormat);
            originalSongButton.BackgroundImageLayout = ImageLayout.Stretch;
            originalSongButton.FlatStyle = FlatStyle.Flat;
            originalSongButton.FlatAppearance.BorderSize = 0; 
            originalSongButton.Click += OriginalSongButton_Click;
            this.Controls.Add(originalSongButton);

            
            replayButton = new Button{ Text = "" };
            replayButton.Name = "replayButton";
            ResizeAndPositionButton(replayButton, 353, 828, 91, 55);
            Rectangle replayButtonCropArea = new Rectangle(353, 828, 91, 55);
            replayButton.BackgroundImage = normalStateImage.Clone(replayButtonCropArea, normalStateImage.PixelFormat);
            replayButton.BackgroundImageLayout = ImageLayout.Stretch;
            replayButton.FlatStyle = FlatStyle.Flat;
            replayButton.FlatAppearance.BorderSize = 0; 
            replayButton.Click += ReplayButton_Click; 
            this.Controls.Add(replayButton);

            
            pauseButton = new Button {
                Text = "",
                Name = "pauseButton"
            };
            ResizeAndPositionButton(pauseButton, 461, 755, 91, 56);
            Rectangle pauseButtonCropArea = new Rectangle(461, 755, 91, 56);
            pauseButton.BackgroundImage = normalStateImage.Clone(pauseButtonCropArea, normalStateImage.PixelFormat);
            pauseButton.BackgroundImageLayout = ImageLayout.Stretch;
            pauseButton.FlatStyle = FlatStyle.Flat;
            pauseButton.FlatAppearance.BorderSize = 0; 
            pauseButton.Click += PauseButton_Click; 
            this.Controls.Add(pauseButton);

            
            playButton = new Button {
                Text = "",
                Name = "playButton",
                Visible = false  
            };
            ResizeAndPositionButton(playButton, 461, 755, 91, 56);
            Rectangle playButtonCropArea = new Rectangle(461, 755, 91, 56);
            playButton.BackgroundImage = normalStateImage.Clone(playButtonCropArea, normalStateImage.PixelFormat);
            playButton.BackgroundImageLayout = ImageLayout.Stretch;
            playButton.FlatStyle = FlatStyle.Flat;
            playButton.FlatAppearance.BorderSize = 0; 
            playButton.Click += PlayButton_Click; 
            this.Controls.Add(playButton);

            
            muteButton = new Button{ Text = "" };
            muteButton.Name = "muteButton";
            ResizeAndPositionButton(muteButton, 461, 828, 91, 55);
            Rectangle muteButtonCropArea = new Rectangle(461, 828, 91, 55);
            muteButton.BackgroundImage = normalStateImage.Clone(muteButtonCropArea, normalStateImage.PixelFormat);
            muteButton.BackgroundImageLayout = ImageLayout.Stretch;
            muteButton.FlatStyle = FlatStyle.Flat;
            muteButton.FlatAppearance.BorderSize = 0; 
            muteButton.Click += MuteUnmuteButton_Click;
            this.Controls.Add(muteButton);

            
            maleKeyButton = new Button{ Text = "" };
            maleKeyButton.Name = "maleKeyButton";
            ResizeAndPositionButton(maleKeyButton, 569, 755, 91, 56);
            Rectangle maleKeyButtonCropArea = new Rectangle(569, 755, 91, 56);
            maleKeyButton.BackgroundImage = normalStateImage.Clone(maleKeyButtonCropArea, normalStateImage.PixelFormat);
            maleKeyButton.BackgroundImageLayout = ImageLayout.Stretch;
            maleKeyButton.FlatStyle = FlatStyle.Flat;
            maleKeyButton.Click += MaleKeyButton_Click;
            maleKeyButton.FlatAppearance.BorderSize = 0; 

            this.Controls.Add(maleKeyButton);

            
            femaleKeyButton = new Button{ Text = "" };
            femaleKeyButton.Name = "femaleKeyButton";
            ResizeAndPositionButton(femaleKeyButton, 570, 828, 90, 55);
            Rectangle femaleKeyButtonCropArea = new Rectangle(570, 828, 90, 55);
            femaleKeyButton.BackgroundImage = normalStateImage.Clone(femaleKeyButtonCropArea, normalStateImage.PixelFormat);
            femaleKeyButton.BackgroundImageLayout = ImageLayout.Stretch;
            femaleKeyButton.FlatStyle = FlatStyle.Flat;
            femaleKeyButton.FlatAppearance.BorderSize = 0; 
            femaleKeyButton.Click += FemaleKeyButton_Click;
            this.Controls.Add(femaleKeyButton);

            
            standardKeyButton = new Button { Text = "" };
            standardKeyButton.Name = "standardKeyButton";
            ResizeAndPositionButton(standardKeyButton, 677, 757, 91, 56);
            Rectangle standardKeyButtonCropArea = new Rectangle(677, 757, 91, 56);
            standardKeyButton.BackgroundImage = normalStateImage.Clone(standardKeyButtonCropArea, normalStateImage.PixelFormat);
            standardKeyButton.BackgroundImageLayout = ImageLayout.Stretch;
            standardKeyButton.FlatStyle = FlatStyle.Flat;
            standardKeyButton.FlatAppearance.BorderSize = 0; 
            standardKeyButton.Click += StandardKeyButton_Click;
            this.Controls.Add(standardKeyButton);

            
            soundEffectButton = new Button { Text = "" };
            soundEffectButton.Name = "soundEffectButton";
            ResizeAndPositionButton(soundEffectButton, 677, 827, 91, 56);
            Rectangle soundEffectButtonCropArea = new Rectangle(677, 827, 91, 56);
            soundEffectButton.BackgroundImage = normalStateImage.Clone(soundEffectButtonCropArea, normalStateImage.PixelFormat);
            soundEffectButton.BackgroundImageLayout = ImageLayout.Stretch;
            soundEffectButton.FlatStyle = FlatStyle.Flat;
            soundEffectButton.FlatAppearance.BorderSize = 0; 
            soundEffectButton.Click += SoundEffectButton_Click;
            this.Controls.Add(soundEffectButton);

            
            pitchUpButton = new Button{ Text = "" };
            pitchUpButton.Name = "pitchUpButton";
            ResizeAndPositionButton(pitchUpButton, 786, 755, 90, 56);
            Rectangle pitchUpButtonCropArea = new Rectangle(786, 755, 90, 56);
            pitchUpButton.BackgroundImage = normalStateImage.Clone(pitchUpButtonCropArea, normalStateImage.PixelFormat);
            pitchUpButton.BackgroundImageLayout = ImageLayout.Stretch;
            pitchUpButton.FlatStyle = FlatStyle.Flat;
            pitchUpButton.FlatAppearance.BorderSize = 0; 
            pitchUpButton.Click += PitchUpButton_Click;
            this.Controls.Add(pitchUpButton);

            
            pitchDownButton = new Button{ Text = "" };
            pitchDownButton.Name = "pitchDownButton";
            ResizeAndPositionButton(pitchDownButton, 786, 828, 90, 55);
            Rectangle pitchDownButtonCropArea = new Rectangle(786, 828, 90, 55);
            pitchDownButton.BackgroundImage = normalStateImage.Clone(pitchDownButtonCropArea, normalStateImage.PixelFormat);
            pitchDownButton.BackgroundImageLayout = ImageLayout.Stretch;
            pitchDownButton.FlatStyle = FlatStyle.Flat;
            pitchDownButton.FlatAppearance.BorderSize = 0; 
            pitchDownButton.Click += PitchDownButton_Click;
            this.Controls.Add(pitchDownButton);

            
            syncScreenButton = new Button { Text = "" };
            syncScreenButton.Name = "syncScreenButton";
            ResizeAndPositionButton(syncScreenButton, 893, 754, 93, 133);
            Rectangle syncScreenButtonCropArea = new Rectangle(893, 754, 93, 133);
            syncScreenButton.BackgroundImage = normalStateImage.Clone(syncScreenButtonCropArea, normalStateImage.PixelFormat);
            syncScreenButton.BackgroundImageLayout = ImageLayout.Stretch;
            syncScreenButton.FlatStyle = FlatStyle.Flat;
            syncScreenButton.FlatAppearance.BorderSize = 0; 
            syncScreenButton.Click += SyncScreenButton_Click;  
            this.Controls.Add(syncScreenButton);

            toggleLightButton = new Button{ Text = "" };
            toggleLightButton.Name = "toggleLightButton";
            
            ResizeAndPositionButton(toggleLightButton, 1002, 756, 91, 130);
            Rectangle toggleLightButtonCropArea = new Rectangle(1002, 756, 91, 130);
            toggleLightButton.BackgroundImage = normalStateImage.Clone(toggleLightButtonCropArea, normalStateImage.PixelFormat);
            toggleLightButton.BackgroundImageLayout = ImageLayout.Stretch;
            toggleLightButton.FlatStyle = FlatStyle.Flat;
            toggleLightButton.FlatAppearance.BorderSize = 0; 
            
            toggleLightButton.Click += ToggleLightButton_Click;
            
            this.Controls.Add(toggleLightButton);

            
            Rectangle previousPageButtonCropArea = new Rectangle(1109, 754, 93, 58);
            InitializeButton(
                ref btnPreviousPage, 
                "btnPreviousPage", 
                1109, 754, 93, 58, 
                previousPageButtonCropArea, 
                normalStateImage, 
                PreviousPageButton_Click
            );

            
            btnReturn = new Button{ Text = "" };
            btnReturn.Name = "btnReturn";
            
            ResizeAndPositionButton(btnReturn, 1218, 755, 92, 57);
            Rectangle returnButtonCropArea = new Rectangle(1218, 755, 92, 57);
            btnReturn.BackgroundImage = normalStateImage.Clone(returnButtonCropArea, normalStateImage.PixelFormat);
            btnReturn.BackgroundImageLayout = ImageLayout.Stretch;
            btnReturn.FlatStyle = FlatStyle.Flat;
            btnReturn.FlatAppearance.BorderSize = 0; 
            
            btnReturn.Click += ShouYeButton_Click;
            
            this.Controls.Add(btnReturn);

            
            Rectangle nextPageButtonCropArea = new Rectangle(1326, 754, 92, 58);
            InitializeButton(
                ref btnNextPage, 
                "btnNextPage", 
                1326, 754, 92, 58, 
                nextPageButtonCropArea, 
                normalStateImage, 
                NextPageButton_Click
            );

            
            Rectangle applauseButtonCropArea = new Rectangle(1110, 828, 91, 55);
            InitializeButton(
                ref btnApplause, 
                "btnApplause", 
                1110, 828, 91, 55, 
                applauseButtonCropArea, 
                normalStateImage, 
                ApplauseButton_Click
            );

            
            Rectangle simplifiedChineseButtonCropArea = new Rectangle(1327, 828, 90, 55);
            InitializeButton(
                ref btnSimplifiedChinese, 
                "btnSimplifiedChinese", 
                1327, 828, 90, 55, 
                simplifiedChineseButtonCropArea, 
                normalStateImage, 
                SimplifiedChineseButton_Click
            );

            
            Rectangle traditionalChineseButtonCropArea = new Rectangle(1219, 828, 90, 55);
            InitializeButton(
                ref btnTraditionalChinese, 
                "btnTraditionalChinese", 
                1219, 828, 90, 55, 
                traditionalChineseButtonCropArea, 
                normalStateImage, 
                TraditionalChineseButton_Click
            );

            
            exitButton = new Button{};
            exitButton.Name = "exitButton";
            ConfigureButton(exitButton, 1394, 2, 1428 - 1394, 37 - 2, 
                resizedNormalStateImage, resizedMouseOverImage, resizedMouseDownImage, 
                (sender, e) => Application.Exit());
        }

        private void InitializeButton(ref Button button, ref Bitmap normalBackground, ref Bitmap activeBackground, string buttonName, int x, int y, int width, int height, string imagePath, EventHandler clickEventHandler)
        {
            button = new Button { Text = "", Name = buttonName };
            ResizeAndPositionButton(button, x, y, width, height);
            Rectangle buttonCropArea = new Rectangle(x, y, width, height);
            normalBackground = new Bitmap(Path.Combine(Application.StartupPath, imagePath));
            activeBackground = mouseDownImage.Clone(buttonCropArea, mouseDownImage.PixelFormat);
            button.BackgroundImage = normalBackground;
            button.BackgroundImageLayout = ImageLayout.Stretch;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            if (clickEventHandler != null)
            {
                button.Click += clickEventHandler;
            }
            this.Controls.Add(button);
        }

        private void InitializeButton(
            ref Button button, 
            string buttonName, 
            int x, int y, 
            int width, int height, 
            Rectangle cropArea, 
            Bitmap normalStateImage, 
            EventHandler clickEventHandler)
        {
            button = new Button { Text = "", Name = buttonName };
            ResizeAndPositionButton(button, x, y, width, height);
            button.BackgroundImage = normalStateImage.Clone(cropArea, normalStateImage.PixelFormat);
            button.BackgroundImageLayout = ImageLayout.Stretch;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            if (clickEventHandler != null)
            {
                button.Click += clickEventHandler;
            }
            this.Controls.Add(button);
        }

public class MultiPagePanel : Panel
{
    private const int ItemHeight = 70;
    private const int RowGap = 2;
    private const int itemsPerPage = 16; // 每頁 16 首歌
    private const float LeftColumnX = 0.03f;
    private const float RightColumnX = 0.52f;
    private const float SongWidth = 0.35f;
    private const float ArtistWidth = 0.12f;
    private const int ArtistVerticalOffset = 2;
    private const string MusicNoteSymbol = "♫";

    private int _currentPageIndex = 0;
    private bool _isSimplified = false;
    public bool IsSimplified
    {
        get { return _isSimplified; }
        set 
        { 
            if (_isSimplified != value)
            {
                _isSimplified = value;
                RefreshDisplay();
            }
        }
    }
    private List<SongData> currentSongList = new List<SongData>();
    private List<Artist> currentArtistList = new List<Artist>();
    private int totalPages = 0;
    private Point mouseDownLocation;    // 新增字段
    private bool isDragging = false;    // 新增字段

    public int currentPageIndex
    {
        get { return _currentPageIndex; }
        set { _currentPageIndex = value; }
    }
            
            public MultiPagePanel()
            {
                this.SetStyle(
                    ControlStyles.OptimizedDoubleBuffer |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.ResizeRedraw,
                    true);
                
                this.BackColor = Color.FromArgb(64, 0, 0, 0);
                this.BorderStyle = BorderStyle.None;
                
                // 添加滑動事件
                this.MouseDown += MultiPagePanel_MouseDown;
                this.MouseMove += MultiPagePanel_MouseMove;
                this.MouseUp += MultiPagePanel_MouseUp;
            }

            private void MultiPagePanel_MouseDown(object sender, MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    mouseDownLocation = e.Location;
                    isDragging = true;
                }
            }

            private void MultiPagePanel_MouseMove(object sender, MouseEventArgs e)
            {
                if (isDragging)
                {
                    int deltaX = e.X - mouseDownLocation.X;
                    if (Math.Abs(deltaX) > 50) // 滑動距離超過50像素才觸發
                    {
                        if (deltaX > 0 && currentPageIndex > 0)
                        {
                            // 向右滑動，上一頁
                            LoadPreviousPage();
                        }
                        else if (deltaX < 0 && currentPageIndex < totalPages - 1)
                        {
                            // 向左滑動，下一頁
                            LoadNextPage();
                        }
                        isDragging = false;
                    }
                }
            }

            private void MultiPagePanel_MouseUp(object sender, MouseEventArgs e)
            {
                isDragging = false;
            }

            public void LoadNextPage()
            {
                if (currentPageIndex < totalPages - 1)
                {
                    currentPageIndex++;
                    RefreshDisplay();
                }
            }

            public void LoadPreviousPage()
            {
                if (currentPageIndex > 0)
                {
                    currentPageIndex--;
                    RefreshDisplay();
                }
            }

            public void LoadSongs(List<SongData> songs, bool clearHistory = false)
            {
                currentSongList = songs;
                currentArtistList.Clear();
                currentPageIndex = 0;
                totalPages = (int)Math.Ceiling(songs.Count / (double)itemsPerPage);
                
                // 直接調用基礎刷新邏輯
                RefreshDisplayBase();
            }

            public void LoadSingers(List<Artist> artists)
            {
                currentArtistList = artists;
                currentSongList.Clear();
                currentPageIndex = 0;
                totalPages = (int)Math.Ceiling(artists.Count / (double)itemsPerPage);
                RefreshDisplay();
            }

            public void LoadPlayedSongs(List<SongData> songs, List<PlayState> states)
            {
                currentSongList = songs;
                currentArtistList.Clear();
                currentPageIndex = 0;
                totalPages = (int)Math.Ceiling(songs.Count / (double)itemsPerPage);
                
                // 直接調用基礎刷新邏輯
                RefreshDisplayBase();
            }

            public void RefreshDisplay()
            {
                this.Controls.Clear();
                RefreshDisplayBase();
                this.Invalidate();
            }

private void RefreshDisplayBase()
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(() => RefreshDisplayBase()));
            return;
        }

        this.SuspendLayout();
        this.Controls.Clear();
        
        int startIndex = currentPageIndex * itemsPerPage;
        int endIndex = Math.Min(startIndex + itemsPerPage, currentSongList.Count);
        
        for (int i = startIndex; i < endIndex; i++)
        {
            CreateSongLabel(currentSongList[i], i - startIndex, startIndex); // 修改處
        }
        
        this.ResumeLayout();
    }

            private void CreateSongLabel(SongData song, int index, int pageOffset)
{
    Label songLabel = new Label();
    string statusText = "";
    
    bool isCurrentlyPlaying = false;
    bool hasBeenPlayed = false;
    bool isLatestInstance = true;
    
    // 只在已点歌曲页面显示状态
    if (PrimaryForm.Instance.isOnOrderedSongsPage)
    {
        // 判断是否正在播放公播歌单
        bool isPlayingPublicList = userRequestedSongs.Count == 0 || 
            (currentSongIndexInHistory >= userRequestedSongs.Count - 1 && PrimaryForm.Instance.videoPlayerForm.IsPlayingPublicSong);

        if (isPlayingPublicList)
        {
            // 如果在播放公播歌单,说明所有已点歌曲都已播放完成
            hasBeenPlayed = true;
            songLabel.ForeColor = Color.Gray;
            statusText = IsSimplified ? "(播毕)" : "(播畢)";
        }
        else 
        {
            // 计算已完成播放的歌曲数量
            int completedCount = 0;
            // 遍历播放历史，计算实际的已完成数量
            for (int i = 0; i <= currentSongIndexInHistory && i < playedSongsHistory.Count; i++) {
                if (i == currentSongIndexInHistory) {
                    completedCount = i + 1; // 当前播放的歌曲
                    break;
                }
                // 检查播放状态
                if (i < playStates.Count) {
                    if (playStates[i] == PlayState.Played || playStates[i] == PlayState.Playing) {
                        completedCount++;
                    }
                    // 如果是切歌状态，不增加计数
                }
            }
            
            // 获取当前歌曲在全局列表中的位置
            int songPosition = pageOffset + index;

            // 判断状态
            if (songPosition < completedCount - 1)
            {
                // 已播放完成
                hasBeenPlayed = true;
                songLabel.ForeColor = Color.Gray;
                statusText = IsSimplified ? "(播毕)" : "(播畢)";
            }
            else if (songPosition == completedCount - 1)
            {
                // 当前正在播放
                isCurrentlyPlaying = true;
                songLabel.ForeColor = Color.LimeGreen;
                statusText = IsSimplified ? "(播放中)" : "(播放中)";
            }
            else
            {
                // 等待播放
                songLabel.ForeColor = Color.White;
                statusText = string.Empty;
            }
        }
    }
    else
    {
        // 非已点歌曲页面,使用默认白色
        songLabel.ForeColor = Color.White;
        statusText = string.Empty;
    }
    
    // 根據簡繁體設置選擇要顯示的文字
    string songText = IsSimplified ? 
        (!string.IsNullOrEmpty(song.SongSimplified) ? song.SongSimplified : song.Song) : 
        song.Song;
    
    string artistText = IsSimplified ? 
        (!string.IsNullOrEmpty(song.ArtistASimplified) ? song.ArtistASimplified : song.ArtistA) : 
        song.ArtistA;

    
    string fullText = songText + statusText;
    int textLength = fullText.Length;
    
    // 先计算状态文字宽度
    Font normalFont = new Font("微軟正黑體", 26, FontStyle.Bold);
    Font mediumFont = new Font("微軟正黑體", 20, FontStyle.Bold);
    Font smallFont = new Font("微軟正黑體", 16, FontStyle.Bold);

    // 调整阈值并考虑状态文字的长度
    if (textLength > 18)
    {
        songLabel.Font = smallFont;
    }
    else if (textLength > 13)
    {
        songLabel.Font = mediumFont;
    }
    else
    {
        songLabel.Font = normalFont;
    }

    songLabel.Text = fullText;
    songLabel.Tag = song;
    songLabel.AutoSize = false;

    // 创建歌手标签
    Label artistLabel = new Label();
    artistLabel.Text = artistText;
    artistLabel.Tag = song;
    artistLabel.AutoSize = false;

    // 计算位置 - 修改为先左边8个,再右边8个的顺序
    bool isLeftColumn = index < 8;  // 前8个在左边
    int row = isLeftColumn ? index : index - 8;  // 如果是右边,需要减去8来计算正确的行号
    float startX = isLeftColumn ? LeftColumnX : RightColumnX;
    int y = row * (ItemHeight + RowGap);

    // 设置标签位置和大小
    int songX = (int)(this.Width * startX);
    int songWidth = (int)(this.Width * SongWidth);
    int artistWidth = (int)(this.Width * ArtistWidth);
    int artistX = songX + songWidth + 10;

    // 如果有人声,添加人声图标
    if (song.HumanVoice == 1)
    {
        PictureBox icon = new PictureBox()
        {
            Image = Image.FromFile(Path.Combine(Application.StartupPath, @"themes\superstar\其他符號_人聲\其他符號_人聲.png")),
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(32, 32),
            Location = new Point(songX + 5, y + 8)
        };
        this.Controls.Add(icon);
        icon.BringToFront();
        
        // 有图标时歌名需要右移
        songLabel.Location = new Point(songX + 42, y);
        songLabel.Size = new Size(songWidth - 47, ItemHeight - 20);
    }
    else
    {
        // 无图标时歌名位置正常
        songLabel.Location = new Point(songX, y);
        songLabel.Size = new Size(songWidth - 10, ItemHeight - 20);
    }

    // 调整歌手标签位置
    artistLabel.Location = new Point(artistX, y + 33);
    artistLabel.Size = new Size(artistWidth - 10, ItemHeight - 35);

    // 创建分隔线面板
    Panel separatorPanel = new Panel
    {
        Location = new Point(songX - 5, y + ItemHeight - 2),
        Size = new Size(songWidth + artistWidth + 20, 2),
        BackColor = Color.FromArgb(80, 255, 255, 255),
        Name = "SeparatorPanel_" + index.ToString()
    };

    // 设置字体和样式
    artistLabel.Font = new Font("微軟正黑體", 16, FontStyle.Bold);
    artistLabel.ForeColor = Color.FromArgb(30,144,255);
    songLabel.TextAlign = ContentAlignment.MiddleLeft;
    artistLabel.TextAlign = ContentAlignment.MiddleRight;

    // 确保背景完全透明
    songLabel.BackColor = Color.Transparent;
    artistLabel.BackColor = Color.Transparent;

    // 添加悬停效果
    EventHandler mouseEnter = (sender, e) => {
        if (!PrimaryForm.Instance.isOnOrderedSongsPage || 
            (!isCurrentlyPlaying && (!hasBeenPlayed || !isLatestInstance)))
        {
            songLabel.ForeColor = Color.Yellow;
            artistLabel.ForeColor = Color.Yellow;
            separatorPanel.BackColor = Color.FromArgb(120, 255, 255, 255);
        }
    };

    EventHandler mouseLeave = (sender, e) => {
        if (PrimaryForm.Instance.isOnOrderedSongsPage)
        {
            if (isCurrentlyPlaying)
            {
                songLabel.ForeColor = Color.LimeGreen;
            }
            else if (hasBeenPlayed && isLatestInstance)
            {
                songLabel.ForeColor = Color.Gray;
            }
            else 
            {
                songLabel.ForeColor = Color.White;
            }
        }
        else
        {
            songLabel.ForeColor = Color.White;
        }
        artistLabel.ForeColor = Color.FromArgb(30,144,255);
        separatorPanel.BackColor = Color.FromArgb(80, 255, 255, 255);
    };

    // 添加事件处理
    songLabel.Click += PrimaryForm.Instance.Label_Click;
    artistLabel.Click += PrimaryForm.Instance.Label_Click;
    songLabel.MouseEnter += mouseEnter;
    songLabel.MouseLeave += mouseLeave;
    artistLabel.MouseEnter += mouseEnter;
    artistLabel.MouseLeave += mouseLeave;
    separatorPanel.MouseEnter += mouseEnter;
    separatorPanel.MouseLeave += mouseLeave;

    // 按正确顺序添加控件
    this.Controls.Add(separatorPanel);
    this.Controls.Add(songLabel);
    this.Controls.Add(artistLabel);

    // 确保控件层次正确
    songLabel.BringToFront();
    artistLabel.BringToFront();
}
            private bool IsLatestInstanceBeforeIndex(SongData song, int currentIndex)
            {
                if (currentSongList == null) return true;
                
                // 从当前索引向前搜索，检查是否是最新实例
                for (int i = currentIndex; i >= 0; i--)
                {
                    if (currentSongList[i].SongNumber == song.SongNumber)
                    {
                        return currentSongList[i] == song;
                    }
                }
                return true;
            }

            private bool HasBeenPlayedBeforeIndex(SongData song, int checkUntil)
            {
                for (int i = 0; i < checkUntil; i++)
                {
                    if (playedSongsHistory[i].SongNumber == song.SongNumber)
                    {
                        return true;
                    }
                }
                return false;
            }

            private bool IsLatestInstanceInCurrentList(SongData song)
            {
                if (currentSongList == null) return true;
                
                for (int i = currentSongList.Count - 1; i >= 0; i--)
                {
                    if (currentSongList[i].SongNumber == song.SongNumber)
                    {
                        return currentSongList[i] == song;
                    }
                }
                return true;
            }
        }

        private void InitializeMultiPagePanel()
        {
            // 獲取螢幕尺寸
            Screen screen = Screen.PrimaryScreen;
            int screenWidth = screen.Bounds.Width;
            int screenHeight = screen.Bounds.Height;
            
            // 計算縮放比例
            float widthRatio = screenWidth / 1440f;
            float heightRatio = screenHeight / 900f;
            
            multiPagePanel = new MultiPagePanel();
            
            // 計算面板位置和大小
            int panelX = (int)(PanelStartLeft * widthRatio);
            int panelY = (int)(PanelStartTop * heightRatio);
            int panelWidth = (int)((PanelEndLeft - PanelStartLeft) * widthRatio);
            int panelHeight = (int)((PanelEndTop - PanelStartTop) * heightRatio);
            
            multiPagePanel.Location = new Point(panelX, panelY);
            multiPagePanel.Size = new Size(panelWidth, panelHeight);
            
            this.Controls.Add(multiPagePanel);
            multiPagePanel.BringToFront();
        }

        private void PrintControlZOrder(Panel panel)
        {
            Console.WriteLine(String.Format("Printing Z-Order for controls in {0}:", panel.Name));
            int index = 0;
            foreach (Control control in panel.Controls)
            {
                
                Console.WriteLine(String.Format("Control Index: {0}, Type: {1}, Location: {2}, Size: {3}, Text: {4}",
                    index,
                    control.GetType().Name,
                    control.Location,
                    control.Size,
                    control.Text));
                index++;
            }
        }

        private SongData currentSelectedSong;

        public void Label_Click(object sender, EventArgs e)
        {
            var label = sender as Label;
            if (label != null && label.Tag is SongData)
            {
                
                currentSelectedSong = label.Tag as SongData;

                this.DoubleBuffered = true;
                this.SuspendLayout();
                
                DrawTextOnVodScreenPictureBox(Path.Combine(Application.StartupPath, @"themes\superstar\點播介面\點播介面_有按鈕.png"), currentSelectedSong);
                SetVodScreenPictureBoxAndButtonsVisibility(true);
                this.ResumeLayout(true);
            }
        }

        public static void WriteLog(string message)
        {
            
            string logFilePath = "logfile.txt"; 

            
            using (StreamWriter sw = new StreamWriter(logFilePath, true)) 
            {
                
                sw.WriteLine(String.Format("{0}: {1}", DateTime.Now, message));
            }
        }

        public static void PrintPlayingSongList()
        {
            Console.WriteLine("當前播放列表:");
            foreach (var song in userRequestedSongs)
            {
                
                string outputText = !string.IsNullOrWhiteSpace(song.ArtistB)
                                    ? String.Format("{0} - {1} - {2}", song.ArtistA, song.ArtistB, song.Song)
                                    : String.Format("{0} - {1}", song.ArtistA, song.Song);
                
                Console.WriteLine(outputText);
            }
        }

        private void NextPageButton_Click(object sender, EventArgs e)
        {
            multiPagePanel.LoadNextPage();
        }

        private void SimplifiedChineseButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("切換到簡體顯示");  // 添加日誌
            if (multiPagePanel != null)
            {
                multiPagePanel.IsSimplified = true;
                btnSimplifiedChinese.Enabled = false;
                btnTraditionalChinese.Enabled = true;
            }
        }

        private void TraditionalChineseButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("切換到繁體顯示");  // 添加日誌
            if (multiPagePanel != null)
            {
                multiPagePanel.IsSimplified = false;
                btnSimplifiedChinese.Enabled = true;
                btnTraditionalChinese.Enabled = false;
            }
        }

        private void PreviousPageButton_Click(object sender, EventArgs e)
        {
            multiPagePanel.LoadPreviousPage();
        }
        
        private void LoadSongData()
        {
            userRequestedSongs = new List<SongData>();
            publicSongList = new List<SongData>();
            playedSongsHistory = new List<SongData>();
            playStates = new List<PlayState>();

            try 
            {
                // 1. 檢查是否能連接到 SVR01
                string serverVideoPath = @"\\SVR01\SuperstarB\video";
                string localVideoPath = @"C:\video";

                if (!Directory.Exists(serverVideoPath))
                {
                    Console.WriteLine("未連接到SVR，使用本地影片");
                    LoadLocalVideoFiles();
                    return;
                }

                // 2. 比較本地和服務器文件夾
                if (!Directory.Exists(localVideoPath))
                {
                    Directory.CreateDirectory(localVideoPath);
                }

                // 獲取服務器和本地的所有文件
                var serverFiles = Directory.GetFiles(serverVideoPath, "*.mpg")
                                         .Select(f => new FileInfo(f))
                                         .ToDictionary(f => f.Name, f => f);
                
                var localFiles = Directory.GetFiles(localVideoPath, "*.mpg")
                                        .Select(f => new FileInfo(f))
                                        .ToDictionary(f => f.Name, f => f);

                // 3. 檢查並更新文件
                foreach (var serverFile in serverFiles)
                {
                    bool needsCopy = false;
                    string localFilePath = Path.Combine(localVideoPath, serverFile.Key);

                    if (!localFiles.ContainsKey(serverFile.Key))
                    {
                        needsCopy = true;
                    }
                    else
                    {
                        var localFile = localFiles[serverFile.Key];
                        if (serverFile.Value.LastWriteTime > localFile.LastWriteTime)
                        {
                            needsCopy = true;
                        }
                    }

                    if (needsCopy)
                    {
                        try
                        {
                            File.Copy(serverFile.Value.FullName, localFilePath, true);
                            Console.WriteLine($"更新影片: {serverFile.Key}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"複製影片失敗 {serverFile.Key}: {ex.Message}");
                        }
                    }
                }

                // 4. 載入更新後的本地文件
                LoadLocalVideoFiles();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新影片失敗：{ex.Message}");
                LoadLocalVideoFiles(); // 出錯時使用本地文件
            }
        }

        // 將原本的本地文件載入邏輯抽取為單獨的方法
        private void LoadLocalVideoFiles()
        {
            try
            {
                string videoDirectory = @"C:\video\";
                string[] videoFiles = Directory.GetFiles(videoDirectory, "*.mpg");
                string pattern = @"^(?<songNumber>\d+)-.*?-(?<songName>[^-]+)-";

                foreach (var songPath in videoFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(songPath);
                    Match match = Regex.Match(fileName, pattern);

                    if (match.Success)
                    {
                        string songNumber = match.Groups["songNumber"].Value;
                        string songName = match.Groups["songName"].Value;

                        SongData song = new SongData(
                            songNumber,           // songNumber
                            "",                   // songNumberB
                            songName,            // song
                            0,                   // songLength
                            "",                  // artistA
                            "",                  // artistB
                            "",                  // language
                            "",                  // category
                            DateTime.Now,        // dateAdded
                            songPath,            // songFilePathHost1
                            "",                  // songFilePathHost2
                            "",                  // songFilePathHost3
                            "",                  // songFilePathHost4
                            "",                  // songFilePathHost5
                            "",                  // songFilePathHost6
                            "",                  // songFilePathHost7
                            "",                  // songFilePathHost8
                            "",                  // songFilePathHost9
                            "",                  // songFilePathHost10
                            "",                  // humanVoice
                            "",                  // songType
                            1                    // priority
                        );
                        publicSongList.Add(song);
                    }
                    else
                    {
                        SongData song = new SongData(
                            "",                  // songNumber
                            "",                  // songNumberB
                            "",                  // song
                            0,                   // songLength
                            "",                  // artistA
                            "",                  // artistB
                            "",                  // language
                            "",                  // category
                            DateTime.Now,        // dateAdded
                            songPath,            // songFilePathHost1
                            "",                  // songFilePathHost2
                            "",                  // songFilePathHost3
                            "",                  // songFilePathHost4
                            "",                  // songFilePathHost5
                            "",                  // songFilePathHost6
                            "",                  // songFilePathHost7
                            "",                  // songFilePathHost8
                            "",                  // songFilePathHost9
                            "",                  // songFilePathHost10
                            "",                  // humanVoice
                            "",                  // songType
                            1                    // priority
                        );
                        publicSongList.Add(song);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read video files: {ex.Message}");
            }
        }

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void LoadImages()
        {
            
            string selectedThemePath = ReadSelectedThemePath(); 
            
            string basePath = Path.Combine(Application.StartupPath, selectedThemePath);
            int targetWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            int targetHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;

            normalStateImage = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\555009.jpg"));
            
            resizedNormalStateImage = ResizeImage(normalStateImage, targetWidth, targetHeight);

            mouseOverImage = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\主選單_已按.jpg"));
            
            resizedMouseOverImage = ResizeImage(mouseOverImage, targetWidth, targetHeight);

            mouseDownImage = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\主選單_已按.jpg"));
            
            resizedMouseDownImage = ResizeImage(mouseDownImage, targetWidth, targetHeight);

            normalStateImageNewSongAlert = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\新歌快報_未按.jpg"));
            
            resizedNormalStateImageForNewSongAlert = ResizeImage(normalStateImageNewSongAlert, targetWidth, targetHeight);

            mouseOverImageNewSongAlert = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\新歌快報_已按.jpg"));
            
            resizedMouseOverImageForNewSongAlert = ResizeImage(mouseOverImageNewSongAlert, targetWidth, targetHeight);

            mouseDownImageNewSongAlert = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\新歌快報_已按.jpg"));
            
            resizedMouseDownImageForNewSongAlert = ResizeImage(mouseDownImageNewSongAlert, targetWidth, targetHeight);

            normalStateImageArtistQuery = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\3.歌星查詢_未按.jpg"));
            
            resizedNormalStateImageForArtistQuery = ResizeImage(normalStateImageArtistQuery, targetWidth, targetHeight);

            mouseOverImageArtistQuery = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\3.歌星查詢_已按.jpg"));
            
            resizedMouseOverImageForArtistQuery = ResizeImage(mouseOverImageArtistQuery, targetWidth, targetHeight);

            mouseDownImageArtistQuery = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\3.歌星查詢_已按.jpg"));
            
            resizedMouseDownImageForArtistQuery = ResizeImage(mouseDownImageArtistQuery, targetWidth, targetHeight);

            normalStateImageSongQuery = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\4.歌名查詢_未按.jpg"));
            
            resizedNormalStateImageForSongQuery = ResizeImage(normalStateImageSongQuery, targetWidth, targetHeight);

            mouseOverImageSongQuery = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\4.歌名查詢_已按.jpg"));
            
            resizedMouseOverImageForSongQuery = ResizeImage(mouseOverImageSongQuery, targetWidth, targetHeight);

            mouseDownImageSongQuery = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\4.歌名查詢_已按.jpg"));
            
            resizedMouseDownImageForSongQuery = ResizeImage(mouseDownImageSongQuery, targetWidth, targetHeight);

            
            normalStateImageLanguageQuery = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\語別查詢_未按.jpg"));
            resizedNormalStateImageForLanguageQuery = ResizeImage(normalStateImageLanguageQuery, targetWidth, targetHeight);

            mouseOverImageLanguageQuery = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\語別查詢_已按.jpg"));
            resizedMouseOverImageForLanguageQuery = ResizeImage(mouseOverImageLanguageQuery, targetWidth, targetHeight);

            mouseDownImageLanguageQuery = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\語別查詢_已按.jpg"));
            resizedMouseDownImageForLanguageQuery = ResizeImage(mouseDownImageLanguageQuery, targetWidth, targetHeight);

            
            normalStateImage6_1 = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\6-1.png"));
            resizedNormalStateImageFor6_1 = ResizeImage(normalStateImage6_1, targetWidth, targetHeight);

            mouseOverImage6_1 = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\6-1.mouseover.png"));
            resizedMouseOverImageFor6_1 = ResizeImage(mouseOverImage6_1, targetWidth, targetHeight);

            mouseDownImage6_1 = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\6-1.mousedown.png"));
            resizedMouseDownImageFor6_1 = ResizeImage(mouseDownImage6_1, targetWidth, targetHeight);

            normalStateImageCategoryQuery = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\類別查詢_未按.jpg"));
            resizedNormalStateImageForCategoryQuery = ResizeImage(normalStateImageCategoryQuery, targetWidth, targetHeight);

            mouseOverImageCategoryQuery = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\類別查詢_已按.jpg"));
            resizedMouseOverImageForCategoryQuery = ResizeImage(mouseOverImageCategoryQuery, targetWidth, targetHeight);

            mouseDownImageCategoryQuery = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\類別查詢_已按.jpg"));
            resizedMouseDownImageForCategoryQuery = ResizeImage(mouseDownImageCategoryQuery, targetWidth, targetHeight);

            
            normalStateImage7_1 = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\7-1.png"));
            resizedNormalStateImageFor7_1 = ResizeImage(normalStateImage7_1, targetWidth, targetHeight);

            mouseOverImage7_1 = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\7-1.mouseover.png"));
            resizedMouseOverImageFor7_1 = ResizeImage(mouseOverImage7_1, targetWidth, targetHeight);

            mouseDownImage7_1 = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\7-1.mousedown.png"));
            resizedMouseDownImageFor7_1 = ResizeImage(mouseDownImage7_1, targetWidth, targetHeight);

            
            normalStateImage7_1_1 = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\7-1-1.png"));
            resizedNormalStateImageFor7_1_1 = ResizeImage(normalStateImage7_1_1, targetWidth, targetHeight);

            mouseOverImage7_1_1 = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\7-1-1.mouseover.png"));
            resizedMouseOverImageFor7_1_1 = ResizeImage(mouseOverImage7_1_1, targetWidth, targetHeight);

            mouseDownImage7_1_1 = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\7-1-1.mousedown.png"));
            resizedMouseDownImageFor7_1_1 = ResizeImage(mouseDownImage7_1_1, targetWidth, targetHeight);

            normalStateImageForPromotionsAndMenu = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\555021.jpg"));
            resizedNormalStateImageForPromotionsAndMenu = ResizeImage(normalStateImageForPromotionsAndMenu, targetWidth, targetHeight);

            try
            {
                string imagePath = Path.Combine(Application.StartupPath, "themes\\superstar\\555019.jpg");

                if (File.Exists(imagePath))
                {
                    normalStateImageForSyncScreen = new Bitmap(imagePath);
                    resizedNormalStateImageForSyncScreen = ResizeImage(normalStateImageForSyncScreen, targetWidth, targetHeight);
                    Console.WriteLine("Image loaded successfully.");
                }
                else
                {
                    Console.WriteLine("Image file does not exist: " + imagePath);
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load image: " + ex.Message);
                
            }

            normalStateImageForSceneSoundEffects = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\555022.jpg"));
            resizedNormalStateImageForSceneSoundEffects = ResizeImage(normalStateImageForSceneSoundEffects, targetWidth, targetHeight);


            normalStateImageHotSong = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\熱門排行_未按.jpg"));
            resizedNormalStateImageForHotSong = ResizeImage(normalStateImageHotSong, targetWidth, targetHeight);

            mouseDownImageHotSong = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\熱門排行_已按.jpg"));
            resizedMouseDownImageForHotSong = ResizeImage(mouseDownImageHotSong, targetWidth, targetHeight);

            normalStateImageForLightControl = new Bitmap(Path.Combine(Application.StartupPath, "themes\\superstar\\選單內介面_燈光控制.jpg"));
            resizedNormalStateImageForLightControl = ResizeImage(normalStateImageForLightControl, targetWidth, targetHeight);
        }

        public Bitmap MakeTransparentImage(string imagePath, Color maskColor)
        {
            Bitmap bmp = new Bitmap(imagePath);
            
            bmp.MakeTransparent(maskColor); 
            
            return bmp;
        }

        
        private void ShowImageOnPictureBoxArtistSearch(string imagePath)
        {
            Bitmap originalImage = new Bitmap(imagePath);

            
            Rectangle cropArea = new Rectangle(593, 135, 507, 508);

            
            Bitmap croppedImage = CropImage(originalImage, cropArea);

            
            pictureBoxArtistSearch.Image = croppedImage;
    
            
            ResizeAndPositionPictureBox(pictureBoxArtistSearch, cropArea.X + offsetXWordCount, cropArea.Y + offsetXWordCount, cropArea.Width, cropArea.Height);
            
            pictureBoxArtistSearch.Visible = true;
        }

        private Bitmap CropImage(Bitmap source, Rectangle section)
        {
            
            Bitmap bmp = new Bitmap(section.Width, section.Height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                
                g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
            }

            return bmp;
        }

        private void DrawTextOnVodScreenPictureBox(string imagePath, SongData songData)
        {
            
            Bitmap originalImage = new Bitmap(imagePath);

            
            VodScreenPictureBox.Image = originalImage;

            int screenWidth = 1440;
            int screenHeight = 900;
            int pictureBoxWidth = 700;
            int pictureBoxHeight = 140;

            int xPosition = (screenWidth - pictureBoxWidth) / 2;
            int yPosition = (screenHeight - pictureBoxHeight) / 2;

            
            using (Graphics g = Graphics.FromImage(originalImage))
            {
                float dpiX = g.DpiX;
                float points = 25;  
                float pixels = points * (dpiX / 72);  

                
                Font font = new Font("微軟正黑體", points, FontStyle.Bold);
                Brush textBrush = Brushes.Black;

                
                string songInfo = songData.Song ?? "未提供歌曲信息";

                
                g.DrawString(songInfo, font, textBrush, new PointF(201, 29)); 

                
            }
    
            
            ResizeAndPositionPictureBox(VodScreenPictureBox, xPosition, yPosition, pictureBoxWidth, pictureBoxHeight);
            
            VodScreenPictureBox.Visible = true;
        }

        public static void ResizeAndPositionButton(Button button, int x, int y, int width, int height)
        {
            // 獲取螢幕尺寸
            Screen screen = Screen.PrimaryScreen;
            int screenWidth = screen.Bounds.Width;
            int screenHeight = screen.Bounds.Height;
            
            // 計算縮放比例
            float widthRatio = screenWidth / 1440f;
            float heightRatio = screenHeight / 900f;
            
            // 調整按鈕位置和大小
            button.Location = new Point(
                (int)(x * widthRatio),
                (int)(y * heightRatio)
            );
            button.Size = new Size(
                (int)(width * widthRatio),
                (int)(height * heightRatio)
            );
        }

        private static void ResizeAndPositionLabel(Label label, int originalX, int originalY, int originalWidth, int originalHeight)
        {
            int screenW = Screen.PrimaryScreen.Bounds.Width;
            int screenH = Screen.PrimaryScreen.Bounds.Height;

            float widthRatio = screenW / (float)1440; 
            float heightRatio = screenH / (float)900; 

            
            label.Location = new Point(
                (int)(originalX * widthRatio),
                (int)(originalY * heightRatio)
            );
            label.Size = new Size(
                (int)(originalWidth * widthRatio),
                (int)(originalHeight * heightRatio)
            );
        }

        private static void ResizeAndPositionPictureBox(PictureBox pictureBox, int originalX, int originalY, int originalWidth, int originalHeight)
        {
            int screenW = Screen.PrimaryScreen.Bounds.Width;
            int screenH = Screen.PrimaryScreen.Bounds.Height;

            float widthRatio = screenW / (float)1440; 
            float heightRatio = screenH / (float)900; 

            
            pictureBox.Location = new Point(
                (int)(originalX * widthRatio),
                (int)(originalY * heightRatio)
            );
            pictureBox.Size = new Size(
                (int)(originalWidth * widthRatio),
                (int)(originalHeight * heightRatio)
            );
        }

        public static void ResizeAndPositionControl(Control control, int x, int y, int width, int height)
        {
            // 獲取螢幕尺寸
            Screen screen = Screen.PrimaryScreen;
            int screenWidth = screen.Bounds.Width;
            int screenHeight = screen.Bounds.Height;
            
            // 計算縮放比例
            float widthRatio = screenWidth / 1440f;
            float heightRatio = screenHeight / 900f;
            
            // 調整控件位置和大小
            control.Location = new Point(
                (int)(x * widthRatio),
                (int)(y * heightRatio)
            );
            control.Size = new Size(
                (int)(width * widthRatio),
                (int)(height * heightRatio)
            );
        }

        private void DeliciousFoodButton_Click(object sender, EventArgs e)
        {
            newSongAlertButton.BackgroundImage = newSongAlertNormalBackground;
            hotPlayButton.BackgroundImage = hotPlayNormalBackground;
            singerSearchButton.BackgroundImage = singerSearchNormalBackground;
            songSearchButton.BackgroundImage = songSearchNormalBackground;
            languageSearchButton.BackgroundImage = languageSearchNormalBackground;
            groupSearchButton.BackgroundImage = groupSearchNormalBackground;
            categorySearchButton.BackgroundImage = categorySearchNormalBackground;
            orderedSongsButton.BackgroundImage = orderedSongsNormalBackground;
            myFavoritesButton.BackgroundImage = myFavoritesNormalBackground;
            promotionsButton.BackgroundImage = promotionsNormalBackground;
            deliciousFoodButton.BackgroundImage = deliciousFoodActiveBackground;
            isOnOrderedSongsPage = false;
            

            SetHotSongButtonsVisibility(false);
            SetNewSongButtonsVisibility(false);
            SetSingerSearchButtonsVisibility(false);
            SetSongSearchButtonsVisibility(false);
            SetPictureBoxLanguageButtonsVisibility(false);
            SetGroupButtonsVisibility(false);
            SetZhuYinSingersAndButtonsVisibility(false);
            SetZhuYinSongsAndButtonsVisibility(false);
            SetEnglishSingersAndButtonsVisibility(false);
            SetEnglishSongsAndButtonsVisibility(false);
            SetPinYinSingersAndButtonsVisibility(false);
            SetPinYinSongsAndButtonsVisibility(false);
            SetPictureBoxCategoryAndButtonsVisibility(false);

            
            

            
            
            promotionsAndMenuPanel.currentPageIndex = 0;

            
            
            promotionsAndMenuPanel.LoadImages(menu); 
            promotionsAndMenuPanel.Visible = true;
            promotionsAndMenuPanel.BringToFront();

            previousPromotionButton.Visible = true;
            previousPromotionButton.BringToFront();
            nextPromotionButton.Visible = true;
            nextPromotionButton.BringToFront();
            
            closePromotionsButton.Visible = true;
            closePromotionsButton.BringToFront();

            
            if (pictureBoxQRCode != null)
            {
                pictureBoxQRCode.Visible = false;
                closeQRCodeButton.Visible = false;
            }
        }

        private void MobileSongRequestButton_Click(object sender, EventArgs e)
        {
            
            SetHotSongButtonsVisibility(false);
            SetNewSongButtonsVisibility(false);
            SetSingerSearchButtonsVisibility(false);
            SetSongSearchButtonsVisibility(false);
            SetPictureBoxLanguageButtonsVisibility(false);
            SetZhuYinSingersAndButtonsVisibility(false);
            SetZhuYinSongsAndButtonsVisibility(false);
            SetEnglishSingersAndButtonsVisibility(false);
            SetEnglishSongsAndButtonsVisibility(false);
            SetPinYinSingersAndButtonsVisibility(false);
            SetPinYinSongsAndButtonsVisibility(false);
            SetGroupButtonsVisibility(false);

            
            if (pictureBoxQRCode != null)
            {
                pictureBoxQRCode.Visible = true;
                pictureBoxQRCode.BringToFront(); 

                closeQRCodeButton.Visible = true;
                closeQRCodeButton.BringToFront();
            }
            else
            {
                Console.WriteLine("pictureBoxQRCode is not initialized!");
            }
        }

         
       
        public async void OriginalSongButton_Click(object sender, EventArgs e)
        {
            
            videoPlayerForm.ToggleVocalRemoval();
            
            
        }

        private void ReplayButton_Click(object sender, EventArgs e)
        {
            videoPlayerForm.ReplayCurrentSong();
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            videoPlayerForm.Pause();

            
            pauseButton.Visible = false;
            playButton.Visible = true;
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            videoPlayerForm.Play();

            
            playButton.Visible = false;
            pauseButton.Visible = true;
        }

        private void ShouYeButton_Click(object sender, EventArgs e)
        {
            autoRefreshTimer.Stop(); // 停止自动刷新
            
            SetHotSongButtonsVisibility(false);
            SetNewSongButtonsVisibility(false);
            SetSingerSearchButtonsVisibility(false);
            SetSongSearchButtonsVisibility(false);
            SetPictureBoxLanguageButtonsVisibility(false);
            SetGroupButtonsVisibility(false);
            SetZhuYinSingersAndButtonsVisibility(false);
            SetZhuYinSongsAndButtonsVisibility(false);
            SetEnglishSingersAndButtonsVisibility(false);
            SetEnglishSongsAndButtonsVisibility(false);
            SetPictureBoxWordCountAndButtonsVisibility(false);
            SetPinYinSingersAndButtonsVisibility(false);
            SetPinYinSongsAndButtonsVisibility(false);
            SetPictureBoxToggleLightAndButtonsVisibility(false);
            inputBoxZhuYinSingers.Text = "";
            inputBoxZhuYinSongs.Text = "";
            inputBoxEnglishSingers.Text = "";
            inputBoxEnglishSongs.Text = "";
            inputBoxPinYinSingers.Text = "";
            inputBoxPinYinSongs.Text = "";
            inputBoxWordCount.Text = "";
            
            
            foreach (var label in songLabels)
            {
                this.Controls.Remove(label);
                label.Dispose();
            }
            songLabels.Clear(); 
        }

        
        private void MuteUnmuteButton_Click(object sender, EventArgs e)
        {
            
            SetHotSongButtonsVisibility(false);
            SetNewSongButtonsVisibility(false);
            SetSingerSearchButtonsVisibility(false);
            SetSongSearchButtonsVisibility(false);
            SetPictureBoxToggleLightAndButtonsVisibility(false);

            if (videoPlayerForm.isMuted)
            {
                
                videoPlayerForm.SetVolume(videoPlayerForm.previousVolume);
                
                videoPlayerForm.isMuted = false;
                OverlayForm.MainForm.HideMuteLabel();
            }
            else
            {
                
                videoPlayerForm.previousVolume = videoPlayerForm.GetVolume();
                videoPlayerForm.SetVolume(-10000);
                
                videoPlayerForm.isMuted = true;
                OverlayForm.MainForm.ShowMuteLabel();
            }
        }

        private string GetRoomNumber()
        {
            string roomNumberFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "RoomNumber.txt");
            try
            {
                
                string roomNumber = File.ReadLines(roomNumberFilePath).First();
                return roomNumber;
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("讀取包廂號文件出錯: {0}", ex.Message));
                return null; 
            }
        }

        
        private void MaleKeyButton_Click(object sender, EventArgs e)
        {
            
            if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
            {
                
                byte[] commandBytesIncreasePitch1 = new byte[] { 0xA2, 0x7F, 0xA4 };         
                SerialPortManager.mySerialPort.Write(commandBytesIncreasePitch1, 0, commandBytesIncreasePitch1.Length);
                OverlayForm.MainForm.HideAllLabels();
                byte[] commandBytesDecreasePitch = new byte[] { 0xA2, 0xB2, 0xA4 };
                SerialPortManager.mySerialPort.Write(commandBytesDecreasePitch, 0, commandBytesDecreasePitch.Length);
                OverlayForm.MainForm.HideAllLabels();
                SerialPortManager.mySerialPort.Write(commandBytesDecreasePitch, 0, commandBytesDecreasePitch.Length);
                OverlayForm.MainForm.HideAllLabels();
                
            }
            else
            {
                MessageBox.Show("串口未開啟，無法發送降調指令。");
            }
            OverlayForm.MainForm.ShowMaleKeyLabel();
        }
        
        private void FemaleKeyButton_Click(object sender, EventArgs e)
        {
            
            if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
            {
                
                byte[] commandBytesIncreasePitch1 = new byte[] { 0xA2, 0x7F, 0xA4 };
                SerialPortManager.mySerialPort.Write(commandBytesIncreasePitch1, 0, commandBytesIncreasePitch1.Length);
                OverlayForm.MainForm.HideAllLabels();
                
                
                byte[] commandBytesIncreasePitch = new byte[] { 0xA2, 0xB1, 0xA4 };
                SerialPortManager.mySerialPort.Write(commandBytesIncreasePitch, 0, commandBytesIncreasePitch.Length);
                 OverlayForm.MainForm.HideAllLabels();
                SerialPortManager.mySerialPort.Write(commandBytesIncreasePitch, 0, commandBytesIncreasePitch.Length); 
                 OverlayForm.MainForm.HideAllLabels();

                
                
            }
            else
            {
                
                MessageBox.Show("串口未開啟，無法發送升調指令。");
            }
            OverlayForm.MainForm.ShowFemaleKeyLabel();
        }

        private void StandardKeyButton_Click(object sender, EventArgs e)
        {
            OverlayForm.MainForm.ShowStandardKeyLabel();
            
            if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
            {
                
                byte[] commandBytesIncreasePitch = new byte[] { 0xA2, 0x7F, 0xA4 };
                SerialPortManager.mySerialPort.Write(commandBytesIncreasePitch, 0, commandBytesIncreasePitch.Length);
                
            }
            else
            {
                MessageBox.Show("串口未開啟，無法發送升調指令。");
            }
        }

        
        private void PitchUpButton_Click(object sender, EventArgs e)
        {
            OverlayForm.MainForm.ShowKeyUpLabel();

            
            if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
            {
                
                byte[] commandBytesIncreasePitch = new byte[] { 0xA2, 0xB1, 0xA4 };
                SerialPortManager.mySerialPort.Write(commandBytesIncreasePitch, 0, commandBytesIncreasePitch.Length);
                
            }
            else
            {
                MessageBox.Show("串口未開啟，無法發送升調指令。");
            }
        }

        private void PitchDownButton_Click(object sender, EventArgs e)
        {
            OverlayForm.MainForm.ShowKeyDownLabel();

            
            if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
            {
                
                byte[] commandBytesDecreasePitch = new byte[] { 0xA2, 0xB2, 0xA4 };
                SerialPortManager.mySerialPort.Write(commandBytesDecreasePitch, 0, commandBytesDecreasePitch.Length);
                
            }
            else
            {
                MessageBox.Show("串口未開啟，無法發送降調指令。");
            }
        }

        
        private void HardMuteButton_Click(object sender, EventArgs e)
        {
            
            MessageBox.Show("硬是消音 功能開發中...");
        }

        
        private void TrackCorrectionButton_Click(object sender, EventArgs e)
        {
            
            if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
            {
                
                byte[] commandBytes = new byte[] { 0xA2, 0xD5, 0xA4 };
                
                
                SerialPortManager.mySerialPort.Write(commandBytes, 0, commandBytes.Length);

                MessageBox.Show("音軌修正指令已發送.");
            }
            else
            {
                MessageBox.Show("Serial port is not open. Cannot send track correction command.");
            }
        }

        
        private void ApplauseButton_Click(object sender, EventArgs e)
        {
            PlayApplauseSound();
        }

        
        private void CheerButton_Click(object sender, EventArgs e)
        {
            
            MessageBox.Show("歡呼 功能開發中...");
        }

        
        private void MockButton_Click(object sender, EventArgs e)
        {
            
            MessageBox.Show("嘲笑 功能開發中...");
        }

        
        private void BooButton_Click(object sender, EventArgs e)
        {
            
            MessageBox.Show("噓聲 功能開發中...");
        }
        

        

        
        private bool isWaiting = false;
        private async void OnServiceBellButtonClick(object sender, EventArgs e)
        {
            if (isWaiting) return;

            isWaiting = true;
            
            // 发送串口命令
            SendCommandThroughSerialPort("a2 53 a4");

            // 显示提示信息
            OverlayForm.MainForm.ShowServiceBellLabel();

            // 延迟3秒
            await Task.Delay(3000);

            // 隐藏提示信息
            OverlayForm.MainForm.HideServiceBellLabel();
            
            isWaiting = false;
        }

        private void SaveInitialControlPositions()
        {
            foreach (Control control in this.Controls)
            {
                initialControlPositions[control] = control.Location;
            }
        }

        private void RestoreInitialControlPositions()
        {
            foreach (Control control in this.Controls)
            {
                if (initialControlPositions.ContainsKey(control))
                {
                    control.Location = initialControlPositions[control];
                }
            }
        }

        private void InitializeSendOffPanel()
        {
            sendOffPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Visible = false
            };

            sendOffPictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = LoadSendOffImage()
            };

            // 初始化服務鈴圖標
            serviceBellPictureBox = new PictureBox { 
                Visible = true,
                Size = new Size(410, 130),  // 调整为更大的尺寸
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = LoadButtonImage("服務鈴.png"),
                Cursor = System.Windows.Forms.Cursors.Hand,
                Location = new Point(757, 151)  // 调整位置到顶部
            };
            
            // 添加服務鈴點擊事件
            serviceBellPictureBox.Click += (sender, e) => {
                if (serviceBellButton != null)
                {
                    OnServiceBellButtonClick(sender, e);
                }
            };

            // 使用 PictureBox 並加載小圖片
            buttonMiddle = new PictureBox { 
                Visible = true,
                Size = new Size(80, 80),  // 稍微縮小一點
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = LoadButtonImage("巨.png"),
                Cursor = System.Windows.Forms.Cursors.Hand
            };

            buttonTopRight = new PictureBox { 
                Visible = true,
                Size = new Size(80, 80),
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = LoadButtonImage("級.png"),
                Cursor = System.Windows.Forms.Cursors.Hand
            };

            buttonTopLeft = new PictureBox { 
                Visible = true,
                Size = new Size(150, 150),
                BackColor = Color.FromArgb(255, 150, 180),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = LoadButtonImage("超.png"),
                Cursor = System.Windows.Forms.Cursors.Hand
            };

            buttonThanks = new PictureBox { 
                Visible = true,
                Size = new Size(150, 150),
                BackColor = Color.FromArgb(255, 150, 180),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = LoadButtonImage("星.png"),
                Cursor = System.Windows.Forms.Cursors.Hand
            };


            buttonTopLeft.Location = new Point(1598,490 ); // 右下
            buttonTopRight.Location = new Point(Screen.PrimaryScreen.Bounds.Width - 80, 0);  // 右上
            buttonMiddle.Location = new Point(0,0);  //左上
            buttonThanks.Location = new Point(831,490);
            

            // 添加點擊事件
            buttonMiddle.Click += buttonMiddle_Click;
            buttonTopRight.Click += buttonTopRight_Click;
            buttonTopLeft.Click += buttonTopLeft_Click;
            buttonThanks.Click += buttonThanks_Click;

            // 添加控件到面板
            sendOffPanel.Controls.Add(sendOffPictureBox);
            sendOffPanel.Controls.Add(serviceBellPictureBox);  // 添加服務鈴
            sendOffPanel.Controls.Add(buttonMiddle);
            sendOffPanel.Controls.Add(buttonTopRight);
            sendOffPanel.Controls.Add(buttonTopLeft);
            sendOffPanel.Controls.Add(buttonThanks);

            // 確保圖片在最上層
            serviceBellPictureBox.BringToFront();  // 確保服務鈴在最上層
            buttonMiddle.BringToFront();
            buttonTopRight.BringToFront();
            buttonTopLeft.BringToFront();
            buttonThanks.BringToFront();

            this.Controls.Add(sendOffPanel);
        }

        // 添加載入按鈕圖片的方法
        private Image LoadButtonImage(string imageName)
        {
            try
            {
                string filePath = Path.Combine(Application.StartupPath, "ButtonImages", imageName);
                if (File.Exists(filePath))
                {
                    return Image.FromFile(filePath);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"載入按鈕圖片時發生錯誤: {ex.Message}");
                return null;
            }
        }

        private Image LoadSendOffImage()
        {
            try
            {
                string filePath = Path.Combine(Application.StartupPath, "VOD_送客畫面.jpg");
                if (File.Exists(filePath))
                {
                    return Image.FromFile(filePath);
                }
                else
                {
                    Console.WriteLine("VOD_送客畫面.jpg 文件未找到。");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加載送客畫面圖片時發生錯誤: {ex.Message}");
                return null;
            }
        }

        // 將 SequenceManager 類保留在這裡
        private class SequenceManager
        {
            private List<string> correctSequence = new List<string> { "超", "級", "巨", "星" };
            private List<string> currentSequence = new List<string>();

            public void ProcessClick(string buttonName)
            {
                currentSequence.Add(buttonName);

                // 檢查是否點擊錯誤
                if (currentSequence.Count <= correctSequence.Count)
                {
                    if (currentSequence[currentSequence.Count - 1] != correctSequence[currentSequence.Count - 1])
                    {
                        // 順序錯誤，重置序列
                        currentSequence.Clear();
                        return;
                    }
                }

                // 檢查是否完成正確序列
                if (currentSequence.Count == correctSequence.Count)
                {
                    try
                    {
                        // 使用 Windows 命令關機
                        System.Diagnostics.Process.Start("shutdown", "/s /t 0");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"關機失敗: {ex.Message}");
                        // 如果關機失敗，退出程式
                        Application.Exit();
                    }
                }
            }
        }

        // 添加 Form Load 事件處理方法
        private void PrimaryForm_Load(object sender, EventArgs e)
        {
            // 確保所有控件都已初始化完成後，再觸發熱門排行按鈕點擊
            if (hotPlayButton != null)
            {
                HotPlayButton_Click(null, EventArgs.Empty);
            }
        }

        private void AutoRefreshTimer_Tick(object sender, EventArgs e)
        {
            if (isOnOrderedSongsPage)
            {
                multiPagePanel.RefreshDisplay();
            }
        }

        private void OrderedSongsButton_Click(object sender, EventArgs e)
        {
            newSongAlertButton.BackgroundImage = newSongAlertNormalBackground;
            hotPlayButton.BackgroundImage = hotPlayNormalBackground;
            singerSearchButton.BackgroundImage = singerSearchNormalBackground;
            songSearchButton.BackgroundImage = songSearchNormalBackground;
            languageSearchButton.BackgroundImage = languageSearchNormalBackground;
            groupSearchButton.BackgroundImage = groupSearchNormalBackground;
            categorySearchButton.BackgroundImage = categorySearchNormalBackground;
            orderedSongsButton.BackgroundImage = orderedSongsActiveBackground;
            myFavoritesButton.BackgroundImage = myFavoritesNormalBackground;
            promotionsButton.BackgroundImage = promotionsNormalBackground;
            deliciousFoodButton.BackgroundImage = deliciousFoodNormalBackground;

            isOnOrderedSongsPage = true;
            autoRefreshTimer.Start(); // 开始自动刷新

            currentSongList = playedSongsHistory;
            totalPages = (int)Math.Ceiling((double)playedSongsHistory.Count / itemsPerPage);
            multiPagePanel.currentPageIndex = 0;

            SetHotSongButtonsVisibility(false);
            SetNewSongButtonsVisibility(false);
            SetSingerSearchButtonsVisibility(false);
            SetSongSearchButtonsVisibility(false);
            SetPictureBoxLanguageButtonsVisibility(false);
            SetGroupButtonsVisibility(false);
            SetZhuYinSingersAndButtonsVisibility(false);
            SetZhuYinSongsAndButtonsVisibility(false);
            SetEnglishSingersAndButtonsVisibility(false);
            SetEnglishSongsAndButtonsVisibility(false);
            SetPictureBoxWordCountAndButtonsVisibility(false);
            SetPinYinSingersAndButtonsVisibility(false);
            SetPinYinSongsAndButtonsVisibility(false);
            SetPictureBoxToggleLightAndButtonsVisibility(false);
            SetPictureBoxSceneSoundEffectsAndButtonsVisibility(false);

            if (pictureBoxQRCode != null)
            {
                pictureBoxQRCode.Visible = false;
                closeQRCodeButton.Visible = false;
            }

            multiPagePanel.LoadPlayedSongs(currentSongList, playStates);
        }
    }
}