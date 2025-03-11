using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void HanYuButtonNewSong_Click(object sender, EventArgs e)
        {
            guoYuButtonNewSong.BackgroundImage = guoYuNewSongNormalBackground;
            taiYuButtonNewSong.BackgroundImage = taiYuNewSongNormalBackground;
            yueYuButtonNewSong.BackgroundImage = yueYuNewSongNormalBackground;
            yingWenButtonNewSong.BackgroundImage = yingWenNewSongNormalBackground;
            riYuButtonNewSong.BackgroundImage = riYuNewSongNormalBackground;
            hanYuButtonNewSong.BackgroundImage = hanYuNewSongActiveBackground;

            int songLimit = ReadNewSongLimit(); 

            hanYuSongs2 = allSongs.Where(song => song.Category == "韓語")
                                .OrderByDescending(song => song.AddedTime) 
                                .Take(songLimit) 
                                .ToList();
            currentPage = 0; 
            currentSongList = hanYuSongs2; 
            totalPages = (int)Math.Ceiling((double)hanYuSongs2.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}