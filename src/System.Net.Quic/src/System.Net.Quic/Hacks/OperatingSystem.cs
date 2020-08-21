using System.Runtime.InteropServices;

namespace System
{
    public class OperatingSystem
    {
        // these methods class has been apparently added after Preview 8
        public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    }
}