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
        private SequenceReader<byte> _reader;
        public SequencePosition Position => _reader.Position;
        public long Consumed => _reader.Consumed;
        public ValueDecoder(ReadOnlySequence<byte> data)
        {
            _reader = new SequenceReader<byte>(data);
        }
        public ValueDecoder(ReadOnlySequence<byte> data,long advance)
        {
            _reader = new SequenceReader<byte>(data);
            _reader.Advance(advance);
            
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadShortInt()
        {
            var tryRead = _reader.TryReadBigEndian(out short val);
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
            var tryRead = _reader.TryRead(out byte val);
            return val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long ReadLongLong()
        {
            var tryRead = _reader.TryReadBigEndian(out long val);
            return val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadLong()
        {
            var tryRead = _reader.TryReadBigEndian(out int val);
            return val;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string ReadStringInternal(int length)
        {
            if (_reader.CurrentSpan.Length < _reader.CurrentSpanIndex + length)
            {
                DecoderThrowHelper.ThrowValueDecoderStringDecodeFailed();
            }
            var stringSpan = _reader.CurrentSpan.Slice(_reader.CurrentSpanIndex, length);
            var str = Encoding.UTF8.GetString(stringSpan);
            _reader.Advance(length);
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

            var lengthBytes = ReadInt() + _reader.Consumed;
            Dictionary<string, object> table = new Dictionary<string, object>();
            while (_reader.Consumed < lengthBytes)
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
