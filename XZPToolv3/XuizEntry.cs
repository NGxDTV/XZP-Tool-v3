using System;

namespace XZPToolv3
{
    /// <summary>
    /// Represents an entry (file) within an XZP archive
    /// </summary>
    public class XuizEntry
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public long Offset { get; set; }
        public int Index { get; set; }

        public XuizEntry()
        {
        }

        public XuizEntry(string name, long size, long offset, int index)
        {
            Name = name;
            Size = size;
            Offset = offset;
            Index = index;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
