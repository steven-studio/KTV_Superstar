using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace DualScreenDemo
{
    public partial class PrimaryForm
    {
        private void YueYuButtonHotSong_Click(object sender, EventArgs e)
        {
            OnHotSongButtonClick(yueYuButtonHotSong, yueYuHotSongActiveBackground, "粵語");
        }
    }
} 