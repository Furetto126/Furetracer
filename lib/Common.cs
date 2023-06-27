using System.Runtime.InteropServices;
using GlmNet;

namespace Lib
{
    class Common
    {
        public static IntPtr GetPointer(object toPointer)
        {
            GCHandle handle = GCHandle.Alloc(toPointer, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            handle.Free();

            return ptr;
        }

        public static string GetRootDirectory()
        {
            string rootDirectory = AppDomain.CurrentDomain.BaseDirectory;

            while (!Directory.Exists(Path.Combine(rootDirectory, "src")))
            {
                rootDirectory = Directory.GetParent(rootDirectory).FullName;
            }

            return rootDirectory;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }
}
