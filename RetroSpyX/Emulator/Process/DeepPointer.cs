using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable 1591

// Note: Please be careful when modifying this because it could break existing components!
namespace LiveSplit.ComponentUtil
#pragma warning restore IDE0079 // Remove unnecessary suppression
{
    using OffsetT = Int32;

    public class DeepPointer
    {
        public readonly List<OffsetT> _offsets;
        public readonly OffsetT _base;
#pragma warning disable IDE0044 // Add readonly modifier
        private string _module;
#pragma warning restore IDE0044 // Add readonly modifier

        public DeepPointer(string module, OffsetT base_, params OffsetT[] offsets)
            : this(base_, offsets)
        {
            _module = module.ToLower();
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DeepPointer(OffsetT base_, params OffsetT[] offsets)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _base = base_;
#pragma warning disable IDE0028 // Simplify collection initialization
            _offsets = new List<OffsetT>();
#pragma warning restore IDE0028 // Simplify collection initialization
            _offsets.Add(0); // deref base first
            _offsets.AddRange(offsets);
        }

#pragma warning disable IDE0034 // Simplify 'default' expression
        public T Deref<T>(Process process, T default_ = default(T)) where T : struct // all value types including structs
#pragma warning restore IDE0034 // Simplify 'default' expression
        {
#pragma warning disable IDE0018 // Inline variable declaration
            T val;
#pragma warning restore IDE0018 // Inline variable declaration
            if (!Deref(process, out val))
                val = default_;
            return val;
        }

        public bool Deref<T>(Process process, out T value) where T : struct
        {
#pragma warning disable IDE0018 // Inline variable declaration
            IntPtr ptr;
#pragma warning restore IDE0018 // Inline variable declaration
            if (!DerefOffsets(process, out ptr)
                || !process.ReadValue(ptr, out value))
            {
#pragma warning disable IDE0034 // Simplify 'default' expression
                value = default(T);
#pragma warning restore IDE0034 // Simplify 'default' expression
                return false;
            }

            return true;
        }

        public byte[] DerefBytes(Process process, int count)
        {
#pragma warning disable IDE0018 // Inline variable declaration
            byte[] bytes;
#pragma warning restore IDE0018 // Inline variable declaration
            if (!DerefBytes(process, count, out bytes))
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                bytes = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
            return bytes;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public bool DerefBytes(Process process, int count, out byte[] value)
        {
#pragma warning disable IDE0018 // Inline variable declaration
            IntPtr ptr;
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning disable CS8601 // Possible null reference assignment.
            if (!DerefOffsets(process, out ptr)
                || !process.ReadBytes(ptr, count, out value))
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                value = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                return false;
            }
#pragma warning restore CS8601 // Possible null reference assignment.

            return true;
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public string DerefString(Process process, int numBytes, string default_ = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
#pragma warning disable IDE0018 // Inline variable declaration
            string str;
#pragma warning restore IDE0018 // Inline variable declaration
            if (!DerefString(process, ReadStringType.AutoDetect, numBytes, out str))
                str = default_;
            return str;
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public string DerefString(Process process, ReadStringType type, int numBytes, string default_ = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
#pragma warning disable IDE0018 // Inline variable declaration
            string str;
#pragma warning restore IDE0018 // Inline variable declaration
            if (!DerefString(process, type, numBytes, out str))
                str = default_;
            return str;
        }

        public bool DerefString(Process process, int numBytes, out string str)
        {
            return DerefString(process, ReadStringType.AutoDetect, numBytes, out str);
        }

        public bool DerefString(Process process, ReadStringType type, int numBytes, out string str)
        {
            var sb = new StringBuilder(numBytes);
            if (!DerefString(process, type, sb))
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                str = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                return false;
            }
            str = sb.ToString();
            return true;
        }

        public bool DerefString(Process process, StringBuilder sb)
        {
            return DerefString(process, ReadStringType.AutoDetect, sb);
        }

        public bool DerefString(Process process, ReadStringType type, StringBuilder sb)
        {
#pragma warning disable IDE0018 // Inline variable declaration
            IntPtr ptr;
#pragma warning restore IDE0018 // Inline variable declaration
            if (!DerefOffsets(process, out ptr)
                || !process.ReadString(ptr, type, sb))
            {
                return false;
            }
            return true;
        }

        public bool DerefOffsets(Process process, out IntPtr ptr)
        {
            bool is64Bit = process.Is64Bit();

            if (!string.IsNullOrEmpty(_module))
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                ProcessModuleWow64Safe module = process.ModulesWow64Safe()
                    .FirstOrDefault(m => m.ModuleName.ToLower() == _module);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                if (module == null)
                {
                    ptr = IntPtr.Zero;
                    return false;
                }

                ptr = module.BaseAddress + _base;
            }
            else
            {
                ptr = process.MainModuleWow64Safe().BaseAddress + _base;
            }


            for (int i = 0; i < _offsets.Count - 1; i++)
            {
                if (!process.ReadPointer(ptr + _offsets[i], is64Bit, out ptr)
                    || ptr == IntPtr.Zero)
                {
                    return false;
                }
            }

#pragma warning disable IDE0056 // Use index operator
            ptr += _offsets[_offsets.Count - 1];
#pragma warning restore IDE0056 // Use index operator
            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3f
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

#pragma warning disable IDE0251 // Make member 'readonly'
        public int IX { get { return (int)X; } }
#pragma warning restore IDE0251 // Make member 'readonly'
#pragma warning disable IDE0251 // Make member 'readonly'
        public int IY { get { return (int)Y; } }
#pragma warning restore IDE0251 // Make member 'readonly'
#pragma warning disable IDE0251 // Make member 'readonly'
        public int IZ { get { return (int)Z; } }
#pragma warning restore IDE0251 // Make member 'readonly'

        public Vector3f(float x, float y, float z) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

#pragma warning disable IDE0251 // Make member 'readonly'
        public float Distance(Vector3f other)
#pragma warning restore IDE0251 // Make member 'readonly'
        {
            float result = (X - other.X) * (X - other.X) +
                (Y - other.Y) * (Y - other.Y) +
                (Z - other.Z) * (Z - other.Z);
            return (float)Math.Sqrt(result);
        }

#pragma warning disable IDE0251 // Make member 'readonly'
        public float DistanceXY(Vector3f other)
#pragma warning restore IDE0251 // Make member 'readonly'
        {
            float result = (X - other.X) * (X - other.X) +
                (Y - other.Y) * (Y - other.Y);
            return (float)Math.Sqrt(result);
        }

#pragma warning disable IDE0251 // Make member 'readonly'
        public bool BitEquals(Vector3f other)
#pragma warning restore IDE0251 // Make member 'readonly'
        {
            return X.BitEquals(other.X)
                   && Y.BitEquals(other.Y)
                   && Z.BitEquals(other.Z);
        }

#pragma warning disable IDE0251 // Make member 'readonly'
        public bool BitEqualsXY(Vector3f other)
#pragma warning restore IDE0251 // Make member 'readonly'
        {
            return X.BitEquals(other.X)
                   && Y.BitEquals(other.Y);
        }

#pragma warning disable IDE0251 // Make member 'readonly'
        public override string ToString()
#pragma warning restore IDE0251 // Make member 'readonly'
        {
            return X + " " + Y + " " + Z;
        }
    }
}
