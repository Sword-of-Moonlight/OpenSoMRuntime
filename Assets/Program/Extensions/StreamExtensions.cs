using System;
using System.IO;

public static class StreamExtensions
{
    public static void ReadExactly(this Stream stream, Span<byte> span)
    {
        int readNum = span.Length;
        int readOff = 0;

        byte[] buffer = new byte[readNum];

        while (readOff < readNum)
        {
            int readTry = stream.Read(buffer, readOff, readNum - readOff);
            if (readTry == 0)
                throw new EndOfStreamException();

            readOff += readTry;
        }

        for (int i = 0; i < readNum; ++i)
            span[i] = buffer[i];
    }

    public static void ReadExactly(this Stream stream, byte[] buffer, int offset, int count)
    {
        int readOff = 0;

        while (readOff < count)
        {
            int readTry = stream.Read(buffer, offset + readOff, count - readOff);
            if (readTry == 0)
                throw new EndOfStreamException();

            readOff += readTry;
        }
    }
}