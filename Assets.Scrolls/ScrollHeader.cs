using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Nonno.Assets.Scrolls;

[StructLayout(LayoutKind.Explicit, Size = 128)]
public unsafe struct ScrollHeader
{
    public const int FORMAT_LENGTH = 31;
    public const int AUTH_LENGTH = 4;
    public const int CREATIONTIME_LENGTH = sizeof(long);
    public const int CRC_LENGTH = 16;
    public const int STRTPOS_LENGTH = sizeof(ulong);

    /*
     * 000:|format                         |
     * 016:|format                       |0|
     * 032:|RESERVED                       |
     * 048:|RESERVED                       |
     * 064:|i|d|r|r|auth   |creationtime   |
     * 080:|crc            |crc-ex         |
     * 096:|RESERVED                       |
     * 112:|RESERVED       |strtpos        |
     */
    [FieldOffset(0)]
    fixed byte _format[FORMAT_LENGTH];
    [FieldOffset(64)]
    byte _ver_i;
    [FieldOffset(65)]
    byte _ver_d;
    [FieldOffset(66)]
    byte _ver_r1;
    [FieldOffset(67)]
    byte _ver_r2;
    [FieldOffset(68)]
    fixed byte _auth[AUTH_LENGTH];
    [FieldOffset(72)]
    fixed byte _creationtime[CREATIONTIME_LENGTH];
    [FieldOffset(80)]
    fixed byte _crc[CRC_LENGTH];
    [FieldOffset(120)]
    fixed byte _strtpos[STRTPOS_LENGTH];

    public ASCIIString Format
    {
        get
        {
            fixed (ScrollHeader* this_ = &this)
            {
                return new ASCIIString(new Span<byte>(this_->_format, FORMAT_LENGTH));
            }
        }
        set
        {
            if (value.Length > FORMAT_LENGTH) throw new ArgumentException("指定する文字列が長すぎます。");

            var span = value.AsSpan();
            int i = 0;
            for (; i < span.Length; i++) _format[i] = span[i];
            for (; i < FORMAT_LENGTH; i++) _format[i] = 0;
        }
    }
    public IDRVersion Version
    {
        get => new(_ver_i, _ver_d, unchecked((ushort)(_ver_r1 << 8 | _ver_r2)));
        set
        {
            _ver_i = value.i;
            _ver_d = value.d;
            _ver_r1 = unchecked((byte)(value.r >> 8));
            _ver_r2 = unchecked((byte)value.r);
        }
    }
    public DateTime CreationTime
    {
        get
        {
            long r = 0;
            fixed (ScrollHeader* this_ = &this)
            {
                Endian.HostByteOrder.Localize(this_->_strtpos, &r);
            }
            return new(r);
        }
        set
        {
            fixed (ScrollHeader* this_ = &this)
            {
                var ticks = value.Ticks;
                Endian.HostByteOrder.Standardize(&ticks, this_->_strtpos);
            }
        }
    }
    public ulong StartPosition
    {
        get
        {
            ulong r = 0;
            fixed (ScrollHeader* this_ = &this)
            {
                Endian.HostByteOrder.Localize(this_->_strtpos, &r);
            }
            return r;
        }
        set
        {
            fixed (ScrollHeader* this_ = &this)
            {
                Endian.HostByteOrder.Standardize(&value, this_->_strtpos);
            }
        }
    }

    public unsafe void Load(Stream from)
    {
        fixed (void* p = &this)
        {
            var this_ = new Span<byte>(p, sizeof(ScrollHeader));
            _ = from.Read(this_);
        }
    }

    public unsafe void Save(Stream to) 
    {
        fixed (void* p = &this)
        {
            var this_ = new Span<byte>(p, sizeof(ScrollHeader));
            to.Write(this_);
        }
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 0)]
public readonly struct IDRVersion
{
    public readonly byte i;
    public readonly byte d;
    public readonly ushort r;

    public uint Number => (unchecked((uint)i) << 24) | (unchecked((uint)d) << 16) | r;

    public IDRVersion(uint number) : this(unchecked((byte)(number >> 24)), unchecked((byte)(number >> 16)), unchecked((ushort)number)) { }
    public IDRVersion(byte i, byte d, ushort r)
    {
        this.i = i;
        this.d = d;
        this.r = r;
    }
}