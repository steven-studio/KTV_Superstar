using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void GroupYueYuButton_Click(object sender, EventArgs e)
        {
            groupGuoYuButton.BackgroundImage = groupGuoYuNormalBackground;
            groupTaiYuButton.BackgroundImage = groupTaiYuNormalBackground;
            groupYueYuButton.BackgroundImage = groupYueYuActiveBackground;
            groupYingWenButton.BackgroundImage = groupYingWenNormalBackground;
            groupRiYuButton.BackgroundImage = groupRiYuNormalBackground;
            groupHanYuButton.BackgroundImage = groupHanYuNormalBackground;

            yueYuSongs = allSongs.Where(song => song.Category == "粵語" && (song.ArtistACategory == "團" || song.ArtistBCategory == "團"))
                                .OrderByDescending(song => song.Plays) 
                                .ToList();
            currentPage = 0; 
            currentSongList = yueYuSongs; 
            totalPages = (int)Math.Ceiling((double)yueYuSongs.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}