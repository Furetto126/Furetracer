using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Windowing.GraphicsLibraryFramework;

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

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)]
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags
        );

        public static char GetCharFromKeyCode(Keys keyCode)
        {
            int virtualKey = (int)keyCode;
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            if (keyCode == Keys.LeftShift || keyCode == Keys.RightShift || keyCode == Keys.LeftAlt || keyCode == Keys.RightAlt || keyCode == Keys.LeftControl || keyCode == Keys.RightControl || keyCode == Keys.Menu || virtualKey < 32 || virtualKey > 126)
            {
                return '\0';
            }

            uint scanCode = MapVirtualKey((uint)virtualKey, 0);
            StringBuilder stringBuilder = new StringBuilder();

            ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, 5, 0);

            if (stringBuilder.Length > 0)
            {
                return stringBuilder[0];
            }

            return '\0';
        }
    }
}
