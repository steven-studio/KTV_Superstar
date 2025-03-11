using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void TaiYuNewSongButtonHotSong_Click(object sender, EventArgs e)
        {
            // 重置其他按钮背景
            UpdateHotSongButtons(taiYuNewSongButtonHotSong, taiYuNewSongHotSongActiveBackground);

            int songLimit = ReadHotSongLimit(); 

            // 使用 AddedTime 排序
            var selectedSongs = allSongs.Where(song => song.Category == "台語")
                                    .OrderByDescending(song => song.AddedTime) 
                                    .Take(songLimit) 
                                    .ToList();
            
            currentPage = 0; 
            currentSongList = selectedSongs; 
            totalPages = (int)Math.Ceiling((double)selectedSongs.Count / itemsPerPage);

            multiPagePanel.currentPageIndex = 0;
            multiPagePanel.LoadSongs(currentSongList);
        }
    }
} 

