using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mina.Extensions;

public static class Streams
{
    public static void Write(this Stream self, string s) => self.Write(System.Text.Encoding.UTF8.GetBytes(s));

    public static void WriteShortByLittleEndian(this Stream self, short n) { Span<byte> buffer = stackalloc byte[2]; BinaryPrimitives.WriteInt16LittleEndian(buffer, n); self.Write(buffer); }
    public static void WriteIntByLittleEndian(this Stream self, int n) { Span<byte> buffer = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(buffer, n); self.Write(buffer); }
    public static void WriteLongByLittleEndian(this Stream self, long n) { Span<byte> buffer = stackalloc byte[8]; BinaryPrimitives.WriteInt64LittleEndian(buffer, n); self.Write(buffer); }
    public static void WriteUShortByLittleEndian(this Stream self, ushort n) { Span<byte> buffer = stackalloc byte[2]; BinaryPrimitives.WriteUInt16LittleEndian(buffer, n); self.Write(buffer); }
    public static void WriteUIntByLittleEndian(this Stream self, uint n) { Span<byte> buffer = stackalloc byte[4]; BinaryPrimitives.WriteUInt32LittleEndian(buffer, n); self.Write(buffer); }
    public static void WriteULongByLittleEndian(this Stream self, ulong n) { Span<byte> buffer = stackalloc byte[8]; BinaryPrimitives.WriteUInt64LittleEndian(buffer, n); self.Write(buffer); }
    public static void WriteFloatByLittleEndian(this Stream self, float n) { Span<byte> buffer = stackalloc byte[4]; BinaryPrimitives.WriteSingleLittleEndian(buffer, n); self.Write(buffer); }
    public static void WriteDoubleByLittleEndian(this Stream self, double n) { Span<byte> buffer = stackalloc byte[8]; BinaryPrimitives.WriteDoubleLittleEndian(buffer, n); self.Write(buffer); }

    public static void WriteShortByBigEndian(this Stream self, short n) { Span<byte> buffer = stackalloc byte[2]; BinaryPrimitives.WriteInt16BigEndian(buffer, n); self.Write(buffer); }
    public static void WriteIntByBigEndian(this Stream self, int n) { Span<byte> buffer = stackalloc byte[4]; BinaryPrimitives.WriteInt32BigEndian(buffer, n); self.Write(buffer); }
    public static void WriteLongByBigEndian(this Stream self, long n) { Span<byte> buffer = stackalloc byte[8]; BinaryPrimitives.WriteInt64BigEndian(buffer, n); self.Write(buffer); }
    public static void WriteUShortByBigEndian(this Stream self, ushort n) { Span<byte> buffer = stackalloc byte[2]; BinaryPrimitives.WriteUInt16BigEndian(buffer, n); self.Write(buffer); }
    public static void WriteUIntByBigEndian(this Stream self, uint n) { Span<byte> buffer = stackalloc byte[4]; BinaryPrimitives.WriteUInt32BigEndian(buffer, n); self.Write(buffer); }
    public static void WriteULongByBigEndian(this Stream self, ulong n) { Span<byte> buffer = stackalloc byte[8]; BinaryPrimitives.WriteUInt64BigEndian(buffer, n); self.Write(buffer); }
    public static void WriteFloatByBigEndian(this Stream self, float n) { Span<byte> buffer = stackalloc byte[4]; BinaryPrimitives.WriteSingleBigEndian(buffer, n); self.Write(buffer); }
    public static void WriteDoubleByBigEndian(this Stream self, double n) { Span<byte> buffer = stackalloc byte[8]; BinaryPrimitives.WriteDoubleBigEndian(buffer, n); self.Write(buffer); }

    public static byte[] ReadBytes(this Stream self, int size)
    {
        var buffer = new byte[size];
        var readed = self.Read(buffer, 0, size);
        return size == readed ? buffer : [.. buffer.Take(readed)];
    }

    public static byte[] ReadPositionBytes(this Stream self, long pos, int size)
    {
        self.Position = pos;
        return self.ReadBytes(size);
    }

    public static IEnumerable<byte> ReadAllBytes(this Stream self, int buffer_size = 1024)
    {
        var buffer = new byte[buffer_size];
        while (true)
        {
            var readed = self.Read(buffer, 0, buffer_size);
            if (readed <= 0) break;
            for (var i = 0; i < readed; i++)
            {
                yield return buffer[i];
            }
        }
    }
}
