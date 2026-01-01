using System;

namespace XZPToolv3
{
    /// <summary>
    /// RC4 encryption/decryption implementation
    /// </summary>
    internal class RC4
    {
        /// <summary>
        /// Encrypt or decrypt data using RC4 algorithm
        /// </summary>
        public static byte[] Process(byte[] key, byte[] data)
        {
            if (key == null || key.Length == 0)
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Initialize S-box
            byte[] s = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                s[i] = (byte)i;
            }

            // Key scheduling algorithm (KSA)
            int j = 0;
            int klen = key.Length;
            for (int i = 0; i < 256; i++)
            {
                j = (j + s[i] + key[i % klen]) & 0xFF;
                // Swap s[i] and s[j]
                byte temp = s[i];
                s[i] = s[j];
                s[j] = temp;
            }

            // Pseudo-random generation algorithm (PRGA)
            byte[] output = new byte[data.Length];
            int x = 0;
            int y = 0;
            for (int n = 0; n < data.Length; n++)
            {
                x = (x + 1) & 0xFF;
                y = (y + s[x]) & 0xFF;

                // Swap s[x] and s[y]
                byte temp = s[x];
                s[x] = s[y];
                s[y] = temp;

                output[n] = (byte)(data[n] ^ s[(s[x] + s[y]) & 0xFF]);
            }

            return output;
        }
    }
}
