using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace XZPToolv3
{
    /// <summary>
    /// Output template for encryption
    /// </summary>
    [Serializable]
    public class OutputTemplate
    {
        public string Name { get; set; }
        public string Header { get; set; }
        public string LinePrefix { get; set; }
        public int BytesPerLine { get; set; }
        public string ByteFormat { get; set; }
        public string ByteSeparator { get; set; }
        public string LineSuffix { get; set; }
        public string Footer { get; set; }

        public OutputTemplate()
        {
            Name = "Default";
            Header = "#include \"stdafx.h\"\n\nBYTE Xui::Notify[118784] = {};\n\nBYTE Xui::XuiData[{SIZE}] = {\n";
            LinePrefix = "\t";
            BytesPerLine = 12;
            ByteFormat = "0x{0:X2}";
            ByteSeparator = ", ";
            LineSuffix = ",";
            Footer = "};\n";
        }

        public OutputTemplate Clone()
        {
            return new OutputTemplate
            {
                Name = this.Name,
                Header = this.Header,
                LinePrefix = this.LinePrefix,
                BytesPerLine = this.BytesPerLine,
                ByteFormat = this.ByteFormat,
                ByteSeparator = this.ByteSeparator,
                LineSuffix = this.LineSuffix,
                Footer = this.Footer
            };
        }
    }

    /// <summary>
    /// Application settings
    /// </summary>
    [Serializable]
    public class AppSettings
    {
        private static AppSettings _instance;
        private static readonly object _lock = new object();
        private static string _settingsPath;

        public bool AlwaysOverwrite { get; set; }
        public string MasterKey { get; set; }
        public List<OutputTemplate> Templates { get; set; }
        public int SelectedTemplateIndex { get; set; }
        public List<string> XuiExtensionFiles { get; set; }

        public AppSettings()
        {
            AlwaysOverwrite = false;
            MasterKey = "9B BC 90 A6 AE 90 A1 3D AC 7F 36 C6 E8 0A 8C 02";
            Templates = new List<OutputTemplate>();
            SelectedTemplateIndex = 0;
            XuiExtensionFiles = new List<string>();
        }

        private static List<OutputTemplate> CreateDefaultTemplates()
        {
            List<OutputTemplate> templates = new List<OutputTemplate>();

            templates.Add(new OutputTemplate
            {
                Name = "C++ (Default)",
                Header = "#include \"stdafx.h\"\n\nBYTE Xui::Notify[118784] = {{}};\n\nBYTE Xui::XuiData[{SIZE}] = {{\n",
                LinePrefix = "\t",
                BytesPerLine = 12,
                ByteFormat = "0x{0:X2}",
                ByteSeparator = ", ",
                LineSuffix = ",",
                Footer = "};\n"
            });

            templates.Add(new OutputTemplate
            {
                Name = "C#",
                Header = "namespace XuiData\n{{\n\tpublic static class Data\n\t{{\n\t\tpublic static byte[] XuiData = new byte[{SIZE}] {{\n",
                LinePrefix = "\t\t\t",
                BytesPerLine = 16,
                ByteFormat = "0x{0:X2}",
                ByteSeparator = ", ",
                LineSuffix = ",",
                Footer = "\t\t}};\n\t}}\n}}\n"
            });

            templates.Add(new OutputTemplate
            {
                Name = "Python",
                Header = "# XUI Data\nxui_data = bytes([\n",
                LinePrefix = "    ",
                BytesPerLine = 16,
                ByteFormat = "0x{0:X2}",
                ByteSeparator = ", ",
                LineSuffix = ",",
                Footer = "])\n"
            });

            return templates;
        }

        private static List<string> CreateDefaultXuiExtensions()
        {
            List<string> extensions = new List<string>();
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string extDir = Path.Combine(baseDir, "XUI", "Extensions");
            string[] files = { "XuiElements.xml", "XamElements.xml", "CustomXuiElements.xml" };

            foreach (string file in files)
            {
                string path = Path.Combine(extDir, file);
                if (File.Exists(path))
                    extensions.Add(path);
            }

            return extensions;
        }

        private static void DeduplicateTemplates(List<OutputTemplate> templates)
        {
            if (templates == null)
                return;

            HashSet<string> seen = new HashSet<string>();
            List<OutputTemplate> unique = new List<OutputTemplate>();
            foreach (OutputTemplate t in templates)
            {
                if (t == null)
                    continue;
                string key = string.Join("|",
                    t.Name ?? "",
                    t.Header ?? "",
                    t.LinePrefix ?? "",
                    t.BytesPerLine.ToString(),
                    t.ByteFormat ?? "",
                    t.ByteSeparator ?? "",
                    t.LineSuffix ?? "",
                    t.Footer ?? "");
                if (seen.Add(key))
                    unique.Add(t);
            }
            templates.Clear();
            templates.AddRange(unique);
        }

        public static AppSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = Load();
                        }
                    }
                }
                return _instance;
            }
        }

        private static string GetSettingsPath()
        {
            if (string.IsNullOrEmpty(_settingsPath))
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appData, "XZPToolv3");

                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                _settingsPath = Path.Combine(appFolder, "settings.xml");
            }
            return _settingsPath;
        }

        public static AppSettings Load()
        {
            string path = GetSettingsPath();

            try
            {
                if (File.Exists(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                    using (FileStream fs = new FileStream(path, FileMode.Open))
                    {
                        AppSettings settings = (AppSettings)serializer.Deserialize(fs);

                        // Ensure at least one template exists
                        if (settings.Templates == null || settings.Templates.Count == 0)
                            settings.Templates = CreateDefaultTemplates();
                        else
                            DeduplicateTemplates(settings.Templates);

                        if (settings.XuiExtensionFiles == null || settings.XuiExtensionFiles.Count == 0)
                            settings.XuiExtensionFiles = CreateDefaultXuiExtensions();

                        return settings;
                    }
                }
            }
            catch
            {
                // If loading fails, return default settings
            }

            AppSettings defaults = new AppSettings();
            defaults.Templates = CreateDefaultTemplates();
            defaults.XuiExtensionFiles = CreateDefaultXuiExtensions();
            return defaults;
        }

        public void Save()
        {
            string path = GetSettingsPath();

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    serializer.Serialize(fs, this);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Failed to save settings: {ex.Message}",
                    "Settings Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        public static void Reload()
        {
            lock (_lock)
            {
                _instance = Load();
            }
        }

        public byte[] GetMasterKeyBytes()
        {
            try
            {
                string[] parts = MasterKey.Split(new[] { ' ', ',', '-' }, StringSplitOptions.RemoveEmptyEntries);
                byte[] key = new byte[parts.Length];

                for (int i = 0; i < parts.Length; i++)
                {
                    key[i] = Convert.ToByte(parts[i].Trim(), 16);
                }

                return key;
            }
            catch
            {
                // Return default key if parsing fails
                return new byte[] { 0x9B, 0xBC, 0x90, 0xA6, 0xAE, 0x90, 0xA1, 0x3D, 0xAC, 0x7F, 0x36, 0xC6, 0xE8, 0x0A, 0x8C, 0x02 };
            }
        }
    }
}
