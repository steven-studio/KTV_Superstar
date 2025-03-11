using System;
using System.Linq;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void NostalgicSongsButton_Click(object sender, EventArgs e)
        {
            loveDuetButton.BackgroundImage = loveDuetNormalBackground;
            talentShowButton.BackgroundImage = talentShowNormalBackground;
            medleyDanceButton.BackgroundImage = medleyDanceNormalBackground;
            ninetiesButton.BackgroundImage = ninetiesNormalBackground;
            nostalgicSongsButton.BackgroundImage = nostalgicSongsActiveBackground;
            chinaSongsButton.BackgroundImage = chinaNormalBackground;
            

            nostalgicSongs = allSongs.Where(song => song.SongGenre.Contains("E1"))
                                .OrderByDescending(song => song.Plays) 
                                .ToList();
            currentPage = 0; 
            currentSongList = nostalgicSongs; 
            totalPages = (int)Math.Ceiling((double)nostalgicSongs.Count / itemsPerPage);

            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}