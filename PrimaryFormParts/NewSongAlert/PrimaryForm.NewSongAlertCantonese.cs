using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void YueYuButtonNewSong_Click(object sender, EventArgs e)
        {
            guoYuButtonNewSong.BackgroundImage = guoYuNewSongNormalBackground;
            taiYuButtonNewSong.BackgroundImage = taiYuNewSongNormalBackground;
            yueYuButtonNewSong.BackgroundImage = yueYuNewSongActiveBackground;
            yingWenButtonNewSong.BackgroundImage = yingWenNewSongNormalBackground;
            riYuButtonNewSong.BackgroundImage = riYuNewSongNormalBackground;
            hanYuButtonNewSong.BackgroundImage = hanYuNewSongNormalBackground;

            int songLimit = ReadNewSongLimit(); 

            yueYuSongs2 = allSongs.Where(song => song.Category == "粵語")
                                .OrderByDescending(song => song.AddedTime) 
                                .Take(songLimit) 
                                .ToList();
            currentPage = 0; 
            currentSongList = yueYuSongs2; 
            totalPages = (int)Math.Ceiling((double)yueYuSongs2.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}