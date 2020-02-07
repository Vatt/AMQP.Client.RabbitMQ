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
                if(_reserved2==null)
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
                source.Slice(_reserved1.Length).CopyTo(_reserved1);
                source.Slice(_reserved1.Length, _reserved2.Length).CopyTo(_reserved2);
            }
        }
        
        public ValueWriter(IBufferWriter<byte> writer)
        {
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
            if (_span.Length >= length)
            {
                var reserved = new Reserved(_span.Slice(0,length));
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
                var first = _span.Slice(_buffered, length);
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
        private void WriteBytes(ReadOnlySpan<byte> source)
        {

            while (source.Length > 0)
            {
                if (_span.Length == 0)
                {
                    GetNextSpan();
                }

                var writable = Math.Min(source.Length, _span.Length);
                source.Slice(0, writable).CopyTo(_span);
                source = source.Slice(writable);
                Advance(writable);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteOctet(byte value)
        {
            if(_span.Length == 0)
            {
                GetNextSpan();
            }
            _span[0] = value;
            Advance(1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteType(char type)
        {
            WriteOctet((byte)type);
        }
  
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShortIntTyped(short shortint)
        {
            WriteType('U');
            WriteShortInt(shortint);
        }
   
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShortInt(short shortint)
        {
            
            if (_span.Length < 2)
            {
                byte[] bytes = ArrayPool<byte>.Shared.Rent(2);
                BinaryPrimitives.WriteInt16BigEndian(bytes, shortint);
                WriteBytes(bytes);
                ArrayPool<byte>.Shared.Return(bytes);
            }
            else
            {
                BinaryPrimitives.WriteInt16BigEndian(_span, shortint);
                Advance(2);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteLongTyped(int longInt)
        {
            WriteType('I');
            WriteLong(longInt);
        }
        public void WriteLong(int longInt)
        {
            if (_span.Length < 4)
            {
                byte[] bytes = ArrayPool<byte>.Shared.Rent(4);
                BinaryPrimitives.WriteInt32BigEndian(bytes, longInt);
                WriteBytes(bytes);
                ArrayPool<byte>.Shared.Return(bytes);
            }
            else
            {
                BinaryPrimitives.WriteInt32BigEndian(_span, longInt);
                Advance(4);
            }
        }
        public void WriteLongLongTyped(long longlong)
        {
            WriteType('L');
            WriteLongLong(longlong);
        }
        public void WriteLongLong(long longlong)
        {
            if (_span.Length < 8)
            {
                byte[] bytes = ArrayPool<byte>.Shared.Rent(8);
                BinaryPrimitives.WriteInt64BigEndian(bytes, longlong);
                WriteBytes(bytes);
                ArrayPool<byte>.Shared.Return(bytes);
            }
            else
            {
                BinaryPrimitives.WriteInt64BigEndian(_span, longlong);
                Advance(8);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShortStrTyped(string str)
        {
            WriteType('s');
            WriteShortStr(str);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteShortStr(string str)
        {
            if(str.Length > byte.MaxValue)
            {
                WriterThrowHelper.ThrowIfValueWriterOutOfRange();
            }
            ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes(str);
            WriteOctet((byte)data.Length);
            WriteBytes(data);
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
            ReadOnlySpan<byte> data = Encoding.UTF8.GetBytes(str);
            WriteLong(data.Length);
            WriteBytes(data);
        }
        private void WriteBoolTyped(bool b)
        {
            WriteType('t');
            WriteBool(b);
        }
        private void WriteBool(bool b)
        {
            WriteOctet(Convert.ToByte(b));
        }
        private void WriteValue(object value)
        {
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
                default: throw new Exception("WriteValue failed");
            }
        }
        public void WriteTableTyped(Dictionary<string, object> table)
        {
            WriteType('F');
            WriteTable(table);
        }
        public void WriteTable(Dictionary<string, object> table)
        {
            
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
