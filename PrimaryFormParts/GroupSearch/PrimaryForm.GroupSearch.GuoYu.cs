using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void GroupGuoYuButton_Click(object sender, EventArgs e)
        {
            groupGuoYuButton.BackgroundImage = groupGuoYuActiveBackground;
            groupTaiYuButton.BackgroundImage = groupTaiYuNormalBackground;
            groupYueYuButton.BackgroundImage = groupYueYuNormalBackground;
            groupYingWenButton.BackgroundImage = groupYingWenNormalBackground;
            groupRiYuButton.BackgroundImage = groupRiYuNormalBackground;
            groupHanYuButton.BackgroundImage = groupHanYuNormalBackground;

            guoYuSongs = allSongs.Where(song => song.Category == "國語" && (song.ArtistACategory == "團" || song.ArtistBCategory == "團"))
                                .OrderByDescending(song => song.Plays) 
                                .ToList();
            currentPage = 0; 
            currentSongList = guoYuSongs; 
            totalPages = (int)Math.Ceiling((double)guoYuSongs.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}