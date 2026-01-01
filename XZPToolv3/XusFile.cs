using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XZPToolv3
{
    public enum XusKeyMode
    {
        String = 0,
        UInt32 = 1,
        Index = 2
    }

    public sealed class XusEntry
    {
        public int IndexKey { get; set; }
        public uint UInt32Key { get; set; }
        public string StringKey { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public sealed class XusFile
    {
        public byte Version { get; private set; } = 2;
        public byte Flags { get; private set; }
        public byte[] Reserved4 { get; private set; } = new byte[4];

        public XusKeyMode KeyMode => (Flags & 0x02) != 0
            ? XusKeyMode.Index
            : ((Flags & 0x01) != 0 ? XusKeyMode.UInt32 : XusKeyMode.String);

        public List<XusEntry> Entries { get; } = new List<XusEntry>();

        public void SetKeyMode(XusKeyMode mode)
        {
            byte bits = 0;
            if (mode == XusKeyMode.UInt32)
                bits = 0x01;
            else if (mode == XusKeyMode.Index)
                bits = 0x02;

            Flags = (byte)((Flags & 0xFC) | bits);
        }

        public static XusFile Load(string path)
        {
            var data = File.ReadAllBytes(path);
            if (data.Length < 0x0C)
                throw new InvalidDataException("File too small");

            uint magic = ReadU32BE(data, 0);
            if (magic != 0x58554953 && magic != 0x58555300)
                throw new InvalidDataException("Bad magic");

            byte ver = data[4];
            if (ver != 2)
                throw new InvalidDataException("Unsupported version");

            var x = new XusFile();
            x.Version = ver;
            x.Flags = data[5];
            x.Reserved4 = new byte[4] { data[6], data[7], data[8], data[9] };
            ushort count = ReadU16BE(data, 0x0A);

            int pos = 0x0C;
            x.Entries.Clear();

            for (int i = 0; i < count; i++)
            {
                string value = ReadCStringUtf8(data, ref pos);
                var mode = x.KeyMode;

                if (mode == XusKeyMode.Index)
                {
                    x.Entries.Add(new XusEntry { IndexKey = i, Value = value });
                }
                else if (mode == XusKeyMode.UInt32)
                {
                    uint key = ReadU32BE(data, pos);
                    pos += 4;
                    x.Entries.Add(new XusEntry { UInt32Key = key, Value = value });
                }
                else
                {
                    string key = ReadCStringUtf8(data, ref pos);
                    x.Entries.Add(new XusEntry { StringKey = key, Value = value });
                }
            }

            return x;
        }

        public void Save(string path)
        {
            if (Entries.Count > ushort.MaxValue)
                throw new InvalidOperationException("Too many entries");

            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(Encoding.ASCII.GetBytes("XUIS"));
                bw.Write((byte)2);
                bw.Write(Flags);
                bw.Write(Reserved4.Length == 4 ? Reserved4 : new byte[4]);
                WriteU16BE(bw, 0);
                WriteU16BE(bw, (ushort)Entries.Count);

                var mode = KeyMode;

                for (int i = 0; i < Entries.Count; i++)
                {
                    var e = Entries[i];
                    WriteCStringUtf8(bw, e.Value);

                    if (mode == XusKeyMode.Index)
                    {
                        continue;
                    }
                    else if (mode == XusKeyMode.UInt32)
                    {
                        WriteU32BE(bw, e.UInt32Key);
                    }
                    else
                    {
                        WriteCStringUtf8(bw, e.StringKey ?? "");
                    }
                }

                File.WriteAllBytes(path, ms.ToArray());
            }
        }

        private static string ReadCStringUtf8(byte[] data, ref int pos)
        {
            int start = pos;
            // PPC loop scans to the 0 terminator before decoding.
            while (pos < data.Length && data[pos] != 0)
                pos++;
            if (pos >= data.Length)
                throw new EndOfStreamException();
            int len = pos - start;
            string s = Encoding.UTF8.GetString(data, start, len);
            pos += 1;
            return s;
        }

        private static void WriteCStringUtf8(BinaryWriter bw, string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s ?? "");
            bw.Write(bytes);
            bw.Write((byte)0);
        }

        private static ushort ReadU16BE(byte[] data, int off)
        {
            return (ushort)((data[off] << 8) | data[off + 1]);
        }

        private static uint ReadU32BE(byte[] data, int off)
        {
            return ((uint)data[off] << 24)
                | ((uint)data[off + 1] << 16)
                | ((uint)data[off + 2] << 8)
                | data[off + 3];
        }

        private static void WriteU16BE(BinaryWriter bw, ushort v)
        {
            bw.Write((byte)(v >> 8));
            bw.Write((byte)(v & 0xFF));
        }

        private static void WriteU32BE(BinaryWriter bw, uint v)
        {
            bw.Write((byte)(v >> 24));
            bw.Write((byte)((v >> 16) & 0xFF));
            bw.Write((byte)((v >> 8) & 0xFF));
            bw.Write((byte)(v & 0xFF));
        }
    }
}
