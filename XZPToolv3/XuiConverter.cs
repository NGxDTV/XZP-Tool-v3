using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XZPToolv3.XUI;

namespace XZPToolv3
{
    /// <summary>
    /// XUI/XUR converter using internal conversion logic
    /// </summary>
    internal class XuiConverter
    {
        private static PROCESS_FLAGS BuildDefaultProcessFlags()
        {
            return new PROCESS_FLAGS
            {
                useXam = false,
                xuiToolVersion = false,
                useAnimations = true,
                extFile = false
            };
        }

        private static List<string> GetExtensionFiles()
        {
            List<string> files = new List<string>();
            string extensionsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XUI", "Extensions");
            string[] defaultFiles = { "XuiElements.xml", "XamElements.xml", "CustomXuiElements.xml", "ControlPackLegacyControl.xml" };

            foreach (string name in defaultFiles)
            {
                string path = Path.Combine(extensionsDir, name);
                if (File.Exists(path))
                    files.Add(path);
            }

            foreach (string extra in AppSettings.Instance.XuiExtensionFiles)
            {
                if (string.IsNullOrWhiteSpace(extra))
                    continue;
                if (File.Exists(extra) && !files.Contains(extra))
                    files.Add(extra);
            }

            return files;
        }

        private static void EnsureXuiClassesLoaded()
        {
            List<string> extensions = GetExtensionFiles();
            if (extensions.Count == 0)
                throw new FileNotFoundException("No XUI extension XML files found. Add them in Settings > XUI Extensions.");

            XuiClass.Instance.BuildClass(extensions);
        }

        /// <summary>
        /// Load XUR file and return object data
        /// </summary>
        public static XUIOBJECTDATA LoadXurFile(string xurPath)
        {
            if (!File.Exists(xurPath))
                throw new FileNotFoundException("XUR file not found", xurPath);

            try
            {
                EnsureXuiClassesLoaded();

                using (FileStream fs = new FileStream(xurPath, FileMode.Open, FileAccess.Read))
                {
                    XurReader reader = new XurReader(fs);
                    XUIOBJECTDATA rootObject = reader.LoadObjectsFromBinary();
                    return rootObject;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load XUR file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load XUI file and return XML document
        /// </summary>
        public static XmlDocument LoadXuiFile(string xuiPath)
        {
            if (!File.Exists(xuiPath))
                throw new FileNotFoundException("XUI file not found", xuiPath);

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xuiPath);
                return xmlDoc;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load XUI file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Convert XUI to XUR
        /// </summary>
        public static bool ConvertXuiToXur(string xuiPath, string xurPath)
        {
            if (!File.Exists(xuiPath))
                throw new FileNotFoundException("XUI file not found", xuiPath);

            try
            {
                EnsureXuiClassesLoaded();

                PROCESS_FLAGS flags = BuildDefaultProcessFlags();
                XuiReader reader = new XuiReader();
                XUIOBJECTDATA objectData = reader.ReadXui(xuiPath, flags);

                if (objectData == null)
                    return false;

                Global.RemoveDesignTimeElements(objectData);

                string dir = Path.GetDirectoryName(xurPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                using (FileStream fs = new FileStream(xurPath, FileMode.Create, FileAccess.Write))
                {
                    XurWriter writer = new XurWriter(fs);
                    writer.WriteObjectsToBinary(objectData);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to convert XUI to XUR: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Convert XUR to XUI
        /// </summary>
        public static bool ConvertXurToXui(string xurPath, string xuiPath)
        {
            if (!File.Exists(xurPath))
                throw new FileNotFoundException("XUR file not found", xurPath);

            try
            {
                EnsureXuiClassesLoaded();

                PROCESS_FLAGS flags = BuildDefaultProcessFlags();
                XUIOBJECTDATA objectData = LoadXurFile(xurPath);

                if (objectData == null)
                    return false;

                string dir = Path.GetDirectoryName(xuiPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                XuiWriter writer = new XuiWriter();
                writer.BuildXui(xuiPath, objectData, flags);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to convert XUR to XUI: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Check if converter is available
        /// </summary>
        public static bool IsAvailable()
        {
            // Internal converter is always available
            return true;
        }
    }

    public enum XuiImportMode
    {
        ImportOriginal,
        ConvertOnly,
        Both
    }
}
