using System;
using System.Collections.Generic;
using System.IO; // For StreamWriter
using System.Drawing; // For Size
using System.Linq; // For LINQ methods like Any
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading.Tasks;
using DirectShowLib;

namespace KTV_Superstar;

public class VideoPlayerForm: Form {
    #region 防止閃屏
    public SongData currentPlayingSong = default!;
    public bool IsPlayingPublicSong {
        get;
        private set;
    } = false;
    protected override CreateParams CreateParams {
        get {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= 0x02000000;
            return cp;
        }
    }
    #endregion

    // 单例实例
    public static VideoPlayerForm Instance { get; private set; } = default!;

    // 导入user32.dll API
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    // Windows API 函數
    [DllImport("user32.dll")]
    static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll")]
    static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

    // MONITORINFO 結構
    [StructLayout(LayoutKind.Sequential)]
    struct MONITORINFO {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    // RECT 結構
    [StructLayout(LayoutKind.Sequential)]
    struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private
    const int GWL_EXSTYLE = -20;
    private
    const int WS_EX_TOPMOST = 0x00000008;
    private
    const uint SWP_NOZORDER = 0x0004;

    public static OverlayForm overlayForm = default!;
    public static List < SongData > playingSongList = default!;
    public static List < SongData > publicPlaylist = default!;
    public static int currentSongIndex = 0;
    private static bool isUserPlaylistPlaying = false;
    public bool isMuted = false;
    public int previousVolume = -1000;
    public bool IsPaused = false;
    private bool isSyncToPrimaryMonitor = false;

    // 媒體播放相關邏輯封裝在 MediaRenderer 中
    public MediaRenderer mediaRenderer = default!;

    public bool IsSyncToPrimaryMonitor {
        get {
            return isSyncToPrimaryMonitor;
        }
        set {
            isSyncToPrimaryMonitor = value;
        }
    }

    private static Screen secondMonitor = default!;

    public VideoPlayerForm() {
        Instance = this;
        // this.DoubleBuffered = true;

        InitializeComponent();

        // 檢查這行是不是漏了！
        mediaRenderer = new MediaRenderer();
        
        this.Load += VideoPlayerForm_Load!;
        this.Shown += VideoPlayerForm_Shown!;
        this.FormClosing += VideoPlayerForm_FormClosing!;
        InitializeOverlayForm(secondMonitor);
        BringOverlayToFront();

        HttpServer.OnDisplayBarrage += DisplayBarrageOnOverlay;
        MonitorMediaEvents();
    }

    private void InitializeComponent() {
        
    }

    private void VideoPlayerForm_Load(object sender, EventArgs e) {
        secondMonitor = ScreenHelper.GetSecondMonitor();
        if (secondMonitor != null) {
            this.FormBorderStyle = FormBorderStyle.None; // 设置窗体没有边框
            this.StartPosition = FormStartPosition.Manual;
            this.Location = secondMonitor.Bounds.Location;
            this.Size = secondMonitor.Bounds.Size;
            // this.DoubleBuffered = true;
        }
        CheckMonitor();
    }

    protected override void OnShown(EventArgs e) {
        base.OnShown(e);
        try {
            if (secondMonitor != null) {
                SetWindowPos(this.Handle, IntPtr.Zero, secondMonitor.Bounds.X, secondMonitor.Bounds.Y,
                    secondMonitor.Bounds.Width, secondMonitor.Bounds.Height, 0);
            }
            IntPtr exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, (IntPtr)(exStyle.ToInt32()));
        } catch (Exception ex) {
            Console.WriteLine("An error occurred in OnShown: " + ex.Message);
        }
    }

    private void VideoPlayerForm_Shown(object sender, EventArgs e) {

        int hr = CoInitializeEx(IntPtr.Zero, COINIT.APARTMENTTHREADED);
        if (hr < 0) {
            Console.WriteLine("Failed to initialize COM library.");
            return;
        }

        mediaRenderer.InitializeGraphBuilders();
        Task.Run(() => MonitorMediaEvents());
    }

    private void VideoPlayerForm_FormClosing(object sender, FormClosingEventArgs e) {
        if (mediaRenderer.VideoWindowPrimary != null) {
            mediaRenderer.VideoWindowPrimary.put_Visible(OABool.False);
            mediaRenderer.VideoWindowPrimary.put_Owner(IntPtr.Zero);
            Marshal.ReleaseComObject(mediaRenderer.VideoWindowPrimary);
            mediaRenderer.VideoWindowPrimary = null!;
        }

        if (mediaRenderer.VideoWindowSecondary != null) {
            mediaRenderer.VideoWindowSecondary.put_Visible(OABool.False);
            mediaRenderer.VideoWindowSecondary.put_Owner(IntPtr.Zero);
            Marshal.ReleaseComObject(mediaRenderer.VideoWindowSecondary);
            mediaRenderer.VideoWindowSecondary = null!;
        }
        // 清理COM
        CoUninitialize();
    }

    // COM API函数声明
    [DllImport("ole32.dll")]
    private static extern int CoInitializeEx(IntPtr pvReserved, COINIT dwCoInit);

    [DllImport("ole32.dll")]
    private static extern void CoUninitialize();

    // CoInitializeEx() 可以选择的参数
    private enum COINIT: int {
        APARTMENTTHREADED = 0x2,
            MULTITHREADED = 0x0
    }

