using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json; // For JsonConvert and JsonException
using System.Windows.Forms; // For MessageBox
using System.Drawing; // For Image
using System.Drawing.Imaging; // For ImageFormat
using ZXing; // For BarcodeWriter and BarcodeFormat
using ZXing.QrCode; // For QrCodeEncodingOptions
using ActionString = System.Action<string>; // 现在可以在代码中使用 ActionString
using SystemAction = System.Action;
using ZXingAction = ZXing.Action;
using System.Threading;
using System.Collections.Concurrent;
namespace DualScreenDemo
{
    public class HttpServer
    {
        private static string _localIP = GetLocalIPAddress();
        private static int _port = 9090; // 或其他方式設置
        // 服务器类变量
        private static SongListManager songListManager;
        // 使用完整命名空间来避免歧义
        public static event ActionString OnDisplayBarrage;
        private static DateTime lastClickTime = DateTime.MinValue;
        public static string randomFolderPath; // 声明全局变量
        private static OverlayForm form;
        private static readonly ConcurrentDictionary<string, byte[]> _fileCache = new ConcurrentDictionary<string, byte[]>();
        private static readonly SemaphoreSlim _requestThrottle = new SemaphoreSlim(20); // 限制并发请求数
        private static readonly CancellationTokenSource _serverCts = new CancellationTokenSource();
         

        public static async Task StartServer(string baseDirectory, int port, SongListManager manager)
        {
            songListManager = manager; // 保存传递的SongListManager实例
            string randomFolderName = CreateRandomFolderAndRedirectHTML(baseDirectory);
            randomFolderPath = randomFolderName; // 初始化全局变量

            // 读取 IP 地址
            string localAddress = GetLocalIPAddress(); // 使用获取的本地 IP
            string externalAddress = "";
            
            // 读取外网地址
            string serverAddressFilePath = @"\\SVR01\superstarb\txt\ip.txt";
            if (File.Exists(serverAddressFilePath))
            {
                externalAddress = File.ReadAllText(serverAddressFilePath).Trim();
            }

            // 启动服务器的逻辑
            HttpListener listener = new HttpListener();
            
            // 添加本地地址前缀
            string localPrefix = String.Format("http://{0}:{1}/", localAddress, port);
            Console.WriteLine("Adding local prefix: " + localPrefix);
            listener.Prefixes.Add(localPrefix);

            // 如果有外网地址，也添加外网地址前缀
            if (!string.IsNullOrEmpty(externalAddress))
            {
                // 解析外网地址和端口
                string[] parts = externalAddress.Split(':');
                string host = parts[0];
                int externalPort = parts.Length > 1 ? int.Parse(parts[1]) : port;
                
                string externalPrefix = String.Format("http://{0}:{1}/", host, externalPort);
                Console.WriteLine("Adding external prefix: " + externalPrefix);
                
                try
                {
                    listener.Prefixes.Add(externalPrefix);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not add external prefix: {ex.Message}");
                }
            }

            // 生成两个二维码内容
            string localQrContent = String.Format("http://{0}:{1}/{2}/windows.html", localAddress, port, randomFolderName);
            
            // 修改外网二维码内容生成
            string externalQrContent = !string.IsNullOrEmpty(externalAddress) ? 
                String.Format("http://{0}/{1}/windows.html", externalAddress, randomFolderName) :
                localQrContent;

            // 生成二维码（这里使用外网地址的二维码，因为通常外网地址更有用）
            string qrImagePath = GenerateQRCode(externalQrContent, Path.Combine(baseDirectory, randomFolderName, "qrcode.png"));

            try
            {
                listener.Start();
                Console.WriteLine("Server started.");

                // 在程序关闭时删除随机文件夹
                AppDomain.CurrentDomain.ProcessExit += (s, e) => DeleteRandomFolder(baseDirectory);
            }
            catch (HttpListenerException ex)
            {
                Console.WriteLine("Error starting server: " + ex.Message);
                return;
            }

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                await ProcessRequestAsync(context, baseDirectory, randomFolderName);
            }
        }

