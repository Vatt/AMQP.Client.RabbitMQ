using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AMQP.Client.RabbitMQ.Encoder
{
    internal ref struct ValueWriter
    {
        private Span<byte> _data;
        public int Position;
        public ValueWriter(Memory<byte> memory)
        {
            _data = memory.Span;
            Position = 0;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckOutOfRange(int length = 0, bool isShortStr = false)
        {
            if (isShortStr)
            {
                if (length > byte.MaxValue)
                {
                    EncoderThrowHelper.ThrowValueWriterOutOfRange();
                }
            }
            if (Position + length > _data.Length)
            {
                EncoderThrowHelper.ThrowValueWriterOutOfRange();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteOctet(byte octet)
        {
            CheckOutOfRange();
            _data[Position] = octet;
            Position++;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShortInt(short shortint)
        {
            CheckOutOfRange(2);
            bool tryResult = BinaryPrimitives.TryWriteInt16BigEndian(_data.Slice(Position, 2), shortint);
            Position += 2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLong(int longInt)
        {
            CheckOutOfRange(4);
            bool tryResult = BinaryPrimitives.TryWriteInt32BigEndian(_data.Slice(Position, 4), longInt);
            Position += 4;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt(int value)
        {
            WriteLong(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLongLong(long longlong)
        {
            CheckOutOfRange(8);
            bool tryResult = BinaryPrimitives.TryWriteInt64BigEndian(_data.Slice(Position, 8), longlong);
            Position += 8;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShortStr(string str)
        {
            ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes(str);
            int length = data.Length;
            CheckOutOfRange(length,true);
            data.CopyTo(_data.Slice(Position,length));
            Position += length;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLongStr(string str)
        {
            ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes(str);
            int length = data.Length;
            CheckOutOfRange(length);
            data.CopyTo(_data.Slice(Position, length));
            Position += length;
        }
    }
}
