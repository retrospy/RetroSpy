using System;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable 1591

namespace LiveSplit.ComponentUtil
#pragma warning restore IDE0079 // Remove unnecessary suppression
{
    using SizeT = UIntPtr;

    public enum MemPageState : uint
    {
        MEM_COMMIT = 0x1000,
        MEM_RESERVE = 0x2000,
        MEM_FREE = 0x10000,
    }

    public enum MemPageType : uint
    {
        MEM_PRIVATE = 0x20000,
        MEM_MAPPED = 0x40000,
        MEM_IMAGE = 0x1000000
    }

    [Flags]
    public enum MemPageProtect : uint
    {
        PAGE_NOACCESS = 0x01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
        PAGE_WRITECOPY = 0x08,
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_GUARD = 0x100,
        PAGE_NOCACHE = 0x200,
        PAGE_WRITECOMBINE = 0x400,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MemoryBasicInformation // MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public MemPageProtect AllocationProtect;
        public SizeT RegionSize;
        public MemPageState State;
        public MemPageProtect Protect;
        public MemPageType Type;
    }

    public static class WinAPI
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer,
#pragma warning restore CA1401 // P/Invokes should not be visible
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
            SizeT nSize, out SizeT lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer,
#pragma warning restore CA1401 // P/Invokes should not be visible
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
            SizeT nSize, out SizeT lpNumberOfBytesWritten);

        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
#pragma warning disable CA1401 // P/Invokes should not be visible
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        public static extern bool EnumProcessModulesEx(IntPtr hProcess, [Out] IntPtr[] lphModule, uint cb,
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning restore CA1401 // P/Invokes should not be visible
            out uint lpcbNeeded, uint dwFilterFlag);

#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
        [DllImport("psapi.dll", SetLastError = true)]
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName,
#pragma warning restore CA1401 // P/Invokes should not be visible
            uint nSize);

        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
#pragma warning disable CA1401 // P/Invokes should not be visible
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, [Out] out MODULEINFO lpmodinfo,
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning restore CA1401 // P/Invokes should not be visible
            uint cb);

#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
        [DllImport("psapi.dll")]
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
#pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName,
#pragma warning restore CA1401 // P/Invokes should not be visible
            uint nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern bool IsWow64Process(IntPtr hProcess,
#pragma warning restore CA1401 // P/Invokes should not be visible
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
            [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process);

        [DllImport("kernel32.dll", SetLastError = true)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern SizeT VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress,
#pragma warning restore CA1401 // P/Invokes should not be visible
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
            [Out] out MemoryBasicInformation lpBuffer, SizeT dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, SizeT dwSize, uint flAllocationType,
#pragma warning restore CA1401 // P/Invokes should not be visible
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
            MemPageProtect flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, SizeT dwSize, uint dwFreeType);
#pragma warning restore CA1401 // P/Invokes should not be visible
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, SizeT dwSize,
#pragma warning restore CA1401 // P/Invokes should not be visible
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
            MemPageProtect flNewProtect, [Out] out MemPageProtect lpflOldProtect);

        [DllImport("ntdll.dll", SetLastError = true)]
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern IntPtr NtSuspendProcess(IntPtr hProcess);
#pragma warning restore CA1401 // P/Invokes should not be visible
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

        [DllImport("ntdll.dll", SetLastError = true)]
#pragma warning disable CA1401 // P/Invokes should not be visible
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        public static extern IntPtr NtResumeProcess(IntPtr hProcess);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning restore CA1401 // P/Invokes should not be visible

        [DllImport("kernel32.dll", SetLastError = true)]
#pragma warning disable CA1401 // P/Invokes should not be visible
#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, SizeT dwStackSize,
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time
#pragma warning restore CA1401 // P/Invokes should not be visible
            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

        [StructLayout(LayoutKind.Sequential)]
        public struct MODULEINFO
        {
            public IntPtr lpBaseOfDll;
            public uint SizeOfImage;
            public IntPtr EntryPoint;
        }
    }
}
