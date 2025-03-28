using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq; 

namespace KTV_Superstar;

public partial class PrimaryForm
{
    private void RiYuButtonHotSong_Click(object sender, EventArgs e)
    {
        OnHotSongButtonClick(riYuButtonHotSong, riYuHotSongActiveBackground, "日語");
    }
}