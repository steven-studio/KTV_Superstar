using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void YingWenButtonNewSong_Click(object sender, EventArgs e)
        {
            guoYuButtonNewSong.BackgroundImage = guoYuNewSongNormalBackground;
            taiYuButtonNewSong.BackgroundImage = taiYuNewSongNormalBackground;
            yueYuButtonNewSong.BackgroundImage = yueYuNewSongNormalBackground;
            yingWenButtonNewSong.BackgroundImage = yingWenNewSongActiveBackground;
            riYuButtonNewSong.BackgroundImage = riYuNewSongNormalBackground;
            hanYuButtonNewSong.BackgroundImage = hanYuNewSongNormalBackground;

            int songLimit = ReadNewSongLimit(); 

            yingWenSongs2 = allSongs.Where(song => song.Category == "英文")
                                .OrderByDescending(song => song.AddedTime) 
                                .Take(songLimit) 
                                .ToList();
            currentPage = 0; 
            currentSongList = yingWenSongs2; 
            totalPages = (int)Math.Ceiling((double)yingWenSongs2.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}