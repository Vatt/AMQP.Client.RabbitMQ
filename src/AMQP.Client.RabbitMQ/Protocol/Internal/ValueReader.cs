using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Internal
{
    internal ref struct ValueReader 
    {
        private SequenceReader<byte> _reader;
        public SequencePosition Position => _reader.Position;
        public ValueReader(ReadOnlySequence<byte> data)
        {
            _reader = new SequenceReader<byte>(data);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadShortInt(out short shortint)
        {
            return _reader.TryReadBigEndian(out shortint);
        }
        public bool ReadShortInt(out ushort shortint)
        {
            var span = _reader.CurrentSpan.Slice((int)_reader.Consumed, 2);
            var result =  BinaryPrimitives.TryReadUInt16BigEndian(span, out shortint);
            if (result)
            {
                _reader.Advance(2);
                return true;
            }
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadOctet(out byte octet)
        {
            return _reader.TryRead(out octet); 
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadLongLong(out long longlong)
        {
           return _reader.TryReadBigEndian(out longlong);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadLong(out int longValue)
        {
            return _reader.TryReadBigEndian(out longValue);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ReadStringInternal(int length, out string stringValue)
        {
            if (_reader.CurrentSpan.Length < _reader.CurrentSpanIndex + length)
            {
                stringValue = String.Empty;
                return false;
            }
            var stringSpan = _reader.CurrentSpan.Slice(_reader.CurrentSpanIndex, length);
            stringValue = Encoding.UTF8.GetString(stringSpan);
            _reader.Advance(length);
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadShortStr(out string shortStr)
        {   
            if(!ReadOctet(out byte len))
            {
                shortStr = String.Empty;
                return false;
            }
            return ReadStringInternal(len,out shortStr);

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadLongStr(out string longString)
        {
            if (!ReadLong(out int len))
            {
                longString = String.Empty;
                return false;
            }
            return ReadStringInternal(len, out longString);
        }
        
        public bool ReadBool(out bool boolValue)
        {
            if (!ReadOctet(out byte val))
            {
                boolValue = default;
                return false;
            }
            boolValue =  Convert.ToBoolean(val);
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadTimestamp(out long timestamp)
        {
            if(!ReadLongLong(out timestamp)) { return false; }
            return true;
        }
        public bool ReadTable(out Dictionary<string, object> table )
        {
            table = default;

            if(!ReadLong(out int tabLen)) { return false; }
            if (tabLen == 0) { return true; }
            var lengthBytes = tabLen + _reader.Consumed;
             table = new Dictionary<string, object>();
            while (_reader.Consumed < lengthBytes)
            {
                if (!ReadShortStr(out string name)) { return false; }
                if (!ReadValue(out object value)) { return false; }
                table.Add(name, value);
            }
            return true;
        }
        public bool ReadValue(out object value)
        {
            value = default;
            if (!ReadOctet(out byte type)) { return false; }
            switch ((char)type)
            {
                case 'F':
                    {
                        var tryRead = ReadTable(out Dictionary<string, object>  table);
                        value = table;
                        return tryRead;
                    }
                case 't':
                    {
                        var tryRead = ReadBool(out bool boolValue);
                        value = boolValue;
                        return tryRead;
                    }
                case 's':
                    {
                        var tryRead = ReadShortStr(out string stringValue);
                        value = stringValue;
                        return tryRead;
                    }
                case 'S':
                    {
                        var tryRead = ReadLongStr(out string stringValue);
                        value = stringValue;
                        return tryRead;
                    }
                default:
                    {
                        ReaderThrowHelper.ThrowIfUnrecognisedType();
                        return false;
                    }
            }
        }
    }
}
