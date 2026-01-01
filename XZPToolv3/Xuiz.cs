using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace XZPToolv3
{
    /// <summary>
    /// XZP file format handler
    /// </summary>
    internal class Xuiz
    {
        private static int _bOff;
        private static int _nOff;

        /// <summary>
        /// Get list of entries in an XZP file
        /// </summary>
        public static List<XuizEntry> GetEntries(string xzpFile)
        {
            List<XuizEntry> entries = new List<XuizEntry>();

            using (BinaryReader br = new BinaryReader(File.Open(xzpFile, FileMode.Open, FileAccess.Read)))
            {
                br.BaseStream.Seek(4, SeekOrigin.Begin);
                int version = Endian.SwapInt32(br.ReadInt32());

                br.BaseStream.Seek(16, SeekOrigin.Begin);
                int dataOffset = Endian.SwapInt32(br.ReadInt32());
                short entryCount = Endian.SwapInt16(br.ReadInt16());

                for (int i = 0; i < entryCount; i++)
                {
                    long position = br.BaseStream.Position;

                    int fileSize = Endian.SwapInt32(br.ReadInt32());
                    int fileOffset = Endian.SwapInt32(br.ReadInt32());
                    int nameLength = br.ReadByte();

                    if (version == 1)
                        nameLength *= 2;

                    string fileName = new string(br.ReadChars(nameLength));

                    if (version == 1)
                        fileName = fileName.Replace("\0", "");

                    XuizEntry entry = new XuizEntry(fileName, fileSize, fileOffset, i);
                    entries.Add(entry);
                }
            }

            return entries;
        }

        /// <summary>
        /// Extract a single entry from XZP file
        /// </summary>
        public static void ExtractEntry(string xzpFile, XuizEntry entry, string outputPath)
        {
            using (BinaryReader br = new BinaryReader(File.Open(xzpFile, FileMode.Open, FileAccess.Read)))
            {
                br.BaseStream.Seek(16, SeekOrigin.Begin);
                int dataOffset = Endian.SwapInt32(br.ReadInt32());

                // Create directory if needed
                string directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Seek to file data
                br.BaseStream.Seek(entry.Offset + dataOffset + 22, SeekOrigin.Begin);

                // Write file
                using (BinaryWriter bw = new BinaryWriter(File.Open(outputPath, FileMode.Create)))
                {
                    byte[] buffer = br.ReadBytes((int)entry.Size);
                    bw.Write(buffer);
                }
            }
        }

        /// <summary>
        /// Extract entire XZP archive
        /// </summary>
        public static void ExtractXuiz(string xzpFile, string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);

            using (BinaryReader br = new BinaryReader(File.Open(xzpFile, FileMode.Open, FileAccess.Read)))
            {
                br.BaseStream.Seek(4, SeekOrigin.Begin);
                int version = Endian.SwapInt32(br.ReadInt32());

                br.BaseStream.Seek(16, SeekOrigin.Begin);
                int dataOffset = Endian.SwapInt32(br.ReadInt32());
                short entryCount = Endian.SwapInt16(br.ReadInt16());

                for (int i = 0; i < entryCount; i++)
                {
                    ExtractRsrc(br, version, outputDirectory, dataOffset);
                }
            }
        }

        private static void ExtractRsrc(BinaryReader br, int version, string outputDirectory, int dataOffset)
        {
            int fileSize = Endian.SwapInt32(br.ReadInt32());
            int fileOffset = Endian.SwapInt32(br.ReadInt32());
            int nameLength = br.ReadByte();

            if (version == 1)
                nameLength *= 2;

            string fileName = new string(br.ReadChars(nameLength));

            if (version == 1)
                fileName = fileName.Replace("\0", "");

            string directory = Path.GetDirectoryName(fileName);
            string outputPath = Path.Combine(outputDirectory, fileName);
            string outputDir = Path.Combine(outputDirectory, directory);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            long position = br.BaseStream.Position;
            br.BaseStream.Seek(fileOffset + dataOffset + 22, SeekOrigin.Begin);

            using (BinaryWriter bw = new BinaryWriter(File.Open(outputPath, FileMode.Create)))
            {
                byte[] buffer = br.ReadBytes(fileSize);
                bw.Write(buffer);
            }

            br.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        /// <summary>
        /// Build XZP archive from directory
        /// </summary>
        public static void BuildXuiz(string sourceDirectory, string outputFile, int version)
        {
            int entryCount = CountFiles(sourceDirectory);

            using (BinaryWriter bw = new BinaryWriter(File.Open(outputFile, FileMode.Create)))
            {
                WriteTempHeader(entryCount, version, bw);
                WriteResourceTable(version, sourceDirectory, sourceDirectory, bw);
                RsrcTableCleanup(bw);
                WriteFilesToXuiz(sourceDirectory, bw);
                WriteFileSize(bw);
            }

            _bOff = 0;
            _nOff = 0;
        }

        private static int CountFiles(string directory)
        {
            int count = Directory.GetFiles(directory).Length;

            foreach (string subDir in Directory.GetDirectories(directory))
            {
                count += CountFiles(subDir);
            }

            return count;
        }

        private static void WriteTempHeader(int entryCount, int version, BinaryWriter bw)
        {
            bw.Write(Endian.SwapInt32(1481984346)); // Magic: XZPU
            bw.Write((version == 1) ? Endian.SwapInt32(1) : Endian.SwapInt32(3));
            bw.Write(Endian.SwapInt32(0));
            bw.Write(Endian.SwapInt32(0));
            bw.Write(Endian.SwapInt32(0));
            bw.Write(Endian.SwapInt16((short)entryCount));
        }

        private static void WriteResourceTable(int version, string baseFolder, string currentDir, BinaryWriter bw)
        {
            string[] files = Directory.GetFiles(currentDir);
            Array.Sort(files);

            foreach (string file in files)
            {
                WriteRsrcToTable(version, baseFolder, file, bw);
            }

            string[] directories = Directory.GetDirectories(currentDir);
            Array.Sort(directories);

            foreach (string dir in directories)
            {
                WriteResourceTable(version, baseFolder, dir, bw);
            }
        }

        private static void WriteRsrcToTable(int version, string baseFolder, string fileName, BinaryWriter bw)
        {
            string relativePath = fileName.Substring(baseFolder.Length + 1);
            FileInfo fileInfo = new FileInfo(fileName);
            long length = fileInfo.Length;

            _bOff += _nOff;
            _nOff = (int)length;

            bw.Write(Endian.SwapInt32((int)length));
            bw.Write(Endian.SwapInt32(_bOff));

            if (version == 1)
            {
                bw.Write((byte)relativePath.Length);
                Encoding utf = Encoding.UTF32;
                byte[] bytes = utf.GetBytes(relativePath);
                string convertedPath = Encoding.Unicode.GetString(bytes);
                convertedPath = convertedPath.Remove(convertedPath.Length - 1, 1);
                convertedPath = convertedPath.Insert(0, "\0");
                char[] chars = convertedPath.ToCharArray();
                foreach (char ch in chars)
                {
                    bw.Write(ch);
                }
            }
            else
            {
                bw.Write((byte)relativePath.Length);
                bw.Write(relativePath.ToCharArray());
            }
        }

        private static void WriteFilesToXuiz(string currentDir, BinaryWriter bw)
        {
            string[] files = Directory.GetFiles(currentDir);
            Array.Sort(files);

            foreach (string file in files)
            {
                WriteFile(file, bw);
            }

            string[] directories = Directory.GetDirectories(currentDir);
            Array.Sort(directories);

            foreach (string dir in directories)
            {
                WriteFilesToXuiz(dir, bw);
            }
        }

        private static void WriteFile(string fileName, BinaryWriter bw)
        {
            FileInfo fileInfo = new FileInfo(fileName);
            long length = fileInfo.Length;

            using (BinaryReader br = new BinaryReader(File.Open(fileName, FileMode.Open, FileAccess.Read)))
            {
                byte[] buffer = new byte[length];
                br.BaseStream.Seek(0, SeekOrigin.Begin);
                br.BaseStream.Read(buffer, 0, (int)length);
                bw.Write(buffer);
            }
        }

        private static void WriteFileSize(BinaryWriter bw)
        {
            if (bw == null) return;

            long fileSize = bw.BaseStream.Position;
            bw.BaseStream.Seek(8, SeekOrigin.Begin);
            bw.Write(Endian.SwapInt32((int)fileSize));
        }

        private static void RsrcTableCleanup(BinaryWriter bw)
        {
            long position = bw.BaseStream.Position;
            bw.BaseStream.Seek(16, SeekOrigin.Begin);
            bw.Write(Endian.SwapInt32((int)position - 22));
            bw.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        /// <summary>
        /// Rename an entry in XZP archive
        /// </summary>
        public static void RenameEntry(string xzpFile, string oldName, string newName)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "XZPTool_" + Guid.NewGuid().ToString());

            try
            {
                // Extract all files
                ExtractXuiz(xzpFile, tempDir);

                // Rename the file
                string oldPath = Path.Combine(tempDir, oldName);
                string newPath = Path.Combine(tempDir, newName);

                if (!File.Exists(oldPath))
                    throw new Exception($"File '{oldName}' not found in archive.");

                if (File.Exists(newPath))
                    throw new Exception($"File '{newName}' already exists in archive.");

                // Create directory for new path if needed
                string newDir = Path.GetDirectoryName(newPath);
                if (!string.IsNullOrEmpty(newDir) && !Directory.Exists(newDir))
                {
                    Directory.CreateDirectory(newDir);
                }

                File.Move(oldPath, newPath);

                // Get version from original file
                int version = GetVersion(xzpFile);

                // Rebuild XZP
                BuildXuiz(tempDir, xzpFile, version);
            }
            finally
            {
                // Cleanup temp directory
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Delete entries from XZP archive
        /// </summary>
        public static void DeleteEntries(string xzpFile, List<string> entriesToDelete)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "XZPTool_" + Guid.NewGuid().ToString());

            try
            {
                // Extract all files
                ExtractXuiz(xzpFile, tempDir);

                // Delete specified files
                foreach (string entryName in entriesToDelete)
                {
                    string filePath = Path.Combine(tempDir, entryName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                // Get version from original file
                int version = GetVersion(xzpFile);

                // Rebuild XZP
                BuildXuiz(tempDir, xzpFile, version);
            }
            finally
            {
                // Cleanup temp directory
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Get version of XZP file
        /// </summary>
        public static int GetVersion(string xzpFile)
        {
            using (BinaryReader br = new BinaryReader(File.Open(xzpFile, FileMode.Open, FileAccess.Read)))
            {
                br.BaseStream.Seek(4, SeekOrigin.Begin);
                return Endian.SwapInt32(br.ReadInt32());
            }
        }

        /// <summary>
        /// Get metadata from XZP file
        /// </summary>
        public static XzpMetadata GetMetadata(string xzpFile)
        {
            using (BinaryReader br = new BinaryReader(File.Open(xzpFile, FileMode.Open, FileAccess.Read)))
            {
                FileInfo fileInfo = new FileInfo(xzpFile);

                br.BaseStream.Seek(0, SeekOrigin.Begin);
                int magic = br.ReadInt32();

                br.BaseStream.Seek(4, SeekOrigin.Begin);
                int version = Endian.SwapInt32(br.ReadInt32());

                br.BaseStream.Seek(16, SeekOrigin.Begin);
                int dataOffset = Endian.SwapInt32(br.ReadInt32());
                short entryCount = Endian.SwapInt16(br.ReadInt16());

                return new XzpMetadata(magic, version, dataOffset, entryCount, fileInfo.Length);
            }
        }

        /// <summary>
        /// Add files to existing XZP archive
        /// </summary>
        public static void AddFiles(string xzpFile, string[] filesToAdd, Func<string, bool> overwriteCallback = null)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "XZPTool_" + Guid.NewGuid().ToString());

            try
            {
                // Extract existing files
                ExtractXuiz(xzpFile, tempDir);

                // Copy new files to temp directory
                foreach (string file in filesToAdd)
                {
                    string fileName = Path.GetFileName(file);
                    string destPath = Path.Combine(tempDir, fileName);

                    // Check if file already exists
                    if (File.Exists(destPath))
                    {
                        bool overwrite = true;

                        if (overwriteCallback != null)
                        {
                            overwrite = overwriteCallback(fileName);
                        }
                        else
                        {
                            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(
                                $"File '{fileName}' already exists in archive. Overwrite?",
                                "File Exists",
                                System.Windows.Forms.MessageBoxButtons.YesNo,
                                System.Windows.Forms.MessageBoxIcon.Question);

                            overwrite = (result == System.Windows.Forms.DialogResult.Yes);
                        }

                        if (!overwrite)
                            continue;
                    }

                    File.Copy(file, destPath, true);
                }

                // Get version from original file
                int version = GetVersion(xzpFile);

                // Rebuild XZP
                BuildXuiz(tempDir, xzpFile, version);
            }
            finally
            {
                // Cleanup temp directory
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Convert XZP between Version 1 and Version 3
        /// </summary>
        public static void ConvertXzp(string xzpFile, int targetVersion)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "XZPTool_" + Guid.NewGuid().ToString());

            try
            {
                // Extract all files
                ExtractXuiz(xzpFile, tempDir);

                // Rebuild with target version
                BuildXuiz(tempDir, xzpFile, targetVersion);
            }
            finally
            {
                // Cleanup temp directory
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch { }
                }
            }
        }
    }
}
