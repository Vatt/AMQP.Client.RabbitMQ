using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Internal
{
    //internal static class SequenceReaderExtensions
    //{
    //    public static bool TryReadBigEndian(ref this SequenceReader<byte> reader, out ushort value)
    //    {
    //        if (!BitConverter.IsLittleEndian)
    //        {
    //            return reader.TryRead(out value);
    //        }

    //        return TryReadReverseEndianness(ref reader, out value);
    //    }
    //    private static bool TryReadReverseEndianness(ref SequenceReader<byte> reader, out ushort value)
    //    {
    //        if (reader.TryRead(out value))
    //        {
    //            value = BinaryPrimitives.ReverseEndianness(value);
    //            return true;
    //        }

    //        return false;
    //    }
    //}

    internal ref struct ValueReader
    {
        private SequenceReader<byte> _reader;
        public SequencePosition Position => _reader.Position;
        public long Consumed => _reader.Consumed;
        public long Remaining => _reader.Remaining;
        public ValueReader(ReadOnlySequence<byte> data, SequencePosition position)
        {
            _reader = new SequenceReader<byte>(data.Slice(position));

        }
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
            if (_reader.Remaining < sizeof(ushort))
            {
                shortint = default;
                return false;
            }


            if (_reader.UnreadSpan.Length >= sizeof(ushort))
            {
                var span = _reader.UnreadSpan.Slice(0, sizeof(ushort));
                if (BinaryPrimitives.TryReadUInt16BigEndian(span, out shortint))
                {
                    _reader.Advance(sizeof(ushort));
                    return true;
                }
                else
                {
                    shortint = default;
                    return false;
                }
            }

            Span<byte> buffer = stackalloc byte[sizeof(ushort)];
            if (_reader.TryCopyTo(buffer))
            {
                if (BinaryPrimitives.TryReadUInt16BigEndian(buffer, out shortint))
                {
                    _reader.Advance(sizeof(ushort));
                    return true;
                }
                else
                {
                    shortint = default;
                    return false;
                }
            }

            shortint = default;
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
                stringValue = Encoding.UTF8.GetString(_reader.UnreadSpan.Slice(0, length));
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
            if (!ReadOctet(out byte len))
            {
                shortStr = String.Empty;
                return false;
            }
            return ReadStringInternal(len, out shortStr);

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
            boolValue = Convert.ToBoolean(val);
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadTimestamp(out long timestamp)
        {
            if (!ReadLongLong(out timestamp)) { return false; }
            return true;
        }
        public bool ReadTable(out Dictionary<string, object> table)
        {
            table = default;

            if (!ReadLong(out int tabLen)) { return false; }
            if (tabLen == 0) { return true; }
            if (_reader.Remaining < tabLen) { return false; }

            table = new Dictionary<string, object>();
            long unreaded = _reader.Remaining;
            while ((unreaded - _reader.Remaining) < tabLen)
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
                        var tryRead = ReadTable(out Dictionary<string, object> table);
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
