using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO; // 添加這行

namespace KTV_Superstar;

public partial class PrimaryForm
{
    private Button orderedSongsButton = default!;
    private Bitmap orderedSongsNormalBackground = default!; 
    private Bitmap orderedSongsActiveBackground = default!;
}