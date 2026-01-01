using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace XZPToolv3.XUI
{
    public class ByteArray : List<byte>
    {
        public long GetSize()
        {
            return Count;
        }

        public void AddDWORD(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 4);
            Array.Reverse(bytes, 0, 4);
            foreach (byte b in bytes) Add(b);
        }

        public void AddWORD(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 2);
            Array.Reverse(bytes, 0, 2);
            foreach (byte b in bytes) Add(b);
        }

        public void AddBYTE(byte value)
        {
            Add(value);
        }

        public void AddBOOL(bool value)
        {
            byte val = (byte)(value == true ? 0x1 : 0x0);
            Add(val);
        }

        public void AddFLOAT(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 4);
            Array.Reverse(bytes, 0, 4);
            foreach (byte b in bytes) Add(b);
        }

        public void AddCHAR(char value)
        {
            Add((byte)value);
        }

        public void AddSTRING(string value, bool addNull = true)
        {
            int stringPos = 0;
            while (stringPos < value.Length)
            {
                Add((byte)value[stringPos]);
                stringPos++;
            }
            if (addNull) Add((byte)0x00);
        }

        public void AddPackedDWORD(uint value)
        {
            if (value > (uint)0xEFF)
            {
                AddBYTE((byte)0xFF);
                AddDWORD((uint)value);
            }
            else if (value > (uint)0xEF)
            {
                uint highPart = value >> 8;
                highPart |= 0xF0;
                byte lowPart = (byte)(value & 0xFF);

                AddBYTE((byte)highPart);
                AddBYTE((byte)lowPart);
            }
            else
            {
                AddBYTE((byte)(value & 0xFF));
            }
        }

        public void AddByteARRAY(ByteArray byteArray)
        {
            foreach (byte b in byteArray)
            {
                AddBYTE(b);
            }
        }

        public void WriteToFile(System.IO.Stream stream)
        {
            foreach (byte b in this)
                stream.WriteByte((byte)b);

            stream.Flush();
        }

        public void WriteToFile(System.IO.Stream stream, long pos)
        {
            long currentPos = stream.Position;
            stream.Seek(pos, SeekOrigin.Begin);
            byte[] byteArray = ToArray();
            for (int x = 0; x < byteArray.Length; x++)
            {
                stream.WriteByte((byte)byteArray.GetValue(x));
            }
            stream.Flush();
            stream.Seek(currentPos, SeekOrigin.Begin);
        }

        public void UpdateDWORD(uint value, long pos)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 4);
            Array.Reverse(bytes, 0, 4);

            foreach (byte b in bytes)
            {
                this[(int)pos] = b;
                pos++;
            }
        }

        public void UpdateWORD(ushort value, long pos)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 2);
            Array.Reverse(bytes, 0, 2);

            foreach (byte b in bytes)
            {
                this[(int)pos] = b;
                pos++;
            }
        }

        public void UpdateBYTE(byte value, long pos)
        {
            this[(int)pos] = value;
        }

        public void UpdateFLOAT(float value, long pos)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Debug.Assert(bytes.Length == 4);
            Array.Reverse(bytes, 0, 4);

            foreach (byte b in bytes)
            {
                this[(int)pos] = b;
                pos++;
            }
        }

        public void UpdateCHAR(char value, long pos)
        {
            this[(int)pos] = (byte)value;
        }
    }
}
