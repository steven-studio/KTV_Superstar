using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void GroupTaiYuButton_Click(object sender, EventArgs e)
        {
            groupGuoYuButton.BackgroundImage = groupGuoYuNormalBackground;
            groupTaiYuButton.BackgroundImage = groupTaiYuActiveBackground;
            groupYueYuButton.BackgroundImage = groupYueYuNormalBackground;
            groupYingWenButton.BackgroundImage = groupYingWenNormalBackground;
            groupRiYuButton.BackgroundImage = groupRiYuNormalBackground;
            groupHanYuButton.BackgroundImage = groupHanYuNormalBackground;

            taiYuSongs = allSongs.Where(song => song.Category == "台語" && (song.ArtistACategory == "團" || song.ArtistBCategory == "團"))
                                .OrderByDescending(song => song.Plays) 
                                .ToList();
            currentPage = 0; 
            currentSongList = taiYuSongs; 
            totalPages = (int)Math.Ceiling((double)taiYuSongs.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}