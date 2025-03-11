using System;
using System.Linq; 
using System.Collections.Generic; 

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {        
        private void TalentShowButton_Click(object sender, EventArgs e)
        {
            loveDuetButton.BackgroundImage = loveDuetNormalBackground;
            talentShowButton.BackgroundImage = talentShowActiveBackground;
            medleyDanceButton.BackgroundImage = medleyDanceNormalBackground;
            ninetiesButton.BackgroundImage = ninetiesNormalBackground;
            nostalgicSongsButton.BackgroundImage = nostalgicSongsNormalBackground;
            chinaSongsButton.BackgroundImage = chinaNormalBackground;
            

            talentShowSongs = allSongs.Where(song => song.SongGenre.Contains("B1"))
                                .OrderByDescending(song => song.Plays) 
                                .ToList();
            currentPage = 0; 
            currentSongList = talentShowSongs; 
            totalPages = (int)Math.Ceiling((double)talentShowSongs.Count / itemsPerPage);

            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
}