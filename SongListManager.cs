using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;  
using System.Windows.Forms;
using System.Globalization;  
using System.Diagnostics;

namespace DualScreenDemo
{
    public class SongListManager
    {
        private static SongListManager _instance;
        public List<SongData> AllSongs { get; private set; }
        public static Dictionary<string, List<SongData>> NewSongLists { get; private set; }
        public static Dictionary<string, List<SongData>> HotSongLists { get; private set; }
        public List<SongData> FavoriteSongs { get; private set; }  
        public const int SongsPerPage = 9;

        public bool IsUserLoggedIn { get; set; }  
        public string UserPhoneNumber { get; set; }  

        public SongListManager()
        {
            AllSongs = new List<SongData>();
            NewSongLists = new Dictionary<string, List<SongData>>();
            HotSongLists = new Dictionary<string, List<SongData>>();
            FavoriteSongs = new List<SongData>();

            // 尝试更新数据库，但无论结果如何都继续运行
            TryUpdateDatabase();

            // 继续使用可用的数据库（可能是更新后的或原本的本地数据库）
            InitializeDatabase();
            LoadSongs();
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
                    _instance = new SongListManager();
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
            string databaseFileName = "KSongDatabase.db";
            string databasePath = Path.Combine(Application.StartupPath, databaseFileName);
            string connectionString = String.Format("Data Source={0};Version=3;", databasePath);

            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string sql = "SELECT COUNT(1) FROM FavoriteSongs WHERE PhoneNumber = @PhoneNumber";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to check phone number in SQLite database: " + ex.Message);
                    return false;
                }
                finally
                {
                    connection.Close();
                }
            }
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

        public void InitializeDatabase()
        {
            string databaseFileName = "KSongDatabase.db";
            string databasePath = Path.Combine(Application.StartupPath, databaseFileName);
            string connectionString = String.Format("Data Source={0};Version=3;", databasePath);

            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string createTableSql = @"
                        CREATE TABLE IF NOT EXISTS FavoriteSongs (
                            PhoneNumber TEXT NOT NULL,
                            SongNumber TEXT NOT NULL,
                            PRIMARY KEY (PhoneNumber, SongNumber)
                        );";
                    using (var command = new SQLiteCommand(createTableSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to initialize SQLite database: " + ex.Message);
                }
            }
        }

