namespace KTV_Superstar;

public interface ISongListRepository
{
    void InitializeDatabase();
    List<SongData> LoadAllSongs();
    void AddFavorite(string phoneNumber, string songNumber);
    List<SongData> LoadFavoriteSongs(string phoneNumber);
    // 根據需要擴充其他方法，例如更新、刪除等
    bool CheckIfPhoneNumberExists(string phoneNumber);
    void AddNewUser(string phoneNumber);
}
