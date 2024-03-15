using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable 1591

// Note: Please be careful when modifying this because it could break existing components!
// http://stackoverflow.com/questions/1456785/a-definitive-guide-to-api-breaking-changes-in-net

namespace LiveSplit.ComponentUtil
#pragma warning restore IDE0079 // Remove unnecessary suppression
{
    using SizeT = UIntPtr;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class ProcessModuleWow64Safe
    {
        public IntPtr BaseAddress { get; set; }
        public IntPtr EntryPointAddress { get; set; }

        public string FileName { get; set; }

        public int ModuleMemorySize { get; set; }
        public string ModuleName { get; set; }
        public FileVersionInfo FileVersionInfo
        {
            get { return FileVersionInfo.GetVersionInfo(FileName); }
        }
        public override string ToString()
        {
#pragma warning disable CS8603 // Possible null reference return.
            return ModuleName ?? base.ToString();
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public enum ReadStringType
    {
        AutoDetect,
        ASCII,
        UTF8,
        UTF16
    }

    public static class ExtensionMethods
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0090 // Use 'new(...)'
        private static Dictionary<int, ProcessModuleWow64Safe[]> ModuleCache = new Dictionary<int, ProcessModuleWow64Safe[]>();
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0044 // Add readonly modifier

        public static ProcessModuleWow64Safe MainModuleWow64Safe(this Process p)
        {
            return p.ModulesWow64Safe().First();
        }

        public static ProcessModuleWow64Safe[] ModulesWow64Safe(this Process p)
        {
            if (ModuleCache.Count > 100)
                ModuleCache.Clear();

            const int LIST_MODULES_ALL = 3;
            const int MAX_PATH = 260;

            var hModules = new IntPtr[1024];

            uint cb = (uint)IntPtr.Size * (uint)hModules.Length;
#pragma warning disable IDE0018 // Inline variable declaration
            uint cbNeeded;
#pragma warning restore IDE0018 // Inline variable declaration

            if (!WinAPI.EnumProcessModulesEx(p.Handle, hModules, cb, out cbNeeded, LIST_MODULES_ALL))
                throw new Win32Exception();
            uint numMods = cbNeeded / (uint)IntPtr.Size;

            int hash = p.StartTime.GetHashCode() + p.Id + (int)numMods;
#pragma warning disable CA1854 // Prefer the 'IDictionary.TryGetValue(TKey, out TValue)' method
            if (ModuleCache.ContainsKey(hash))
                return ModuleCache[hash];
#pragma warning restore CA1854 // Prefer the 'IDictionary.TryGetValue(TKey, out TValue)' method

            var ret = new List<ProcessModuleWow64Safe>();

            // everything below is fairly expensive, which is why we cache!
            var sb = new StringBuilder(MAX_PATH);
            for (int i = 0; i < numMods; i++)
            {
                sb.Clear();
                if (WinAPI.GetModuleFileNameEx(p.Handle, hModules[i], sb, (uint)sb.Capacity) == 0)
                    throw new Win32Exception();
                string fileName = sb.ToString();

                sb.Clear();
                if (WinAPI.GetModuleBaseName(p.Handle, hModules[i], sb, (uint)sb.Capacity) == 0)
                    throw new Win32Exception();
                string baseName = sb.ToString();

#pragma warning disable IDE0059 // Unnecessary assignment of a value
                var moduleInfo = new WinAPI.MODULEINFO();
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                if (!WinAPI.GetModuleInformation(p.Handle, hModules[i], out moduleInfo, (uint)Marshal.SizeOf(moduleInfo)))
                    throw new Win32Exception();

                ret.Add(new ProcessModuleWow64Safe()
                {
                    FileName = fileName,
                    BaseAddress = moduleInfo.lpBaseOfDll,
                    ModuleMemorySize = (int)moduleInfo.SizeOfImage,
                    EntryPointAddress = moduleInfo.EntryPoint,
                    ModuleName = baseName
                });
            }

            ModuleCache.Add(hash, ret.ToArray());

            return ret.ToArray();
        }

        public static IEnumerable<MemoryBasicInformation> MemoryPages(this Process process, bool all = false)
        {
            // hardcoded values because GetSystemInfo / GetNativeSystemInfo can't return info for remote process
            var min = 0x10000L;
            var max = process.Is64Bit() ? 0x00007FFFFFFEFFFFL : 0x7FFEFFFFL;

            var mbiSize = (SizeT)Marshal.SizeOf(typeof(MemoryBasicInformation));

            var addr = min;
            do
            {
#pragma warning disable IDE0018 // Inline variable declaration
                MemoryBasicInformation mbi;
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning disable CA2020 // Prevent behavioral change
                if (WinAPI.VirtualQueryEx(process.Handle, (IntPtr)addr, out mbi, mbiSize) == (SizeT)0)
                    break;
#pragma warning restore CA2020 // Prevent behavioral change
                addr += (long)mbi.RegionSize;

                // don't care about reserved/free pages
                if (mbi.State != MemPageState.MEM_COMMIT)
                    continue;

                // probably don't care about guarded pages
                if (!all && (mbi.Protect & MemPageProtect.PAGE_GUARD) != 0)
                    continue;

                // probably don't care about image/file maps
                if (!all && mbi.Type != MemPageType.MEM_PRIVATE)
                    continue;

                yield return mbi;

            } while (addr < max);
        }

        public static bool Is64Bit(this Process process)
        {
#pragma warning disable IDE0018 // Inline variable declaration
            bool procWow64;
#pragma warning restore IDE0018 // Inline variable declaration
            WinAPI.IsWow64Process(process.Handle, out procWow64);
            if (Environment.Is64BitOperatingSystem && !procWow64)
                return true;
            return false;
        }

        public static bool ReadValue<T>(this Process process, IntPtr addr, out T val) where T : struct
        {
            var type = typeof(T);
            type = type.IsEnum ? Enum.GetUnderlyingType(type) : type;

#pragma warning disable IDE0034 // Simplify 'default' expression
            val = default(T);
#pragma warning restore IDE0034 // Simplify 'default' expression
#pragma warning disable IDE0018 // Inline variable declaration
            object val2;
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            if (!ReadValue(process, addr, type, out val2))
                return false;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

#pragma warning disable CS8605 // Unboxing a possibly null value.
            val = (T)val2;
#pragma warning restore CS8605 // Unboxing a possibly null value.

            return true;
        }

        public static bool ReadValue(Process process, IntPtr addr, Type type, out object? val)
        {
#pragma warning disable IDE0018 // Inline variable declaration
            byte[]? bytes;
#pragma warning restore IDE0018 // Inline variable declaration

            val = null;
            int size = type == typeof(bool) ? 1 : Marshal.SizeOf(type);
            if (!ReadBytes(process, addr, size, out bytes))
                return false;

            val = ResolveToType(bytes, type);

            return true;
        }

        public static bool ReadBytes(this Process process, IntPtr addr, int count, out byte[]? val)
        {
            var bytes = new byte[count];

#pragma warning disable IDE0018 // Inline variable declaration
            SizeT read;
#pragma warning restore IDE0018 // Inline variable declaration
            val = null;
            if (!WinAPI.ReadProcessMemory(process.Handle, addr, bytes, (SizeT)bytes.Length, out read)
                || read != (SizeT)bytes.Length)
                return false;

            val = bytes;

            return true;
        }

        public static bool ReadPointer(this Process process, IntPtr addr, out IntPtr val)
        {
            return ReadPointer(process, addr, process.Is64Bit(), out val);
        }

        public static bool ReadPointer(this Process process, IntPtr addr, bool is64Bit, out IntPtr val)
        {
            var bytes = new byte[is64Bit ? 8 : 4];

#pragma warning disable IDE0018 // Inline variable declaration
            SizeT read;
#pragma warning restore IDE0018 // Inline variable declaration
            val = IntPtr.Zero;
            if (!WinAPI.ReadProcessMemory(process.Handle, addr, bytes, (SizeT)bytes.Length, out read)
                || read != (SizeT)bytes.Length)
                return false;

#pragma warning disable CA2020 // Prevent behavioral change
            val = is64Bit ? (IntPtr)BitConverter.ToInt64(bytes, 0) : (IntPtr)BitConverter.ToUInt32(bytes, 0);
#pragma warning restore CA2020 // Prevent behavioral change

            return true;
        }

        public static bool ReadString(this Process process, IntPtr addr, int numBytes, out string str)
        {
            return ReadString(process, addr, ReadStringType.AutoDetect, numBytes, out str);
        }

        public static bool ReadString(this Process process, IntPtr addr, ReadStringType type, int numBytes, out string str)
        {
            var sb = new StringBuilder(numBytes);
            if (!ReadString(process, addr, type, sb))
            {
                str = string.Empty;
                return false;
            }

            str = sb.ToString();

            return true;
        }

        public static bool ReadString(this Process process, IntPtr addr, StringBuilder sb)
        {
            return ReadString(process, addr, ReadStringType.AutoDetect, sb);
        }

        public static bool ReadString(this Process process, IntPtr addr, ReadStringType type, StringBuilder sb)
        {
            var bytes = new byte[sb.Capacity];
#pragma warning disable IDE0018 // Inline variable declaration
            SizeT read;
#pragma warning restore IDE0018 // Inline variable declaration
            if (!WinAPI.ReadProcessMemory(process.Handle, addr, bytes, (SizeT)bytes.Length, out read)
                || read != (SizeT)bytes.Length)
                return false;

            if (type == ReadStringType.AutoDetect)
            {
                if (read.ToUInt64() >= 2 && bytes[1] == '\x0')
                    sb.Append(Encoding.Unicode.GetString(bytes));
                else
                    sb.Append(Encoding.UTF8.GetString(bytes));
            }
            else if (type == ReadStringType.UTF8)
                sb.Append(Encoding.UTF8.GetString(bytes));
            else if (type == ReadStringType.UTF16)
                sb.Append(Encoding.Unicode.GetString(bytes));
            else
                sb.Append(Encoding.ASCII.GetString(bytes));

            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == '\0')
                {
                    sb.Remove(i, sb.Length - i);
                    break;
                }
            }

            return true;
        }

#pragma warning disable IDE0034 // Simplify 'default' expression
        public static T ReadValue<T>(this Process process, IntPtr addr, T default_ = default(T)) where T : struct
#pragma warning restore IDE0034 // Simplify 'default' expression
        {
#pragma warning disable IDE0018 // Inline variable declaration
            T val;
#pragma warning restore IDE0018 // Inline variable declaration
            if (!process.ReadValue(addr, out val))
                val = default_;
            return val;
        }

        public static byte[] ReadBytes(this Process process, IntPtr addr, int count)
        {
#pragma warning disable IDE0018 // Inline variable declaration
            byte[] bytes;
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            if (!process.ReadBytes(addr, count, out bytes))
#pragma warning disable CS8603 // Possible null reference return.
                return null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning disable CS8603 // Possible null reference return.
            return bytes;
#pragma warning restore CS8603 // Possible null reference return.
        }

#pragma warning disable IDE0034 // Simplify 'default' expression
        public static IntPtr ReadPointer(this Process process, IntPtr addr, IntPtr default_ = default(IntPtr))
#pragma warning restore IDE0034 // Simplify 'default' expression
        {
#pragma warning disable IDE0018 // Inline variable declaration
            IntPtr ptr;
#pragma warning restore IDE0018 // Inline variable declaration
            if (!process.ReadPointer(addr, out ptr))
                return default_;
            return ptr;
        }

        public static string? ReadString(this Process process, IntPtr addr, int numBytes, string? default_ = null)
        {
#pragma warning disable IDE0018 // Inline variable declaration
            string? str;
#pragma warning restore IDE0018 // Inline variable declaration
            if (!process.ReadString(addr, numBytes, out str))
                return default_;
            return str;
        }

        public static string? ReadString(this Process process, IntPtr addr, ReadStringType type, int numBytes, string? default_ = null)
        {
#pragma warning disable IDE0018 // Inline variable declaration
            string? str;
#pragma warning restore IDE0018 // Inline variable declaration
            if (!process.ReadString(addr, type, numBytes, out str))
                return default_;
            return str;
        }

        public static bool WriteBytes(this Process process, IntPtr addr, byte[] bytes)
        {
#pragma warning disable IDE0018 // Inline variable declaration
            SizeT written;
#pragma warning restore IDE0018 // Inline variable declaration
            if (!WinAPI.WriteProcessMemory(process.Handle, addr, bytes, (SizeT)bytes.Length, out written)
                || written != (SizeT)bytes.Length)
                return false;

            return true;
        }

        private static bool WriteJumpOrCall(Process process, IntPtr addr, IntPtr dest, bool call)
        {
            var x64 = process.Is64Bit();

            int jmpLen = x64 ? 12 : 5;

            var instruction = new List<byte>(jmpLen);
            if (x64)
            {
                instruction.AddRange(new byte[] { 0x48, 0xB8 }); // mov rax immediate
                instruction.AddRange(BitConverter.GetBytes((long)dest));
                instruction.AddRange(new byte[] { 0xFF, call ? (byte)0xD0 : (byte)0xE0 }); // jmp/call rax
            }
            else
            {
#pragma warning disable CA2020 // Prevent behavioral change
                int offset = unchecked((int)dest - (int)(addr + jmpLen));
#pragma warning restore CA2020 // Prevent behavioral change
                instruction.AddRange(new byte[] { call ? (byte)0xE8 : (byte)0xE9 }); // jmp/call immediate
                instruction.AddRange(BitConverter.GetBytes(offset));
            }

#pragma warning disable IDE0018 // Inline variable declaration
            MemPageProtect oldProtect;
#pragma warning restore IDE0018 // Inline variable declaration
            process.VirtualProtect(addr, jmpLen, MemPageProtect.PAGE_EXECUTE_READWRITE, out oldProtect);
            bool success = process.WriteBytes(addr, instruction.ToArray());
            process.VirtualProtect(addr, jmpLen, oldProtect);

            return success;
        }

        public static bool WriteJumpInstruction(this Process process, IntPtr addr, IntPtr dest)
        {
            return WriteJumpOrCall(process, addr, dest, false);
        }

        public static bool WriteCallInstruction(this Process process, IntPtr addr, IntPtr dest)
        {
            return WriteJumpOrCall(process, addr, dest, true);
        }

        public static IntPtr WriteDetour(this Process process, IntPtr src, int overwrittenBytes, IntPtr dest)
        {
            int jmpLen = process.Is64Bit() ? 12 : 5;
            if (overwrittenBytes < jmpLen)
                throw new ArgumentOutOfRangeException(nameof(overwrittenBytes),
                    $"must be >= length of jmp instruction ({jmpLen})");

            // allocate memory to store the original src prologue bytes we overwrite with jump to dest
            // along with the jump back to src
            IntPtr gate;
            if ((gate = process.AllocateMemory(jmpLen + overwrittenBytes)) == IntPtr.Zero)
                throw new Win32Exception();

            try
            {
                // read the original bytes from the prologue of src
                var origSrcBytes = process.ReadBytes(src, overwrittenBytes);
#pragma warning disable IDE0270 // Use coalesce expression
                if (origSrcBytes == null)
                    throw new Win32Exception();
#pragma warning restore IDE0270 // Use coalesce expression

                // write the original prologue of src into the start of gate
                if (!process.WriteBytes(gate, origSrcBytes))
                    throw new Win32Exception();

                // write the jump from the end of the gate back to src
                if (!process.WriteJumpInstruction(gate + overwrittenBytes, src + overwrittenBytes))
                    throw new Win32Exception();

                // finally write the jump from src to dest
                if (!process.WriteJumpInstruction(src, dest))
                    throw new Win32Exception();

                // nop the leftover bytes in the src prologue
                int extraBytes = overwrittenBytes - jmpLen;
                if (extraBytes > 0)
                {
                    var nops = Enumerable.Repeat((byte)0x90, extraBytes).ToArray();
#pragma warning disable IDE0018 // Inline variable declaration
                    MemPageProtect oldProtect;
#pragma warning restore IDE0018 // Inline variable declaration
                    if (!process.VirtualProtect(src + jmpLen, nops.Length, MemPageProtect.PAGE_EXECUTE_READWRITE,
                        out oldProtect))
                        throw new Win32Exception();
                    if (!process.WriteBytes(src + jmpLen, nops))
                        throw new Win32Exception();
                    process.VirtualProtect(src + jmpLen, nops.Length, oldProtect);
                }
            }
            catch
            {
                process.FreeMemory(gate);
                throw;
            }

            return gate;
        }

        static object ResolveToType(byte[]? bytes, Type type)
        {
            object val;

            if (type == typeof(int))
            {
#pragma warning disable CS8604 // Possible null reference argument.
                val = BitConverter.ToInt32(bytes, 0);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            else if (type == typeof(uint))
            {
#pragma warning disable CS8604 // Possible null reference argument.
                val = BitConverter.ToUInt32(bytes, 0);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            else if (type == typeof(float))
            {
#pragma warning disable CS8604 // Possible null reference argument.
                val = BitConverter.ToSingle(bytes, 0);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            else if (type == typeof(double))
            {
#pragma warning disable CS8604 // Possible null reference argument.
                val = BitConverter.ToDouble(bytes, 0);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            else if (type == typeof(byte))
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                val = bytes[0];
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            else if (type == typeof(bool))
            {
                if (bytes == null)
                    val = false;
                else
                    val = (bytes[0] != 0);
            }
            else if (type == typeof(short))
            {
#pragma warning disable CS8604 // Possible null reference argument.
                val = BitConverter.ToInt16(bytes, 0);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            else // probably a struct
            {
                var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                try
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    val = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                }
                finally
                {
                    handle.Free();
                }
            }

#pragma warning disable CS8603 // Possible null reference return.
            return val;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static IntPtr AllocateMemory(this Process process, int size)
        {
            return WinAPI.VirtualAllocEx(process.Handle, IntPtr.Zero, (SizeT)size, (uint)MemPageState.MEM_COMMIT,
                MemPageProtect.PAGE_EXECUTE_READWRITE);
        }

        public static bool FreeMemory(this Process process, IntPtr addr)
        {
            const uint MEM_RELEASE = 0x8000;
            return WinAPI.VirtualFreeEx(process.Handle, addr, SizeT.Zero, MEM_RELEASE);
        }

        public static bool VirtualProtect(this Process process, IntPtr addr, int size, MemPageProtect protect,
            out MemPageProtect oldProtect)
        {
            return WinAPI.VirtualProtectEx(process.Handle, addr, (SizeT)size, protect, out oldProtect);
        }

        public static bool VirtualProtect(this Process process, IntPtr addr, int size, MemPageProtect protect)
        {
#pragma warning disable IDE0018 // Inline variable declaration
            MemPageProtect oldProtect;
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            return WinAPI.VirtualProtectEx(process.Handle, addr, (SizeT)size, protect, out oldProtect);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        }

        public static IntPtr CreateThread(this Process process, IntPtr startAddress, IntPtr parameter)
        {
#pragma warning disable IDE0018 // Inline variable declaration
            IntPtr threadId;
#pragma warning restore IDE0018 // Inline variable declaration
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            return WinAPI.CreateRemoteThread(process.Handle, IntPtr.Zero, (SizeT)0, startAddress, parameter, 0,
                out threadId);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        }

        public static IntPtr CreateThread(this Process process, IntPtr startAddress)
        {
            return CreateThread(process, startAddress, IntPtr.Zero);
        }

        public static void Suspend(this Process process)
        {
            WinAPI.NtSuspendProcess(process.Handle);
        }

        public static void Resume(this Process process)
        {
            WinAPI.NtResumeProcess(process.Handle);
        }

        public static float ToFloatBits(this uint i)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
        }

        public static uint ToUInt32Bits(this float f)
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(f), 0);
        }

        public static bool BitEquals(this float f, float o)
        {
            return ToUInt32Bits(f) == ToUInt32Bits(o);
        }
    }
}
