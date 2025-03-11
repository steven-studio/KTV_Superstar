using System;
using DirectShowLib;

namespace DualScreenDemo
{
    public class SampleGrabberCallback : ISampleGrabberCB
    {
        private VideoPlayerForm form;

        public SampleGrabberCallback(VideoPlayerForm form)
        {
            this.form = form;
        }

        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            
            return 0;
        }

        public int SampleCB(double SampleTime, IMediaSample pSample)
        {
            
            return 0;
        }
    }
}