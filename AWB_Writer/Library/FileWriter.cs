﻿using Syroot.BinaryData;
using System.IO;
using System.Text;
using System.Linq;

namespace Library.IO
{
    public class FileWriter : BinaryDataWriter
    {
        public bool ReverseMagic { get; set; } = false;

        public void CheckByteOrderMark(uint ByteOrderMark)
        {
            if (ByteOrderMark == 0xFEFF)
                ByteOrder = ByteOrder.BigEndian;
            else
                ByteOrder = ByteOrder.LittleEndian;
        }

        public FileWriter(Stream stream, bool leaveOpen = false)
            : base(stream, Encoding.ASCII, leaveOpen)
        {
        }

        public FileWriter(Stream stream, Encoding encoding, bool leaveOpen = false)
    : base(stream, encoding, leaveOpen)
        {
        }

        public FileWriter(string fileName)
             : this(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
        {
        }

        public FileWriter(byte[] data)
             : this(new MemoryStream(data))
        {
        }

        public void WriteSignature(string value)
        {
            if (ReverseMagic)
                Write(Encoding.ASCII.GetBytes(new string(value.Reverse().ToArray())));
            else
                Write(Encoding.ASCII.GetBytes(value));
        }

        public void WriteString(string value, Encoding encoding = null)
        {
            Write(value, BinaryStringFormat.ZeroTerminated, encoding ?? Encoding);
        }

        public void WriteUint64Offset(long target)
        {
            long pos = Position;
            using (TemporarySeek(target, SeekOrigin.Begin))
            {
                Write(pos);
            }
        }

        public void SetByteOrder(bool IsBigEndian)
        {
            if (IsBigEndian)
                ByteOrder = ByteOrder.BigEndian;
            else
                ByteOrder = ByteOrder.LittleEndian;
        }

        public void WriteString(string text, uint fixedSize, Encoding encoding = null)
        {
            long pos = Position;
            WriteString(text, encoding);
            SeekBegin(pos + fixedSize);
        }

        public void Write(object value, long pos)
        {
            using (TemporarySeek(pos, SeekOrigin.Begin)) {
                if (value is uint) Write((uint)value);
                else if (value is int) Write((int)value);
                else if (value is long) Write((long)value);
                else if (value is ulong) Write((ulong)value);
                else if (value is ushort) Write((ushort)value);
                else if (value is short) Write((short)value);
                else if (value is sbyte) Write((sbyte)value);
                else if (value is byte) Write((byte)value);
            }
        }

        //Writes the total size of a section as a uint. 
        public void WriteSectionSizeU32(long position, long startPosition, long endPosition) {
            WriteSectionSizeU32(position, endPosition - startPosition);
        }

        public void WriteSectionSizeU32(long position, long size) {
            using (TemporarySeek(position, System.IO.SeekOrigin.Begin))
            {
                Write((uint)(size));
            }
        }

        //
        // RelativeOffsetPosition controls the relative position the offset starts at
        //
        public void WriteUint32Offset(long target, long relativePosition = 0) 
        {
            long pos = Position;
            using (TemporarySeek(target, SeekOrigin.Begin))
            {
                Write((uint)(pos - relativePosition));
            }
        }

        public void WriteUint16Offset(long target, long relativePosition)
        {
            long pos = Position;
            using (TemporarySeek(target, SeekOrigin.Begin))
            {
                Write((ushort)(pos - relativePosition));
            }
        }

        public void SeekBegin(uint Offset) { Seek(Offset, SeekOrigin.Begin); }
        public void SeekBegin(int Offset) { Seek(Offset, SeekOrigin.Begin); }
        public void SeekBegin(long Offset) { Seek(Offset, SeekOrigin.Begin); }

        public void Write(OpenTK.Vector2 v)
        {
            Write(v.X);
            Write(v.Y);
        }
        public void Write(OpenTK.Vector3 v)
        {
            Write(v.X);
            Write(v.Y);
            Write(v.Z);
        }
        public void Write(OpenTK.Vector4 v)
        {
            Write(v.X);
            Write(v.Y);
            Write(v.Z);
            Write(v.W);
        }

        /// <summary>
        /// Aligns the data by writing bytes (rather than seeking)
        /// </summary>
        /// <param name="alignment"></param>
        /// <param name="value"></param>
        public void AlignBytes(int alignment, byte value = 0x00)
        {
            var startPos = Position;
            long position = Seek((-Position % alignment + alignment) % alignment, SeekOrigin.Current);

            Seek(startPos, System.IO.SeekOrigin.Begin);
            while (Position != position) {
                Write(value);
            }
        }
    }
}
