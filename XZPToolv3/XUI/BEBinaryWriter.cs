using System;
using System.IO;
using System.Text;

namespace XZPToolv3.XUI
{
    /// <summary>
    /// Extends the <see cref="BinaryWriter" /> class to allow writing values in the BigEndian format.
    /// </summary>
    public class BEBinaryWriter : BinaryWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BEBinaryWriter"/> class.
        /// </summary>
        public BEBinaryWriter() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BEBinaryWriter"/> class.
        /// </summary>
        /// <param name="output">The supplied stream.</param>
        public BEBinaryWriter(Stream output) : base(output) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BEBinaryWriter"/> class.
        /// </summary>
        /// <param name="output">The supplied stream.</param>
        /// <param name="encoding">The character encoding.</param>
        public BEBinaryWriter(Stream output, Encoding encoding) : base(output, encoding) { }

        /// <summary>
        /// Writes a two-byte signed integer to the current stream using BigEndian encoding
        /// and advances the stream position by two bytes.
        /// </summary>
        public override void Write(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes, 0, 2);
            Write(bytes);
        }

        /// <summary>
        /// Writes a four-byte signed integer to the current stream using BigEndian encoding
        /// and advances the stream position by four bytes.
        /// </summary>
        public override void Write(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes, 0, 4);
            Write(bytes);
        }

        /// <summary>
        /// Writes an eight-byte signed integer to the current stream using BigEndian encoding
        /// and advances the stream position by eight bytes.
        /// </summary>
        public override void Write(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes, 0, 8);
            Write(bytes);
        }

        /// <summary>
        /// Writes a four-byte floating-point value to the current stream using BigEndian encoding
        /// and advances the stream position by four bytes.
        /// </summary>
        public override void Write(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes, 0, 4);
            Write(bytes);
        }

        /// <summary>
        /// Writes an eight-byte floating-point value to the current stream using BigEndian encoding
        /// and advances the stream position by eight bytes.
        /// </summary>
        public override void Write(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes, 0, 8);
            Write(bytes);
        }
    }
}
