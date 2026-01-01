using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace XZPToolv3
{
    /// <summary>
    /// Helper class to extract Windows system icons for file types
    /// </summary>
    public static class SystemIconHelper
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes,
            ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
            public uint dwAttributes;
        }

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_SMALLICON = 0x000000001;
        private const uint SHGFI_LARGEICON = 0x000000000;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        /// <summary>
        /// Get icon for a file extension
        /// </summary>
        public static Icon GetIconForExtension(string extension, bool smallIcon = true)
        {
            if (string.IsNullOrEmpty(extension))
                return null;

            // Ensure extension starts with a dot
            if (!extension.StartsWith("."))
                extension = "." + extension;

            SHFILEINFO shinfo = new SHFILEINFO();
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
            flags |= smallIcon ? SHGFI_SMALLICON : SHGFI_LARGEICON;

            IntPtr hImg = SHGetFileInfo(extension, FILE_ATTRIBUTE_NORMAL, ref shinfo,
                (uint)Marshal.SizeOf(shinfo), flags);

            if (shinfo.hIcon != IntPtr.Zero)
            {
                Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
                DestroyIcon(shinfo.hIcon);
                return icon;
            }

            return null;
        }

        /// <summary>
        /// Get folder icon
        /// </summary>
        public static Icon GetFolderIcon(bool smallIcon = true)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;
            flags |= smallIcon ? SHGFI_SMALLICON : SHGFI_LARGEICON;

            IntPtr hImg = SHGetFileInfo("folder", FILE_ATTRIBUTE_DIRECTORY, ref shinfo,
                (uint)Marshal.SizeOf(shinfo), flags);

            if (shinfo.hIcon != IntPtr.Zero)
            {
                Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
                DestroyIcon(shinfo.hIcon);
                return icon;
            }

            return null;
        }

        /// <summary>
        /// Populate ImageList with common file type icons
        /// </summary>
        public static void PopulateImageList(ImageList imageList)
        {
            imageList.Images.Clear();
            imageList.ColorDepth = ColorDepth.Depth32Bit;

            // Index 0: Folder
            Icon folderIcon = GetFolderIcon(true);
            if (folderIcon != null)
                imageList.Images.Add("folder", folderIcon);

            // Common file extensions
            string[] extensions = new string[]
            {
                ".txt", ".xml", ".ini", ".cfg", ".log",  // Text files
                ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".ico", // Images
                ".exe", ".dll", ".sys", ".xex",          // Executables
                ".zip", ".rar", ".7z", ".xzp",           // Archives
                ".mp3", ".wav", ".ogg",                  // Audio
                ".mp4", ".avi", ".mkv",                  // Video
                ".pdf", ".doc", ".docx",                 // Documents
                ".xui", ".lua", ".json", ".xur"                 // Special
            };

            foreach (string ext in extensions)
            {
                Icon icon = GetIconForExtension(ext, true);
                if (icon != null)
                {
                    string key = ext.TrimStart('.');
                    imageList.Images.Add(key, icon);
                }
            }
        }

        /// <summary>
        /// Get image key for file extension
        /// </summary>
        public static string GetImageKey(string fileName, bool isDirectory = false)
        {
            if (isDirectory)
                return "folder";

            string ext = Path.GetExtension(fileName).ToLower();
            if (string.IsNullOrEmpty(ext))
                return "txt"; // Default

            string key = ext.TrimStart('.');
            return key;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}
