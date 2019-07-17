using System;
using System.Collections.Generic;
using System.Text;

namespace VDrumExplorer.Midi
{
    internal static class SysExUtility
    {
        internal static byte CalculateChecksum(byte[] data, int start, int length)
        {
            byte sum = 0;
            for (int i = 0; i < length; i++)
            {
                sum += data[i + start];
            }
            return (byte) ((0x80 - (sum & 0x7f)) & 0x7f);
        }
    }
}
