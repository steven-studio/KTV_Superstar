using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;  
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;

namespace KTV_Superstar;

public class SongListManager
{
    private static SongListManager? _instance;
    private readonly ISongListRepository _repository;

    public List<SongData> AllSongs { get; private set; }
    public static Dictionary<string, List<SongData>>? NewSongLists { get; private set; }
    public static Dictionary<string, List<SongData>>? HotSongLists { get; private set; }
    public List<SongData> FavoriteSongs { get; private set; }  
    public const int SongsPerPage = 9;

    public bool IsUserLoggedIn { get; set; }  
    public string? UserPhoneNumber { get; set; }  

    // 透過依賴注入傳入 repository 實例
    public SongListManager(ISongListRepository repository)
    {
        _repository = repository;
        AllSongs = new List<SongData>();
        NewSongLists = new Dictionary<string, List<SongData>>();
        HotSongLists = new Dictionary<string, List<SongData>>();
        FavoriteSongs = new List<SongData>();

        TryUpdateDatabase();
        // 呼叫 repository 層來初始化資料庫
        _repository.InitializeDatabase();
        AllSongs = _repository.LoadAllSongs();
        InitializeNewSongLists();
        InitializeHotSongLists();
    }

    private bool TryUpdateDatabase()
    {
        try 
        {
            // 1. 检查是否能连接到 SVR01
            if (!Directory.Exists(@"\\SVR01\SuperstarB"))
            {
                Console.WriteLine("未連接到SVR使用本地DB");
                return true; // 继续使用本地数据库
            }

            // 2. 比较本地和服务器文件
            string localDbPath = Path.Combine(Application.StartupPath, "KSongDatabase.db");
            string serverDbPath = @"\\SVR01\SuperstarB\KSongDatabase.db";

            if (!File.Exists(localDbPath))
            {
                Console.WriteLine("本地無db");
            }
            else
            {
                FileInfo localFile = new FileInfo(localDbPath);
                FileInfo serverFile = new FileInfo(serverDbPath);

                if (serverFile.LastWriteTime <= localFile.LastWriteTime)
                {
                    Console.WriteLine("歌單已是最新");
                    return true;
                }
            }

            // 3. 需要更新时，复制新文件
            Process copyProcess = new Process();
            copyProcess.StartInfo.FileName = "cmd.exe";
            copyProcess.StartInfo.Arguments = "/C copy /Y \\\\SVR01\\SuperstarB\\KSongDatabase.db KSongDatabase.db";
            copyProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            copyProcess.StartInfo.CreateNoWindow = true;
            copyProcess.Start();
            copyProcess.WaitForExit();

            if (copyProcess.ExitCode == 0)
            {
                Console.WriteLine("歌單更新成功");
            }
            else
            {
                Console.WriteLine("歌單複製失敗，使用本地歌單");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新歌單失敗：{ex.Message}");
            return true; // 出错时继续使用本地数据库
        }
    }

    public static SongListManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var repo = new SQLiteSongListRepository(System.IO.Path.Combine(Application.StartupPath, "KSongDatabase.db"));
                _instance = new SongListManager(repo);
            }
            return _instance;
        }
    }

    public List<SongData> GetSongsByArtist(string artistName)
    {
        return AllSongs.Where(song => song.ArtistA == artistName || song.ArtistB == artistName).ToList();
    }
    
    public bool CheckIfPhoneNumberExists(string phoneNumber)
    {
        if (_repository != null)
        {
            return _repository.CheckIfPhoneNumberExists(phoneNumber);
        }
        return false;
    }
    
    public void UserLogin(string phoneNumber)
    {
        IsUserLoggedIn = true;
        UserPhoneNumber = phoneNumber;
        LoadFavoriteSongs();
        Console.WriteLine(String.Format("UserLoggedIn: {0}, PhoneNumber: {1}", IsUserLoggedIn, UserPhoneNumber));
    }
    
    public void UserLogout()
    {
        IsUserLoggedIn = false;
        UserPhoneNumber = null;
        FavoriteSongs.Clear();
    }

    public void AddNewUser(string phoneNumber)
    {
        _repository?.AddNewUser(phoneNumber);
    }

    public void LoadFavoriteSongs()
    {
        if (!IsUserLoggedIn || string.IsNullOrEmpty(UserPhoneNumber))
        {
            Console.WriteLine("User is not logged in or phone number is missing.");
            FavoriteSongs.Clear();
            return;
        }

        // 先清空清單，然後先加入 header 資料（例如空資料或自定義的標題）
        FavoriteSongs.Clear();
        FavoriteSongs.Add(new SongData("", "", UserPhoneNumber + " 的歌單", 0, "", "", "", "", DateTime.MinValue, "", "", "", "", "", "", "", "", "", "", "", "", 1));

        // 從 repository 載入 Favorite 歌曲清單
        List<SongData> repoFavorites = _repository.LoadFavoriteSongs(UserPhoneNumber);
        // 將 repository 的清單合併進 FavoriteSongs
        FavoriteSongs.AddRange(repoFavorites);

        // 如果有需要更新 UI（例如 multiPagePanel），也在此處呼叫
        if (PrimaryForm.Instance?.multiPagePanel != null)
        {
            PrimaryForm.Instance.multiPagePanel.currentPageIndex = 0;
            PrimaryForm.Instance.multiPagePanel.LoadSongs(FavoriteSongs);
        }
    }

    public void AddToFavorite(string songNumber)
    {
        if (!IsUserLoggedIn || string.IsNullOrEmpty(UserPhoneNumber))
        {
            Console.WriteLine("User is not logged in.");
            return;
        }

        try
        {
            // 委派資料庫操作給 repository
            _repository.AddFavorite(UserPhoneNumber, songNumber);

            // 更新本地業務清單
            var song = AllSongs.FirstOrDefault(s => s.SongNumber == songNumber);
            if (song != null)
            {
                FavoriteSongs.Add(song);
                Console.WriteLine($"Added song {songNumber} to favorites.");
            }
            else
            {
                Console.WriteLine($"Song {songNumber} not found in AllSongs.");
            }
        }
        catch (Exception ex)
        {
            // 捕捉 repository 拋出的例外，例如重複新增情形
            Console.WriteLine("Failed to add song to favorites: " + ex.Message);
        }
    }

    public List<SongData> SearchSongsBySinger(string keyword)
    {
        var keywordLower = keyword.ToLower();
        return AllSongs.Where(song => song.ArtistA.ToLower().Contains(keywordLower)
                                    || song.ArtistB.ToLower().Contains(keywordLower))
                    .ToList();
    }

    public List<SongData> SearchSongsByName(string keyword)
    {
        var keywordLower = keyword.ToLower();
        return AllSongs.Where(song => song.Song.ToLower().Contains(keywordLower)).ToList();
    }
    
    public SongData? SearchSongByNumber(string songNumber)
    {
        foreach (var song in AllSongs)
        {
            if (song.SongNumber == songNumber)
            {
                return song; 
            }
        }
        return null; 
    }

    private void InitializeNewSongLists()
    {
        int songLimit = PrimaryForm.ReadNewSongLimit(); 
        
        string[] categories = new string[] { "國語", "台語", "粵語", "英文", "日語", "韓語" };
        
        foreach (var category in categories)
        {
            var songsInCategory = AllSongs
                .Where(s => s.Category == category)
                .OrderByDescending(s => s.AddedTime)
                .Take(songLimit)  
                .ToList();
            NewSongLists?.Add(category, songsInCategory);
        }
    }

    private void InitializeHotSongLists()
    {
        int songLimit = PrimaryForm.ReadHotSongLimit(); 
        
        string[] categories = new string[] { "國語", "台語", "英文", "日語", "韓語" };
        
        foreach (var category in categories)
        {
            var songsInCategory = AllSongs
                .Where(s => s.Category == category)
                .OrderByDescending(s => s.Plays)
                .Take(songLimit)  
                .ToList();
            
            HotSongLists?.Add(category, songsInCategory);
        }
    }

    public List<SongData> GetNewSongsByCategory(string category)
    {
        if (NewSongLists != null && NewSongLists.ContainsKey(category))
            return NewSongLists[category];
        else
            return new List<SongData>();  
    }

    public List<SongData> GetHotSongsByCategory(string category)
    {
        if (HotSongLists != null && HotSongLists.ContainsKey(category))
            return HotSongLists[category];
        else
            return new List<SongData>();  
    }

    public List<SongData> GetFavoriteSongsByPhoneNumber()
    {
        if (string.IsNullOrEmpty(UserPhoneNumber))
        {
            return new List<SongData>();
        }
        return _repository.LoadFavoriteSongs(UserPhoneNumber);
    }
}
