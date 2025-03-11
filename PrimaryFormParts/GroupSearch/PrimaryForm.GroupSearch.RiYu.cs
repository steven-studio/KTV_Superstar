using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void GroupRiYuButton_Click(object sender, EventArgs e)
        {
            groupGuoYuButton.BackgroundImage = groupGuoYuNormalBackground;
            groupTaiYuButton.BackgroundImage = groupTaiYuNormalBackground;
            groupYueYuButton.BackgroundImage = groupYueYuNormalBackground;
            groupYingWenButton.BackgroundImage = groupYingWenNormalBackground;
            groupRiYuButton.BackgroundImage = groupRiYuActiveBackground;
            groupHanYuButton.BackgroundImage = groupHanYuNormalBackground;

            riYuSongs = allSongs.Where(song => song.Category == "日語" && (song.ArtistACategory == "團" || song.ArtistBCategory == "團"))
                                .OrderByDescending(song => song.Plays) 
                                .ToList();
            currentPage = 0; 
            currentSongList = riYuSongs; 
            totalPages = (int)Math.Ceiling((double)riYuSongs.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}