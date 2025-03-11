using System;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq;


namespace DualScreenDemo
{
    public class SerialPortManager
    {
        internal static SerialPort mySerialPort;
        private readonly CommandHandler commandHandler;

        public SerialPortManager(CommandHandler commandHandler)
        {
            this.commandHandler = commandHandler;
        }

    public void InitializeSerialPort()
    {
        string[] ports = SerialPort.GetPortNames();
        Console.WriteLine("可用的串列埠:");
        foreach (var port in ports)
        {
            Console.WriteLine(port);
        }

        // 定義優先選擇的串列埠順序
        string[] preferredPorts = { "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM1" };

        // 選擇第一個符合的串列埠
        string selectedPort = preferredPorts.FirstOrDefault(port => ports.Contains(port));

        if (!string.IsNullOrEmpty(selectedPort))
        {
            mySerialPort = new SerialPort(selectedPort);
            Console.WriteLine($"已選擇串列埠: {selectedPort}");
        }
        else
        {
            MessageBox.Show("未找到任何可用的串列埠！");
            return;
        }

        // 配置串列埠參數
        mySerialPort.BaudRate = 9600;
        mySerialPort.Parity = Parity.None;
        mySerialPort.StopBits = StopBits.One;
        mySerialPort.DataBits = 8;
        mySerialPort.Handshake = Handshake.None;

        // 綁定資料接收事件
        mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

        try
        {
            mySerialPort.Open();
            Console.WriteLine($"{selectedPort} 串列埠已成功開啟。");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"開啟 {selectedPort} 串列埠時發生錯誤: {ex.Message}");
        }
    }


        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;

                if (!sp.IsOpen)
                {
                    // Console.WriteLine("串列埠未開啟，無法接收資料。");
                    return;
                }

                int bytesToRead = sp.BytesToRead;
                if (bytesToRead > 0)
                {
                    
                    byte[] buffer = new byte[bytesToRead];
                    int bytesRead = sp.Read(buffer, 0, bytesToRead);

                    
                    StringBuilder hexData = new StringBuilder(bytesRead * 2);
                    for (int i = 0; i < bytesRead; i++)
                    {
                        hexData.AppendFormat("{0:X2}", buffer[i]);
                    }

                    string indata = hexData.ToString();
                    // Console.WriteLine($"接收到的資料 (Hex): {indata}");

                    
                    Task.Run(() =>
                    {
                        try
                        {
                            commandHandler.ProcessData(indata);
                        }
                        catch (Exception processEx)
                        {
                            // Console.WriteLine($"處理資料時發生錯誤: {processEx.Message}");
                        }
                    });
                }
                else
                {
                    // Console.WriteLine("未接收到任何資料。");
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"接收資料時發生錯誤: {ex.Message}");
            }
        }



        public static void CloseSerialPortSafely()
        {
            if (mySerialPort != null)
            {
                try
                {
                    if (mySerialPort.IsOpen)
                    {
                        mySerialPort.Close();
                        // Console.WriteLine("串列埠已成功關閉。");
                    }
                }
                catch (Exception ex)
                {
                    // Console.WriteLine($"關閉串列埠時發生錯誤: {ex.Message}");
                }
            }
        }

        // public void EnsurePortConnection()
        // {
        //     try
        //     {
        //         if (mySerialPort == null || !mySerialPort.IsOpen)
        //         {
        //             // Console.WriteLine("串列埠已中斷，重新初始化...");
        //             InitializeSerialPort();
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         // Console.WriteLine($"檢查串列埠連接時發生錯誤: {ex.Message}");
        //     }
        // }


        // public static void LogData(string data)
        // {
        //     string filePath = Path.Combine(Application.StartupPath, "dataLog.txt");

        //     try
        //     {
        //         File.AppendAllText(filePath, $"{DateTime.Now}: {data}{Environment.NewLine}");
        //         // Console.WriteLine($"資料已記錄到日誌: {data}");
        //     }
        //     catch (Exception ex)
        //     {
        //         // Console.WriteLine($"記錄日誌時發生錯誤: {ex.Message}");
        //     }
        // }
        // public void CheckAndResetConnection()
        // {
        //     if (mySerialPort == null || !mySerialPort.IsOpen)
        //     {
        //         // Console.WriteLine("發現串列埠問題，嘗試重啟連線...");
        //         EnsurePortConnection();
        //     }
        // }
        // public void SendHeartbeat()
        // {
        //     if (mySerialPort != null && mySerialPort.IsOpen)
        //     {
        //         try
        //         {
        //             mySerialPort.Write("HEARTBEAT");
        //             // Console.WriteLine("已發送心跳信號");
        //         }
        //         catch (Exception ex)
        //         {
        //             // Console.WriteLine($"發送心跳信號時發生錯誤: {ex.Message}");
        //         }
        //     }
        // }
    }
}
