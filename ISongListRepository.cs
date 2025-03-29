namespace KTV_Superstar;

public interface ISongListRepository
{
    List<SongData> LoadAllSongs();
    void InitializeDatabase();
    void AddFavorite(string phoneNumber, string songNumber);
    List<SongData> LoadFavoriteSongs(string phoneNumber);
    // 根據需要擴充其他方法，例如更新、刪除等
}
