using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void TaiYuButtonHotSong_Click(object sender, EventArgs e)
        {
            OnHotSongButtonClick(taiYuButtonHotSong, taiYuHotSongActiveBackground, "台語");
        }
    }
} 