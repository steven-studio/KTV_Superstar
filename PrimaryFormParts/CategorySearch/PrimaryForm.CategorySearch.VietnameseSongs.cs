using System;
using System.Linq;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void VietnameseSongsButton_Click(object sender, EventArgs e)
        {
            loveDuetButton.BackgroundImage = loveDuetNormalBackground;
            talentShowButton.BackgroundImage = talentShowNormalBackground;
            medleyDanceButton.BackgroundImage = medleyDanceNormalBackground;
            ninetiesButton.BackgroundImage = ninetiesNormalBackground;
            nostalgicSongsButton.BackgroundImage = nostalgicSongsNormalBackground;
            chinaSongsButton.BackgroundImage = chinaNormalBackground;
            

            vietnameseSongs = allSongs.Where(song => song.SongGenre.Contains("G1"))
                                .OrderByDescending(song => song.Plays) 
                                .ToList();
            currentPage = 0; 
            currentSongList = vietnameseSongs; 
            totalPages = (int)Math.Ceiling((double)vietnameseSongs.Count / itemsPerPage);

            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}