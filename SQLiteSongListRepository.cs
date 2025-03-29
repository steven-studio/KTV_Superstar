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
        List<SongData> allSongs = new List<SongData>();
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
        // 實作從資料庫讀取 Favorite 歌曲的邏輯
        return favoriteSongs;
    }
}
