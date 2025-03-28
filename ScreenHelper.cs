using System;
using System.Linq; 
using System.Windows.Forms; 

namespace KTV_Superstar;

public static class ScreenHelper
{
    public static Screen GetSecondMonitor()
    {
        return Screen.AllScreens.FirstOrDefault(s => !s.Primary) ?? Screen.PrimaryScreen!;
    }
}