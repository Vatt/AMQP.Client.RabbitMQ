using AMQP.Client.RabbitMQ.Protocol.ThrowHelpers;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace AMQP.Client.RabbitMQ.Protocol.Internal
{

    internal ref struct ValueWriter
    {
        private byte _bitAccumulator;
        private int _bitMask;
        private bool _needBitFlush;

        private IBufferWriter<byte> _output;
        private Span<byte> _span;
        private int _buffered;
        public int Written { get; private set; }

        internal readonly ref struct Reserved
        {
            private readonly Span<byte> _reserved1;
            private readonly Span<byte> _reserved2;
            public Reserved(Span<byte> r1, Span<byte> r2)
            {
                _reserved1 = r1;
                _reserved2 = r2;
            }
            public Reserved(Span<byte> r1)
            {
                _reserved1 = r1;
                _reserved2 = null;
            }
            public void Write(Span<byte> source)
            {
                if (_reserved2 == null)
                {
                    WriteSingleSpan(source);
                }
                else
                {
                    WriteMultiSpan(source);
                }
            }
            private void WriteSingleSpan(Span<byte> source)
            {
                Debug.Assert(_reserved1.Length == source.Length);
                source.CopyTo(_reserved1);
            }
            private void WriteMultiSpan(Span<byte> source)
            {
                Debug.Assert(source.Length == _reserved1.Length + _reserved2.Length);
                source.Slice(0, _reserved1.Length).CopyTo(_reserved1);
                source.Slice(_reserved1.Length, _reserved2.Length).CopyTo(_reserved2);
            }
        }

        public ValueWriter(IBufferWriter<byte> writer)
        {
            _needBitFlush = false;
            _bitAccumulator = 0;
            _bitMask = 1;

            _output = writer;
            _span = _output.GetSpan();
            _buffered = 0;
            Written = 0;
        }
        public Reserved Reserve(int length)
        {
            if (length > 4096)
            {
                //TODO: do something
            }
            if (_span.Length == 0)
            {
                GetNextSpan();
            }
            if (_span.Length >= length)
            {
                var reserved = new Reserved(_span.Slice(0, length));
                Advance(length);
                if (_span.Length == 0)
                {
                    GetNextSpan();
                }
                return reserved;
            }
            else
            {
                var secondLen = length - _span.Length;
                var first = _span.Slice(0, _span.Length);
                _output.Advance(first.Length);
                GetNextSpan();
                var second = _span.Slice(0, secondLen);
                Advance(secondLen);
                return new Reserved(first, second);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Commit()
        {
            var buffered = _buffered;
            if (buffered > 0)
            {
                _buffered = 0;
                _output.Advance(buffered);
            }

        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ReadOnlySpan<byte> source)
        {
            WriteBytes(source);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            _buffered += count;
            Written += count;
            _span = _span.Slice(count);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetNextSpan(int size = 0)
        {
            if (_buffered > 0)
            {
                Commit();
            }
            _span = _output.GetSpan(size);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(ReadOnlySpan<byte> source)
        {
            BitFlush();
            var slice = source;
            while (slice.Length > 0)
            {
                if (_span.Length == 0)
                {
                    GetNextSpan();
                }

                var writable = Math.Min(slice.Length, _span.Length);
                slice.Slice(0, writable).CopyTo(_span);
                slice = slice.Slice(writable);
                Advance(writable);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteOctet(byte value)
        {
            BitFlush();
            if (_span.Length == 0)
            {
                GetNextSpan();
            }
            _span[0] = value;
            Advance(1);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteType(char type)
        {
            BitFlush();
            WriteOctet((byte)type);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShortIntTyped(short shortint)
        {
            BitFlush();
            WriteType('U');
            WriteShortInt(shortint);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShortInt(short shortint)
        {
            BitFlush();

            if (_span.Length < sizeof(short))
            {
                Span<byte> bytes = new byte[2];
                BinaryPrimitives.WriteInt16BigEndian(bytes, shortint);
                WriteBytes(bytes);
            }
            else
            {
                BinaryPrimitives.WriteInt16BigEndian(_span, shortint);
                Advance(sizeof(short));
            }
        }


        public void WriteShortInt(ushort shortint)
        {
            BitFlush();

            if (_span.Length < sizeof(ushort))
            {
                Span<byte> bytes = new byte[2];
                BinaryPrimitives.WriteUInt16BigEndian(bytes, shortint);
                WriteBytes(bytes);
            }
            else
            {
                BinaryPrimitives.WriteUInt16BigEndian(_span, shortint);
                Advance(sizeof(ushort));
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLongTyped(int longInt)
        {
            BitFlush();
            WriteType('I');
            WriteLong(longInt);
        }


        public void WriteLong(int longInt)
        {
            BitFlush();
            if (_span.Length < sizeof(int))
            {
                Span<byte> bytes = new byte[4];
                BinaryPrimitives.WriteInt32BigEndian(bytes, longInt);
                WriteBytes(bytes);
            }
            else
            {
                BinaryPrimitives.WriteInt32BigEndian(_span, longInt);
                Advance(sizeof(int));
            }
        }


        public void WriteLongLongTyped(long longlong)
        {
            BitFlush();
            WriteType('L');
            WriteLongLong(longlong);
        }


        public void WriteLongLong(long longlong)
        {
            BitFlush();
            if (_span.Length < sizeof(long))
            {
                Span<byte> bytes = new byte[8];
                BinaryPrimitives.WriteInt64BigEndian(bytes, longlong);
                WriteBytes(bytes);
            }
            else
            {
                BinaryPrimitives.WriteInt64BigEndian(_span, longlong);
                Advance(sizeof(long));
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShortStrTyped(string str)
        {
            BitFlush();
            WriteType('s');
            WriteShortStr(str);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShortStr(string str)
        {
            BitFlush();

            int length = Encoding.UTF8.GetMaxByteCount(str.Length);
            if (_span.Length >= length + sizeof(byte))
            {
                int byteCount = Encoding.UTF8.GetBytes(str, _span.Slice(sizeof(byte)));
                if (byteCount > 255)
                {
                    WriterThrowHelper.ThrowIfValueWriterOutOfRange();
                }
                _span[0] = (byte)byteCount;
                Advance(byteCount + sizeof(byte));
            }
            else
            {
                if (str.Length > byte.MaxValue)
                {
                    WriterThrowHelper.ThrowIfValueWriterOutOfRange();
                }
                ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes(str);
                WriteOctet((byte)data.Length);
                WriteBytes(data);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLongStrTyped(string str)
        {
            WriteType('S');
            WriteLongStr(str);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLongStr(string str)
        {
            BitFlush();

            int length = Encoding.UTF8.GetMaxByteCount(str.Length);
            if (_span.Length >= length + sizeof(int))
            {
                int byteCount = Encoding.UTF8.GetBytes(str, _span.Slice(sizeof(int)));
                BinaryPrimitives.WriteInt32BigEndian(_span, byteCount);
                Advance(byteCount + sizeof(int));
            }
            else
            {
                ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes(str);
                WriteLong(data.Length);
                WriteBytes(data);
            }
        }


        public void WriteBoolTyped(bool b)
        {
            BitFlush();
            WriteType('t');
            WriteBool(b);
        }


        public void WriteBool(bool b)
        {
            WriteOctet(Convert.ToByte(b));
        }


        public void WriteBit(bool value)
        {
            if (_bitMask > 0x80)
            {
                BitFlush();
            }
            if (value)
            {
                // The cast below is safe, because the combination of
                // the test against 0x80 above, and the action of
                // BitFlush(), causes m_bitMask never to exceed 0x80
                // at the point the following statement executes.
                _bitAccumulator = (byte)(_bitAccumulator | (byte)_bitMask);
            }
            _bitMask = _bitMask << 1;
            _needBitFlush = true;
        }


        public void BitFlush()
        {
            if (_span.Length == 0)
            {
                GetNextSpan();
            }
            if (_needBitFlush)
            {
                _span[0] = _bitAccumulator;
                Advance(1);
                ResetBitAccumulator();
            }
        }


        private void ResetBitAccumulator()
        {
            _needBitFlush = false;
            _bitAccumulator = 0;
            _bitMask = 1;
        }

        private void WriteValue(object value)
        {
            BitFlush();
            switch (value)
            {
                case short shortInt:
                    {
                        WriteShortIntTyped(shortInt);
                        break;
                    }
                case int longInt:
                    {
                        WriteLongTyped(longInt);
                        break;
                    }
                case long longLongInt:
                    {
                        WriteLongLongTyped(longLongInt);
                        break;
                    }
                case string str:
                    {
                        WriteLongStrTyped(str);
                        break;
                    }
                case Dictionary<string, object> table:
                    {
                        WriteTableTyped(table);
                        break;
                    }
                case bool b:
                    {
                        WriteBoolTyped(b);
                        break;
                    }
                default: throw new Exception($"WriteValue ({value.ToString()} failed");
            }
        }
        public void WriteTableTyped(Dictionary<string, object> table)
        {
            WriteType('F');
            WriteTable(table);
        }
        public void WriteTable(Dictionary<string, object> table)
        {
            BitFlush();
            if (table == null)
            {
                WriteLong(0);
                return;
            }
            Span<byte> sizeSpan = stackalloc byte[4];
            var reserved = Reserve(4);
            int first = Written;
            foreach (var pair in table)
            {
                WriteShortStr(pair.Key);
                WriteValue(pair.Value);

            }
            int size = Written - first;
            BinaryPrimitives.WriteInt32BigEndian(sizeSpan, size);
            reserved.Write(sizeSpan);
        }
    }
}
