using System.Collections.Generic;

namespace XZPToolv3.XUI
{
    public sealed class Log
    {
        private static readonly Log instance = new Log();

        public static Log Instance
        {
            get { return instance; }
        }

        public List<string> missingClasses;
        public List<uint> flagErrors;
        public List<string> failedScenes;
        public List<string> completedScenes;
        public List<string> sharedresImages;
        public List<string> xamImages;
        public List<string> skinImages;
        public List<string> hudImages;
        public List<string> customImages;

        private Log()
        {
            missingClasses = new List<string>();
            flagErrors = new List<uint>();
            failedScenes = new List<string>();
            completedScenes = new List<string>();
            sharedresImages = new List<string>();
            xamImages = new List<string>();
            skinImages = new List<string>();
            hudImages = new List<string>();
            customImages = new List<string>();
        }
    }
}