        public void AddNewUser(string phoneNumber)
        {
            string databaseFileName = "KSongDatabase.db";
            string databasePath = Path.Combine(Application.StartupPath, databaseFileName);
            string connectionString = String.Format("Data Source={0};Version=3;", databasePath);

            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    
                    string checkTableSql = "SELECT name FROM sqlite_master WHERE type='table' AND name='FavoriteSongs';";
                    using (var checkCommand = new SQLiteCommand(checkTableSql, connection))
                    {
                        var result = checkCommand.ExecuteScalar();
                        if (result == null)
                        {
                            throw new Exception("Table 'FavoriteSongs' does not exist.");
                        }
                    }

                    string sql = "INSERT INTO FavoriteSongs (PhoneNumber, SongNumber) VALUES (@PhoneNumber, @SongNumber)";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                        command.Parameters.AddWithValue("@SongNumber", "000000");
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to add new user to SQLite database: " + ex.Message);
                }
            }
        }

        private void LoadSongs()
        {
            string databaseFileName = "KSongDatabase.db";
            string databasePath = Path.Combine(Application.StartupPath, databaseFileName);
            
            Console.WriteLine(databasePath);
            string connectionString = String.Format("Data Source={0};Version=3;", databasePath);

            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();  

                    string sql = "SELECT 歌曲編號, 語別, 歌曲名稱, 點播次數, [歌星 A], [歌星 B], 新增日期, [路徑 1], [路徑 2], 歌曲檔名, 歌曲注音, 歌曲拼音, 歌星A分類, 歌星B分類, 歌星A注音, 歌星B注音, 歌星A簡體, 歌星B簡體, 歌名簡體, 分類, 歌星A拼音, 歌星B拼音, 人聲 FROM SongLibrary";  
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())  
                            {
                                string songNumber = reader["歌曲編號"].ToString();
                                string category = reader["語別"].ToString();
                                string song = reader["歌曲名稱"].ToString();
                                int plays = Convert.ToInt32(reader["點播次數"]);
                                string artistA = reader["歌星 A"].ToString();
                                string artistB = reader["歌星 B"].ToString();
                                string artistACategory = reader["歌星A分類"].ToString(); 
                                string artistBCategory = reader["歌星B分類"].ToString(); 
                                string dateValue = reader["新增日期"].ToString();
                                DateTime addedTime;

                                if (string.IsNullOrWhiteSpace(dateValue))
                                {
                                    // Console.WriteLine(String.Format("Date value is null or empty for song: {0}. Setting to default DateTime.", song));
                                    addedTime = DateTime.Now; 
                                }
                                else
                                {
                                    try
                                    {
                                        addedTime = DateTime.ParseExact(dateValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                    }
                                    catch (System.FormatException ex)
                                    {
                                        // Console.WriteLine(String.Format("Invalid date format for song: {0}. Error: {1}", song, ex.Message));
                                        addedTime = DateTime.Now; 
                                    }
                                }
                                string basePathHost1 = reader["路徑 1"].ToString();
                                string basePathHost2 = reader["路徑 2"].ToString();
                                string fileName = reader["歌曲檔名"].ToString();
                                string songFilePathHost1 = Path.Combine(basePathHost1, fileName);
                                string songFilePathHost2 = Path.Combine(basePathHost2, fileName);
                                string phoneticNotation = reader["歌曲注音"].ToString();
                                string pinyinNotation = reader["歌曲拼音"].ToString();
                                string artistAPhonetic = reader["歌星A注音"].ToString();
                                string artistBPhonetic = reader["歌星B注音"].ToString();
                                string artistASimplified = reader["歌星A簡體"].ToString();
                                string artistBSimplified = reader["歌星B簡體"].ToString();
                                string songSimplified = reader["歌名簡體"].ToString();
                                string songGenre = reader["分類"].ToString();  
                                string artistAPinyin = reader["歌星A拼音"].ToString();
                                string artistBPinyin = reader["歌星B拼音"].ToString();
                                int humanVoice = Convert.ToInt32(reader["人聲"]);  

                                AllSongs.Add(new SongData(songNumber, category, song, plays, artistA, artistB, artistACategory, artistBCategory, addedTime, songFilePathHost1, songFilePathHost2, phoneticNotation, pinyinNotation, artistAPhonetic, artistBPhonetic, artistASimplified, artistBSimplified, songSimplified, songGenre, artistAPinyin, artistBPinyin, humanVoice));
                            }
                        }
                    }

                    connection.Close();  
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load songs from SQLite database: " + ex.Message);
                }
            }
        }

        public void LoadFavoriteSongs()
        {
            if (!IsUserLoggedIn || string.IsNullOrEmpty(UserPhoneNumber))
                return;

            string databaseFileName = "KSongDatabase.db";
            string databasePath = Path.Combine(Application.StartupPath, databaseFileName);
            string connectionString = String.Format("Data Source={0};Version=3;", databasePath);

            
            FavoriteSongs.Clear();
            FavoriteSongs.Add(new SongData("", "", UserPhoneNumber + " 的歌單", 0, "", "", "", "", DateTime.MinValue, "", "", "", "", "", "", "", "", "", "", "", "", 1));

            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();  

                    string sql = @"
                        SELECT 
                            sl.歌曲編號, sl.語別, sl.歌曲名稱, sl.點播次數, 
                            sl.[歌星 A], sl.[歌星 B], sl.新增日期, sl.[路徑 1], 
                            sl.[路徑 2], sl.歌曲檔名, sl.歌曲注音, sl.歌曲拼音, 
                            sl.歌星A分類, sl.歌星B分類, sl.歌星A注音, sl.歌星B注音, 
                            sl.歌星A簡體, sl.歌星B簡體, sl.歌名簡體, sl.分類, 
                            sl.歌星A拼音, sl.歌星B拼音, sl.人聲
                        FROM 
                            FavoriteSongs fs 
                        JOIN 
                            SongLibrary sl 
                        ON 
                            fs.SongNumber = sl.歌曲編號 
                        WHERE 
                            fs.PhoneNumber = @PhoneNumber";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PhoneNumber", UserPhoneNumber);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())  
                            {
                                string songNumber = reader["歌曲編號"].ToString();
                                string category = reader["語別"].ToString();
                                string song = reader["歌曲名稱"].ToString();
                                int plays = Convert.ToInt32(reader["點播次數"]);
                                string artistA = reader["歌星 A"].ToString();
                                string artistB = reader["歌星 B"].ToString();
                                string artistACategory = reader["歌星A分類"].ToString();
                                string artistBCategory = reader["歌星B分類"].ToString(); 
                                DateTime addedTime;

                                try
                                {
                                    addedTime = DateTime.ParseExact(reader["新增日期"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                }
                                catch (System.FormatException)
                                {
                                    addedTime = DateTime.Now;
                                }

                                string basePathHost1 = reader["路徑 1"].ToString();
                                string basePathHost2 = reader["路徑 2"].ToString();
                                string fileName = reader["歌曲檔名"].ToString();
                                string songFilePathHost1 = Path.Combine(basePathHost1, fileName);
                                string songFilePathHost2 = Path.Combine(basePathHost2, fileName);
                                string phoneticNotation = reader["歌曲注音"].ToString();
                                string pinyinNotation = reader["歌曲拼音"].ToString();
                                string artistAPhonetic = reader["歌星A注音"].ToString();
                                string artistBPhonetic = reader["歌星B注音"].ToString();
                                string artistASimplified = reader["歌星A簡體"].ToString();
                                string artistBSimplified = reader["歌星B簡體"].ToString();
                                string songSimplified = reader["歌名簡體"].ToString();
                                string songGenre = reader["分類"].ToString(); 
                                string artistAPinyin = reader["歌星A拼音"].ToString();
                                string artistBPinyin = reader["歌星B拼音"].ToString();
                                int humanVoice = Convert.ToInt32(reader["人聲"]);  

                                FavoriteSongs.Add(new SongData(
                                    songNumber, category, song, plays, artistA, artistB, 
                                    artistACategory, artistBCategory, addedTime, songFilePathHost1, 
                                    songFilePathHost2, phoneticNotation, pinyinNotation, 
                                    artistAPhonetic, artistBPhonetic, artistASimplified, 
                                    artistBSimplified, songSimplified, songGenre, 
                                    artistAPinyin, artistBPinyin, humanVoice));
                            }
                            PrimaryForm.Instance.multiPagePanel.currentPageIndex = 0;
                            PrimaryForm.Instance.multiPagePanel.LoadSongs(FavoriteSongs);
                        }
                    }

                    connection.Close();  
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load favorite songs from SQLite database: " + ex.Message);
                }
            }
        }

        public void AddToFavorite(string songNumber)
        {
            if (!IsUserLoggedIn || string.IsNullOrEmpty(UserPhoneNumber))
            {
                Console.WriteLine("User is not logged in.");
                return;
            }

            string databaseFileName = "KSongDatabase.db";
            string databasePath = Path.Combine(Application.StartupPath, databaseFileName);
            string connectionString = String.Format("Data Source={0};Version=3;", databasePath);

            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    
                    string checkSql = "SELECT COUNT(*) FROM FavoriteSongs WHERE PhoneNumber = @PhoneNumber AND SongNumber = @SongNumber";
                    using (var checkCommand = new SQLiteCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@PhoneNumber", UserPhoneNumber);
                        checkCommand.Parameters.AddWithValue("@SongNumber", songNumber);
                        long count = (long)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            
                            Console.WriteLine(String.Format("Song {0} is already in favorites.", songNumber));
                            return;
                        }
                    }

                    string sql = "INSERT INTO FavoriteSongs (PhoneNumber, SongNumber) VALUES (@PhoneNumber, @SongNumber)";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PhoneNumber", UserPhoneNumber);
                        command.Parameters.AddWithValue("@SongNumber", songNumber);
                        command.ExecuteNonQuery();
                    }

                    connection.Close();

                    
                    var song = AllSongs.FirstOrDefault(s => s.SongNumber == songNumber);
                    if (song != null)
                    {
                        FavoriteSongs.Add(song);
                        Console.WriteLine(String.Format("Added song {0} to favorites.", songNumber));
                    }
                    else
                    {
                        Console.WriteLine(String.Format("Song {0} not found in AllSongs.", songNumber));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to add song to favorites: " + ex.Message);
                }
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

        
        public SongData SearchSongByNumber(string songNumber)
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

                
                NewSongLists.Add(category, songsInCategory);
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

                
                HotSongLists.Add(category, songsInCategory);
            }
        }

        public List<SongData> GetNewSongsByCategory(string category)
        {
            
            if (NewSongLists.ContainsKey(category))
                return NewSongLists[category];
            else
                return new List<SongData>();  
        }

        public List<SongData> GetHotSongsByCategory(string category)
        {
            
            if (HotSongLists.ContainsKey(category))
                return HotSongLists[category];
            else
                return new List<SongData>();  
        }
        public List<SongData> GetFavoriteSongsByPhoneNumber()
        {
            List<SongData> favoriteSongs = new List<SongData>();

            if (string.IsNullOrEmpty(UserPhoneNumber))
                return favoriteSongs;

            string databaseFileName = "KSongDatabase.db";
            string databasePath = Path.Combine(Application.StartupPath, databaseFileName);
            string connectionString = String.Format("Data Source={0};Version=3;", databasePath);

            using (var connection = new SQLiteConnection(connectionString))
            {
                try
                {
                    connection.Open();  

                    string sql = @"
                        SELECT 
                            sl.歌曲編號, sl.語別, sl.歌曲名稱, sl.點播次數, 
                            sl.[歌星 A], sl.[歌星 B], sl.新增日期, sl.[路徑 1], 
                            sl.[路徑 2], sl.歌曲檔名, sl.歌曲注音, sl.歌曲拼音, 
                            sl.歌星A分類, sl.歌星B分類, sl.歌星A注音, sl.歌星B注音, 
                            sl.歌星A簡體, sl.歌星B簡體, sl.歌名簡體, sl.分類, 
                            sl.歌星A拼音, sl.歌星B拼音, sl.人聲
                        FROM 
                            FavoriteSongs fs 
                        JOIN 
                            SongLibrary sl 
                        ON 
                            fs.SongNumber = sl.歌曲編號 
                        WHERE 
                            fs.PhoneNumber = @PhoneNumber";

                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PhoneNumber", UserPhoneNumber);

                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())  
                            {
                                string songNumber = reader["歌曲編號"].ToString();
                                string category = reader["語別"].ToString();
                                string song = reader["歌曲名稱"].ToString();
                                int plays = Convert.ToInt32(reader["點播次數"]);
                                string artistA = reader["歌星 A"].ToString();
                                string artistB = reader["歌星 B"].ToString();
                                string artistACategory = reader["歌星A分類"].ToString();
                                string artistBCategory = reader["歌星B分類"].ToString();
                                DateTime addedTime;

                                try
                                {
                                    addedTime = DateTime.ParseExact(reader["新增日期"].ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                }
                                catch (System.FormatException)
                                {
                                    addedTime = DateTime.Now;
                                }

                                string basePathHost1 = reader["路徑 1"].ToString();
                                string basePathHost2 = reader["路徑 2"].ToString();
                                string fileName = reader["歌曲檔名"].ToString();
                                string songFilePathHost1 = Path.Combine(basePathHost1, fileName);
                                string songFilePathHost2 = Path.Combine(basePathHost2, fileName);
                                string phoneticNotation = reader["歌曲注音"].ToString();
                                string pinyinNotation = reader["歌曲拼音"].ToString();
                                string artistAPhonetic = reader["歌星A注音"].ToString();
                                string artistBPhonetic = reader["歌星B注音"].ToString();
                                string artistASimplified = reader["歌星A簡體"].ToString();
                                string artistBSimplified = reader["歌星B簡體"].ToString();
                                string songSimplified = reader["歌名簡體"].ToString();
                                string songGenre = reader["分類"].ToString(); 
                                string artistAPinyin = reader["歌星A拼音"].ToString();
                                string artistBPinyin = reader["歌星B拼音"].ToString();
                                int humanVoice = Convert.ToInt32(reader["人聲"]);

                                favoriteSongs.Add(new SongData(
                                    songNumber, category, song, plays, artistA, artistB, 
                                    artistACategory, artistBCategory, addedTime, songFilePathHost1, 
                                    songFilePathHost2, phoneticNotation, pinyinNotation, 
                                    artistAPhonetic, artistBPhonetic, artistASimplified, 
                                    artistBSimplified, songSimplified, songGenre, 
                                    artistAPinyin, artistBPinyin, humanVoice));
                            }
                        }
                    }

                    connection.Close();  
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load favorite songs from SQLite database: " + ex.Message);
                }
            }

            return favoriteSongs;
        }
    }
}