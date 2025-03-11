using System;
using System.Runtime.InteropServices;

namespace DualScreenDemo
{
    public class ComInterop
    {
        [DllImport("ole32.dll")]
        public static extern int CoInitializeEx(IntPtr pvReserved, int dwCoInit);

        public const int COINIT_APARTMENTTHREADED = 0x2; 
        public const int COINIT_MULTITHREADED = 0x0;    
    }
}