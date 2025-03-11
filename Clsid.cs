using System;

namespace DualScreenDemo
{    
    public static class Clsid
    {
        public static readonly Guid LAVSplitter = new Guid("171252A0-8820-4AFE-9DF8-5C92B2D66B04");
        public static readonly Guid LAVVideoDecoder = new Guid("EE30215D-164F-4A92-A4EB-9D4C13390F9F");
        public static readonly Guid LAVAudioDecoder = new Guid("E8E73B6B-4CB3-44A4-BE99-4F7BCB96E491");
        public static readonly Guid VideoRenderer = new Guid("B87BEB7B-8D29-423F-AE4D-6582C10175AC");
        public static readonly Guid AudioSwitcher = new Guid("1367C6F9-3FDD-42E9-AE3A-BC57F4C40D6D"); 
    }
}