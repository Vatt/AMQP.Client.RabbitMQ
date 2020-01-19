using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AMQP.Client.RabbitMQ.Decoder
{

    internal ref struct ValueDecoder
    {
        public SequenceReader<byte> reader;
        public ValueDecoder(ReadOnlySequence<byte> data)
        {
            reader = new SequenceReader<byte>(data);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadShortInt()
        {
            var tryRead = reader.TryReadBigEndian(out short val);
            return val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt()
        {
            return ReadLong();

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadOctet()
        {
            var tryRead = reader.TryRead(out byte val);
            return val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadLongLong()
        {
            var tryRead = reader.TryReadBigEndian(out long val);
            return val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadLong()
        {
            var tryRead = reader.TryReadBigEndian(out int val);
            return val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ReadStringInternal(int length)
        {
            if (reader.CurrentSpan.Length < reader.CurrentSpanIndex + length)
            {
                DecoderThrowHelper.ThrowValueDecoderStringDecodeFailed();
            }
            var stringSpan = reader.CurrentSpan.Slice(reader.CurrentSpanIndex, length);
            var str = Encoding.UTF8.GetString(stringSpan);
            reader.Advance(length);
            return str;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadShortStr()
        {
            return ReadStringInternal(ReadOctet()); 

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadLongStr()
        {
            return ReadStringInternal(ReadInt());
        }
        public Dictionary<string, object> ReadTable()
        {

            var lengthBytes = ReadInt() + reader.Consumed;
            Dictionary<string, object> table = new Dictionary<string, object>();
            while (reader.Consumed < lengthBytes)
            {
                string name = ReadShortStr();
                object value = ReadValue();
                table.Add(name, value);
            }
            return table;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                default:
                    {
                        DecoderThrowHelper.ThrowValueDecoderUnrecognisedType();
                        return null;
                    }
            }
        }
    }
}
