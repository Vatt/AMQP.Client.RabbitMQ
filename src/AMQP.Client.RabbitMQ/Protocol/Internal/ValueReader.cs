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
        public void Advance(long count)
        {
            _reader.Advance(count);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadShortInt(out short shortint)
        {
            return _reader.TryReadBigEndian(out shortint);
        }
        public bool ReadShortInt(out ushort shortint)
        {
            var span = _reader.UnreadSpan.Slice(0, sizeof(ushort));
            var result =  BinaryPrimitives.TryReadUInt16BigEndian(span, out shortint);
            if (result)
            {
                _reader.Advance(sizeof(ushort));
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
            if (_reader.Remaining < length)
            {
                stringValue = string.Empty;
                return false;
            }

            if (_reader.UnreadSpan.Length >= length)
            {
                stringValue = Encoding.UTF8.GetString(_reader.UnreadSpan.Slice(length));
                _reader.Advance(length);
                return true;
            }

            Span<byte> span = stackalloc byte[length];
            if (!_reader.TryCopyTo(span))
            {
                stringValue = string.Empty;
                return false;
            }

            stringValue = Encoding.UTF8.GetString(span);
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
            if (_reader.UnreadSpan.Length < tabLen) { return false; }

            table = new Dictionary<string, object>();
            int unreaded = _reader.UnreadSpan.Length;
            while ((unreaded - _reader.UnreadSpan.Length) < tabLen)
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
