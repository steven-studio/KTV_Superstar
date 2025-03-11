using System;
using System.Collections.Generic;
using System.Data.SQLite; 
using System.IO; 
using System.Linq; 
using System.Windows.Forms; 

namespace DualScreenDemo
{
    public class ArtistManager
    {
        private static ArtistManager _instance;
        public List<Artist> AllArtists { get; private set; }
        
        public static ArtistManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ArtistManager();
                }
                return _instance;
            }
        }

        public ArtistManager()
        {
            AllArtists = new List<Artist>();
            LoadArtists();

        }

        private void LoadArtists()
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

                    string sql = "SELECT 歌手姓名, 歌手注音, 歌手分類, 歌手筆畫 FROM ArtistLibrary";  
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        using (SQLiteDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())  
                            {
                                string artist = reader["歌手姓名"].ToString();
                                string phonetic = reader["歌手注音"].ToString();
                                string category = reader["歌手分類"].ToString();
                                string strokesStr = reader["歌手筆畫"].ToString();

                                
                                if (string.IsNullOrEmpty(strokesStr))
                                {
                                    // Console.WriteLine("歌手筆畫的值為空或無效");
                                }
                                
                                if (double.TryParse(strokesStr, out double strokesDouble))
                                {
                                    int strokes = (int)Math.Round(strokesDouble); 
                                    AllArtists.Add(new Artist(artist, phonetic, category, strokes));
                                    
                                }
                                else
                                {
                                    // Console.WriteLine($"Failed to parse '歌手筆畫' value: {strokesStr}");
                                }
                            }
                        }
                    }

                    connection.Close();  

                    
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to load artists from SQLite database: " + ex.Message);
                }
            }
        }

        private void PrintAllArtists()
        {
            Console.WriteLine("All Artists:");
            foreach (var artist in AllArtists)
            {
                Console.WriteLine(artist.ToString());
            }
        }

        public List<Artist> GetArtistsByCategoryAndStrokeCountRange(string category, int minStrokes, int maxStrokes)
        {
            if (category == "全部")
            {
                return AllArtists.Where(artist => artist.Strokes >= minStrokes && artist.Strokes <= maxStrokes).ToList();
            }
            else
            {
                return AllArtists.Where(artist => artist.Category == category && artist.Strokes >= minStrokes && artist.Strokes <= maxStrokes).ToList();
            }
        }
    }
}