    private void ConfigureSampleGrabber(IBaseFilter sampleGrabberFilter) {
        ISampleGrabber sampleGrabber = (ISampleGrabber) sampleGrabberFilter;
        AMMediaType mediaType = new AMMediaType {
            majorType = MediaType.Video,
                subType = MediaSubType.RGB24,
                formatType = FormatType.VideoInfo
        };
        sampleGrabber.SetMediaType(mediaType);
        DsUtils.FreeAMMediaType(mediaType);

        sampleGrabber.SetBufferSamples(false);
        sampleGrabber.SetOneShot(false);
        sampleGrabber.SetCallback(new SampleGrabberCallback(this), 1);
    }

    private void CheckMonitor() {
        Screen screen = Screen.FromHandle(this.Handle);
    }

    public void SyncToPrimaryMonitor() {
        if (PrimaryForm.Instance?.primaryScreenPanel != null)
        {
            PrimaryForm.Instance.primaryScreenPanel.Visible = true;
        }
        if (PrimaryForm.Instance?.primaryScreenPanel != null)
        {
            PrimaryForm.Instance.primaryScreenPanel.BringToFront();
        }
        if (PrimaryForm.Instance?.syncServiceBellButton != null)
        {
            PrimaryForm.Instance.syncServiceBellButton.Visible = true;
        }
        if (PrimaryForm.Instance?.syncServiceBellButton != null)
        {
            PrimaryForm.Instance.syncServiceBellButton.BringToFront();
        }
        if (PrimaryForm.Instance?.syncCutSongButton != null)
        {
            PrimaryForm.Instance.syncCutSongButton.Visible = true;
        }
        if (PrimaryForm.Instance?.syncCutSongButton != null)
        {
            PrimaryForm.Instance.syncCutSongButton.BringToFront();
        }
        if (PrimaryForm.Instance?.syncReplayButton != null)
        {
            PrimaryForm.Instance.syncReplayButton.Visible = true;
        }
        if (PrimaryForm.Instance?.syncReplayButton != null)
        {
            PrimaryForm.Instance.syncReplayButton.BringToFront();
        }
        if (PrimaryForm.Instance?.syncOriginalSongButton != null)
        {
            PrimaryForm.Instance.syncOriginalSongButton.Visible = true;
        }
        if (PrimaryForm.Instance?.syncOriginalSongButton != null)
        {
            PrimaryForm.Instance.syncOriginalSongButton.BringToFront();
        }
        if (PrimaryForm.Instance?.syncMuteButton != null)
        {
            PrimaryForm.Instance.syncMuteButton.Visible = true;
        }
        if (PrimaryForm.Instance?.syncMuteButton != null)
        {
            PrimaryForm.Instance.syncMuteButton.BringToFront();
        }
        if (IsPaused) {
            if (PrimaryForm.Instance?.syncPlayButton != null)
            {
                PrimaryForm.Instance.syncPlayButton.Visible = true;
            }
            if (PrimaryForm.Instance?.syncPlayButton != null)
            {
                PrimaryForm.Instance.syncPlayButton.BringToFront();
            }
        } else {
            if (PrimaryForm.Instance?.syncPauseButton != null)
            {
                PrimaryForm.Instance.syncPauseButton.Visible = true;
            }
            if (PrimaryForm.Instance?.syncPauseButton != null)
            {
                PrimaryForm.Instance.syncPauseButton.BringToFront();
            }
        }
        if (PrimaryForm.Instance?.syncVolumeUpButton != null)
        {
            PrimaryForm.Instance.syncVolumeUpButton.Visible = true;
        }
        if (PrimaryForm.Instance?.syncVolumeUpButton != null)
        {
            PrimaryForm.Instance.syncVolumeUpButton.BringToFront();
        }
        if (PrimaryForm.Instance?.syncVolumeDownButton != null)
        {
            PrimaryForm.Instance.syncVolumeDownButton.Visible = true;
        }
        if (PrimaryForm.Instance?.syncVolumeDownButton != null)
        {
            PrimaryForm.Instance.syncVolumeDownButton.BringToFront();
        }
        if (PrimaryForm.Instance?.syncMicUpButton != null)
        {
            PrimaryForm.Instance.syncMicUpButton.Visible = true;
        }
        if (PrimaryForm.Instance?.syncMicUpButton != null)
        {
            PrimaryForm.Instance.syncMicUpButton.BringToFront();
        }
        if (PrimaryForm.Instance?.syncMicDownButton != null)
        {
            PrimaryForm.Instance.syncMicDownButton.Visible = true;
        }
        if (PrimaryForm.Instance?.syncMicDownButton != null)
        {
            PrimaryForm.Instance.syncMicDownButton.BringToFront();
        }
        if (PrimaryForm.Instance?.syncCloseButton != null)
        {
            PrimaryForm.Instance.syncCloseButton.Visible = true;
        }
        if (PrimaryForm.Instance?.syncCloseButton != null)
        {
            PrimaryForm.Instance.syncCloseButton.BringToFront();
        }
        try {
            if (mediaRenderer.VideoRendererPrimary == null) {
                // Console.WriteLine("VMR9 is not initialized.");
                return;
            }

            mediaRenderer.VideoWindowPrimary = (IVideoWindow) mediaRenderer.VideoRendererPrimary;
            if (PrimaryForm.Instance?.primaryScreenPanel != null)
            {
                _ = mediaRenderer.VideoWindowPrimary.put_Owner(PrimaryForm.Instance.primaryScreenPanel.Handle); // 设置为 primaryScreenPanel 的句柄
            }
            mediaRenderer.VideoWindowPrimary.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren);
            mediaRenderer.VideoWindowPrimary.SetWindowPosition(0, 0, 1620, 1080); // 调整视频窗口大小以填满黑色区域
            mediaRenderer.VideoWindowPrimary.put_Visible(OABool.True);

            // Console.WriteLine("Video window configured successfully.");
        } catch (Exception ex) {
            Console.WriteLine(String.Format("Error syncing to primary monitor: {0}", ex.Message));
            MessageBox.Show(String.Format("Error syncing to primary monitor: {0}", ex.Message));
        }
    }
    public void ClosePrimaryScreenPanel() {
        try {
            if (PrimaryForm.Instance?.primaryScreenPanel != null)
            {
                PrimaryForm.Instance.primaryScreenPanel.Visible = false;
            }
            if (PrimaryForm.Instance?.syncServiceBellButton != null)
            {
                PrimaryForm.Instance.syncServiceBellButton.Visible = false;
            }
            if (PrimaryForm.Instance?.syncCutSongButton != null)
            {
                PrimaryForm.Instance.syncCutSongButton.Visible = false;
            }
            if (PrimaryForm.Instance?.syncReplayButton != null)
            {
                PrimaryForm.Instance.syncReplayButton.Visible = false;
            }
            if (PrimaryForm.Instance?.syncOriginalSongButton != null)
            {
                PrimaryForm.Instance.syncOriginalSongButton.Visible = false;
            }
            if (PrimaryForm.Instance?.syncMuteButton != null)
            {
                PrimaryForm.Instance.syncMuteButton.Visible = false;
            }
            if (PrimaryForm.Instance?.syncPauseButton != null)
            {
                PrimaryForm.Instance.syncPauseButton.Visible = false;
            }
            if (PrimaryForm.Instance?.syncPlayButton != null)
            {
                PrimaryForm.Instance.syncPlayButton.Visible = false;
            }
            if (PrimaryForm.Instance?.syncVolumeUpButton != null)
            {
                PrimaryForm.Instance.syncVolumeUpButton.Visible = false;
            }
            if (PrimaryForm.Instance?.syncVolumeDownButton != null)
            {
                PrimaryForm.Instance.syncVolumeDownButton.Visible = false;
            }
            if (PrimaryForm.Instance?.syncMicUpButton != null)
            {
                PrimaryForm.Instance.syncMicUpButton.Visible = false;
            }
            if (PrimaryForm.Instance?.syncMicDownButton != null)
            {
                PrimaryForm.Instance.syncMicDownButton.Visible = false;
            }
            if (PrimaryForm.Instance?.syncCloseButton != null)
            {
                PrimaryForm.Instance.syncCloseButton.Visible = false;
            }
            if (mediaRenderer.VideoWindowPrimary != null) {
                if (PrimaryForm.Instance?.primaryScreenPanel != null)
                {
                    _ = mediaRenderer.VideoWindowPrimary.put_Owner(PrimaryForm.Instance.primaryScreenPanel.Handle); // 绑定到主屏幕的特定区域
                }
                mediaRenderer.VideoWindowPrimary.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren | WindowStyle.ClipSiblings);
                mediaRenderer.VideoWindowPrimary.put_Visible(OABool.False); // 初始化时隐藏
            }
            IsSyncToPrimaryMonitor = false;
        } catch (Exception ex) {
            Console.WriteLine(String.Format("Error closing primary screen panel: {0}", ex.Message));
            MessageBox.Show(String.Format("Error closing primary screen panel: {0}", ex.Message));
        }
    }

    [DllImport("gdi32.dll", ExactSpelling = true)]
    public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight,
        IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

    public enum TernaryRasterOperations: uint {
        SRCCOPY = 0x00CC0020,
    }

    private void DisplayBarrageOnOverlay(string text) {
        if (overlayForm.InvokeRequired) {
            overlayForm.Invoke(new System.Action(() => overlayForm.DisplayBarrage(text)));
        } else {
            overlayForm.DisplayBarrage(text);
        }
    }

    public async Task InitializePublicPlaylist(List < SongData > initialPlaylist) {
        publicPlaylist = initialPlaylist;
        await PlayPublicPlaylist(); // 开始播放公播歌单
    }

    public async Task SetPlayingSongList(List < SongData > songList) {
        Console.WriteLine("SetPlayingSongList called");
        mediaRenderer.StopAndReleaseResources();

        playingSongList = songList;
        isUserPlaylistPlaying = playingSongList != null && playingSongList.Any();
        IsPlayingPublicSong = false; // 设置为不在播放公播

        if (isUserPlaylistPlaying) {
            currentSongIndex = -1;
            await PlayNextSong();
        } else {
            await InitializeAndPlayPublicPlaylist();
        }
    }

    public async Task PlayPublicPlaylist() {
        Console.WriteLine("開始播放公播清單...");

        // 在切换到公播之前，确保最后一首用户歌曲状态正确
        if (PrimaryForm.currentSongIndexInHistory >= 0 &&
            PrimaryForm.currentSongIndexInHistory < PrimaryForm.playStates.Count) {
            PrimaryForm.playStates[PrimaryForm.currentSongIndexInHistory] = PlayState.Played;
            Console.WriteLine($"切換到公播前更新最後一首歌曲狀態為已播放，索引：{PrimaryForm.currentSongIndexInHistory}");

            // 强制刷新显示
            if (PrimaryForm.Instance?.multiPagePanel != null) {
                PrimaryForm.Instance.multiPagePanel.LoadPlayedSongs(
                    PrimaryForm.playedSongsHistory,
                    PrimaryForm.playStates
                );
            }
        }

        isUserPlaylistPlaying = false;
        IsPlayingPublicSong = true; // 设置为正在播放公播
        currentSongIndex = -1;

        try {
            // 重新整理公播清單
            publicPlaylist = new List < SongData > ();

            // 首先添加 welcome.mpg
            string welcomePath = @"C:\video\welcome.mpg";
            if (File.Exists(welcomePath)) {
                publicPlaylist.Add(new SongData(
                    "0", "", "歡迎光臨", 0, "", "", "", "",
                    DateTime.Now, welcomePath, "", "", "", "",
                    "", "", "", "", "", "", "", 1
                ));
            }

            // 添加 BGM 序列
            for (int i = 1; i <= 99; i++) {
                string bgmPath = $@"C:\video\BGM{i:D2}.mpg";
                if (File.Exists(bgmPath)) {
                    publicPlaylist.Add(new SongData(
                        i.ToString(), "", $"背景音樂{i:D2}", 0, "", "", "", "",
                        DateTime.Now, bgmPath, "", "", "", "",
                        "", "", "", "", "", "", "", 1
                    ));
                }
            }

            // 如果公播清單為空，使用原有的歌曲
            if (publicPlaylist.Count == 0) {
                string videoDirectory = @"C:\video\";
                string[] videoFiles = Directory.GetFiles(videoDirectory, "*.mpg");
                foreach(var songPath in videoFiles) {
                    string fileName = Path.GetFileNameWithoutExtension(songPath);
                    publicPlaylist.Add(new SongData(
                        "0", "", fileName, 0, "", "", "", "",
                        DateTime.Now, songPath, "", "", "", "",
                        "", "", "", "", "", "", "", 1
                    ));
                }
            }

            await Task.Delay(100); // 添加短暂延迟
            await PlayNextSong();
        } catch (Exception ex) {
            Console.WriteLine($"播放公播清單時發生錯誤: {ex.Message}");
            // 可以在这里添加重试逻辑
            await Task.Delay(1000);
            await PlayNextSong();
        }
    }

    // private static async Task UpdateMarqueeTextForCurrentSong(SongData song)
    // {
    //     string text;

    //     if (string.IsNullOrEmpty(song?.Song))
    //     {
    //         text = string.Empty;
    //     }
    //     else
    //     {
    //         text = String.Format("正在播放：{0} ", song.Song);
    //     }
    //     overlayForm.UpdateSongDisplayLabel(text);

    //     await Task.Delay(5000);
    // }

    public static async Task UpdateMarqueeTextForNextSong(SongData song) {
        string nextSongText = String.Format("下一首3：{0}", song.Song);

        if (overlayForm.InvokeRequired) {
            overlayForm.Invoke(new MethodInvoker(() => {
                overlayForm.UpdateMarqueeText(nextSongText, OverlayForm.MarqueeStartPosition.Middle, Color.White);
            }));
        } else {
            overlayForm.UpdateMarqueeText(nextSongText, OverlayForm.MarqueeStartPosition.Middle, Color.White);
        }
        await Task.Delay(5000);

        // 重置跑马灯文本
        if (overlayForm.InvokeRequired) {
            overlayForm.Invoke(new MethodInvoker(() => {
                overlayForm.ResetMarqueeTextToWelcomeMessage();
            }));
        } else {
            overlayForm.ResetMarqueeTextToWelcomeMessage();
        }
    }
    public void UpdateNextSongFromPlaylist() {
        List < SongData > currentPlaylist = isUserPlaylistPlaying ? playingSongList : publicPlaylist;

        if (currentPlaylist == null || currentPlaylist.Count == 0) {
            overlayForm?.UpdateNextSongLabelFromPlaylist(isUserPlaylistPlaying, null!);
            return;
        }

        int currentSongIndex = currentPlaylist.IndexOf(currentPlayingSong);

        if (currentSongIndex == -1 || currentSongIndex + 1 >= currentPlaylist.Count) {
            overlayForm?.UpdateNextSongLabelFromPlaylist(isUserPlaylistPlaying, null!);
        } else {
            SongData nextSong = currentPlaylist[currentSongIndex + 1];
            overlayForm?.UpdateNextSongLabelFromPlaylist(isUserPlaylistPlaying, currentPlayingSong);
        }
    }

    public async Task PlayNextSong() {
        while (!MediaRenderer.IsInitializationComplete) {
            await Task.Delay(100);
        }

        Console.WriteLine("開始播放下一首歌曲...");
        List < SongData > currentPlaylist = isUserPlaylistPlaying ? playingSongList : publicPlaylist;

        if (!currentPlaylist.Any()) return;

        if (!isUserPlaylistPlaying) {
            currentSongIndex = (currentSongIndex + 1) % currentPlaylist.Count;
            Console.WriteLine($"順序播放: currentSongIndex = {currentSongIndex}, currentPlaylist.Count = {currentPlaylist.Count}");
        } else {
            if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen) {
                byte[] commandBytesIncreasePitch1 = new byte[] {
                    0xA2,
                    0x7F,
                    0xA4
                };
                SerialPortManager.mySerialPort.Write(commandBytesIncreasePitch1, 0, commandBytesIncreasePitch1.Length);
            }
            currentSongIndex = (currentSongIndex + 1) % currentPlaylist.Count;
        }

        var songToPlay = currentPlaylist[currentSongIndex];
        var pathToPlay = File.Exists(songToPlay.SongFilePathHost1) ? songToPlay.SongFilePathHost1 : songToPlay.SongFilePathHost2;
        if (!File.Exists(pathToPlay)) {
            Console.WriteLine($"文件不存在：{pathToPlay}");
            return;
        }
        currentPlayingSong = songToPlay;
        UpdateNextSongFromPlaylist();
        overlayForm.DisplayQRCodeOnOverlay(HttpServer.randomFolderPath);
        overlayForm.HidePauseLabel();

        try {
            // 確保在UI線程上執行COM對象操作
            if (this.InvokeRequired) {
                this.Invoke(new Action(async () => {
                    await InitializeAndPlayMedia(pathToPlay);
                }));
            } else {
                await InitializeAndPlayMedia(pathToPlay);
            }
        } catch (Exception ex) {
            Console.WriteLine($"播放時發生錯誤: {ex.Message}");
            // 嘗試重新初始化並重播
            try {
                await Task.Delay(1000);
                mediaRenderer.StopAndReleaseResources();
                await Task.Delay(1000);

                // 重新初始化 COM
                int hr = CoInitializeEx(IntPtr.Zero, COINIT.APARTMENTTHREADED);
                if (hr >= 0) {
                    mediaRenderer.InitializeGraphBuilders();
                    // 再次尝试播放
                    await InitializeAndPlayMedia(pathToPlay);
                }
            } catch (Exception retryEx) {
                Console.WriteLine($"重試播放時發生錯誤: {retryEx.Message}");
            }
        }
    }

    private async Task InitializeAndPlayMedia(string pathToPlay) {
        mediaRenderer.InitializeAndPlayMedia(pathToPlay);

        // 配置副螢幕視頻窗口：傳入 VideoPlayerForm 的句柄與第二螢幕尺寸
        await mediaRenderer.ConfigureSecondaryVideoWindow(this.Handle, secondMonitor.Bounds.Width, secondMonitor.Bounds.Height);

        // 音量處理
        if (isMuted) {
            mediaRenderer.SetVolume(-10000);
        }

        // 開始播放
        if (mediaRenderer.MediaControlPrimary != null) mediaRenderer.MediaControlPrimary.Run();
        if (mediaRenderer.MediaControlSecondary != null) mediaRenderer.MediaControlSecondary.Run();

        if (isSyncToPrimaryMonitor) {
            SyncToPrimaryMonitor();
        }
    }

    public async Task SkipToNextSong() {
        try {
            mediaRenderer.StopAndReleaseResources();
            await Task.Delay(100);

            if (isUserPlaylistPlaying && playingSongList != null && playingSongList.Count > 0) {
                // 还有用户歌曲要播放
                playingSongList.RemoveAt(0);
                if (playingSongList.Count == 0) {
                    // 用户歌单播完，切换到公播
                    Console.WriteLine("用戶播放列表已清空，切換至公播清單");
                    isUserPlaylistPlaying = false;
                    lastPlayedIndex = -1;
                    currentSongIndex = -1;
                    await InitializeAndPlayPublicPlaylist();
                } else {
                    // 继续播放用户歌单
                    currentSongIndex = -1;
                    await PlayNextSong();
                }
            } else {
                // 当前是公播状态，重新初始化公播列表
                await InitializeAndPlayPublicPlaylist();
            }

            if (PrimaryForm.currentSongIndexInHistory >= 0 && PrimaryForm.currentSongIndexInHistory < PrimaryForm.playedSongsHistory.Count) {
                var currentSong = PrimaryForm.playedSongsHistory[PrimaryForm.currentSongIndexInHistory];
                if (playingSongList!.Count > 0 && currentSong == playingSongList[0]) {
                    PrimaryForm.playStates[PrimaryForm.currentSongIndexInHistory] = PlayState.Played;
                }
            }
            PrimaryForm.currentSongIndexInHistory += 1;
        } catch (Exception ex) {
            Console.WriteLine($"切換歌曲時發生錯誤: {ex.Message}");
            await InitializeAndPlayPublicPlaylist();
        }
    }

    // 新增一个方法来处理公播列表的初始化和播放
    private async Task InitializeAndPlayPublicPlaylist() {
        try {
            isUserPlaylistPlaying = false;
            currentSongIndex = -1;

            // 重新初始化公播列表
            publicPlaylist = new List < SongData > ();

            // 添加 welcome.mpg
            string welcomePath = @"C:\video\welcome.mpg";
            if (File.Exists(welcomePath)) {
                publicPlaylist.Add(new SongData(
                    "0", "", "歡迎光臨", 0, "", "", "", "",
                    DateTime.Now, welcomePath, "", "", "", "",
                    "", "", "", "", "", "", "", 1
                ));
            }

            // 添加 BGM 序列
            for (int i = 1; i <= 99; i++) {
                string bgmPath = $@"C:\video\BGM{i:D2}.mpg";
                if (File.Exists(bgmPath)) {
                    publicPlaylist.Add(new SongData(
                        i.ToString(), "", $"背景音樂{i:D2}", 0, "", "", "", "",
                        DateTime.Now, bgmPath, "", "", "", "",
                        "", "", "", "", "", "", "", 1
                    ));
                }
            }

            if (publicPlaylist.Count > 0) {
                await PlayNextSong();
            }
        } catch (Exception ex) {
            Console.WriteLine($"初始化公播清單時發生錯誤: {ex.Message}");
        }
    }

    public void ReplayCurrentSong() {
        List < SongData > currentPlaylist = isUserPlaylistPlaying ? playingSongList : publicPlaylist;
        if (!currentPlaylist.Any()) return;
        var songToPlay = currentPlaylist[currentSongIndex];
        var pathToPlay = File.Exists(songToPlay.SongFilePathHost1) ? songToPlay.SongFilePathHost1 : songToPlay.SongFilePathHost2;
        if (!File.Exists(pathToPlay)) {
            MessageBox.Show("File does not exist on both hosts.");
            return;
        }
        // UpdateMarqueeTextForCurrentSong(songToPlay);

        try {
            if (mediaRenderer.MediaControlPrimary != null) {
                mediaRenderer.MediaControlPrimary.Stop();
            }
            if (mediaRenderer.MediaControlSecondary != null) {
                mediaRenderer.MediaControlSecondary.Stop();
            }

            if (mediaRenderer.VideoWindowPrimary != null) {
                mediaRenderer.VideoWindowPrimary.put_Visible(OABool.False); // 隐藏主屏幕窗口，避免干扰
            }

            if (mediaRenderer.VideoWindowSecondary != null) {
                mediaRenderer.VideoWindowSecondary.put_Visible(OABool.False); // 隐藏副屏幕窗口，避免闪烁
            }

            // 清理并初始化 DirectShow 图表
            RemoveAllFilters(mediaRenderer.GraphBuilderPrimary);
            RemoveAllFilters(mediaRenderer.GraphBuilderSecondary);
            mediaRenderer.InitializeGraphBuilders();

            // 渲染媒体文件
            mediaRenderer.RenderMediaFilePrimary(pathToPlay);
            mediaRenderer.RenderMediaFileSecondary(pathToPlay);

            // 绑定视频窗口到副屏幕
            mediaRenderer.VideoWindowSecondary = (IVideoWindow)mediaRenderer.VideoRendererSecondary;
            if (mediaRenderer.VideoWindowSecondary != null) {
                mediaRenderer.VideoWindowSecondary.put_Owner(this.Handle); // 副屏幕窗口句柄
                mediaRenderer.VideoWindowSecondary.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipChildren | WindowStyle.ClipSiblings);
                mediaRenderer.VideoWindowSecondary.SetWindowPosition(0, 0, secondMonitor.Bounds.Width, secondMonitor.Bounds.Height);
                mediaRenderer.VideoWindowSecondary.put_Visible(OABool.True); // 显示窗口
            }

            // 音量处理
            if (isMuted) {
                mediaRenderer.SetVolume(-10000); // 静音
            }

            // 开始播放
            mediaRenderer.MediaControlPrimary?.Run();
            mediaRenderer.MediaControlSecondary?.Run();

            if (isSyncToPrimaryMonitor) {
                SyncToPrimaryMonitor();
            }
        } catch (Exception ex) {
            Console.WriteLine(String.Format("Error replaying song: {0}", ex.Message));
            MessageBox.Show(String.Format("Error replaying song: {0}", ex.Message));
        }
    }

    public static void SaveGraphFile(IGraphBuilder graph, string filename) {
        var writer = new StreamWriter(filename);
        IFilterGraph2 graph2 = (IFilterGraph2)graph;

        if (graph2 != null) {
            IEnumFilters enumFilters;
            graph2.EnumFilters(out enumFilters);

            enumFilters.Reset();
            IBaseFilter[] filters = new IBaseFilter[1];
            while (enumFilters.Next(1, filters, IntPtr.Zero) == 0) {
                FilterInfo filterInfo;
                filters[0].QueryFilterInfo(out filterInfo);
                writer.WriteLine("Filter: " + filterInfo.achName);
                IEnumPins enumPins;
                filters[0].EnumPins(out enumPins);
                enumPins.Reset();
                IPin[] pins = new IPin[1];
                while (enumPins.Next(1, pins, IntPtr.Zero) == 0) {
                    PinInfo pinInfo;
                    pins[0].QueryPinInfo(out pinInfo);
                    writer.WriteLine("  Pin: " + pinInfo.name);
                    Marshal.ReleaseComObject(pins[0]);
                }
                Marshal.ReleaseComObject(enumPins);
                Marshal.ReleaseComObject(filters[0]);
            }

            Marshal.ReleaseComObject(enumFilters);
        }

        writer.Close();
    }

    private static void RemoveAllFilters(IGraphBuilder graph) {
        IEnumFilters enumFilters;
        graph.EnumFilters(out enumFilters);
        IBaseFilter[] filters = new IBaseFilter[1];
        while (enumFilters.Next(1, filters, IntPtr.Zero) == 0) {
            graph.RemoveFilter(filters[0]);
            Marshal.ReleaseComObject(filters[0]);
        }
        Marshal.ReleaseComObject(enumFilters);
    }

    private void InitializeOverlayForm(Screen secondaryScreen) {
        overlayForm = new OverlayForm();
        Screen secondMonitor = ScreenHelper.GetSecondMonitor();
        if (secondMonitor != null) {
            overlayForm.Location = secondMonitor.WorkingArea.Location;
            overlayForm.StartPosition = FormStartPosition.Manual;
            overlayForm.Size = new Size(secondMonitor.WorkingArea.Width, secondMonitor.WorkingArea.Height);
        }
        overlayForm.ShowInTaskbar = false;
        overlayForm.Owner = this;
        overlayForm.Show();
        this.Focus();
    }

    private bool isPlayingNext = false;
    private int lastPlayedIndex = -1; // 追蹤上一首播放的索引

    public void MonitorMediaEvents() {
        Console.WriteLine("開始監聽媒體事件...");

        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    if (mediaRenderer.MediaControlSecondary == null || isPlayingNext)
                    {
                        await Task.Delay(500);
                        continue;
                    }

                    // 只檢查播放進度
                    IMediaSeeking mediaSeekingSecondary = (IMediaSeeking)mediaRenderer.GraphBuilderSecondary;
                    if (mediaSeekingSecondary != null && !isPlayingNext)
                    {
                        long currentPosition = 0;
                        long duration = 0;

                        if (mediaSeekingSecondary.GetCurrentPosition(out currentPosition) >= 0 &&
                            mediaSeekingSecondary.GetDuration(out duration) >= 0)
                        {
                            double currentSeconds = currentPosition / 10000000.0;
                            double durationSeconds = duration / 10000000.0;

                            // 添加更严格的结束条件判断
                            bool isAtEnd = durationSeconds > 0 && currentSeconds > 0 &&
                                Math.Abs(currentSeconds - durationSeconds) < 0.1 && // 确保真的到了结尾
                                !IsPaused;

                            if (isAtEnd && !isPlayingNext)
                            {
                                Console.WriteLine($"檢測到歌曲結束 - 當前位置: {currentSeconds:F2}秒, 總時長: {durationSeconds:F2}秒");

                                if (!isPlayingNext)
                                {
                                    isPlayingNext = true;

                                    // 添加额外的保护：确保在切换前停止当前播放
                                    if (mediaRenderer.MediaControlSecondary != null)
                                    {
                                        mediaRenderer.MediaControlSecondary.Stop();
                                    }

                                    _ = this.BeginInvoke(new Action(async () =>
                                    {
                                        try
                                        {
                                            // 在切换前释放资源
                                            mediaRenderer.StopAndReleaseResources();
                                            await Task.Delay(100); // 给予足够的时间释放资源

                                            if (isUserPlaylistPlaying && playingSongList != null)
                                            {
                                                if (playingSongList.Count > 0)
                                                {
                                                    try
                                                    {
                                                        // 移除當前播放的歌曲
                                                        playingSongList.RemoveAt(0);

                                                        // 更新播放状态逻辑
                                                        if (PrimaryForm.currentSongIndexInHistory >= 0)
                                                        {
                                                            // 将当前播放的歌曲标记为已播放
                                                            PrimaryForm.playStates[PrimaryForm.currentSongIndexInHistory] = PlayState.Played;

                                                            // 如果还有下一首歌
                                                            if (playingSongList.Count > 0)
                                                            {
                                                                PrimaryForm.currentSongIndexInHistory++;
                                                                // 将下一首歌标记为正在播放
                                                                if (PrimaryForm.currentSongIndexInHistory < PrimaryForm.playStates.Count)
                                                                {
                                                                    PrimaryForm.playStates[PrimaryForm.currentSongIndexInHistory] = PlayState.Playing;
                                                                }
                                                            }
                                                        }

                                                        if (playingSongList.Count == 0)
                                                        {
                                                            Console.WriteLine("用戶播放列表已播放完畢，切換到公共播放列表");

                                                            // 确保当前歌曲状态更新为已播放
                                                            if (PrimaryForm.currentSongIndexInHistory >= 0 &&
                                                                PrimaryForm.currentSongIndexInHistory < PrimaryForm.playStates.Count)
                                                            {
                                                                PrimaryForm.playStates[PrimaryForm.currentSongIndexInHistory] = PlayState.Played;
                                                                Console.WriteLine($"已將最後一首歌曲狀態更新為已播放，索引：{PrimaryForm.currentSongIndexInHistory}");

                                                                // 强制刷新显示
                                                                PrimaryForm.Instance?.multiPagePanel?.LoadPlayedSongs(
                                                                    PrimaryForm.playedSongsHistory,
                                                                    PrimaryForm.playStates
                                                                );
                                                            }

                                                            // 重置播放状态但保留历史记录
                                                            isUserPlaylistPlaying = false;
                                                            currentSongIndex = -1;

                                                            // 确保所有未播放的歌曲状态被清除
                                                            for (int i = PrimaryForm.currentSongIndexInHistory + 1; i < PrimaryForm.playStates.Count; i++)
                                                            {
                                                                if (PrimaryForm.playStates[i] == PlayState.Playing)
                                                                {
                                                                    PrimaryForm.playStates[i] = PlayState.NotPlayed;
                                                                }
                                                            }

                                                            // 添加延迟以确保状态更新完成
                                                            await Task.Delay(200);
                                                            await InitializeAndPlayPublicPlaylist();
                                                        }
                                                        else
                                                        {
                                                            currentSongIndex = -1;
                                                            await PlayNextSong();
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Console.WriteLine($"處理播放列表時發生錯誤: {ex.Message}");
                                                        lastPlayedIndex = -1;
                                                        isUserPlaylistPlaying = false;
                                                        currentSongIndex = -1;
                                                        await InitializeAndPlayPublicPlaylist();
                                                    }
                                                }
                                            }
                                            else if (!isUserPlaylistPlaying && publicPlaylist != null)
                                            {
                                                await PlayNextSong();
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"切換歌曲時發生錯誤: {ex.Message}");
                                            // 发生错误时重置状态并切换到公播
                                            lastPlayedIndex = -1;
                                            isUserPlaylistPlaying = false;
                                            currentSongIndex = -1;
                                            await InitializeAndPlayPublicPlaylist();
                                        }
                                        finally
                                        {
                                            isPlayingNext = false;
                                        }
                                    }));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"監控媒體事件時發生錯誤: {ex.Message}");
                    isPlayingNext = false;

                    // 添加重试机制
                    if (!isUserPlaylistPlaying && publicPlaylist != null)
                    {
                        await Task.Delay(1000);
                        await PlayNextSong();
                    }

                    await Task.Delay(1000);
                }

                await Task.Delay(500);
            }
        });
    }

    public void BringOverlayToFront() {
        if (overlayForm != null) {
            if (!overlayForm.Visible) {
                overlayForm.Show();
            }

            overlayForm.BringToFront();
            overlayForm.TopMost = true;
        }
    }

    private void UpdateSyncButtons() {
        if (IsPaused) {
            if (PrimaryForm.Instance?.syncPlayButton != null) {
                PrimaryForm.Instance.syncPlayButton.Visible = true;
            }
            if (PrimaryForm.Instance?.syncPauseButton != null) {
                PrimaryForm.Instance.syncPauseButton.Visible = false;
            }
        } else {
            if (PrimaryForm.Instance?.syncPlayButton != null) {
                PrimaryForm.Instance.syncPlayButton.Visible = false;
            }
            if (PrimaryForm.Instance?.syncPauseButton != null) {
                PrimaryForm.Instance.syncPauseButton.Visible = true;
            }
        }
    }

    public async Task AddSongToPlaylist(SongData songData) {
        try {
            var filePath1 = songData.SongFilePathHost1;
            var filePath2 = songData.SongFilePathHost2;

            if (!File.Exists(filePath1) && !File.Exists(filePath2)) {
                PrimaryForm.WriteLog(String.Format("File not found on both hosts: {0} and {1}", filePath1, filePath2));
                return;
            }

            bool wasEmpty = PrimaryForm.userRequestedSongs.Count == 0;

            // 查找所有相同歌曲的索引
            var sameNameIndices = new List < int > ();
            for (int i = 0; i < PrimaryForm.playedSongsHistory.Count; i++) {
                if (PrimaryForm.playedSongsHistory[i].Song == songData.Song) {
                    sameNameIndices.Add(i);
                }
            }

            // 添加新歌到播放列表
            PrimaryForm.userRequestedSongs.Add(songData);
            PrimaryForm.playedSongsHistory.Add(songData);

            // 更新状态
            if (wasEmpty) {
                // 如果是空列表，设置为正在播放
                PrimaryForm.playStates.Add(PlayState.Playing);
                PrimaryForm.currentSongIndexInHistory = PrimaryForm.playedSongsHistory.Count - 1;

                // 确保之前相同歌曲的状态为已播放
                foreach(int index in sameNameIndices) {
                    if (index < PrimaryForm.currentSongIndexInHistory) {
                        PrimaryForm.playStates[index] = PlayState.Played;
                    }
                }

                await VideoPlayerForm.Instance.SetPlayingSongList(PrimaryForm.userRequestedSongs);
            } else {
                // 如果不是空列表，设置为未播放
                PrimaryForm.playStates.Add(PlayState.NotPlayed);

                // 更新所有相同歌曲的状态
                foreach(int index in sameNameIndices.Where(i => i < PrimaryForm.playedSongsHistory.Count - 1)) {
                    if (index < PrimaryForm.currentSongIndexInHistory) {
                        // 之前的相同歌曲标记为已播放
                        PrimaryForm.playStates[index] = PlayState.Played;
                    } else if (index == PrimaryForm.currentSongIndexInHistory) {
                        // 当前播放的歌曲保持Playing状态
                        PrimaryForm.playStates[index] = PlayState.Playing;
                    } else {
                        // 后面的相同歌曲标记为未播放
                        PrimaryForm.playStates[index] = PlayState.NotPlayed;
                    }
                }
            }

            VideoPlayerForm.Instance.UpdateNextSongFromPlaylist();
            PrimaryForm.PrintPlayingSongList();
        } catch (Exception ex) {
            Console.WriteLine("Error occurred: " + ex.Message);
        }
    }

    public async Task InsertSongToPlaylist(SongData songData) {
        try {
            var filePath1 = songData.SongFilePathHost1;
            var filePath2 = songData.SongFilePathHost2;

            if (!File.Exists(filePath1) && !File.Exists(filePath2)) {
                PrimaryForm.WriteLog(String.Format("File not found on both hosts: {0} and {1}", filePath1, filePath2));
                return;
            }

            bool wasEmpty = PrimaryForm.userRequestedSongs.Count == 0;

            // 查找所有相同歌曲的索引
            var sameNameIndices = new List < int > ();
            for (int i = 0; i < PrimaryForm.playedSongsHistory.Count; i++) {
                if (PrimaryForm.playedSongsHistory[i].Song == songData.Song) {
                    sameNameIndices.Add(i);
                }
            }

            if (wasEmpty) {
                // 如果列表为空，直接添加
                PrimaryForm.userRequestedSongs.Add(songData);
                PrimaryForm.playedSongsHistory.Add(songData);
                PrimaryForm.playStates.Add(PlayState.Playing);
                PrimaryForm.currentSongIndexInHistory = PrimaryForm.playedSongsHistory.Count - 1;

                // 更新之前相同歌曲的状态
                foreach(int index in sameNameIndices) {
                    if (index < PrimaryForm.currentSongIndexInHistory) {
                        PrimaryForm.playStates[index] = PlayState.Played;
                    }
                }

                await VideoPlayerForm.Instance.SetPlayingSongList(PrimaryForm.userRequestedSongs);
            } else {
                // 插入到当前播放歌曲之后
                int insertIndex = PrimaryForm.currentSongIndexInHistory + 1;

                PrimaryForm.userRequestedSongs.Insert(1, songData);
                PrimaryForm.playedSongsHistory.Insert(insertIndex, songData);
                PrimaryForm.playStates.Insert(insertIndex, PlayState.NotPlayed);

                // 更新所有相同歌曲的状态
                foreach(int index in sameNameIndices) {
                    if (index < PrimaryForm.currentSongIndexInHistory) {
                        // 之前的相同歌曲标记为已播放
                        PrimaryForm.playStates[index] = PlayState.Played;
                    } else if (index == PrimaryForm.currentSongIndexInHistory) {
                        // 当前播放的歌曲保持Playing状态
                        PrimaryForm.playStates[index] = PlayState.Playing;
                    } else if (index > insertIndex) {
                        // 后面的相同歌曲标记为未播放
                        PrimaryForm.playStates[index] = PlayState.NotPlayed;
                    }
                }
            }

            VideoPlayerForm.Instance.UpdateNextSongFromPlaylist();
            PrimaryForm.PrintPlayingSongList();
        } catch (Exception ex) {
            Console.WriteLine("Error occurred: " + ex.Message);
        }
    }
}