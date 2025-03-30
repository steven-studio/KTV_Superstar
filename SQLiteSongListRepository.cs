using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;

namespace KTV_Superstar;

public class SQLiteSongListRepository : ISongListRepository
{
    private readonly string _databasePath;
    public SQLiteSongListRepository(string databasePath)
    {
        _databasePath = databasePath;
    }

    public void InitializeDatabase()
    {
        // 這裡是初始化資料庫的邏輯
    }

    public List<SongData> LoadAllSongs()
    {
        // 使用 _databasePath 產生連線字串
        string connectionString = String.Format("Data Source={0};Version=3;", _databasePath);
        List<SongData> allSongs = new List<SongData>();
        
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
                            string songNumber = reader["歌曲編號"]?.ToString() ?? string.Empty;
                            string category = reader["語別"].ToString() ?? string.Empty;
                            string song = reader["歌曲名稱"].ToString() ?? string.Empty;
                            int plays = Convert.ToInt32(reader["點播次數"]);
                            string artistA = reader["歌星 A"].ToString() ?? string.Empty;
                            string artistB = reader["歌星 B"].ToString() ?? string.Empty;
                            string artistACategory = reader["歌星A分類"].ToString() ?? string.Empty; 
                            string artistBCategory = reader["歌星B分類"].ToString() ?? string.Empty; 
                            string dateValue = reader["新增日期"].ToString() ?? string.Empty;
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
                                catch (System.FormatException)
                                {
                                    // Console.WriteLine(String.Format("Invalid date format for song: {0}. Error: {1}", song, ex.Message));
                                    addedTime = DateTime.Now; 
                                }
                            }
                            string basePathHost1 = reader["路徑 1"].ToString() ?? string.Empty;
                            string basePathHost2 = reader["路徑 2"].ToString() ?? string.Empty;
                            string fileName = reader["歌曲檔名"].ToString() ?? string.Empty;
                            string songFilePathHost1 = Path.Combine(basePathHost1, fileName);
                            string songFilePathHost2 = Path.Combine(basePathHost2, fileName);
                            string phoneticNotation = reader["歌曲注音"].ToString() ?? string.Empty;
                            string pinyinNotation = reader["歌曲拼音"].ToString() ?? string.Empty;
                            string artistAPhonetic = reader["歌星A注音"].ToString() ?? string.Empty;
                            string artistBPhonetic = reader["歌星B注音"].ToString() ?? string.Empty;
                            string artistASimplified = reader["歌星A簡體"].ToString() ?? string.Empty;
                            string artistBSimplified = reader["歌星B簡體"].ToString() ?? string.Empty;
                            string songSimplified = reader["歌名簡體"].ToString() ?? string.Empty;
                            string songGenre = reader["分類"].ToString() ?? string.Empty;  
                            string artistAPinyin = reader["歌星A拼音"].ToString() ?? string.Empty;
                            string artistBPinyin = reader["歌星B拼音"].ToString() ?? string.Empty;
                            int humanVoice = Convert.ToInt32(reader["人聲"]);  

                            allSongs.Add(new SongData(songNumber, category, song, plays, artistA, artistB, artistACategory, artistBCategory, addedTime, songFilePathHost1, songFilePathHost2, phoneticNotation, pinyinNotation, artistAPhonetic, artistBPhonetic, artistASimplified, artistBSimplified, songSimplified, songGenre, artistAPinyin, artistBPinyin, humanVoice));
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

        // 實作從 SQLite 資料庫讀取所有歌曲的邏輯
        return allSongs;
    }

    public void AddFavorite(string phoneNumber, string songNumber)
    {
        // 實作新增 Favorite 的邏輯
    }

    public List<SongData> LoadFavoriteSongs(string phoneNumber)
    {
        List<SongData> favoriteSongs = new List<SongData>();

        string connectionString = string.Format("Data Source={0};Version=3;", _databasePath);

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
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // 根據你的邏輯讀取資料
                            string songNumber = reader["歌曲編號"].ToString() ?? string.Empty;
                            string category = reader["語別"].ToString() ?? string.Empty;
                            string song = reader["歌曲名稱"].ToString() ?? string.Empty;
                            int plays = Convert.ToInt32(reader["點播次數"]);
                            string artistA = reader["歌星 A"].ToString() ?? string.Empty;
                            string artistB = reader["歌星 B"].ToString() ?? string.Empty;
                            // 其他欄位……
                            string artistACategory = reader["歌星A分類"].ToString() ?? string.Empty;
                            string artistBCategory = reader["歌星B分類"].ToString() ?? string.Empty; 

                            DateTime addedTime;
                            string dateValue = reader["新增日期"].ToString() ?? string.Empty;
                            if (string.IsNullOrWhiteSpace(dateValue))
                            {
                                addedTime = DateTime.Now;
                            }
                            else
                            {
                                try
                                {
                                    addedTime = DateTime.ParseExact(dateValue, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                }
                                catch (System.FormatException)
                                {
                                    addedTime = DateTime.Now;
                                }
                            }
                            // …讀取剩餘欄位
                            string basePathHost1 = reader["路徑 1"].ToString() ?? string.Empty;
                            string basePathHost2 = reader["路徑 2"].ToString() ?? string.Empty;
                            string fileName = reader["歌曲檔名"].ToString() ?? string.Empty;
                            string songFilePathHost1 = Path.Combine(basePathHost1, fileName);
                            string songFilePathHost2 = Path.Combine(basePathHost2, fileName);
                            string phoneticNotation = reader["歌曲注音"].ToString() ?? string.Empty;
                            string pinyinNotation = reader["歌曲拼音"].ToString() ?? string.Empty;
                            string artistAPhonetic = reader["歌星A注音"].ToString() ?? string.Empty;
                            string artistBPhonetic = reader["歌星B注音"].ToString() ?? string.Empty;
                            string artistASimplified = reader["歌星A簡體"].ToString() ?? string.Empty;
                            string artistBSimplified = reader["歌星B簡體"].ToString() ?? string.Empty;
                            string songSimplified = reader["歌名簡體"].ToString() ?? string.Empty;
                            string songGenre = reader["分類"].ToString() ?? string.Empty; 
                            string artistAPinyin = reader["歌星A拼音"].ToString() ?? string.Empty;
                            string artistBPinyin = reader["歌星B拼音"].ToString() ?? string.Empty;
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

    public bool CheckIfPhoneNumberExists(string phoneNumber)
    {
        string connectionString = string.Format("Data Source={0};Version=3;", _databasePath);

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

    public void AddNewUser(string phoneNumber)
    {
        string connectionString = string.Format("Data Source={0};Version=3;", _databasePath);

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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to add new user to SQLite database: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
