using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void RiYuButtonNewSong_Click(object sender, EventArgs e)
        {
            guoYuButtonNewSong.BackgroundImage = guoYuNewSongNormalBackground;
            taiYuButtonNewSong.BackgroundImage = taiYuNewSongNormalBackground;
            yueYuButtonNewSong.BackgroundImage = yueYuNewSongNormalBackground;
            yingWenButtonNewSong.BackgroundImage = yingWenNewSongNormalBackground;
            riYuButtonNewSong.BackgroundImage = riYuNewSongActiveBackground;
            hanYuButtonNewSong.BackgroundImage = hanYuNewSongNormalBackground;

            int songLimit = ReadNewSongLimit(); 

            riYuSongs2 = allSongs.Where(song => song.Category == "日語")
                                .OrderByDescending(song => song.AddedTime) 
                                .Take(songLimit) 
                                .ToList();
            currentPage = 0; 
            currentSongList = riYuSongs2; 
            totalPages = (int)Math.Ceiling((double)riYuSongs2.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}