        private static async Task ProcessRequestWithTimeout(HttpListenerContext context, string baseDirectory, string randomFolderName)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try
            {
                await _requestThrottle.WaitAsync(cts.Token);
                try
                {
                    await ProcessRequestAsync(context, baseDirectory, randomFolderName);
                }
                finally
                {
                    _requestThrottle.Release();
                }
            }
            catch (OperationCanceledException)
            {
                context.Response.StatusCode = 408; // Request Timeout
                context.Response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
                try
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
                catch { }
            }
        }

        public static void DeleteRandomFolder(string baseDirectory)
        {
            string fullPath = Path.Combine(baseDirectory, randomFolderPath);
            if (Directory.Exists(fullPath))
            {
                try
                {
                    Directory.Delete(fullPath, true);
                    Console.WriteLine("Deleted random folder: " + fullPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error deleting random folder: " + ex.Message);
                }
            }
        }

        public static string GetServerAddress()
        {
            return String.Format("http://{0}:{1}/", _localIP, _port);
            // return String.Format("http://111.246.145.170:8080/");
        }

        private static string CreateRandomFolderAndRedirectHTML(string baseDirectory)
        {
            string randomFolderName = Path.GetRandomFileName().Replace(".", "");
            string randomFolderPath = Path.Combine(baseDirectory, randomFolderName);
            Directory.CreateDirectory(randomFolderPath);
            Console.WriteLine(String.Format("Created random folder: {0}", randomFolderPath));

            string sourceHTMLPath = Path.Combine(baseDirectory, "windows.html");
            string targetHTMLPath = Path.Combine(randomFolderPath, "windows.html");
            File.Copy(sourceHTMLPath, targetHTMLPath, true);
            Console.WriteLine(String.Format("Copied windows.html to {0}", targetHTMLPath));

            // 在生成的 HTML 中注入随机路径
            string htmlContent = File.ReadAllText(sourceHTMLPath);
            htmlContent = htmlContent.Replace("var randomFolder = '';", String.Format("var randomFolder = '{0}';", randomFolderName));
            File.WriteAllText(targetHTMLPath, htmlContent);

            return randomFolderName;
        }

        // 生成 QR 码并返回图像路径
        public static string GenerateQRCode(string content, string savePath)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = 165,
                    Width = 165,
                    Margin = 0
                }
            };

            using (var bitmap = writer.Write(content))
            {
                bitmap.Save(savePath, ImageFormat.Png);
                return savePath;
            }
        }

        private static async Task ProcessRequestAsync(HttpListenerContext context, string baseDirectory, string randomFolderName)
        {
            try
            {
                // 添加CORS头
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                // 处理OPTIONS请求
                if (context.Request.HttpMethod == "OPTIONS")
                {
                    context.Response.StatusCode = 200;
                    context.Response.Close();
                    return;
                }

                // 添加基本的缓存控制
                context.Response.Headers.Add("Cache-Control", "public, max-age=3600");
                
                if (context.Request.HttpMethod == "POST")
                {
                    string relativePath = context.Request.Url.AbsolutePath.Replace($"/{randomFolderName}", "");
                    Console.WriteLine("Received request for path: " + relativePath);

                    switch (relativePath)
                    {
                        case "/search":
                            await HandleSearchRequest(context);
                            break;
                        case "/signal":
                            await HandleSignalRequest(context);
                            break;
                        case "/sound-control":
                            await HandleSoundControlRequest(context);
                            break;
                        case "/send-sticker":
                            await HandleStickerRequest(context);
                            break;
                        case "/order-song":
                            await HandleOrderSongRequest(context);
                            break;
                        case "/insert-song":
                            await HandleInsertSongRequest(context);
                            break;
                        case "/ordered-song":
                            await HandleOrderSongListRequest(context);
                            break;
                        case "/message":
                            await HandlemessageRequest(context);
                            break;
                        case "/favorite":
                            await HandleFavoriteRequest(context);
                            break;
                        default:
                            context.Response.StatusCode = 404;
                            break;
                    }
                }
                else if (context.Request.HttpMethod == "GET")
                {
                    // 获取请求的完整URL路径
                    string requestPath = context.Request.Url.AbsolutePath;
                    
                    // 如果是根路径访问，直接返回404
                    if (requestPath == "/" || requestPath == "/windows.html")
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        return;
                    }

                    string requestedFile = context.Request.Url.LocalPath.Replace($"/{randomFolderName}/", "");
                    
                    if (string.IsNullOrEmpty(requestedFile.Trim('/')))
                    {
                        requestedFile = "windows.html";
                    }

                    await HandleGetRequest(context, baseDirectory, requestedFile);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
                try
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
                catch { }
            }
            finally
            {
                try
                {
                    context.Response.Close();
                }
                catch { }
            }
        }

        private static async Task SendJsonResponseAsync(HttpListenerContext context, object data, int statusCode = 200)
        {
            try
            {
                string jsonResponse = JsonConvert.SerializeObject(data);
                byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json; charset=utf-8";
                context.Response.ContentLength64 = buffer.Length;
                
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending JSON response: {ex.Message}");
                context.Response.StatusCode = 500;
            }
        }

        private static async Task HandleSearchRequest(HttpListenerContext context)
        {
            try
            {
                string requestBody = await ReadRequestBodyAsync(context.Request);
                var searchRequest = JsonConvert.DeserializeObject<SearchRequest>(requestBody);
                
                List<SongData> searchResults;
                switch (searchRequest.Type)
                {
                    case "new-songs":
                        searchResults = SongListManager.NewSongLists["國語"];
                        break;
                    case "top-ranking":
                        searchResults = SongListManager.HotSongLists["國語"];
                        break;
                    case "singer":
                        searchResults = songListManager.SearchSongsBySinger(searchRequest.Query);
                        break;
                    case "song":
                        searchResults = songListManager.SearchSongsByName(searchRequest.Query);
                        break;
                    default:
                        searchResults = new List<SongData>();
                        break;
                }

                await SendJsonResponseAsync(context, searchResults);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling search request: {ex.Message}");
                await SendJsonResponseAsync(context, new { error = "Search failed" }, 500);
            }
        }

        private static async Task<string> ReadRequestBodyAsync(HttpListenerRequest request)
        {
            using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
            return await reader.ReadToEndAsync();
        }

        private static async Task HandleSignalRequest(HttpListenerContext context)
        {
            Console.WriteLine("Handling signal request...");
            string requestBody;
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            Console.WriteLine("Received POST body: " + requestBody);

            var responseMessage = new { status = "Signal received" };
            string jsonResponse = JsonConvert.SerializeObject(responseMessage);
            byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.StatusCode = (int)HttpStatusCode.OK;

            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        private static async Task HandleSoundControlRequest(HttpListenerContext context)
        {
            
            string requestBody;
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            Console.WriteLine("Received sound control command: " + requestBody);

            try
            {
                var data = JsonConvert.DeserializeObject<SoundControlRequest>(requestBody);

                switch (data.Command)
                {
                    case "pause":
                    // 执行暂停操作
                        if (VideoPlayerForm.Instance.isPaused)
                        {
                            PrimaryForm.Instance.videoPlayerForm.Play();
                            PrimaryForm.Instance.pauseButton.Visible = true;
                            PrimaryForm.Instance.playButton.Visible = false;
                            PrimaryForm.Instance.syncPauseButton.Visible = true;
                            PrimaryForm.Instance.syncPlayButton.Visible = false;
                        }
                        else
                        {
                            PrimaryForm.Instance.videoPlayerForm.Pause();
                            PrimaryForm.Instance.pauseButton.Visible = false;
                            PrimaryForm.Instance.playButton.Visible = true;
                            PrimaryForm.Instance.syncPauseButton.Visible = false;
                            PrimaryForm.Instance.syncPlayButton.Visible = true;
                        }
                        break;
                    case "volume_up":
                        // 执行音量增大操作
                        PrimaryForm.SendCommandThroughSerialPort("a2 b3 a4");
                        OverlayForm.MainForm.Invoke(new System.Action(() => {
                            OverlayForm.MainForm.ShowVolumeUpLabel();
                        }));
                        break;
                    case "mic_up":
                        // 执行麦克风增大操作
                        PrimaryForm.SendCommandThroughSerialPort("a2 b5 a4");
                        OverlayForm.MainForm.Invoke(new System.Action(() => {
                            OverlayForm.MainForm.ShowMicUpLabel();
                        }));
                        break;
                    case "mute":
                        // 执行静音操作
                        PrimaryForm.Instance.Invoke(new System.Action(() =>
                        {
                            if (PrimaryForm.Instance.videoPlayerForm.isMuted)
                            {
                                // 取消静音，恢复之前的音量
                                PrimaryForm.Instance.videoPlayerForm.SetVolume(PrimaryForm.Instance.videoPlayerForm.previousVolume);
                                // muteButton.Text = "Mute";
                                PrimaryForm.Instance.videoPlayerForm.isMuted = false;
                                OverlayForm.MainForm.HideMuteLabel();
                            }
                            else
                            {
                                // 静音，将音量设置为-10000
                                PrimaryForm.Instance.videoPlayerForm.previousVolume = PrimaryForm.Instance.videoPlayerForm.GetVolume();
                                PrimaryForm.Instance.videoPlayerForm.SetVolume(-10000);
                                // muteButton.Text = "Unmute";
                                PrimaryForm.Instance.videoPlayerForm.isMuted = true;
                                OverlayForm.MainForm.ShowMuteLabel();
                            }
                        }));
                        break;
                    case "volume_down":
                        // 执行音量减小操作
                        PrimaryForm.SendCommandThroughSerialPort("a2 b4 a4");
                        OverlayForm.MainForm.Invoke(new System.Action(() => {
                            OverlayForm.MainForm.ShowVolumeDownLabel();
                        }));
                        break;
                    case "mic_down":
                        // 执行麦克风减小操作
                        PrimaryForm.SendCommandThroughSerialPort("a2 b6 a4");
                        OverlayForm.MainForm.Invoke(new System.Action(() => {
                            OverlayForm.MainForm.ShowMicDownLabel();
                        }));
                        break;
                    case "original_song":
                        if (PrimaryForm.Instance.InvokeRequired)
                        {
                            PrimaryForm.Instance.Invoke(new System.Action(() => 
                            {
                                PrimaryForm.Instance.videoPlayerForm.ToggleVocalRemoval();
                            }));
                        }
                        else
                        {
                            PrimaryForm.Instance.videoPlayerForm.ToggleVocalRemoval();
                        }
                        // 执行原唱操作
                        
                        break;
                    case "service":
                        // 执行服务操作
                        PrimaryForm.SendCommandThroughSerialPort("a2 53 a4");
                        OverlayForm.MainForm.Invoke(new System.Action(() => {
                            OverlayForm.MainForm.ShowServiceBellLabel();
                        }));
                        // 异步处理等待和隐藏标签
                        await HttpServer.HandleServiceBellAsync();
                        break;
                    case "replay":
                        // 执行重唱操作
                        PrimaryForm.Instance.Invoke(new System.Action(() =>
                        {
                            // 在这里执行按钮点击后的操作
                            // 比如切歌操作
                            PrimaryForm.Instance.videoPlayerForm.ReplayCurrentSong();
                        }));     
                        break;
                    case "male_key":
                        // 执行男调操作
                        OverlayForm.MainForm.Invoke(new System.Action(() => {
                            OverlayForm.MainForm.ShowMaleKeyLabel();
                        }));
                        if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
                        {
                            // 假設 0xA2, 0xC1, 0xA4 是升調的指令
                            byte[] commandBytesIncreasePitch1 = new byte[] { 0xA2, 0x7F, 0xA4 };
                            SerialPortManager.mySerialPort.Write(commandBytesIncreasePitch1, 0, commandBytesIncreasePitch1.Length);
                            byte[] commandBytesDecreasePitch = new byte[] { 0xA2, 0xB2, 0xA4 };
                            SerialPortManager.mySerialPort.Write(commandBytesDecreasePitch, 0, commandBytesDecreasePitch.Length);
                            SerialPortManager.mySerialPort.Write(commandBytesDecreasePitch, 0, commandBytesDecreasePitch.Length);
                            // MessageBox.Show("升調指令已發送。");
                        }
                        else
                        {
                            MessageBox.Show("串口未開啟，無法發送升調指令。");
                        }
                        break;
                    case "female_key":
                        // 执行女调操作
                        OverlayForm.MainForm.Invoke(new System.Action(() => {
                            OverlayForm.MainForm.ShowFemaleKeyLabel();
                        }));
                        if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
                        {
                            // 假設 0xA2, 0xC1, 0xA4 是升調的指令
                            byte[] commandBytesIncreasePitch1 = new byte[] { 0xA2, 0x7F, 0xA4 };
                            SerialPortManager.mySerialPort.Write(commandBytesIncreasePitch1, 0, commandBytesIncreasePitch1.Length);
                            byte[] commandBytesIncreasePitch = new byte[] { 0xA2, 0xB1, 0xA4 };
                            SerialPortManager.mySerialPort.Write(commandBytesIncreasePitch, 0, commandBytesIncreasePitch.Length);
                            SerialPortManager.mySerialPort.Write(commandBytesIncreasePitch, 0, commandBytesIncreasePitch.Length);
                            // MessageBox.Show("升調指令已發送。");
                        }
                        else
                        {
                            MessageBox.Show("串口未開啟，無法發送升調指令。");
                        }
                        break;
                    case "cut":
                        // 执行切歌操作
                        if (PrimaryForm.Instance.InvokeRequired)
                        {
                            PrimaryForm.Instance.Invoke(new System.Action(() => 
                            {
                                PrimaryForm.Instance.videoPlayerForm.SkipToNextSong();
                            }));
                        }
                        else
                        {
                            PrimaryForm.Instance.videoPlayerForm.SkipToNextSong();
                        }
                        break;
                    case "lower_key":
                        // 执行降调操作
                        OverlayForm.MainForm.Invoke(new System.Action(() => {
                            OverlayForm.MainForm.ShowKeyDownLabel();
                        }));

                        // MessageBox.Show("降調功能啟動");
                        if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
                        {
                            // 假設 0xA2, 0xC2, 0xA4 是降調的指令
                            byte[] commandBytesDecreasePitch = new byte[] { 0xA2, 0xB2, 0xA4 };
                            SerialPortManager.mySerialPort.Write(commandBytesDecreasePitch, 0, commandBytesDecreasePitch.Length);
                            // MessageBox.Show("降調指令已發送。");
                        }
                        else
                        {
                            // MessageBox.Show("串口未開啟，無法發送降調指令。");
                        }
                        break;
                    case "standard_key":
                        // 执行标准调操作
                        OverlayForm.MainForm.Invoke(new System.Action(() => {
                            OverlayForm.MainForm.ShowStandardKeyLabel();
                        }));
                        if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
                        {
                            // 假設 0xA2, 0xC1, 0xA4 是升調的指令
                            byte[] commandBytesIncreasePitch = new byte[] { 0xA2, 0x7F, 0xA4 };
                            SerialPortManager.mySerialPort.Write(commandBytesIncreasePitch, 0, commandBytesIncreasePitch.Length);
                            // MessageBox.Show("升調指令已發送。");
                        }
                        else
                        {
                            MessageBox.Show("串口未開啟，無法發送升調指令。");
                        }
                        break;
                    case "raise_key":
                        // 执行升调操作
                        OverlayForm.MainForm.Invoke(new System.Action(() => {
                            OverlayForm.MainForm.ShowKeyUpLabel();
                        }));

                        // MessageBox.Show("升調功能啟動");
                        if (SerialPortManager.mySerialPort != null && SerialPortManager.mySerialPort.IsOpen)
                        {
                            // 假設 0xA2, 0xC1, 0xA4 是升調的指令
                            byte[] commandBytesIncreasePitch = new byte[] { 0xA2, 0xB1, 0xA4 };
                            SerialPortManager.mySerialPort.Write(commandBytesIncreasePitch, 0, commandBytesIncreasePitch.Length);
                            // MessageBox.Show("升調指令已發送。");
                        }
                        else
                        {
                            MessageBox.Show("串口未開啟，無法發送升調指令。");
                        }
                        break;
                    default:
                        Console.WriteLine("Unknown command: " + data.Command);
                        break;
                }

                var response = new { status = "success" };
                string jsonResponse = JsonConvert.SerializeObject(response);
                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(jsonResponse);
                context.Response.StatusCode = (int)HttpStatusCode.OK;

                using (var streamWriter = new StreamWriter(context.Response.OutputStream))
                {
                    await streamWriter.WriteAsync(jsonResponse);
                    await streamWriter.FlushAsync();
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine("JSON parsing error: " + ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.OutputStream.WriteAsync(new byte[0], 0, 0);
            }
        }
        private static async Task HandleServiceBellAsync()
        {
            await Task.Delay(3000); // 等待 3 秒
            OverlayForm.MainForm.Invoke(new System.Action(() => {
                OverlayForm.MainForm.HideServiceBellLabel();
            }));
        }
        private static async Task HandleOrderSongListRequest(HttpListenerContext context)
        {
            try
            {
                // 读取请求的内容
                string requestBody;
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                Console.WriteLine("Received order song request: " + requestBody);
                
                // 检查 playedSongsHistory 是否存在且不为空
                if (PrimaryForm.playedSongsHistory != null && PrimaryForm.playedSongsHistory.Count > 0)
                {
                    Console.WriteLine("Played Songs History Count: " + PrimaryForm.playedSongsHistory.Count);
                    foreach (var song in PrimaryForm.playedSongsHistory)
                    {
                        Console.WriteLine($"Song: {song.Song}, ArtistA: {song.ArtistA}");
                    }

                    // 根据播放历史确定每首歌的播放状态
                    var playStates = DeterminePlayStates(PrimaryForm.playedSongsHistory);

                    // 创建响应数据
                    var response = new
                    {
                        playingSongList = PrimaryForm.playedSongsHistory
                            .Select((song, index) => CreateSongResponse(song, playStates[index])) // 使用新的播放状态
                            .ToList(),
                        // 生成播放历史
                        currentSongIndexInHistory = PrimaryForm.currentSongIndexInHistory
                    };

                    string jsonResponse = JsonConvert.SerializeObject(response);
                    Console.WriteLine("Serialized JSON Response: " + jsonResponse);

                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await SendResponseAsync(context, jsonResponse);
                }
                else
                {
                    // 如果播放历史为空，回传一条消息
                    var response = new { status = "info", message = "No songs in the played history" };
                    string jsonResponse = JsonConvert.SerializeObject(response);
                    Console.WriteLine("Sending empty response: " + jsonResponse);

                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await SendResponseAsync(context, jsonResponse);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error handling order song request: " + ex.Message);
                context.Response.ContentType = "application/json"; 
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await SendResponseAsync(context, "{\"status\": \"error\", \"message\": \"An error occurred\"}");
            }
        }

        // 确定每首歌的播放状态
        private static List<PlayState?> DeterminePlayStates(List<SongData> playedSongsHistory)
        {
            var playStates = new List<PlayState?>();
            bool foundPlaying = false; // 标记是否已找到正在播放的歌曲

            for (int i = 0; i < playedSongsHistory.Count; i++)
            {
                // 这里可以根据您的业务逻辑来确定每首歌的播放状态
                if (i == PrimaryForm.currentSongIndexInHistory)
                {
                    playStates.Add(PlayState.Playing);
                    foundPlaying = true; // 找到正在播放的歌曲
                }
                else if (foundPlaying)
                {
                    playStates.Add(null); // 找到播放中的歌曲后，后面的状态设置为 null
                }
                else
                {
                    playStates.Add(PlayState.NotPlayed); // 未播放状态
                }
            }

            return playStates;
        }

        // 用于创建歌曲响应对象，包括播放状态
        private static object CreateSongResponse(SongData song, PlayState? playState)
        {
            return new
            {
                song.Song,
                song.ArtistA,
                song.SongFilePathHost1,
                song.SongFilePathHost2,
                PlayState = playState.HasValue ? (playState.Value == PlayState.Playing ? "播放中" : "播放完畢") : null // 如果状态为 null，不返回状态信息
            };
        }

        // 生成播放状态
        
        private static async Task HandleOrderSongRequest(HttpListenerContext context)
        {
            try
            {
                string requestBody;
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                Console.WriteLine("Received order song request: " + requestBody);

                // 解析 JSON 为 Song 对象
                var song = JsonConvert.DeserializeObject<SongData>(requestBody);
                
                if (song != null)
                {
                    Console.WriteLine($"Ordering Song: {song.Song} by {song.ArtistA}");
                    // 这里可以添加处理逻辑，例如将歌曲加入到播放列表或数据库中
                    OverlayForm.MainForm.AddSongToPlaylist(song);  
 
                    var response = new { status = "success", message = "Song ordered successfully" };
                    string jsonResponse = JsonConvert.SerializeObject(response);
                    await SendResponseAsync(context, jsonResponse);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await SendResponseAsync(context, "{\"status\": \"error\", \"message\": \"Invalid song data\"}");
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine("JSON parsing error: " + ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await SendResponseAsync(context, "{\"status\": \"error\", \"message\": \"Invalid JSON format\"}");
            }
        }

        private static async Task HandleInsertSongRequest(HttpListenerContext context)
        {
            try
            {
                string requestBody;
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                Console.WriteLine("Received insert song request: " + requestBody);

                // 解析 JSON 为 Song 对象
                var song = JsonConvert.DeserializeObject<SongData>(requestBody);

                if (song != null)
                {
                    Console.WriteLine($"Inserting Song: {song.Song} by {song.ArtistA}");
                    // 这里可以添加插播歌曲的处理逻辑
                    OverlayForm.MainForm.InsertSongToPlaylist(song);

                    var response = new { status = "success", message = "Song inserted successfully" };
                    string jsonResponse = JsonConvert.SerializeObject(response);
                    await SendResponseAsync(context, jsonResponse);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await SendResponseAsync(context, "{\"status\": \"error\", \"message\": \"Invalid song data\"}");
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine("JSON parsing error: " + ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await SendResponseAsync(context, "{\"status\": \"error\", \"message\": \"Invalid JSON format\"}");
            }
        }

        public class SoundControlRequest
        {
            public string Command { get; set; }
        }

        private static async Task HandleStickerRequest(HttpListenerContext context)
        {
            string requestBody;
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            Console.WriteLine("Received sticker ID: " + requestBody);

            try
            {
                var data = JsonConvert.DeserializeObject<StickerRequest>(requestBody);
                string stickerId = data.StickerId;

                // 处理 stickerId 的逻辑，例如显示贴图
                if (OverlayForm.MainForm != null)
                {
                    OverlayForm.MainForm.DisplaySticker(stickerId);
                }
                else
                {
                    Console.WriteLine("MainForm is null.");
                }

                var response = new { status = "success" };
                string jsonResponse = JsonConvert.SerializeObject(response);
                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(jsonResponse);
                context.Response.StatusCode = (int)HttpStatusCode.OK;

                using (var streamWriter = new StreamWriter(context.Response.OutputStream))
                {
                    await streamWriter.WriteAsync(jsonResponse);
                    await streamWriter.FlushAsync();
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine("JSON parsing error: " + ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.OutputStream.WriteAsync(new byte[0], 0, 0);
            }
        }

        public class StickerRequest
        {
            public string StickerId { get; set; }
        }

        // 封装响应代码以避免重复
        async static Task SendResponseAsync(HttpListenerContext context, string jsonResponse) {
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = Encoding.UTF8.GetByteCount(jsonResponse);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            using (var streamWriter = new StreamWriter(context.Response.OutputStream)) {
                await streamWriter.WriteAsync(jsonResponse);
                await streamWriter.FlushAsync();
            }
        }
        static void InvokeAction(System.Action action)
        {
            if (OverlayForm.MainForm.InvokeRequired)
            {
                OverlayForm.MainForm.Invoke(action);
            }
            else
            {
                action();
            }
        }
        private static async Task HandlemessageRequest(HttpListenerContext context)
        {
            try
            {
                // 初始化 form（如果未初始化

                // 读取请求体中的消息
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    // 异步读取整个请求体内容
                    string json = await reader.ReadToEndAsync();

                    // 确保 JSON 内容不为空
                    if (!string.IsNullOrEmpty(json))
                    {
                        // 返回成功响应
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "application/json";  // 确保返回 JSON 格式
                        int startIndex = json.IndexOf("\"message\":\"") + 11; // 11 是 "message\":\"" 的长度
                        int endIndex = json.IndexOf("\"", startIndex);
                        string Messagefist ="藏鏡人:";

                        // 如果找到了 "message" 字段
                        if (startIndex >= 0 && endIndex >= 0)
                        {
                            // 提取 "message" 字段的值
                            string message = json.Substring(startIndex, endIndex - startIndex);
                            for(int i=0;i<3;i++)
                            {
                                string Messagelast="";
                                for(int j=0;j < message.Length;j++){
                                    Messagelast += message[j];
                                    await Task.Delay(300);
                                    // 将读取到的 "message" 字段传递给 UI 控件显示
                                    InvokeAction(() => OverlayForm.MainForm.ShowmessageLabel(Messagefist+Messagelast+'_'));
                                }
                                await Task.Delay(3000);
                            }                          
                        }
                    }
                    else
                    {
                        // 如果没有有效的 JSON 数据，返回错误响应
                        context.Response.StatusCode = 400; // Bad Request
                        context.Response.StatusDescription = "Invalid JSON data.";
                    }
                }
            }
            catch (JsonException ex)
            {
                context.Response.StatusCode = 400;
                Console.WriteLine("解析留言数据时出错");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                Console.WriteLine("服务器内部错误");
            }
            finally
            {
                context.Response.Close();
            }
        }

        private static async Task OutputMessageAsync(string message, Label messageLabel)
        {
            messageLabel.Text = ""; // 清空现有的文本

            foreach (char c in message)
            {
                messageLabel.Text += c;  // 逐字显示消息
                await Task.Delay(500);  // 每个字符间隔 0.5 秒
            }

            // 模拟等待 5 秒后继续其他操作
            await Task.Delay(5000);
        }

        private static string GetMimeType(string filePath)
        {
            // 获取MIME类型的逻辑
            string mimeType = "application/octet-stream"; // Default MIME type
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            // MIME type lookup based on file extension
            switch (extension)
            {
                case ".html":
                case ".htm":
                    mimeType = "text/html";
                    break;
                case ".css":
                    mimeType = "text/css";
                    break;
                case ".js":
                    mimeType = "application/javascript";
                    break;
                case ".png":
                    mimeType = "image/png";
                    break;
                case ".jpg":
                case ".jpeg":
                    mimeType = "image/jpeg";
                    break;
                case ".gif":
                    mimeType = "image/gif";
                    break;
                case ".svg":
                    mimeType = "image/svg+xml";
                    break;
                case ".json":
                    mimeType = "application/json";
                    break;
                // Add more cases for other file types as needed
            }

            return mimeType;
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString(); // 返回找到的 IPv4 地址
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        // 启动服务器的公共方法
        private static async Task HandleFavoriteRequest(HttpListenerContext context)
        {
            try
            {
                if (PrimaryForm.isPhoneNumberValid && !string.IsNullOrEmpty(PrimaryForm.phonenumber))
                {
                    string phone = PrimaryForm.phonenumber; // 直接通过类名访问
                    // 登录用户
                    SongListManager.Instance.UserLogin(phone);

                    // 获取用户的收藏歌曲
                    var favoriteSongs = SongListManager.Instance.GetFavoriteSongsByPhoneNumber();

                    // 创建响应数据
                    var response = new
                    {
                        isLoggedIn = true,
                        favoriteSongList = favoriteSongs
                            .Select(song => new
                            {
                                song.Song,
                                song.ArtistA,
                                song.SongNumber,
                                song.Category,
                                song.PhoneticNotation,
                                song.PinyinNotation,
                                song.ArtistAPhonetic,
                                song.ArtistBPhonetic,
                                song.ArtistASimplified,
                                song.ArtistBSimplified,
                                song.SongSimplified,
                                song.SongGenre,
                                song.ArtistAPinyin,
                                song.ArtistBPinyin,
                                song.HumanVoice,
                                song.AddedTime,
                                song.SongFilePathHost1,
                                song.SongFilePathHost2    // 例如语言类别等信息
                            })
                            .ToList(),
                        status = "success",
                        message = "Favorites retrieved successfully"
                    };

                    string jsonResponse = JsonConvert.SerializeObject(response);
                    Console.WriteLine("Serialized JSON Response: " + jsonResponse);

                    // 返回响应
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await SendResponseAsyncs(context, jsonResponse);
                }
                else
                {
                    // 如果手机号无效，返回未登录的状态
                    var response = new
                    {
                        isLoggedIn = false,
                        status = "error",
                        message = "Invalid mobile number or user not logged in"
                    };
                    string jsonResponse = JsonConvert.SerializeObject(response);
                    Console.WriteLine("Sending not logged in response: " + jsonResponse);

                    // 返回错误响应
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await SendResponseAsyncs(context, jsonResponse);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling favorite song request: {ex.Message}");

                // 增强异常信息，返回详细的错误
                var errorResponse = new
                {
                    status = "error",
                    message = "An error occurred while processing your request",
                    errorDetails = ex.Message  // 可以把错误信息传递给前端，帮助调试
                };

                string jsonResponse = JsonConvert.SerializeObject(errorResponse);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await SendResponseAsyncs(context, jsonResponse);
            }
        }

        // 异步响应发送方法
        private static async Task SendResponseAsyncs(HttpListenerContext context, string responseContent)
        {
            try
            {
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    await writer.WriteAsync(responseContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending response: " + ex.Message);
            }
        }
        
        private static async Task HandleGetRequest(HttpListenerContext context, string baseDirectory, string requestedFile)
        {
            try
            {
                // 获取请求的完整URL路径
                string requestPath = context.Request.Url.AbsolutePath;
                
                // 检查URL是否包含随机文件夹名
                if (!requestPath.Contains("/" + randomFolderPath + "/"))
                {
                    Console.WriteLine($"Access denied: Request path {requestPath} does not contain valid random folder");
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                string filePath = Path.Combine(baseDirectory, requestedFile.TrimStart('/'));
                
                if (!File.Exists(filePath))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return;
                }

                string contentType = GetMimeType(filePath);
                context.Response.ContentType = contentType;
                
                // 对于静态文件使用缓存
                if (_fileCache.TryGetValue(filePath, out byte[] cachedContent))
                {
                    context.Response.ContentLength64 = cachedContent.Length;
                    await context.Response.OutputStream.WriteAsync(cachedContent, 0, cachedContent.Length);
                    return;
                }

                // 读取文件内容
                byte[] buffer;
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    buffer = new byte[fileStream.Length];
                    await fileStream.ReadAsync(buffer, 0, buffer.Length);
                }
                
                // 缓存静态文件
                if (contentType.StartsWith("text/") || contentType.Contains("javascript") || contentType.Contains("json"))
                {
                    _fileCache.TryAdd(filePath, buffer);
                }

                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error serving file: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
    }
}