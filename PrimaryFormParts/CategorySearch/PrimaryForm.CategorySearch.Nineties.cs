using System;
using System.Linq;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void NinetiesButton_Click(object sender, EventArgs e)
        {
            loveDuetButton.BackgroundImage = loveDuetNormalBackground;
            talentShowButton.BackgroundImage = talentShowNormalBackground;
            medleyDanceButton.BackgroundImage = medleyDanceNormalBackground;
            ninetiesButton.BackgroundImage = ninetiesActiveBackground;
            nostalgicSongsButton.BackgroundImage = nostalgicSongsNormalBackground;
            chinaSongsButton.BackgroundImage = chinaNormalBackground;
            

            ninetiesSongs = allSongs.Where(song => song.SongGenre.Contains("D1"))
                                .OrderByDescending(song => song.Plays) 
                                .ToList();
            currentPage = 0; 
            currentSongList = ninetiesSongs; 
            totalPages = (int)Math.Ceiling((double)ninetiesSongs.Count / itemsPerPage);

            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}