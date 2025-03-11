using System;
using System.Net;
using System.Net.Sockets; 
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Threading.Tasks;
using System.IO; // 為 Path 和 File 提供支持
using System.Windows.Forms; // 為 Invoke 和 Form 控件提供支持
using System.Collections.Generic;

namespace DualScreenDemo
{
    public class TCPServer
    {
        private TcpListener listener;
        private const int Port = 1000; 
        private readonly string hostNameSuffix;
        private bool isProcessingCommand = false;
        

        public TCPServer()
        {
            listener = new TcpListener(IPAddress.Any, Port);
            hostNameSuffix = GetHostNameSuffix();
        }

        private bool IsFormReady(Form form)
        {
            if (form == null) return false;
            bool isReady = false;
            try
            {
                if (form.IsHandleCreated && !form.IsDisposed)
                {
                    if (form.InvokeRequired)
                    {
                        form.Invoke(new Action(() => isReady = true));
                    }
                    else
                    {
                        isReady = true;
                    }
                }
            }
            catch
            {
                isReady = false;
            }
            return isReady;
        }

        private async Task SafeInvoke(Form form, Action action, int maxRetries = 10)
        {
            if (form == null) return;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (IsFormReady(form))
                    {
                        if (form.InvokeRequired)
                        {
                            form.Invoke(action);
                        }
                        else
                        {
                            action();
                        }
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Invoke attempt {i + 1} failed: {ex.Message}");
                }

                await Task.Delay(500); // 等待500毫秒后重试
            }
            Console.WriteLine("Failed to invoke action after maximum retries");
        }

        public void Start()
        {
            listener.Start();
            Console.WriteLine("Server started on port " + Port + ".");
            try {
                string stateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "states.txt");
                string initialState = ReadStateFile(stateFilePath);

                if (initialState.Equals("CLOSE", StringComparison.OrdinalIgnoreCase))
                {
                    _ = SafeInvoke(PrimaryForm.Instance, () =>
                    {
                        try {
                            foreach (Control ctrl in PrimaryForm.Instance.Controls)
                            {
                                ctrl.Enabled = false;
                            }
                            PrimaryForm.Instance.ShowSendOffScreen();
                        }
                        catch (Exception ex) {
                            Console.WriteLine($"顯示送客畫面時發生錯誤: {ex.Message}");
                        }
                    });
                }

                while (true)
                {
                    Console.WriteLine("Waiting for connections...");
                    using (TcpClient client = listener.AcceptTcpClient())
                    {
                        Console.WriteLine("Connected!");
                        NetworkStream stream = client.GetStream();

                        while (client.Connected)
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0)
                                break;

                            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            Console.WriteLine("Received: " + request.Trim());

                            if (request.Length < 5)
                            {
                                continue;
                            }

                            string requestHostSuffix = request.Substring(0, 3);
                            string command = request.Substring(4);

                            if (requestHostSuffix.Equals(hostNameSuffix, StringComparison.OrdinalIgnoreCase))
                            {
                                if (command.Trim().Equals("X", StringComparison.OrdinalIgnoreCase))
                                {
                                    _ = SafeInvoke(VideoPlayerForm.Instance, async () =>
                                    {
                                        if (IsFormReady(PrimaryForm.Instance))
                                        {
                                            await SafeInvoke(PrimaryForm.Instance, () =>
                                            {
                                                PrimaryForm.Instance.ShowSendOffScreen();

                                                Console.WriteLine("開始設置新的播放列表");
                                                
                                                string closePath = @"C:\video\CLOSE.MPG";
                                                if (File.Exists(closePath))
                                                {
                                                    SongData closeSong = new SongData(
                                                        "", "", "結束播放", 0, "", "", "", "", 
                                                        DateTime.Now, closePath, "", "", "", "", 
                                                        "", "", "", "", "", "", "", 1
                                                    );

                                                    VideoPlayerForm.playingSongList = new List<SongData>();
                                                    
                                                    if (VideoPlayerForm.Instance.currentPlayingSong != null)
                                                    {
                                                        VideoPlayerForm.playingSongList.Add(VideoPlayerForm.Instance.currentPlayingSong);
                                                    }
                                                    
                                                    VideoPlayerForm.playingSongList.Add(closeSong);
                                                    PrimaryForm.userRequestedSongs = new List<SongData>();

                                                    if (IsFormReady(OverlayForm.MainForm))
                                                    {
                                                        OverlayForm.MainForm.nextSongLabel.Visible = false;
                                                    }
                                                    
                                                    Console.WriteLine("已設置新的播放列表，包含當前歌曲和 CLOSE.MPG");
                                                }
                                                else
                                                {
                                                    Console.WriteLine($"錯誤: 找不到檔案 {closePath}");
                                                }
                                            });
                                        }
                                    });

                                    UpdateStateFile(stateFilePath, "CLOSE");
                                    continue;
                                }

                                if (command.Trim().Equals("O", StringComparison.OrdinalIgnoreCase))
                                {
                                    _ = SafeInvoke(PrimaryForm.Instance, () =>
                                    {
                                        PrimaryForm.Instance.HideSendOffScreen();
                                    });

                                    UpdateStateFile(stateFilePath, "OPEN");
                                    continue;
                                }
                            }

                            if (IsFormReady(OverlayForm.MainForm))
                            {
                                string message = request.Trim();
                                string pattern = @"^(全部|\d{4})\((白色|紅色|綠色|黑色|藍色)\)-";
                                Match match = Regex.Match(message, pattern);

                                _ = SafeInvoke(OverlayForm.MainForm, () =>
                                {
                                    if (match.Success)
                                    {
                                        string marqueeMessage = message.Substring(match.Value.Length).Trim();
                                        Color textColor = GetColorFromString(match.Groups[2].Value);
                                        OverlayForm.MainForm.UpdateMarqueeText(marqueeMessage, OverlayForm.MarqueeStartPosition.Middle, textColor);
                                    }
                                    else
                                    {
                                        string marqueeMessage = "系統公告: " + message;
                                        OverlayForm.MainForm.UpdateMarqueeTextSecondLine(marqueeMessage);
                                    }
                                });
                            }

                            if (request.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
                            {
                                break;
                            }
                        }

                        Console.WriteLine("Connection closed.");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            finally
            {
                listener.Stop();
            }
        }

        private Color GetColorFromString(string colorName)
        {
            switch (colorName)
            {
                case "白色":
                    return Color.White;
                case "紅色":
                    return Color.Red;
                case "綠色":
                    return Color.LightGreen;
                case "黑色":
                    return Color.Black;
                case "藍色":
                    return Color.LightBlue;
                default:
                    return Color.Black; 
            }
        }

        private string GetHostNameSuffix()
        {
            string hostName = Dns.GetHostName();
            return hostName.Length > 3 ? hostName.Substring(hostName.Length - 3) : hostName;
        }
        private string ReadStateFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string state = File.ReadAllText(filePath).Trim();
                    Console.WriteLine($"✅ State file read: {filePath} -> {state}");
                    return state;
                }
                else
                {
                    Console.WriteLine("⚠️ State file not found. Creating new file with default state: OPEN");
                    UpdateStateFile(filePath, "OPEN");
                    return "OPEN";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to read state file: {ex.Message}");
                return "OPEN"; // 默認為 OPEN
            }
        }
        private void UpdateStateFile(string filePath, string state)
        {
            try
            {
                File.WriteAllText(filePath, state);
                Console.WriteLine($"✅ State file updated: {filePath} -> {state}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to update state file: {ex.Message}");
            }
        }
    }
}