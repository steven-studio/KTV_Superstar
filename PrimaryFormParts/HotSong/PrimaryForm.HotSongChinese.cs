using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace KTV_Superstar;

public partial class PrimaryForm
{
    private void GuoYuButtonHotSong_Click(object sender, EventArgs e)
    {
        OnHotSongButtonClick(guoYuButtonHotSong, guoYuHotSongActiveBackground, "國語");
    }
}