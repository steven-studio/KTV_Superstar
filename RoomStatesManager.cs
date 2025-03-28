using System;
using System.Collections.Generic;
using System.IO;

namespace KTV_Superstar;

public static class RoomStateManager
{
    private const string RoomStateFilePath = "roomstates.txt";

    /// <summary>
    /// 判斷當前主機名稱是否對應於Service狀態。
    /// </summary>
    public static void CheckAndHandleRoomState()
    {
        try
        {
            // 讀取roomstates.txt內容
            var roomStates = ParseRoomStatesFile(RoomStateFilePath);
            string currentHostName = Environment.MachineName;

            // 查找當前主機名稱的狀態
            if (roomStates.TryGetValue(currentHostName, out string? state))
            {
                Console.WriteLine($"Host: {currentHostName}, State: {state}");
                
                if (state == "NonService")
                {
                    // 呼叫開關台功能
                    PrimaryForm.Instance.ShowSendOffScreen();
                }
                else
                {
                    Console.WriteLine($"Room state for {currentHostName} is not 'Service'.");
                }
            }
            else
            {
                Console.WriteLine($"Host {currentHostName} not found in room states.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking room state: {ex.Message}");
        }
    }

    /// <summary>
    /// 解析roomstates.txt文件並返回字典格式的房間狀態。
    /// </summary>
    /// <param name="filePath">文件路徑</param>
    /// <returns>房間名稱與狀態的字典</returns>
    private static Dictionary<string, string> ParseRoomStatesFile(string filePath)
    {
        // 使用忽略大小寫的比較器
        var roomStates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File {filePath} does not exist.");
            return roomStates;
        }

        foreach (var line in File.ReadLines(filePath))
        {
            // 跳過空行或不符合格式的行
            if (string.IsNullOrWhiteSpace(line) || !line.Contains(";"))
                continue;

            var parts = line.Split(';');
            if (parts.Length < 5)
                continue;

            // 將主機名稱與狀態對應
            string hostName = parts[0]; // 第一欄為主機名稱
            string status = parts[4];   // 第五欄為狀態
            roomStates[hostName] = status;
        }

        return roomStates;
    }
}
