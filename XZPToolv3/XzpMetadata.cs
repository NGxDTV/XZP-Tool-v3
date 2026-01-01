using System;

namespace XZPToolv3
{
    /// <summary>
    /// Metadata information about an XZP archive
    /// </summary>
    public class XzpMetadata
    {
        public int Magic { get; set; }
        public int Version { get; set; }
        public int DataOffset { get; set; }
        public int EntryCount { get; set; }
        public long FileSize { get; set; }

        public XzpMetadata()
        {
        }

        public XzpMetadata(int magic, int version, int dataOffset, int entryCount, long fileSize)
        {
            Magic = magic;
            Version = version;
            DataOffset = dataOffset;
            EntryCount = entryCount;
            FileSize = fileSize;
        }

        public string GetMagicString()
        {
            byte[] bytes = BitConverter.GetBytes(Endian.SwapInt32(Magic));
            return System.Text.Encoding.ASCII.GetString(bytes);
        }
    }
}
