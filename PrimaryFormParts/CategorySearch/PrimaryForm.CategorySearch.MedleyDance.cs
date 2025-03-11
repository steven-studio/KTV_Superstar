using System;
using System.Linq;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void MedleyDanceButton_Click(object sender, EventArgs e)
        {
            loveDuetButton.BackgroundImage = loveDuetNormalBackground;
            talentShowButton.BackgroundImage = talentShowNormalBackground;
            medleyDanceButton.BackgroundImage = medleyDanceActiveBackground;
            ninetiesButton.BackgroundImage = ninetiesNormalBackground;
            nostalgicSongsButton.BackgroundImage = nostalgicSongsNormalBackground;
            chinaSongsButton.BackgroundImage = chinaNormalBackground;
            

            medleyDanceSongs = allSongs.Where(song => song.SongGenre.Contains("C1"))
                                .OrderByDescending(song => song.Plays) 
                                .ToList();
            currentPage = 0; 
            currentSongList = medleyDanceSongs; 
            totalPages = (int)Math.Ceiling((double)medleyDanceSongs.Count / itemsPerPage);

            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}