using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void GroupYingWenButton_Click(object sender, EventArgs e)
        {
            groupGuoYuButton.BackgroundImage = groupGuoYuNormalBackground;
            groupTaiYuButton.BackgroundImage = groupTaiYuNormalBackground;
            groupYueYuButton.BackgroundImage = groupYueYuNormalBackground;
            groupYingWenButton.BackgroundImage = groupYingWenActiveBackground;
            groupRiYuButton.BackgroundImage = groupRiYuNormalBackground;
            groupHanYuButton.BackgroundImage = groupHanYuNormalBackground;

            yingWenSongs = allSongs.Where(song => song.Category == "英文" && (song.ArtistACategory == "團" || song.ArtistBCategory == "團"))
                                .OrderByDescending(song => song.Plays) 
                                .ToList();
            currentPage = 0; 
            currentSongList = yingWenSongs; 
            totalPages = (int)Math.Ceiling((double)yingWenSongs.Count / itemsPerPage);

            
            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}