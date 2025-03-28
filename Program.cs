using System;
using System.Globalization;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using OfficeOpenXml; // 引用 EPPlus 的命名空間
using DirectShowLib;
using DirectShowLib.Dvd;
using DirectShowLib.BDA;
using DirectShowLib.DES;
using DirectShowLib.DMO;
using ZXing;
using ZXing.QrCode;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;
using System.Diagnostics;

namespace KTV_Superstar;

static class Program
{
    // 定义全局变量
    internal static SongListManager songListManager = default!;
    internal static ArtistManager artistManager = default!;
    internal static SerialPortManager serialPortManager = default!;
    private static PrimaryForm primaryForm = default!; // 儲存實例的參考

    [DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            // COM 初始化
            int hr = ComInterop.CoInitializeEx(IntPtr.Zero, ComInterop.COINIT_APARTMENTTHREADED);
            if (hr < 0)
            {
                Console.WriteLine("Failed to initialize COM library.");
                return;
            }

            // URL ACL 配置
            string ipAddress = "192.168.88.66";
            string port = "9090";
            string url = $"http://{ipAddress}:{port}/";

            if (!IsUrlAclExists(url))
            {
                RunBatchFileToAddUrlAcl(ipAddress, port);
            }

            // 啟用 DPI 感知（僅適用於 Windows Vista 及以上）
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 初始化管理器

            songListManager = SongListManager.Instance;  // 使用单例
            artistManager = new ArtistManager();

            var commandHandler = new CommandHandler(songListManager);
            serialPortManager = new SerialPortManager(commandHandler);
            serialPortManager.InitializeSerialPort();

            // 啟動 HandwritingRecognitionServer (獨立啟動外部exe)
            //StartHandwritingRecognitionServer();

            // 輸出屏幕信息
            Console.WriteLine($"Virtual Screen: {SystemInformation.VirtualScreen}");
            foreach (var screen in Screen.AllScreens)
            {
                Console.WriteLine($"Screen: {screen.DeviceName} Resolution: {screen.Bounds.Width}x{screen.Bounds.Height}");
            }

            // Start servers
            Task.Run(() => HttpServerManager.StartServer());
            Task.Run(() => TCPServerManager.StartServer());

            // 當應用程序關閉時，關閉 SerialPort
            Application.ApplicationExit += (sender, e) => SerialPortManager.CloseSerialPortSafely();
            // 註冊顯示設置更改事件
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;

            primaryForm = new PrimaryForm();
            primaryForm.allSongs = songListManager.AllSongs;
            primaryForm.allArtists = artistManager.AllArtists;

            // 設置 PrimaryForm 的位置和大小
            primaryForm.StartPosition = FormStartPosition.Manual;
            primaryForm.Location = new Point(0, 0); // 將窗體放置於屏幕左上角
            primaryForm.Size = new Size(Screen.PrimaryScreen!.Bounds.Width, Screen.PrimaryScreen.Bounds.Height); // 設置為滿屏

            InitializeSecondaryScreen();

            // primaryForm.Show(); // 移除這一行
            // primaryForm.ShowSendOffScreen();

            // 其他初始化代碼...
            RoomStateManager.CheckAndHandleRoomState();
            
            // 主窗體顯示代碼...
            Application.Run(primaryForm);
        }
        catch (Exception ex)
        {
            WriteLog(ex.ToString());
        }
        finally
        {
            SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        }
    }

    private static bool IsUrlAclExists(string url)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = "http show urlacl",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo)!)
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string output = reader.ReadToEnd();
                    return output.Contains(url); // 检查是否包含指定 URL
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("检查 URL ACL 时出错: " + ex.Message);
            return false;
        }
    }

    // 动态创建并运行批处理文件以添加 URL ACL
    private static void RunBatchFileToAddUrlAcl(string ipAddress, string port)
    {
        // 确保批处理文件在当前程序的同一目录下
        string batchFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AddUrlAcl.bat");

        try
        {
            // 创建批处理内容
            string batchContent = 
                $"netsh http add urlacl url=http://{ipAddress}:{port}/ user=Everyone\n"; 

            // 写入批处理文件，确保使用 UTF-8 编码
            File.WriteAllText(batchFilePath, batchContent);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe", // 使用 cmd 执行
                Arguments = $"/c \"{batchFilePath}\"", // /c 参数用于执行命令后关闭命令窗口
                UseShellExecute = true, // 使用系统外壳程序来启动
                Verb = "runas" // 以管理员身份运行
            };

            Process process = Process.Start(startInfo)!;
            process.WaitForExit(); // 等待批处理执行完成
        }
        catch (Exception ex)
        {
            Console.WriteLine("执行批处理文件失败: " + ex.Message);
        }
    }

    private async static void InitializeSecondaryScreen()
    {
        if (Screen.AllScreens.Length > 1)
        {
            var secondaryScreen = Screen.AllScreens.FirstOrDefault(s => !s.Primary);
            if (secondaryScreen != null)
            {
                // 确保 primaryForm 和 videoPlayerForm 已经正确初始化
                if (primaryForm.videoPlayerForm == null)
                {
                    primaryForm.videoPlayerForm = new VideoPlayerForm();
                }

                // 设置 videoPlayerForm 的位置和大小
                // primaryForm.videoPlayerForm.StartPosition = FormStartPosition.Manual;
                // primaryForm.videoPlayerForm.Location = secondaryScreen.WorkingArea.Location;
                // primaryForm.videoPlayerForm.Size = secondaryScreen.WorkingArea.Size;

                // 显示 videoPlayerForm 在第二显示器
                primaryForm.videoPlayerForm.Show();

                // 初始化公共播放列表
                await primaryForm.videoPlayerForm.InitializePublicPlaylist(primaryForm.publicSongList);
            }
        }
    }

    private static void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        // UI操作應該放在try-catch塊中
        try
        {
            if (Screen.AllScreens.Length > 1)
            {
                primaryForm.Invoke(new System.Action(async () =>
                {
                    if (primaryForm.videoPlayerForm == null)
                    {
                        var filePath = @"C:\\video\\100015-周杰倫&aMei-不該-國語-vL-100-11000001.mpg";
                        if (File.Exists(filePath))
                        {
                            Screen secondaryScreen = Screen.AllScreens.FirstOrDefault(s => !s.Primary)!;
                            if (secondaryScreen != null)
                            {
                                primaryForm.videoPlayerForm = new VideoPlayerForm();
                                // primaryForm.primaryMediaPlayerForm = new PrimaryMediaPlayerForm(primaryForm, primaryForm.secondaryMediaPlayerForm);
                                await primaryForm.videoPlayerForm.InitializePublicPlaylist(primaryForm.publicSongList);
                                primaryForm.videoPlayerForm.Show();
                            }
                        }
                        else
                        {
                            Console.WriteLine("File not found.");
                        }
                    }
                }));
            }
        }
        catch (Exception ex)
        {
            WriteLog("Error during display settings changed: " + ex.Message);
        }
    }

    static void WriteLog(string message)
    {
        // 指定日志文件的路径
        string logFilePath = "mainlog.txt"; // 请根据需要修改文件路径

        try
        {
            // 使用 StreamWriter 来向日志文件追加文本
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(String.Format("[{0}] {1}", DateTime.Now, message));
            }
        }
        catch (Exception ex)
        {
            // 如果写入日志文件时发生错误，这里可以处理这些异常
            // 例如：打印到控制台
            Console.WriteLine(String.Format("Error writing to log file: {0}", ex.Message));
        }
    }

    private static Form CreatePrimaryForm()
    {
        return new Form
        {
            WindowState = FormWindowState.Maximized,
            FormBorderStyle = FormBorderStyle.None
        };
    }

    private static Form CreateSecondaryForm(Screen screen)
    {
        return new Form
        {
            Text = "Secondary Screen Form",
            StartPosition = FormStartPosition.Manual,
            Bounds = screen.Bounds,
            WindowState = FormWindowState.Maximized,
            FormBorderStyle = FormBorderStyle.None
        };
    }

    // 建議你另外撰寫此函式，來處理外部程序啟動
    private static void StartHandwritingRecognitionServer()
    {
        try
        {
            // 指定外部執行檔的位置，建議使用相對路徑
            string handwritingServerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HandwritingRecognitionServer.exe");

            if (!File.Exists(handwritingServerPath))
            {
                throw new FileNotFoundException("HandwritingRecognitionServer.exe 找不到", handwritingServerPath);
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = handwritingServerPath,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            WriteLog($"啟動 HandwritingRecognitionServer 失敗: {ex.Message}");
            MessageBox.Show($"HandwritingRecognitionServer 無法啟動: {ex.Message}");
        }
    }
}