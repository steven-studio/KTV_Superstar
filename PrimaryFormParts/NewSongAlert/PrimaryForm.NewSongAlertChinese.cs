using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void GuoYuButtonNewSong_Click(object sender, EventArgs e)
        {
            guoYuButtonNewSong.BackgroundImage = guoYuNewSongActiveBackground;
            taiYuButtonNewSong.BackgroundImage = taiYuNewSongNormalBackground;
            yueYuButtonNewSong.BackgroundImage = yueYuNewSongNormalBackground;
            yingWenButtonNewSong.BackgroundImage = yingWenNewSongNormalBackground;
            riYuButtonNewSong.BackgroundImage = riYuNewSongNormalBackground;
            hanYuButtonNewSong.BackgroundImage = hanYuNewSongNormalBackground;

            int songLimit = ReadNewSongLimit(); 

            guoYuSongs2 = allSongs.Where(song => song.Category == "國語")
                                .OrderByDescending(song => song.AddedTime) 
                                .Take(songLimit) 
                                .ToList();
            currentPage = 0; 
            currentSongList = guoYuSongs2; 
            totalPages = (int)Math.Ceiling((double)guoYuSongs2.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}