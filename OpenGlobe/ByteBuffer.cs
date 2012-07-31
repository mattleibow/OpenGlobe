namespace OpenGlobe
{
    using System;

    /// <summary>
    /// A simple wrapper for reading values out of a byte array.
    /// </summary>
    public class ByteBuffer
    {
        private readonly byte[] array;

        private int position;

        public ByteBuffer(byte[] head)
        {
            this.array = head;
        }

        public int Int32
        {
            get
            {
                if (BitConverter.IsLittleEndian)
                {
                    return (this.Byte & 0xff) +
                           ((this.Byte & 0xff) << 8) +
                           ((this.Byte & 0xff) << 16) +
                           (this.Byte << 24);
                }

                return (this.Byte << 24) +
                       ((this.Byte & 0xff) << 16) +
                       ((this.Byte & 0xff) << 8) +
                       (this.Byte & 0xff);
            }
        }

        public byte Byte
        {
            get
            {
                return this.array[this.position++];
            }
        }

        public static ByteBuffer Wrap(byte[] head)
        {
            return new ByteBuffer(head);
        }
    }
}