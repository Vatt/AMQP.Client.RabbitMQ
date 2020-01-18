using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AMQP.Client.RabbitMQ
{
    internal ref struct ValueDecoder
    {
        public SequenceReader<byte> reader;
        public int Position { get; private set; }
        public ReadOnlySpan<byte> Data;

        public ValueDecoder(ReadOnlySpan<byte> data)
        {
            Position = 0;
            Data = data;
            reader = new SequenceReader<byte>();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SkipMany(int value)
        {
            Position += value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadShortInt()
        {
            int val = BinaryPrimitives.ReadInt16BigEndian(Data.Slice(Position, 2));
            Position += 2;
            return val;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt()
        {
            int val = BinaryPrimitives.ReadInt32BigEndian(Data.Slice(Position, 4));
            Position += 4;
            return val;

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadOctet()
        {
            var val = Data[Position];
            Position++;
            return val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadLongLong()
        {
            var val = BinaryPrimitives.ReadInt64BigEndian(Data.Slice(Position, 8));
            Position += 8;
            return val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadLong()
        {
            var val = BinaryPrimitives.ReadInt32BigEndian(Data.Slice(Position, 4));
            Position += 4;
            return val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadShortStr()
        {
            int length = ReadOctet();
            var str = Encoding.UTF8.GetString(Data.Slice(Position, length));
            Position += length;
            return str;
        }
        public Dictionary<string, object> ReadTable()
        {

            var lengthBytes = ReadInt() + Position;
            Dictionary<string, object> table = new Dictionary<string, object>();
            while (Position < lengthBytes)
            {
                string name = ReadShortStr();
                object value = ReadValue();
                table.Add(name, value);
            }
            return table;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadLongStr()
        {
            int length = ReadInt();
            var str = Encoding.UTF8.GetString(Data.Slice(Position, length));
            Position += length;
            return str;
        }
        public bool ReadBool()
        {
            return Convert.ToBoolean(ReadOctet());
        }
        public object ReadValue()
        {
            char type = (char)ReadOctet();
            switch (type)
            {
                case 'F': return ReadTable();
                case 't': return ReadBool();
                case 's': return ReadShortStr();
                case 'S': return ReadLongStr();
                default: throw new ArgumentException("Unrecognised type");
            }
        }
    }
}
