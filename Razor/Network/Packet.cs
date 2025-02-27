using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Assistant
{
    public enum PacketPath
    {
        ClientToServer,
        RazorToServer,
        ServerToClient,
        RazorToClient,

        PacketVideo,
    }

    public class Packet
    {
        private static readonly byte[] m_Buffer = new byte[4]; // Internal format buffer.
        private MemoryStream m_Stream;
        private bool m_DynSize;
        private byte m_PacketID;

        internal Packet()
        {
            m_Stream = new MemoryStream();
        }

        internal Packet(byte packetID)
        {
            m_PacketID = packetID;
            m_DynSize = true;
        }

        internal Packet(byte packetID, int capacity)
        {
            m_Stream = new MemoryStream(capacity);

            m_PacketID = packetID;
            m_DynSize = false;

            m_Stream.WriteByte(packetID);
        }

        internal Packet(byte[] data, int len, bool dynLen)
        {
            m_Stream = new MemoryStream(len);
            m_PacketID = data[0];
            m_DynSize = dynLen;

            m_Stream.Position = 0;
            m_Stream.Write(data, 0, len);

            MoveToData();
        }

        internal void EnsureCapacity(int capacity)
        {
            m_Stream = new MemoryStream(capacity);
            Write(m_PacketID);
            if (m_DynSize)
                Write((short)0);
        }

        internal byte[] Compile()
        {
            if (m_DynSize)
            {
                m_Stream.Seek(1, SeekOrigin.Begin);
                Write((ushort)m_Stream.Length);
            }

            return ToArray();
        }

        internal void MoveToData()
        {
            m_Stream.Position = m_DynSize ? 3 : 1;
        }

        internal void Copy(Packet p)
        {
            m_Stream = new MemoryStream((int)p.Length);
            byte[] data = p.ToArray();
            m_Stream.Write(data, 0, data.Length);

            m_DynSize = p.m_DynSize;
            m_PacketID = p.m_PacketID;

            MoveToData();
        }

        /*internal override int GetHashCode()
        {
            long oldPos = m_Stream.Position;

            int code = 0;

            m_Stream.Position = 0;

            while ( m_Stream.Length - m_Stream.Position >= 4 )
                code ^= ReadInt32();

            code ^= ReadByte() | (ReadByte() << 8) | (ReadByte() << 16) | (ReadByte() << 24);

            m_Stream.Position = oldPos;

            return code;
        }*/

        /*
        internal static void Log(string line, params object[] args)
        {
            Log(String.Format(line, args));
        }

        internal static void Log(string line)
        {
            PacketLogger.SharedInstance.LogString(line);  
        }

        internal static unsafe void Log(PacketPath path, byte* buff, int len)
        {
            Log(path, buff, len, false);
        }

        internal static unsafe void Log(PacketPath path, byte* buff, int len, bool blocked)
        {
            var data = new byte[len];
            System.Runtime.InteropServices.Marshal.Copy((IntPtr)buff, data, 0, len);
            PacketLogger.SharedInstance.LogPacketData(path, data, blocked);      
        }
        */

        internal long Seek(int offset, SeekOrigin origin)
        {
            return m_Stream.Seek(offset, origin);
        }

        internal int ReadInt32()
        {
            if (m_Stream.Position + 4 > m_Stream.Length)
                return 0;

            return (ReadByte() << 24)
                | (ReadByte() << 16)
                | (ReadByte() << 8)
                | ReadByte();
        }

        internal short ReadInt16()
        {
            if (m_Stream.Position + 2 > m_Stream.Length)
                return 0;
            return (short)((ReadByte() << 8) | ReadByte());
        }

        internal byte ReadByte()
        {
            if (m_Stream.Position + 1 > m_Stream.Length)
                return 0;
            return (byte)m_Stream.ReadByte();
        }

        internal uint ReadUInt32()
        {
            if (m_Stream.Position + 4 > m_Stream.Length)
                return 0;
            return (uint)((ReadByte() << 24)
                | (ReadByte() << 16)
                | (ReadByte() << 8)
                | ReadByte());
        }

        internal ushort ReadUInt16()
        {
            if (m_Stream.Position + 2 > m_Stream.Length)
                return 0;
            return (ushort)((ReadByte() << 8) | ReadByte());
        }

        internal sbyte ReadSByte()
        {
            if (m_Stream.Position + 1 > m_Stream.Length)
                return 0;
            return (sbyte)m_Stream.ReadByte();
        }

        internal bool ReadBoolean()
        {
            if (m_Stream.Position + 1 > m_Stream.Length)
                return false;
            return (m_Stream.ReadByte() != 0);
        }

        internal string ReadUnicodeStringLE()
        {
            StringBuilder sb = new();

            int c;

            while (m_Stream.Position + 1 < m_Stream.Length && (c = ReadByte() | (ReadByte() << 8)) != 0)
                sb.Append((char)c);

            return sb.ToString();
        }

        internal string ReadUnicodeStringLESafe()
        {
            StringBuilder sb = new();

            int c;

            while (m_Stream.Position + 1 < m_Stream.Length && (c = ReadByte() | (ReadByte() << 8)) != 0)
            {
                if (IsSafeChar(c))
                    sb.Append((char)c);
            }

            return sb.ToString();
        }

        internal string ReadUnicodeStringSafe()
        {
            StringBuilder sb = new();

            int c;

            while (m_Stream.Position + 1 < m_Stream.Length && (c = ReadUInt16()) != 0)
            {
                if (IsSafeChar(c))
                    sb.Append((char)c);
            }

            return sb.ToString();
        }

        internal string ReadUnicodeString()
        {
            StringBuilder sb = new();

            int c;

            while (m_Stream.Position + 1 < m_Stream.Length && (c = ReadUInt16()) != 0)
                sb.Append((char)c);

            return sb.ToString();
        }

        internal bool IsSafeChar(int c)
        {
            return (c >= 0x20 && c < 0xFFFE);
        }

        internal string ReadUTF8StringSafe(int fixedLength)
        {
            if (m_Stream.Position >= m_Stream.Length)
                return String.Empty;

            long bound = m_Stream.Position + fixedLength;
            if (bound > m_Stream.Length)
                bound = m_Stream.Length;

            int count = 0;
            long index = m_Stream.Position;
            long start = m_Stream.Position;

            while (index < bound && ReadByte() != 0)
                ++count;

            m_Stream.Seek(start, SeekOrigin.Begin);

            index = 0;

            byte[] buffer = new byte[count];
            int value;
            while (m_Stream.Position < bound && (value = ReadByte()) != 0)
                buffer[index++] = (byte)value;

            string s = Encoding.UTF8.GetString(buffer);

            bool isSafe = true;

            for (int i = 0; isSafe && i < s.Length; ++i)
                isSafe = IsSafeChar(s[i]);

            m_Stream.Seek(start + fixedLength, SeekOrigin.Begin);

            if (isSafe)
                return s;

            StringBuilder sb = new(s.Length);

            foreach (char t in s)
            {
                if (IsSafeChar(t))
                    sb.Append(t);
            }

            return sb.ToString();
        }

        internal string ReadUTF8StringSafe()
        {
            if (m_Stream.Position >= m_Stream.Length)
                return String.Empty;

            int count = 0;
            long index = m_Stream.Position;
            long start = index;

            while (index < m_Stream.Length && ReadByte() != 0)
                ++count;

            m_Stream.Seek(start, SeekOrigin.Begin);

            index = 0;

            byte[] buffer = new byte[count];
            int value;
            while (m_Stream.Position < m_Stream.Length && (value = ReadByte()) != 0)
                buffer[index++] = (byte)value;

            string s = Encoding.UTF8.GetString(buffer);

            bool isSafe = true;

            for (int i = 0; isSafe && i < s.Length; ++i)
                isSafe = IsSafeChar(s[i]);

            if (isSafe)
                return s;

            StringBuilder sb = new(s.Length);

            foreach (char t in s)
            {
                if (IsSafeChar(t))
                    sb.Append(t);
            }

            return sb.ToString();
        }

        internal string ReadUTF8String()
        {
            if (m_Stream.Position >= m_Stream.Length)
                return String.Empty;

            int count = 0;
            long index = m_Stream.Position;
            long start = index;

            while (index < m_Stream.Length && ReadByte() != 0)
                ++count;

            m_Stream.Seek(start, SeekOrigin.Begin);

            index = 0;

            byte[] buffer = new byte[count];
            int value;
            while (m_Stream.Position < m_Stream.Length && (value = ReadByte()) != 0)
                buffer[index++] = (byte)value;

            return Encoding.UTF8.GetString(buffer);
        }

        internal string ReadString()
        {
            return ReadStringSafe();
        }

        internal string ReadStringSafe()
        {
            StringBuilder sb = new();

            int c;

            while (m_Stream.Position < m_Stream.Length && (c = ReadByte()) != 0)
                sb.Append((char)c);

            return sb.ToString();
        }

        internal string ReadUnicodeStringSafe(int fixedLength)
        {
            return ReadUnicodeString(fixedLength);
        }

        internal string ReadUnicodeString(int fixedLength)
        {
            long bound = m_Stream.Position + (fixedLength << 1);
            long end = bound;

            if (bound > m_Stream.Length)
                bound = m_Stream.Length;

            StringBuilder sb = new();

            int c;

            while ((m_Stream.Position + 1) < bound && (c = ReadUInt16()) != 0)
                if (IsSafeChar(c))
                    sb.Append((char)c);

            m_Stream.Seek(end, SeekOrigin.Begin);

            return sb.ToString();
        }

        internal string ReadStringSafe(int fixedLength)
        {
            return ReadString(fixedLength);
        }

        internal string ReadString(int fixedLength)
        {
            long bound = m_Stream.Position + fixedLength;

            if (bound > m_Stream.Length)
                bound = m_Stream.Length;

            long end = bound;

            StringBuilder sb = new();

            int c;

            while (m_Stream.Position < bound && (c = ReadByte()) != 0)
                sb.Append((char)c);

            m_Stream.Seek(end, SeekOrigin.Begin);

            return sb.ToString();
        }

        /////////////////////////////////////////////
        ///Packet Writer/////////////////////////////
        /////////////////////////////////////////////
        internal void Write(bool value)
        {
            m_Stream.WriteByte((byte)(value ? 1 : 0));
        }

        internal void Write(byte value)
        {
            m_Stream.WriteByte(value);
        }

        internal void Write(sbyte value)
        {
            m_Stream.WriteByte((byte)value);
        }

        internal void Write(short value)
        {
            m_Buffer[0] = (byte)(value >> 8);
            m_Buffer[1] = (byte)value;

            m_Stream.Write(m_Buffer, 0, 2);
        }

        internal void Write(ushort value)
        {
            m_Buffer[0] = (byte)(value >> 8);
            m_Buffer[1] = (byte)value;

            m_Stream.Write(m_Buffer, 0, 2);
        }

        internal void Write(int value)
        {
            m_Buffer[0] = (byte)(value >> 24);
            m_Buffer[1] = (byte)(value >> 16);
            m_Buffer[2] = (byte)(value >> 8);
            m_Buffer[3] = (byte)value;

            m_Stream.Write(m_Buffer, 0, 4);
        }

        internal void Write(uint value)
        {
            m_Buffer[0] = (byte)(value >> 24);
            m_Buffer[1] = (byte)(value >> 16);
            m_Buffer[2] = (byte)(value >> 8);
            m_Buffer[3] = (byte)value;

            m_Stream.Write(m_Buffer, 0, 4);
        }

        internal void Write(byte[] buffer, int offset, int size)
        {
            m_Stream.Write(buffer, offset, size);
        }

        internal void WriteAsciiFixed(string value, int size)
        {
            if (value == null)
                value = String.Empty;

            byte[] buffer = Encoding.ASCII.GetBytes(value);

            if (buffer.Length >= size)
            {
                m_Stream.Write(buffer, 0, size);
            }
            else
            {
                m_Stream.Write(buffer, 0, buffer.Length);

                byte[] pad = new byte[size - buffer.Length];

                m_Stream.Write(pad, 0, pad.Length);
            }
        }

        internal void WriteAsciiNull(string value)
        {
            if (value == null)
                value = String.Empty;

            byte[] buffer = Encoding.ASCII.GetBytes(value);

            m_Stream.Write(buffer, 0, buffer.Length);
            m_Stream.WriteByte(0);
        }

        internal void WriteLittleUniNull(string value)
        {
            if (value == null)
                value = String.Empty;

            byte[] buffer = Encoding.Unicode.GetBytes(value);

            m_Stream.Write(buffer, 0, buffer.Length);

            m_Buffer[0] = 0;
            m_Buffer[1] = 0;
            m_Stream.Write(m_Buffer, 0, 2);
        }

        internal void WriteLittleUniFixed(string value, int size)
        {
            if (value == null)
                value = String.Empty;

            size *= 2;

            byte[] buffer = Encoding.Unicode.GetBytes(value);

            if (buffer.Length >= size)
            {
                m_Stream.Write(buffer, 0, size);
            }
            else
            {
                m_Stream.Write(buffer, 0, buffer.Length);

                byte[] pad = new byte[size - buffer.Length];

                m_Stream.Write(pad, 0, pad.Length);
            }
        }

        internal void WriteBigUniNull(string value)
        {
            if (value == null)
                value = String.Empty;

            byte[] buffer = Encoding.BigEndianUnicode.GetBytes(value);

            m_Stream.Write(buffer, 0, buffer.Length);

            m_Buffer[0] = 0;
            m_Buffer[1] = 0;
            m_Stream.Write(m_Buffer, 0, 2);
        }

        internal void WriteBigUniFixed(string value, int size)
        {
            if (value == null)
                value = String.Empty;

            size *= 2;

            byte[] buffer = Encoding.BigEndianUnicode.GetBytes(value);

            if (buffer.Length >= size)
            {
                m_Stream.Write(buffer, 0, size);
            }
            else
            {
                m_Stream.Write(buffer, 0, buffer.Length);

                byte[] pad = new byte[size - buffer.Length];

                m_Stream.Write(pad, 0, pad.Length);
            }
        }

        internal void WriteUTF8Fixed(string value, int size)
        {
            if (value == null)
                value = String.Empty;

            size *= 2;

            byte[] buffer = Encoding.UTF8.GetBytes(value);

            if (buffer.Length >= size)
            {
                m_Stream.Write(buffer, 0, size);
            }
            else
            {
                m_Stream.Write(buffer, 0, buffer.Length);

                byte[] pad = new byte[size - buffer.Length];

                m_Stream.Write(pad, 0, pad.Length);
            }
        }

        internal void WriteUTF8Null(string value)
        {
            if (value == null)
                value = String.Empty;

            byte[] buffer = Encoding.UTF8.GetBytes(value);

            m_Stream.Write(buffer, 0, buffer.Length);
            m_Buffer[0] = 0;
            m_Buffer[1] = 0;
            m_Stream.Write(m_Buffer, 0, 2);
        }

        internal void Fill()
        {
            byte[] buffer = new byte[m_Stream.Capacity - Position];
            m_Stream.Write(buffer, 0, buffer.Length);
        }

        internal void Fill(int length)
        {
            m_Stream.Write(new byte[length], 0, length);
        }

        internal int PacketID
        {
            get
            {
                return m_PacketID;
            }
        }

        internal long Length
        {
            get
            {
                return m_Stream.Length;
            }
        }

        internal long Position
        {
            get
            {
                return m_Stream.Position;
            }
            set
            {
                m_Stream.Position = value;
            }
        }

        internal MemoryStream UnderlyingStream
        {
            get
            {
                return m_Stream;
            }
        }

        internal long Seek(long offset, SeekOrigin origin)
        {
            return m_Stream.Seek(offset, origin);
        }

        internal byte[] ToArray()
        {
            return m_Stream.ToArray();
        }
    }

    public unsafe sealed class PacketReader
    {
        //private unsafe byte* m_Data;
        private readonly byte[] m_Data;
        private int m_Pos;
        private readonly int m_Length;
        private readonly bool m_Dyn;

        internal unsafe PacketReader(byte* buff, int len, bool dyn)
        {
            m_Data = new byte[len];
            System.Runtime.InteropServices.Marshal.Copy((IntPtr)buff, m_Data, 0, len);
            m_Length = len;
            m_Pos = 0;
            m_Dyn = dyn;
        }

        internal unsafe PacketReader(byte[] buff, bool dyn)
        {
            //fixed (byte* p = buff)
            //  m_Data = p;
            m_Data = buff;
            m_Length = buff.Length;
            m_Pos = 0;
            m_Dyn = dyn;
        }

        internal void MoveToData()
        {
            m_Pos = m_Dyn ? 3 : 1;
        }

        internal int Seek(int offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.End:
                    m_Pos = m_Length - offset;
                    break;

                case SeekOrigin.Current:
                    m_Pos += offset;
                    break;

                case SeekOrigin.Begin:
                    m_Pos = offset;
                    break;
            }
            if (m_Pos < 0)
                m_Pos = 0;
            else if (m_Pos > m_Length)
                m_Pos = m_Length;
            return m_Pos;
        }

        internal int Length { get { return m_Length; } }

        internal bool DynamicLength { get { return m_Dyn; } }

        internal unsafe byte[] CopyBytes(int offset, int count)
        {
            byte[] read = new byte[count];
            for (m_Pos = offset; m_Pos < offset + count && m_Pos < m_Length; m_Pos++)
                read[m_Pos - offset] = m_Data[m_Pos];
            return read;
        }

        internal PacketReader GetCompressedReader()
        {
            int fullLen = ReadInt32(); // Compressed Gump data length (CLen or CTxtLen)
            byte[] buff = new byte[1];

            if (fullLen >= 4)
            {
                int packLen = ReadInt32(); // Decompressed Gump data length (DLen or DTxtLen)
                int destLen = packLen + 1000;

                if (destLen < 0)
                    destLen = 0;

                buff = new byte[destLen];

                if (fullLen > 4 && destLen > 0)
                {
                    ZLibError result = ZLibError.Z_ERRNO;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        result = DLLImport.ZLib_linux.uncompress(buff, ref destLen, CopyBytes(this.Position, fullLen - 4), fullLen - 4);

                    }
                    else
                    {
                        result = DLLImport.ZLib_windows.uncompress(buff, ref destLen, CopyBytes(this.Position, fullLen - 4), fullLen - 4);
                    }

                    if (result != ZLibError.Z_OK)
                    {
                        destLen = 0;
                        buff = new byte[1];
                    }
                }
                else
                {
                    destLen = 0;
                    buff = new byte[1];
                }
            }
            else
            {
                buff = new byte[1];
            }
            //if (buff.Length == 65310)
            //{
            //  System.Diagnostics.Debugger.Break();
            //}
            return new PacketReader(buff, false);
        }

        internal unsafe byte ReadByte()
        {
            try
            {
                if (m_Pos + 1 > m_Length || m_Data == null)
                    return 0;

                return m_Data[m_Pos++];
            }
            catch
            {
                Engine.LogCrash(new Exception("ReadByte Exception"));
                return 0;
            }
        }

        internal int ReadInt32()
        {
            return (ReadByte() << 24)
                | (ReadByte() << 16)
                | (ReadByte() << 8)
                | ReadByte();
        }

        internal short ReadInt16()
        {
            return (short)((ReadByte() << 8) | ReadByte());
        }

        internal uint ReadUInt32()
        {
            return (uint)(
                  (ReadByte() << 24)
                | (ReadByte() << 16)
                | (ReadByte() << 8)
                | ReadByte());
        }

        internal uint ReadUInt32BE()
        {
            byte b0, b1, b2, b3;
            b0 = ReadByte();
            b1 = ReadByte();
            b2 = ReadByte();
            b3 = ReadByte();
            return b3 | (uint)b2 << 8 | (uint)b1 << 16 | (uint)b0 << 24;
        }


        internal ulong ReadRawUInt64()
        {
            return
                ((ulong)ReadByte() << 0)
                | ((ulong)ReadByte() << 8)
                | ((ulong)ReadByte() << 16)
                | ((ulong)ReadByte() << 24)
                | ((ulong)ReadByte() << 32)
                | ((ulong)ReadByte() << 40)
                | ((ulong)ReadByte() << 48)
                | ((ulong)ReadByte() << 56);
        }

        internal ushort ReadUInt16()
        {
            return (ushort)((ReadByte() << 8) | ReadByte());
        }

        internal unsafe sbyte ReadSByte()
        {
            if (m_Pos + 1 > m_Length)
                return 0;
            return (sbyte)m_Data[m_Pos++];
        }

        internal bool ReadBoolean()
        {
            return (ReadByte() != 0);
        }

        internal string ReadUnicodeStringLE()
        {
            return ReadUnicodeString();
        }

        internal string ReadUnicodeStringLESafe()
        {
            return ReadUnicodeStringSafe();
        }

        internal string ReadUnicodeStringSafe()
        {
            StringBuilder sb = new();

            int c;

            while ((c = ReadUInt16()) != 0)
            {
                if (IsSafeChar(c))
                    sb.Append((char)c);
            }

            return sb.ToString();
        }

        internal string ReadUnicodeString()
        {
            StringBuilder sb = new();

            int c;

            while ((c = ReadUInt16()) != 0)
                sb.Append((char)c);

            return sb.ToString();
        }

        internal bool IsSafeChar(int c)
        {
            return (c >= 0x20 && c < 0xFFFE);
        }

        internal string ReadUTF8StringSafe(int fixedLength)
        {
            if (m_Pos >= m_Length)
                return String.Empty;

            int bound = m_Pos + fixedLength;
            if (bound > m_Length)
                bound = m_Length;

            int count = 0;
            int index = m_Pos;
            int start = m_Pos;

            while (index < bound && ReadByte() != 0)
                ++count;

            Seek(start, SeekOrigin.Begin);

            index = 0;

            byte[] buffer = new byte[count];
            int value;
            while (m_Pos < bound && (value = ReadByte()) != 0)
                buffer[index++] = (byte)value;

            string s = Encoding.UTF8.GetString(buffer);

            bool isSafe = true;

            for (int i = 0; isSafe && i < s.Length; ++i)
                isSafe = IsSafeChar(s[i]);

            Seek(start + fixedLength, SeekOrigin.Begin);

            if (isSafe)
                return s;

            StringBuilder sb = new(s.Length);

            foreach (char t in s)
            {
                if (IsSafeChar(t))
                    sb.Append(t);
            }

            return sb.ToString();
        }

        internal string ReadUTF8StringSafe()
        {
            if (m_Pos >= m_Length)
                return String.Empty;

            int count = 0;
            int index = m_Pos;
            int start = index;

            while (index < m_Length && ReadByte() != 0)
                ++count;

            Seek(start, SeekOrigin.Begin);

            index = 0;

            byte[] buffer = new byte[count];
            int value;
            while (m_Pos < m_Length && (value = ReadByte()) != 0)
                buffer[index++] = (byte)value;

            string s = Encoding.UTF8.GetString(buffer);

            bool isSafe = true;

            for (int i = 0; isSafe && i < s.Length; ++i)
                isSafe = IsSafeChar(s[i]);

            if (isSafe)
                return s;

            StringBuilder sb = new(s.Length);

            foreach (char t in s)
            {
                if (IsSafeChar(t))
                    sb.Append(t);
            }

            return sb.ToString();
        }

        internal string ReadUTF8String()
        {
            if (m_Pos >= m_Length)
                return String.Empty;

            int count = 0;
            int index = m_Pos;
            int start = index;

            while (index < m_Length && ReadByte() != 0)
                ++count;

            Seek(start, SeekOrigin.Begin);

            index = 0;

            byte[] buffer = new byte[count];
            int value;
            while (m_Pos < m_Length && (value = ReadByte()) != 0)
                buffer[index++] = (byte)value;

            return Encoding.UTF8.GetString(buffer);
        }

        internal string ReadString()
        {
            return ReadStringSafe();
        }

        internal string ReadStringSafe()
        {
            StringBuilder sb = new();

            int c;

            while (m_Pos < m_Length && (c = ReadByte()) != 0)
                sb.Append((char)c);

            return sb.ToString();
        }

        internal string ReadUnicodeStringSafe(int fixedLength)
        {
            return ReadUnicodeString(fixedLength);
        }

        internal string ReadUnicodeString(int fixedLength)
        {
            int bound = m_Pos + (fixedLength << 1);
            int end = bound;

            if (bound > m_Length)
                bound = m_Length;

            StringBuilder sb = new();

            int c;
            while ((m_Pos + 1) < bound && (c = ReadUInt16()) != 0)
                if (IsSafeChar(c))
                    sb.Append((char)c);

            Seek(end, SeekOrigin.Begin);

            return sb.ToString();
        }

        internal string ReadUnicodeStringBE(int fixedLength)
        {
            int bound = m_Pos + (fixedLength << 1);
            int end = bound;

            if (bound > m_Length)
                bound = m_Length;

            StringBuilder sb = new();

            int c;

            while ((m_Pos + 1) < bound)
            {
                c = (ushort)(ReadByte() | (ReadByte() << 8));
                sb.Append((char)c);
            }

            Seek(end, SeekOrigin.Begin);

            return sb.ToString();
        }

        internal string ReadStringSafe(int fixedLength)
        {
            return ReadString(fixedLength);
        }

        internal string ReadString(int fixedLength)
        {
            int bound = m_Pos + fixedLength;
            int end = bound;

            if (bound > m_Length)
                bound = m_Length;

            StringBuilder sb = new();

            int c;

            while (m_Pos < bound && (c = ReadByte()) != 0)
                sb.Append((char)c);

            Seek(end, SeekOrigin.Begin);

            return sb.ToString();
        }

        internal unsafe byte PacketID { get { return m_Data[0]; } }
        internal int Position { get { return m_Pos; } set { m_Pos = value; } }

        internal bool AtEnd { get { return m_Pos >= m_Length; } }
    }
}
