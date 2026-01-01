using System;
using System.IO;
using System.Text;

namespace XZPToolv3.XUI
{
    /// <summary>
    /// Extends the <see cref="BinaryReader" /> class to allow reading values in the BigEndian format.
    /// </summary>
    public class BEBinaryReader : BinaryReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BEBinaryReader"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public BEBinaryReader(System.IO.Stream stream) : base(stream) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BEBinaryReader"/> class.
        /// </summary>
        /// <param name="input">The supplied stream.</param>
        /// <param name="encoding">The character encoding.</param>
        public BEBinaryReader(System.IO.Stream input, Encoding encoding) : base(input, encoding) { }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream using big endian encoding
        /// and advances the current position of the stream by two bytes.
        /// </summary>
        public override short ReadInt16()
        {
            byte[] byteArray = new byte[2];
            int iBytesRead = Read(byteArray, 0, 2);
            if (iBytesRead != 2)
            {
                throw new EndOfStreamException($"Failed to read 2 bytes for Int16. Only {iBytesRead} bytes were read.");
            }

            Array.Reverse(byteArray);
            return BitConverter.ToInt16(byteArray, 0);
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using big endian encoding
        /// and advances the position of the stream by two bytes.
        /// </summary>
        public override ushort ReadUInt16()
        {
            byte[] byteArray = new byte[2];
            int iBytesRead = Read(byteArray, 0, 2);
            if (iBytesRead != 2)
            {
                throw new EndOfStreamException($"Failed to read 2 bytes for UInt16. Only {iBytesRead} bytes were read.");
            }

            Array.Reverse(byteArray);
            return BitConverter.ToUInt16(byteArray, 0);
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream using big endian encoding
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        public override int ReadInt32()
        {
            byte[] byteArray = new byte[4];
            int iBytesRead = Read(byteArray, 0, 4);
            if (iBytesRead != 4)
            {
                throw new EndOfStreamException($"Failed to read 4 bytes for Int32. Only {iBytesRead} bytes were read.");
            }

            Array.Reverse(byteArray);
            return BitConverter.ToInt32(byteArray, 0);
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream using big endian encoding
        /// and advances the position of the stream by four bytes.
        /// </summary>
        public override uint ReadUInt32()
        {
            byte[] byteArray = new byte[4];
            int iBytesRead = Read(byteArray, 0, 4);
            if (iBytesRead != 4)
            {
                throw new EndOfStreamException($"Failed to read 4 bytes for UInt32. Only {iBytesRead} bytes were read.");
            }

            Array.Reverse(byteArray);

            return BitConverter.ToUInt32(byteArray, 0);
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream using big endian encoding
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        public override long ReadInt64()
        {
            byte[] byteArray = new byte[8];
            int iBytesRead = Read(byteArray, 0, 8);
            if (iBytesRead != 8)
            {
                throw new EndOfStreamException($"Failed to read 8 bytes for Int64. Only {iBytesRead} bytes were read.");
            }

            Array.Reverse(byteArray);
            return BitConverter.ToInt64(byteArray, 0);
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream using big endian encoding
        /// and advances the position of the stream by eight bytes.
        /// </summary>
        public override ulong ReadUInt64()
        {
            byte[] byteArray = new byte[8];
            int iBytesRead = Read(byteArray, 0, 8);
            if (iBytesRead != 8)
            {
                throw new EndOfStreamException($"Failed to read 8 bytes for UInt64. Only {iBytesRead} bytes were read.");
            }

            Array.Reverse(byteArray);
            return BitConverter.ToUInt64(byteArray, 0);
        }

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream using big endian encoding
        /// and advances the current position of the stream by four bytes.
        /// </summary>
        public override float ReadSingle()
        {
            byte[] byteArray = new byte[4];
            int iBytesRead = Read(byteArray, 0, 4);
            if (iBytesRead != 4)
            {
                throw new EndOfStreamException($"Failed to read 4 bytes for Single. Only {iBytesRead} bytes were read.");
            }

            Array.Reverse(byteArray);
            return BitConverter.ToSingle(byteArray, 0);
        }

        /// <summary>
        /// Reads an 8-byte floating point value from the current stream using big endian encoding
        /// and advances the current position of the stream by eight bytes.
        /// </summary>
        public override double ReadDouble()
        {
            byte[] byteArray = new byte[8];
            int iBytesRead = Read(byteArray, 0, 8);
            if (iBytesRead != 8)
            {
                throw new EndOfStreamException($"Failed to read 8 bytes for Double. Only {iBytesRead} bytes were read.");
            }

            Array.Reverse(byteArray);
            return BitConverter.ToDouble(byteArray, 0);
        }

        /// <summary>
        /// Reads a packed unsigned long value (variable length encoding)
        /// </summary>
        public uint ReadPackedUlong()
        {
            // Read the next byte in our stream
            uint firstVal = ReadByte();

            uint finalResult = 0;
            if (firstVal != 0xFF)
            {
                if (firstVal < 0xF0)
                {
                    // Set this byte as our result
                    finalResult = firstVal;
                }
                else
                {
                    // Read the next byte and combine
                    uint secondVal = ReadByte();
                    uint highPart = (firstVal << 8) & 0xF00;
                    finalResult = highPart | secondVal;
                }
            }
            else
            {
                // Return the next 4 bytes
                finalResult = ReadUInt32();
            }
            // Return result
            return finalResult;
        }
    }
}
