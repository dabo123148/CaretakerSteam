using System;
using System.Text;

namespace Caretaker
{
    public class ByteIterator
    {
        private byte[] data;
        private int index = 0;
        public ByteIterator(byte[] pData)
        {
            data = pData;
        }
        public ByteIterator(byte[] pData, int start)
        {
            data = pData;
            index = start;
        }
        public byte next()
        {
            index++;
            return data[index - 1];
        }
        public bool hasNext()
        {
            if (index < data.Length) return true;
            return false;
        }
        public string readstring()
        {
            byte currentbyte = next();
            string rg = "";
            while (currentbyte != 0x00)
            {
                rg += Encoding.UTF8.GetString(new byte[] { currentbyte });
                currentbyte = next();
            }
            return rg;
        }
        public uint readshort()
        {
            index += 2;
            return BitConverter.ToUInt16(data, index - 2);
        }
        public long readlong()
        {
            index += 4;
            return BitConverter.ToUInt32(data, index-4);
        }
        public float readfloat()
        {
            index += 4;
            return BitConverter.ToSingle(data, index - 4);
        }
    }
}
