using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace wscloudy.Compress
{
    public class ByteHelper
    {
        public const ushort COMPRESSION_FORMAT_LZNT1 = 2;
        public const ushort COMPRESSION_ENGINE_MAXIMUM = 0x100;

        [DllImport("ntdll.dll")]
        public static extern uint RtlGetCompressionWorkSpaceSize(ushort dCompressionFormat, out uint dNeededBufferSize, out uint dUnknown);

        [DllImport("ntdll.dll")]
        public static extern uint RtlCompressBuffer(ushort dCompressionFormat, byte[] dSourceBuffer, int dSourceBufferLength, byte[] dDestinationBuffer,
        int dDestinationBufferLength, uint dUnknown, out int dDestinationSize, IntPtr dWorkspaceBuffer);

        [DllImport("ntdll.dll")]
        public static extern uint RtlDecompressBuffer(ushort dCompressionFormat, byte[] dDestinationBuffer, int dDestinationBufferLength, byte[] dSourceBuffer, int dSourceBufferLength, out uint dDestinationSize);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr LocalAlloc(int uFlags, IntPtr sizetdwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LocalFree(IntPtr hMem);


        public static byte[] Decompress(byte[] buffer)
        {
            var outBuf = new byte[buffer.Length * 6];
            uint dwSize = 0, dwRet = 0;
            uint ret = RtlGetCompressionWorkSpaceSize(COMPRESSION_FORMAT_LZNT1, out dwSize, out dwRet);
            if (ret != 0) return null;

            ret = RtlDecompressBuffer(COMPRESSION_FORMAT_LZNT1, outBuf, outBuf.Length, buffer, buffer.Length, out dwRet);
            if (ret != 0) return null;

            Array.Resize(ref outBuf, (Int32)dwRet);
            return outBuf;
        }


        public static byte[] Compress(byte[] buffer)
        {
            var outBuf = new byte[buffer.Length * 6];
            uint dwSize = 0, dwRet = 0;
            uint ret = RtlGetCompressionWorkSpaceSize(COMPRESSION_FORMAT_LZNT1 | COMPRESSION_ENGINE_MAXIMUM, out dwSize, out dwRet);
            if (ret != 0) return null;

            int dstSize = 0;
            IntPtr hWork = LocalAlloc(0, new IntPtr(dwSize));
            ret = RtlCompressBuffer(COMPRESSION_FORMAT_LZNT1 | COMPRESSION_ENGINE_MAXIMUM, buffer, buffer.Length, outBuf, outBuf.Length, 0, out dstSize, hWork);
            if (ret != 0) return null;

            LocalFree(hWork);

            Array.Resize(ref outBuf, dstSize);
            return outBuf;
        }
    }
}
