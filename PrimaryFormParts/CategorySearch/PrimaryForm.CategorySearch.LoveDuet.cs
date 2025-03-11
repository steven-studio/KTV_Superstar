using System;
using System.Linq;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void LoveDuetButton_Click(object sender, EventArgs e)
        {
            loveDuetButton.BackgroundImage = loveDuetActiveBackground;
            talentShowButton.BackgroundImage = talentShowNormalBackground;
            medleyDanceButton.BackgroundImage = medleyDanceNormalBackground;
            ninetiesButton.BackgroundImage = ninetiesNormalBackground;
            nostalgicSongsButton.BackgroundImage = nostalgicSongsNormalBackground;
            chinaSongsButton.BackgroundImage = chinaNormalBackground;
            

            loveDuetSongs = allSongs.Where(song => song.SongGenre.Contains("A1"))
                                .OrderByDescending(song => song.Plays) 
                                .ToList();
            currentPage = 0; 
            currentSongList = loveDuetSongs; 
            totalPages = (int)Math.Ceiling((double)loveDuetSongs.Count / itemsPerPage);

            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}