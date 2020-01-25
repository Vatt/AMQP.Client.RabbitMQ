using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AMQP.Client.RabbitMQ.Encoder
{
    internal ref struct ValueWriter
    {
        public  Memory<byte> _data;
        public int Position { get; private set; }
        public ValueWriter(Memory<byte> memory)
        {
            _data = memory;
            Position = 0;
        }
        public ValueWriter(Memory<byte> memory, int startPosition)
        {
            _data = memory;
            Position = startPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(int value)
        {
            Position = value;
        }
        public void Reset()
        {
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
        public int WriteBytes(byte[] bytes)
        {

            bytes.CopyTo(_data.ToArray(), Position);
            Position += bytes.Length;
            return bytes.Length;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int WriteOctet(byte octet)
        {
            CheckOutOfRange();
            _data.Span[Position] = octet;
            Position++;
            return 1;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int WriteShortIntTyped(short shortint)
        {
            CheckOutOfRange(3);
            WriteType('U');
            WriteShortInt(shortint);
            return 3;
        }
        public int WriteShortInt(short shortint)
        {
            CheckOutOfRange(2);
            bool tryResult = BinaryPrimitives.TryWriteInt16BigEndian(_data.Span.Slice(Position, 2), shortint);
            Position += 2;
            return 2;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int WriteLongTyped(int longInt)
        {
            CheckOutOfRange(5);
            WriteType('I');
            WriteLong(longInt);
            return 5;
        }
        public int WriteLong(int longInt)
        {
            CheckOutOfRange(4);
            bool tryResult = BinaryPrimitives.TryWriteInt32BigEndian(_data.Span.Slice(Position, 4), longInt);
            Position += 4;
            return 4;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int WriteIntTyped(int value)
        {
           return WriteLongTyped(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int WriteLongLongTyped(long longlong)
        {
            CheckOutOfRange(9);
            WriteType('L');
            WriteLongLong(longlong);
            return 9;
        }
        public int WriteLongLong(long longlong)
        {
            CheckOutOfRange(8);
            bool tryResult = BinaryPrimitives.TryWriteInt64BigEndian(_data.Span.Slice(Position, 8), longlong);
            Position += 8;
            return 8;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int WriteShortStrTyped(string str)
        {
            int oldPos = Position;
             /*
             * string length + byte(type) + short(size)
            */
            CheckOutOfRange(str.Length + 1 + 2, true);
            WriteType('s');
            WriteShortInt((short)str.Length);
            WriteShortStr(str);
            
            return Position - oldPos;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int WriteShortStr(string str)
        {
            int oldPos = Position;
            ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes(str);
            /*
             * string length + byte(size)
            */
            CheckOutOfRange(data.Length + 1, true);
            WriteOctet((byte)data.Length);
            data.CopyTo(_data.Span.Slice(Position, data.Length));
            Position += data.Length;
            return Position - oldPos;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int WriteLongStrTyped(string str)
        {
            int oldPos = Position;
            ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes(str);
            CheckOutOfRange(data.Length + 1 + 4);
            WriteType('S');
            WriteLongStr(str);            
            return Position - oldPos;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int WriteLongStr(string str)
        {
            int oldPos = Position;
            ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes(str);
            CheckOutOfRange(data.Length + 4);            
            WriteLong(data.Length);
            data.CopyTo(_data.Span.Slice(Position, data.Length));
            Position += data.Length;
            return Position - oldPos;
        }
        public int WriteTableTyped(Dictionary<string,object> table)
        {
            int oldPos = Position;
            int tabSize = 0;
            WriteType('F');
            Span<byte> sizeSpan = _data.Span.Slice(Position, 4);
            Position += 4;
                       
            foreach(var pair in table)
            {
                tabSize += WriteShortStr(pair.Key);
                tabSize += WriteValue(pair.Value);

            }
            BinaryPrimitives.WriteInt32BigEndian(sizeSpan, tabSize);
            return Position - oldPos;
        }
        public int WriteTable(Dictionary<string, object> table)
        {
            int oldPos = Position;
            int tabSize = 0;
            Span<byte> sizeSpan = _data.Span.Slice(Position, 4);
            Position += 4;

            foreach (var pair in table)
            {
                tabSize += WriteShortStr(pair.Key);
                tabSize += WriteValue(pair.Value);

            }
            BinaryPrimitives.WriteInt32BigEndian(sizeSpan, tabSize);
            return Position - oldPos;
        }

        private int WriteValue(object value)
        {
            switch(value)
            {
                case short shortInt:return WriteShortIntTyped(shortInt);
                case int longInt: return WriteIntTyped(longInt);
                case long longLongInt: return WriteLongLongTyped(longLongInt); 
                case string str: return WriteLongStrTyped(str);
                case Dictionary<string, object> table: return WriteTableTyped(table);
                case bool b: return WriteBoolTyped(b);
                default: throw new Exception("WriteValue failed");
            }
        }

        private int WriteBoolTyped(bool b)
        {
            WriteType('t');
            WriteOctet(Convert.ToByte(b));
            return 2;
        }
        private int WriteBool(bool b)
        {
            return WriteOctet(Convert.ToByte(b));

        }

        private int WriteType(char type)
        {
            return WriteOctet((byte)type);
        }
    }
}
