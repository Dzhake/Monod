using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// Creates symlink using P/Invoke.
/// </summary>
public static class NativeSymlink
{
    public static void Create(string linkPath, string targetPath, bool isDirectory)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            CreateWindows(linkPath, targetPath, isDirectory);
        }
        else
        {
            CreateUnix(linkPath, targetPath);
        }
    }

    private static void CreateWindows(string linkPath, string targetPath, bool isDirectory)
    {
        const int SYMBOLIC_LINK_FLAG_DIRECTORY = 0x1;
        const int SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE = 0x2;

        // this flag might cause method to error with windows version lower than 10
        int flags = 0;

        if (Environment.OSVersion.Version is { Major: >= 11 } or { Major: 10, Build: >= 14972 })
            flags |= SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE;

        if (isDirectory)
            flags |= SYMBOLIC_LINK_FLAG_DIRECTORY;

        if (!CreateSymbolicLink(linkPath, targetPath, flags))
        {
            int error = Marshal.GetLastWin32Error();
            throw new Win32Exception(error);
        }
    }

    private static void CreateUnix(string linkPath, string targetPath)
    {
        if (symlink(targetPath, linkPath) != 0)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.U1)]
    private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

    [DllImport("libc", SetLastError = true)]
    private static extern int symlink(string target, string linkpath);
}