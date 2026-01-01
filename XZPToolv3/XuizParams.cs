using System;

namespace XZPToolv3
{
    /// <summary>
    /// Parameter container for XZP operations
    /// </summary>
    public class XuizParams
    {
        public string FileName { get; set; }
        public string BuildDirectory { get; set; }
        public string BuildFilePath { get; set; }
        public int BuildVersion { get; set; }
        public string ExtractPath { get; set; }

        public XuizParams()
        {
            BuildVersion = 3; // Default to version 3
        }
    }
}
